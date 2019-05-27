using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gw2Launcher.Tools.Dat.Compression.IO;
using Gw2Launcher.Tools.Dat.Compression.Tree;

namespace Gw2Launcher.Tools.Dat.Compression
{
    class Archive
    {
        private struct BitCode
        {
            public BitCode(byte bits, uint code, ushort value)
            {
                this.bits = bits;
                this.code = code;
                this.value = value;
            }

            public byte bits;
            public uint code;
            public ushort value;
        }

        private class Node
        {
            public ushort value;
            public uint weight;
            public Node n0, n1;
        }

        private class CopyData
        {
            public byte value;
            public ushort count;
            public ushort length;
            public int[] positions;
            public ushort index;
            public byte add;
        }

        private BinaryTree tPrimary;

        public Archive()
        {
            tPrimary = BuildTree();
        }

        private byte[][] GetBitValues()
        {
            var bitcodes = new byte[][]
            {
                new byte[] { 8, 9, 10},
                new byte[] { 0, 7, 11, 12},
                new byte[] { 6, 41, 42, 224},
                new byte[] { 4, 5, 32, 40, 43, 44, 64, 74},
                new byte[] { 3, 13, 37, 38, 39, 72, 73},
                new byte[] { 36, 71, 75, 76, 105, 106},
                new byte[] { 35, 70, 96, 99, 103, 104, 136, 137, 160, 232},
                new byte[] { 1, 2, 45, 67, 68, 69, 101, 102, 128, 135, 138, 168, 169, 192, 201, 233},
                new byte[] { 14, 77, 100, 107, 108, 132, 133, 139, 164, 165, 170, 200, 229},
                new byte[] { 131, 134, 166, 167, 199, 202, 231},
                new byte[] { 34, 46, 140, 196, 228, 230},
                new byte[] { 78, 109, 198, 236},
                new byte[] { 15, 16, 17, 141, 171, 172, 204, 234},
                new byte[] { 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 33, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 65, 66, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 97, 98, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 129, 130, 142, 143, 144, 145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 161, 162, 163, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 193, 194, 195, 197, 203, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 225, 226, 227, 235, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255},
            };

            return bitcodes;
        }

        private BinaryTree BuildTree()
        {
            var tree = new BinaryTree();
            var bitvalues = GetBitValues();

            uint code = 7;
            byte bits = 3;

            foreach (var values in bitvalues)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    tree.Add(bits, code, values[i]);
                    --code;
                }
                code = code << 1 | 1;
                ++bits;
            }

