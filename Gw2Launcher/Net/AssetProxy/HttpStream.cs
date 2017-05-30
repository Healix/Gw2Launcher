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

        protected Stream inner;

        #region HttpHeader

        public abstract class HttpHeader
        {
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

                    if (command.ToLower().StartsWith("http/"))
                        return HttpResponseHeader.Create(command, headers);
                    else
                        return HttpRequestHeader.Create(command, headers);
                }
                catch (Exception e)
                {
                    throw e;
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
                    return (transfer != null && transfer.ToLower() == "chunked");
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
                string value;
                value = headers["connection"];
                if (value != null && value.Equals("keep-alive", StringComparison.OrdinalIgnoreCase))
                {
                    value = headers["keep-alive"];

                    KeepAliveOptions keepAlive = new KeepAliveOptions();
                    keepAlive.keepAlive = true;

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

        public HttpStream(Stream inner)
            : base()
        {
            this.inner = inner;

            this.Encoding = System.Text.Encoding.UTF8;
        }

        public Stream BaseStream
        {
            get
            {
                return inner;
            }
            set
            {
                inner = value;
            }
        }

        public Encoding Encoding
        {
            get;
            set;
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
                                break;
                            case 0:  //\0
                                newLine = 0;
                                break;
                            default:
                                newLine = 0;
                                break;
                        }

                        offset++;

                        if (newLine == 2)
                        {
                            header = this.header = HttpHeader.Create(this.Encoding.GetString(buffer, 0, offset));

                            ProcessHeader(this.header);

                            return offset - start;
                        }
                        //else if (offset == count)
                        //{
                        //    byte[] _buffer = new byte[count + HEADER_BUFFER_LENGTH];
                        //    Array.Copy(buffer, _buffer, count);
                        //    buffer = _buffer;
                        //    count += HEADER_BUFFER_LENGTH;
                        //}
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
                catch (Exception e)
                {
                    throw e;
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
                catch (Exception e)
                {
                    throw e;
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
    }
}
