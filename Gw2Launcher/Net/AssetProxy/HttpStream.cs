using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace Gw2Launcher.Net.AssetProxy
{
    class HttpStream : Stream
    {
        protected const int HTTP_STREAM_TYPE_HEADER_LENGTH = 3;
        protected const int HEADER_BUFFER_LENGTH = 512;
        protected const int MAXIMUM_HEADER_LENGTH = 1024 * 16;

        protected HttpHeader header;

        protected long contentLength;
        protected long contentBytesRead;

        protected bool isContentChunked;
        protected int chunkLength;
        protected int chunkBytesRemaining;
        protected int headerLength;

        protected Stream inner;

        #region HttpHeader

        public abstract class HttpHeader
        {
            public enum ConnectionType
            {
                None,
                KeepAlive,
                Closed,
                Unknown,
            }

            protected HttpHeader(string command, WebHeaderCollection headers)
            {
                this.Headers = headers;

                ProcessCommand(command);
                ProcessHeaders(headers);
            }

            protected HttpHeader()
            {

            }

            public static HttpHeader Create(string header)
            {
                StringReader reader = new StringReader(header);
                WebHeaderCollection headers = new WebHeaderCollection();

                try
                {
                    string line = reader.ReadLine();
                    while (line.Length == 0)
                        line = reader.ReadLine();

                    string command = line;

                    while ((line = reader.ReadLine()) != null && line.Length > 0)
                    {
                        int i = line.IndexOf(':');
                        if (i != -1)
                        {
                            string key = line.Substring(0, i);
                            string value = line.Substring(i + 1).Trim();
                            headers.Add(key, value);
                        }
                    }

                    if (command.StartsWith("http/", StringComparison.OrdinalIgnoreCase))
                        return HttpResponseHeader.Create(command, headers);
                    else
                        return HttpRequestHeader.Create(command, headers);
                }
                catch
                {
                    throw;
                }
                finally
                {
                    reader.Dispose();
                }
            }

            protected abstract void ProcessCommand(string line);

            protected abstract void ProcessHeaders(WebHeaderCollection headers);

            public WebHeaderCollection Headers
            {
                get;
                protected set;
            }

            public int ContentLength
            {
                get
                {
                    if (this.Headers == null)
                        return -1;

                    int contentLength = -1;
                    string length;
                    if (this is HttpResponseHeader)
                        length = this.Headers[System.Net.HttpResponseHeader.ContentLength];
                    else
                        length = this.Headers[System.Net.HttpRequestHeader.ContentLength];
                    if (length != null)
                        Int32.TryParse(length, out contentLength);
                    return contentLength;
                }
            }

            public bool IsChunked
            {
                get
                {
                    if (this.Headers == null)
                        return false;

                    string transfer;
                    if (this is HttpResponseHeader)
                        transfer = this.Headers[System.Net.HttpResponseHeader.TransferEncoding];
                    else
                        transfer = this.Headers[System.Net.HttpRequestHeader.TransferEncoding];
                    return (transfer != null && transfer.Equals("chunked", StringComparison.OrdinalIgnoreCase));
                }
            }

            public ConnectionType Connection
            {
                get
                {
                    if (this.Headers == null)
                        return ConnectionType.None;

                    string connection;
                    if (this is HttpResponseHeader)
                        connection = this.Headers[System.Net.HttpResponseHeader.Connection];
                    else
                        connection = this.Headers[System.Net.HttpRequestHeader.Connection];

                    if (connection == null)
                        return ConnectionType.None;
                    else if (connection.Equals("keep-alive", StringComparison.OrdinalIgnoreCase))
                        return ConnectionType.KeepAlive;
                    else if (connection.Equals("close", StringComparison.OrdinalIgnoreCase))
                        return ConnectionType.KeepAlive;
                    else
                        return ConnectionType.Unknown;
                }
            }
        }

        public class HttpResponseHeader : HttpHeader
        {
            public const int STATUS_CODE_CACHED = -200;

            public struct KeepAliveOptions
            {
                public bool keepAlive;
                public int max;
                public int timeout;
            }

            protected HttpResponseHeader(string command, WebHeaderCollection headers)
                : base(command, headers)
            { }

            protected HttpResponseHeader(int statusCode, string statusDescription)
            {
                ProtocolVersion = new Version(1, 1);
                StatusCode = (HttpStatusCode)statusCode;
                StatusDescription = statusDescription;
            }

            public static HttpResponseHeader Create(string command, WebHeaderCollection headers)
            {
                return new HttpResponseHeader(command, headers);
            }

            public static HttpResponseHeader Cached()
            {
                return new HttpResponseHeader(STATUS_CODE_CACHED, "Cached");
            }

            protected override void ProcessCommand(string line)
            {
                //example: HTTP/1.1 200 OK
                string version, statusCode, statusDescription;
                int i, last;

                i = line.IndexOf(' ');
                version = line.Substring(0, i);
                last = i + 1;

                i = line.IndexOf(' ', i + 1);
                statusCode = line.Substring(last, i - last);

                statusDescription = line.Substring(i + 1);

                this.ProtocolVersion = new Version(version.Substring(version.IndexOf('/') + 1));
                this.StatusCode = (HttpStatusCode)Int32.Parse(statusCode);
                this.StatusDescription = statusDescription;
            }

            protected override void ProcessHeaders(WebHeaderCollection headers)
            {
                var value = headers["connection"];

                if (value == null || value.Equals("keep-alive", StringComparison.OrdinalIgnoreCase))
                {
                    var keepAlive = new KeepAliveOptions()
                    {
                        keepAlive = true,
                    };

                    value = headers["keep-alive"];

                    if (value != null)
                    {
                        int i, j;

                        i = value.IndexOf("max=", StringComparison.OrdinalIgnoreCase);
                        if (i != -1)
                        {
                            i += 4;
                            j = value.IndexOf(',', i);
                            if (j == -1)
                                j = value.Length;

                            Int32.TryParse(value.Substring(i, j - i), out keepAlive.max);
                        }

                        i = value.IndexOf("timeout=", StringComparison.OrdinalIgnoreCase);
                        if (i != -1)
                        {
                            i += 8;
                            j = value.IndexOf(',', i);
                            if (j == -1)
                                j = value.Length;

                            Int32.TryParse(value.Substring(i, j - i), out keepAlive.timeout);
                        }
                    }

                    this.KeepAlive = keepAlive;
                }
            }

            public Version ProtocolVersion
            {
                get;
                protected set;
            }

            public HttpStatusCode StatusCode
            {
                get;
                protected set;
            }

            public string StatusDescription
            {
                get;
                protected set;
            }

            public KeepAliveOptions KeepAlive
            {
                get;
                protected set;
            }

            public override string ToString()
            {
                return "HTTP/" + ProtocolVersion.Major + "." + ProtocolVersion.Minor + " " + (int)StatusCode + " " + StatusDescription + "\r\n" + Headers.ToString();
            }
        }

        public class HttpRequestHeader : HttpHeader
        {
            protected HttpRequestHeader(string command, WebHeaderCollection headers)
                : base(command, headers)
            { }

            public static HttpRequestHeader Create(string command, WebHeaderCollection headers)
            {
                return new HttpRequestHeader(command, headers);
            }

            protected override void ProcessCommand(string line)
            {
                //example: GET / HTTP/1.1
                string method, location, version;
                int i, last;

                i = line.IndexOf(' ');
                method = line.Substring(0, i);
                last = i + 1;

                i = line.LastIndexOf(' ');
                version = line.Substring(i + 1);

                location = line.Substring(last, i - last);

                this.ProtocolVersion = new Version(version.Substring(version.IndexOf('/') + 1));
                this.Method = method.ToUpper();
                this.Location = location;
            }

            protected override void ProcessHeaders(WebHeaderCollection headers)
            {
            }

            public string Method
            {
                get;
                private set;
            }

            public string Location
            {
                get;
                private set;
            }

            public Version ProtocolVersion
            {
                get;
                private set;
            }

            public string Host
            {
                get
                {
                    if (this.Headers == null)
                        return null;

                    return base.Headers[System.Net.HttpRequestHeader.Host];
                }
            }

            public override string ToString()
            {
                return Method + " " + Location + " HTTP/" + ProtocolVersion.Major + "." + ProtocolVersion.Minor + "\r\n" + Headers.ToString();
            }
        }

        #endregion HttpHeader

        private class ContentStream : Stream
        {
            private HttpStream inner;
            private long positionStart, position, length;

            public ContentStream(HttpStream stream)
                : base()
            {
                this.inner = stream;
                this.positionStart = stream.CanSeek ? stream.Position : 0;
                this.position = 0;

                if (stream.contentLength != -1)
                    this.length = stream.contentLength;
                else if (stream.CanSeek)
                    SeekLength();
                else
                    this.length = -1;
            }

            public override bool CanRead
            {
                get 
                {
                    return inner.CanRead;
                }
            }

            public override bool CanSeek
            {
                get 
                {
                    return inner.CanSeek;
                }
            }

            public override bool CanWrite
            {
                get 
                {
                    return false;
                }
            }

            public override void Flush()
            {
                inner.Flush();
            }

            public override long Length
            {
                get 
                {
                    return this.length;
                }
            }

            public override long Position
            {
                get
                {
                    return position;
                }
                set
                {
                    if (position > length || position < 0)
                        throw new EndOfStreamException();
                    position = value;

                    if (inner.isContentChunked)
                    {
                        SeekToPosition(value);
                    }
                    else
                    {
                        inner.Position = positionStart + position;
                        inner.contentBytesRead = position;
                    }
                }
            }

            private void SeekToPosition(long position)
            {
                //seek through the chunk headers to find the content position

                inner.Position = positionStart;
                this.position = 0;

                long contentBytes = 0;
                byte[] buffer = new byte[20];
                int read;

                do
                {
                    int chunk;
                    read = inner.ReadChunkHeader(buffer, 0, out chunk);
                    if (read > 0)
                    {
                        inner.chunkBytesRemaining = chunk;
                        inner.chunkLength = chunk;
                        if (chunk == 0)
                            throw new EndOfStreamException();

                        if (position < contentBytes + chunk)
                        {
                            int offset = (int)(position - contentBytes);
                            inner.chunkBytesRemaining -= offset;
                            inner.Position += offset;
                            this.position += offset;

                            break;
                        }
                        else
                        {
                            contentBytes += chunk;
                            inner.Position += chunk;
                            this.position += chunk;
                        }
                    }
                    else
                        throw new EndOfStreamException();
                }
                while (true);
            }

            private void SeekLength()
            {
                //seek through the chunk headers to find the content length

                inner.Position = positionStart;
                this.position = 0;

                long contentBytes = 0;
                byte[] buffer = new byte[20];
                int read;

                do
                {
                    int chunk;
                    read = inner.ReadChunkHeader(buffer, 0, out chunk);
                    if (read > 0)
                    {
                        inner.chunkBytesRemaining = chunk;
                        inner.chunkLength = chunk;
                        if (chunk == 0)
                        {
                            break;
                        }

                        contentBytes += chunk;
                        inner.Position += chunk;
                        this.position += chunk;
                    }
                    else
                        throw new EndOfStreamException();
                }
                while (true);

                this.length = contentBytes;

                inner.Position = positionStart;
                this.position = 0;
                inner.chunkBytesRemaining = 0;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read;
                if (inner.isContentChunked)
                {
                    if (inner.chunkBytesRemaining == -1)
                        return 0;

                    if (inner.chunkBytesRemaining == 0)
                    {
                        int chunk;
                        read = inner.ReadChunkHeader(buffer, offset, out chunk);
                        if (read > 0)
                        {
                            inner.chunkBytesRemaining = chunk;
                            inner.chunkLength = chunk;
                            if (chunk == 0)
                            {
                                inner.chunkBytesRemaining = -1;
                                return 0;
                            }
                        }
                        else
                            return read;
                    }

                    if (inner.chunkBytesRemaining < count)
                        count = inner.chunkBytesRemaining;
                    read = inner.Read(buffer, offset, count);

                    if (position == length && inner.chunkBytesRemaining == 0)
                    {
                        //read final chunk
                        int chunk;
                        if (inner.ReadChunkHeader(new byte[20], 0, out chunk) > 0)
                        {
                            inner.chunkBytesRemaining = chunk;
                            inner.chunkLength = chunk;
                            if (chunk == 0)
                                inner.chunkBytesRemaining = -1;
                        }
                    }
                }
                else
                {
                    read = inner.Read(buffer,offset,count);
                }
                
                this.position += read;

                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long p;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        p = offset;
                        break;
                    case SeekOrigin.End:
                        p = length + offset;
                        break;
                    case SeekOrigin.Current:
                        p = position + offset;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                if (p < 0 || p > length)
                    throw new EndOfStreamException();

                this.Position = p;

                return p;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Processes the stream as HTTP
        /// </summary>
        public HttpStream(Stream inner)
            : base()
        {
            this.inner = inner;
            this.Encoding = System.Text.Encoding.UTF8;
        }

        /// <summary>
        /// Processes the stream as HTTPS
        /// </summary>
        public HttpStream(Stream inner, string hostname, bool validateCertificates)
            : base()
        {
            SetBaseStream(inner, false, hostname, validateCertificates);
            this.Encoding = System.Text.Encoding.UTF8;
        }

        public void SetBaseStream(Stream stream, bool closeExisting)
        {
            if (closeExisting && this.inner != null)
                this.inner.Dispose();
            this.inner = stream;
        }

        public void SetBaseStream(Stream stream, bool closeExisting, string hostname, bool validateCertificates)
        {
            SslStream ssl = new SslStream(stream, false,
                new RemoteCertificateValidationCallback(
                    delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        if (errors == SslPolicyErrors.None)
                            return true;
                        else
                            return !validateCertificates;
                    }));

            try
            {
                ssl.AuthenticateAsClient(hostname);
            }
            catch
            {
                ssl.Dispose();
                throw;
            }

            SetBaseStream(ssl, closeExisting);
        }

        public Stream BaseStream
        {
            get
            {
                return inner;
            }
        }

        public Encoding Encoding
        {
            get;
            set;
        }

        public long ContentLengthProcessed
        {
            get
            {
                return contentBytesRead;
            }
        }

        private void ResetContentStream()
        {
            this.contentLength = 0;
            this.contentBytesRead = 0;
            this.isContentChunked = false;
            this.chunkLength = 0;
            this.chunkBytesRemaining = 0;
        }

        public int ReadHeader(byte[] buffer, int offset, out HttpHeader header)
        {
            ResetContentStream();

            int start = offset;
            int newLine = 0;
                        
            Stream inner = this.inner;

            do
            {
                try
                {
                    int read = inner.Read(buffer, offset, 1);

                    if (read > 0)
                    {
                        switch (buffer[offset])
                        {
                            case 13: //\r
                                break;
                            case 10: //\n
                                newLine++;

                                if (newLine == 2)
                                {
                                    offset++;

                                    header = this.header = HttpHeader.Create(this.Encoding.GetString(buffer, 0, offset));

                                    ProcessHeader(this.header);

                                    headerLength = offset - start;
                                    return offset - start;
                                }

                                break;
                            case 0:  //\0
                                newLine = 0;
                                break;
                            default:
                                newLine = 0;
                                break;
                        }

                        offset++;
                    }
                    else
                        break;
                }
                catch
                {
                    break;
                }
            }
            while (true);

            if (offset - start == 0)
            {
                header = null;
                return 0;
            }

            throw new Exception("No headers available");
        }

        protected void ProcessHeader(HttpHeader header)
        {
            this.isContentChunked = header.IsChunked;
            this.contentLength = header.ContentLength;

            if (header is HttpResponseHeader)
            {
                HttpResponseHeader response = (HttpResponseHeader)header;
            }
            else if (header is HttpRequestHeader)
            {
                HttpRequestHeader request = (HttpRequestHeader)header;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            inner.Write(buffer, offset, count);
        }

        public int ReadChunkHeader(byte[] buffer, int offset, out int chunkLength)
        {
            //example: \r\n[hex string]\r\n

            StringBuilder s = new StringBuilder(8);
            int start = offset;

            do
            {
                int read = inner.Read(buffer, offset, 1);
                if (read > 0)
                {
                    char c = (char)buffer[offset];
                    offset++;

                    if (char.IsLetterOrDigit(c))
                        s.Append(c);
                    else if (c == '\n' && s.Length > 0)
                        break;
                }
                else
                    break;
            }
            while (true);

            if (Int32.TryParse(s.ToString(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out chunkLength))
            {
                chunkBytesRemaining = chunkLength;

                if (chunkLength == 0)
                {
                    //continue reading \r\n end of file
                    while (inner.Read(buffer, offset, 1) > 0)
                    {
                        char c = (char)buffer[offset];
                        offset++;

                        if (c == '\n')
                            break;
                    }
                }
            }
            else
                throw new Exception("Unknown chunk header " + s.ToString());

            return offset - start;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read, total;

            if (isContentChunked)
            {
                if (chunkBytesRemaining == -1)
                    return 0;

                if (chunkBytesRemaining == 0)
                {
                    read = ReadChunkHeader(buffer, offset, out chunkBytesRemaining);
                    if (read > 0)
                    {
                        offset += read;
                        count -= read;
                        total = read;
                        chunkLength = chunkBytesRemaining;
                        if (chunkLength == 0)
                        {
                            chunkBytesRemaining = -1;
                            return read;
                        }
                    }
                    else
                        return read;
                }
                else
                    total = 0;

                try
                {
                    if (chunkBytesRemaining < count)
                        count = chunkBytesRemaining;
                    total += read = inner.Read(buffer, offset, count);
                    chunkBytesRemaining -= read;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                if (contentLength != -1)
                {
                    long l = contentLength - contentBytesRead;
                    if (l == 0)
                        return 0;
                    if (l < count)
                        count = (int)l;
                }
                else
                {
                    return 0;
                }

                try
                {
                    total = read = inner.Read(buffer, offset, count);
                }
                catch
                {
                    throw;
                }
            }

            contentBytesRead += read;
            return total;
        }

        private int ReadChunkHeader()
        {
            //example: \r\n[hex string]\r\n

            StringBuilder s = new StringBuilder(8);
            byte[] buffer = new byte[1];

            do
            {
                int read = inner.Read(buffer, 0, 1);
                if (read > 0)
                {
                    char c = (char)buffer[0];
                    if (char.IsLetterOrDigit(c))
                        s.Append(c);
                    else if (c == '\n' && s.Length > 0)
                        break;
                }
                else
                    break;
            }
            while (true);

            int length = 0;

            if (Int32.TryParse(s.ToString(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out length))
            {
                if (length == 0)
                {
                    //continue reading \r\n end of file
                    while (inner.Read(buffer, 0, 1) > 0)
                    {
                        char c = (char)buffer[0];
                        if (c == '\n')
                            break;
                    }
                }
            }
            else
                throw new Exception("Unknown chunk header " + s.ToString());

            return length;
        }

        public void WriteHeader(string method, string location, Version version, WebHeaderCollection headers)
        {
            StringBuilder s = new StringBuilder(512);
            s.AppendLine(method + " " + location + " HTTP/" + version);

            WriteHeader(s, headers);
        }

        public void WriteHeader(Version version, HttpStatusCode statusCode, string statusDescription, WebHeaderCollection headers)
        {
            StringBuilder s = new StringBuilder(512);
            s.AppendLine("HTTP/" + version + " " + (int)statusCode + " " + statusDescription);

            WriteHeader(s, headers);
        }

        private void WriteHeader(StringBuilder s, WebHeaderCollection headers)
        {
            for (int i = 0, length = headers.Count; i < length; i++)
            {
                s.AppendLine(headers.GetKey(i) + ": " + headers[i]);
            }
            s.AppendLine();

            byte[] buffer = Encoding.ASCII.GetBytes(s.ToString());

            inner.Write(buffer, 0, buffer.Length);
        }

        public override bool CanRead
        {
            get
            {
                return inner.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return inner.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get 
            {
                return inner.CanWrite;
            }
        }

        public override void Flush()
        {
            inner.Flush();
        }

        public override long Length
        {
            get 
            {
                return inner.Length;
            }
        }

        public override long Position
        {
            get
            {
                return inner.Position;
            }
            set
            {
                inner.Position = value;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            inner.SetLength(value);
        }

        /// <summary>
        /// Returns a stream bound to the response content and allows for seeking within the
        /// stream if applicable
        /// </summary>
        public Stream GetContentStream()
        {
            return new ContentStream(this);
        }
    }
}