            return tree;
        }

        private BitCode[] BuildTable()
        {
            var bitvalues = GetBitValues();
            var codes = new BitCode[256];

            uint code = 7;
            byte bits = 3;

            foreach (var values in bitvalues)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    var value = values[i];
                    codes[value] = new BitCode(bits, code, value);
                    --code;
                }
                code = code << 1 | 1;
                ++bits;
            }

            return codes;
        }

        /// <summary>
        /// Decompresses a raw file stream (doesn't include headers or chunks)
        /// </summary>
        /// <param name="uncompressedSize">Size of the uncompressed file, or -1 if unknown</param>
        public byte[] DecompressRaw(Stream stream, int uncompressedSize)
        {
            using (var fs = new IO.FileStream(stream, true, false, false))
            {
                fs.BlockSize = 0;

                return DecompressStream(fs, false, uncompressedSize);
            }
        }

        /// <summary>
        /// Decompresses a raw file stream (doesn't include headers or chunks)
        /// </summary>
        /// <param name="uncompressedSize">Size of the uncompressed file, or -1 if unknown</param>
        public byte[] DecompressRaw(byte[] data, int uncompressedSize)
        {
            using (var stream = new MemoryStream(data, false))
            {
                using (var fs = new IO.FileStream(stream, true, false, false))
                {
                    fs.BlockSize = 0;

                    return DecompressStream(fs, false, uncompressedSize);
                }
            }
        }

        /// <summary>
        /// Decompresses a dat file stream (includes headers and chunks)
        /// </summary>
        /// <param name="limit">Maximum number of bytes to read from the stream</param>
        public byte[] Decompress(Stream stream, int limit)
        {
            using (var fs = new IO.FileStream(stream, true, false, false))
            {
                return DecompressStream(fs, true, limit);
            }
        }

        /// <summary>
        /// Decompresses a dat file stream (includes headers and chunks)
        /// </summary>
        /// <param name="limit">Maximum number of bytes to read from the stream</param>
        public byte[] Decompress(byte[] data, int limit)
        {
            using (var stream = new MemoryStream(data, false))
            {
                using (var fs = new IO.FileStream(stream, true, false, false))
                {
                    return DecompressStream(fs, true, limit);
                }
            }
        }

        private byte[] DecompressStream(Stream stream, bool hasHeader, int maxBytes)
        {
            //note: the general method is from gw2DatTools - fixed to decompress all dat files, including files from the patch server

            using (var bs = new BitStream(stream))
            {
                int uncompressedSize;
                bool unknownSize;

                if (hasHeader)
                {
                    //compressed dat files have headers and a 4-byte crc at the end of every 65k block

                    var header = bs.ReadBits(32);
                    if (header != 2147549192)
                        throw new Exception("Unknown header");
                    uncompressedSize = (int)bs.ReadBits(32);
                    unknownSize = false;
                }
                else
                {
                    if (maxBytes >= 0)
                    {
                        uncompressedSize = maxBytes;
                        unknownSize = false;
                    }
                    else
                    {
                        uncompressedSize = (int)((stream.Length - stream.Position) * 3.5);
                        maxBytes = int.MaxValue;
                        unknownSize = true;
                    }
                }

                if (bs.ReadBits(4) != 0)
                    throw new Exception("Unknown code");
                var writeAdd = bs.ReadBits(4) + 1;

                var root = tPrimary.Root;
                var bufferPosition = 0;
                var buffer = new byte[uncompressedSize];

                try
                {
                    while (bufferPosition < uncompressedSize || unknownSize)
                    {
                        var tCode = ReadTree(bs, root).Root; //code tree used for this data
                        var tCopy = ReadTree(bs, root).Root; //special codes to copy data blocks

                        var limit = (bs.ReadBits(4) + 1) << 12;

                        while (limit > 0)
                        {
                            if (bufferPosition >= uncompressedSize)
                            {
                                if (unknownSize)
                                {
                                    uncompressedSize = buffer.Length + 256 + (int)(stream.Length - stream.Position) * 2;
                                    Array.Resize(ref buffer, uncompressedSize);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            var n = ReadNode(bs, tCode);

                            --limit;

                            //0-255 for normal data, 256+ for special codes
                            if (n.Value < 0x100)
                            {
                                buffer[bufferPosition++] = (byte)n.Value;
                                continue;
                            }

                            uint code, q, size, offset;

                            code = (uint)(n.Value - 0x100);
                            q = code / 4;

                            if (q == 0)
                            {
                                size = code;
                            }
                            else if (q < 7)
                            {
                                size = (1U << (byte)(q - 1)) * (code % 4 + 4);
                            }
                            else if (code == 28)
                            {
                                size = 255;
                            }
                            else
                            {
                                throw new Exception("Unknown write size code");
                            }

                            if (q > 1 && code != 28)
                            {
                                size |= bs.ReadBits((byte)(q - 1));
                            }

                            size += writeAdd;

                            n = ReadNode(bs, tCopy);
                            code = n.Value;
                            q = code / 2;

                            if (q == 0)
                            {
                                offset = code;
                            }
                            else if (q < 17)
                            {
                                offset = (1U << (byte)(q - 1)) * (code % 2 + 2);
                            }
                            else
                            {
                                throw new Exception("Unknown write offset code");
                            }

                            if (q > 1)
                            {
                                offset |= bs.ReadBits((byte)(q - 1));
                            }

                            ++offset;

                            if (bufferPosition + size > uncompressedSize)
                            {
                                if (unknownSize)
                                {
                                    uncompressedSize = buffer.Length + (int)size + (int)(stream.Length - stream.Position) * 2;
                                    Array.Resize(ref buffer, uncompressedSize);
                                }
                                else
                                {
                                    size = (uint)(uncompressedSize - bufferPosition);
                                }
                            }

                            while (size > 0)
                            {
                                buffer[bufferPosition] = buffer[bufferPosition - offset];
                                ++bufferPosition;
                                --size;
                            }
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    if (unknownSize)
                    {
                        Array.Resize(ref buffer, bufferPosition);
                    }
                    else
                    {
                        throw;
                    }
                }

                return buffer;
            }
        }

        private BinaryTree ReadTree(BitStream bs, BinaryTree.Node root)
        {
            var count = (ushort)bs.ReadBits(16);

            if (count > 285)
                throw new Exception("Unexpected number of symbols");

            var builder = new TreeBuilder()
            {
                Value = (ushort)(count - 1), //used as the root value when no nodes were added
            };

            while (count > 0)
            {
                var n = ReadNode(bs, root);
                var bits = (byte)(n.Value & 0x1F);
                var symbols = (ushort)((n.Value >> 5) + 1);

                if (symbols > count)
                    throw new Exception("Unexpected number of symbols in tree");

                if (bits == 0)
                {
                    count -= symbols;
                }
                else
                {
                    while (symbols > 0)
                    {
                        builder.Add(bits, --count);
                        --symbols;
                    }
                }
            }

            return builder.Build();
        }

        private BinaryTree.Node ReadNode(BitStream bs, BinaryTree.Node root)
        {
            var n = root;

            while (!n.HasValue)
            {
                n = n.GetNext(bs.ReadBit());
            }

            return n;
        }

        private Node Dequeue(Queue<Node> q1, Queue<Node> q2)
        {
            if (q1.Count > 0)
            {
                if (q2.Count > 0 && q2.Peek().weight < q1.Peek().weight)
                    return q2.Dequeue();
                return q1.Dequeue();
            }
            else
            {
                return q2.Dequeue();
            }
        }

        public int Compress(byte[] data, int offset, int length, Stream output)
        {
            var startPosition = output.Position;

            using (var bs = new Dat.Compression.IO.BitStream(new Dat.Compression.IO.FileStream(output, true, true, true)))
            {
                ushort max;
                CopyData copy;
                var table = BuildCompressionTable(data, offset, length, out max, out copy);
                var codes = BuildTable();

                bs.WriteBits(2147549192, 32); //header
                bs.WriteBits((uint)data.Length, 32); //uncompressed file size

                //write size modifier
                if (copy != null)
                    bs.WriteBits(copy.add, 8);
                else
                    bs.WriteBits(0, 8);

                var ofs = 0;
                var hasCopy = copy != null && copy.count > 0;
                var next = hasCopy ? copy.positions[0] : 0;
                var pindex = 0;

                do
                {
                    bs.WriteBits((uint)max + 1, 16); //code symbols count

                    //code symbols. must be in order from max to 0, up to 8 symbols per code
                    for (int i = max; i >= 0; i--)
                    {
                        var v0 = table[i];
                        uint v;

                        if (i > 0)
                        {
                            var v1 = table[i - 1];
                            v = 1;

                            while (v1.bits == v0.bits && v < 8 && i - v > 0)
                            {
                                ++v;
                                v1 = table[i - v];
                            }

                            if (v > 1)
                            {
                                i -= (byte)(v - 1);
                                v = (v - 1) << 5 | v0.bits;
                            }
                            else
                            {
                                v = v0.bits;
                            }
                        }
                        else
                        {
                            v = v0.bits;
                        }

                        var b = codes[v];
                        bs.WriteBits(b.code, b.bits);
                    }

                    if (copy != null)
                    {
                        bs.WriteBits(1, 16); //copy symbols count

                        //the copy tree is only used to write zeroes, so it won't be containing any nodes
                        var b = codes[0];
                        bs.WriteBits(b.code, b.bits);
                    }
                    else
                    {
                        bs.WriteBits(0, 16); //copy symbols count
                    }

                    var remaining = data.Length - ofs;
                    var blimit = (remaining - 1) >> 12;
                    if (blimit > 15)
                        blimit = 15;
                    var limit = (blimit + 1) << 12;

                    bs.WriteBits((uint)blimit, 4); //code limit

                    do
                    {
                        var b = data[ofs];

                        if (hasCopy && next == ofs)
                        {
                            var code = table[copy.index];
                            bs.WriteBits(code.code, code.bits);
                            ofs += copy.length - 1;
                            if (hasCopy = --copy.count > 0)
                                next = copy.positions[++pindex];
                        }
                        else
                        {
                            var code = table[b];
                            bs.WriteBits(code.code, code.bits);
                        }

                        ++ofs;
                    }
                    while (--limit > 0 && ofs < data.Length);
                }
                while (ofs < data.Length);
            }

            return (int)(output.Position - startPosition);
        }

        private void Traverse(Node n, BitCode[] codes, byte bits, uint code)
        {
            if (n.n0 == n.n1)
            {
                codes[n.value] = new BitCode()
                {
                    bits = bits,
                    code = code,
                    value = n.value,
                };
            }
            else
            {
                if (n.n0 != null)
                    Traverse(n.n0, codes, (byte)(bits + 1), code << 1);
                if (n.n1 != null)
                    Traverse(n.n1, codes, (byte)(bits + 1), (code << 1) | 1);
            }
        }

        private BitCode[] BuildCompressionTable(byte[] data, int offset, int length, out ushort max, out CopyData copy)
        {
            const byte WRITE_ADD_LIMIT = 15;
            const byte CODE_LIMIT = 7;
            const ushort COPY_LIMIT = WRITE_ADD_LIMIT + 1 + CODE_LIMIT;

            var nodes = new Node[264]; //0-255 normal 256-284 special (264+ not used here)
            var zeroes = new Dictionary<int, List<int>>();
            var count = 0;

            //build array of most common bytes
            for (var i = offset; i < length; i++)
            {
                var b = data[i];
                var n = nodes[b];

                if (n == null)
                {
                    nodes[b] = n = new Node()
                    {
                        value = b,
                    };
                    ++count;
                }

                ++n.weight;

                if (b == 0)
                {
                    //some files contain a lot of empty data, searching for zeroes

                    var zerocount = i;

                    while (i < length - 1 && data[i + 1] == b)
                    {
                        ++i;
                    }

                    zerocount = i - zerocount;
                    n.weight += (uint)zerocount;

                    //zeroes will probably be compressed to 1 bit, whereas the copy code will probably be 10+ bits, so it'll take more than 10 to be worthwhile
                    if (zerocount > 10)
                    {
                        ++zerocount;
                        List<int> l;
                        if (!zeroes.TryGetValue(zerocount, out l))
                            zeroes[zerocount] = l = new List<int>();
                        l.Add(i - zerocount + 1);
                    }

                    zerocount = 0;
                }
            }

            if (zeroes.Count > 0)
            {
                var zcounts = new int[zeroes.Count];
                zeroes.Keys.CopyTo(zcounts, 0);
                Array.Sort(zcounts);

                var zrt = 0;
                var zct = 0;
                var zri = -1;
                var pcount = 0;

                for (var i = zcounts.Length - 1; i >= 0; --i)
                {
                    //find how many zeroes will make the most worthwhile reduction,
                    //assuming zeroes are compressed as 1 bit and copy codes are 10+
                    //thus at least 11+ zeroes are needed (+1 to act as the reference)

                    var zi = zcounts[i];
                    var zc = zi - 1;
                    var pc = zeroes[zi].Count;
                    int zr;

                    if (zc > COPY_LIMIT) //max limit
                    {
                        //check if the limit or splitting is more worthwhile

                        zr = pc * (COPY_LIMIT - 10);

                        zc = zc / (zc / COPY_LIMIT + 1);
                        var pc2 = (zi - 1) / zc * pc;
                        var zr2 = pc2 * (zc - 10);

                        if (zr2 > zr)
                        {
                            zr = zr2;
                            pc = pc2;
                        }
                        else
                            zc = COPY_LIMIT;
                    }
                    else
                        zr = pc * (zc - 10);

                    for (var j = i + 1; j < zcounts.Length; ++j)
                    {
                        zi = zcounts[j];
                        var c = (zi - 1) / zc * zeroes[zi].Count;
                        pc += c;
                        zr += c * (zc - 10);
                    }

                    if (zr > zrt)
                    {
                        zct = zc;
                        zrt = zr;
                        zri = i;
                        pcount = pc;
                    }
                }

                if (zri != -1)
                {
                    //create ordered array of copy positions

                    var positions = new int[pcount];
                    var i = 0;

                    for (var j = zri; j < zcounts.Length; ++j)
                    {
                        var zi = zcounts[j];
                        var z = zeroes[zi];

                        for (var k = 0; k < z.Count; ++k)
                        {
                            var l = z[k] + 1;
                            var zc = zi - 1;

                            while (zc >= zct)
                            {
                                positions[i++] = l;

                                l += zct;
                                zc -= zct;
                            }
                        }
                    }

                    Array.Sort(positions);

                    var add = zct - 1;
                    var index = 256;
                    if (add > WRITE_ADD_LIMIT)
                    {
                        index += add - WRITE_ADD_LIMIT;
                        add = WRITE_ADD_LIMIT;
                    }

                    //nodes[0].weight -= (uint)(positions.Length * (zct - 1));

                    nodes[index] = new Node()
                    {
                        value = (ushort)index,
                        weight = (uint)i,
                    };

                    copy = new CopyData()
                    {
                        count = (ushort)i,
                        value = 0,
                        length = (ushort)zct,
                        positions = positions,
                        index = (ushort)index,
                        add = (byte)add,
                    };

                    ++count;
                }
                else
                    copy = null;
            }
            else
                copy = null;

            //sort by least to most common
            Array.Sort(nodes,
                delegate(Node a, Node b)
                {
                    if (a != null && b != null)
                    {
                        int c = (int)a.weight - (int)b.weight;
                        if (c == 0)
                            c = a.value - b.value;
                        return c;
                    }
                    else if (a == b)
                        return 0;
                    else if (a != null)
                        return -1;
                    else
                        return 1;
                });

            //build tree
            var q1 = new Queue<Node>(count);
            var q2 = new Queue<Node>(count / 2);

            for (var i = 0; i < count; i++)
            {
                q1.Enqueue(nodes[i]);
            }

            while (q1.Count + q2.Count > 1)
            {
                var a = Dequeue(q1, q2);
                var b = Dequeue(q1, q2);

                var parent = new Node()
                {
                    weight = a.weight + b.weight,
                    n0 = a,
                    n1 = b,
                };

                q2.Enqueue(parent);
            }

            var root = Dequeue(q1, q2);
            var table = new BitCode[264];

            q1.Enqueue(root);

            byte bits = 0;
            uint code = 0;
            ushort queued = 0;
            var comparer = Comparer<Node>.Create(
                delegate(Node a, Node b)
                {
                    return a.value - b.value;
                });
            max = 0;
            count = 1;

            //build bit code table
            do
            {
                var n = q1.Dequeue();

                if (n.n0 == n.n1)
                {
                    nodes[queued++] = n;
                }
                else
                {
                    if (n.n0 != null)
                        q1.Enqueue(n.n0);
                    if (n.n1 != null)
                        q1.Enqueue(n.n1);
                }

                if (--count == 0)
                {
                    if (queued > 0)
                    {
                        Array.Sort(nodes, 0, queued, comparer);
                        for (var i = 0; i < queued; i++)
                        {
                            n = nodes[i];
                            table[n.value] = new BitCode(bits, code, n.value);
                            --code;
                        }
                        var v = nodes[queued - 1].value;
                        if (v > max)
                            max = v;
                        queued = 0;
                    }

                    ++bits;
                    count = q1.Count;

                    code = code << 1 | 1;
                }
            }
            while (count > 0);

            return table;
        }
    }
}
