using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Dat.Compression.Tree
{
    class BinaryTree
    {
        public class Node
        {
            public Node n0, n1;
            private ushort value;

            public bool HasChildren
            {
                get
                {
                    return n1 != null || n0 != null;
                }
            }

            public bool HasValue
            {
                get
                {
                    return value != 0;
                }
            }

            public ushort Value
            {
                get
                {
                    return (ushort)(value - 1);
                }
                set
                {
                    this.value = (ushort)(value + 1);
                }
            }

            public Node GetNext(bool bit)
            {
                if (bit)
                    return n1;
                return n0;
            }
        }

        public BinaryTree()
        {
            root = new Node();
        }

        private Node root;

        public Node Root
        {
            get
            {
                return root;
            }
        }

        /// <summary>
        /// Adds the specified value
        /// </summary>
        /// <param name="bits">Number of bits in the key</param>
        /// <param name="code">Bits to determine node traversal</param>
        /// <param name="value">Value given to the node</param>
        public void Add(byte bits, uint code, ushort value)
        {
            var n = root;
            var isNew = false;

            while (bits > 0)
            {
                --bits;
                var b = (code >> bits) & 1;

                if (b == 1)
                {
                    if (isNew = n.n1 == null)
                        n.n1 = new Node();
                    n = n.n1;
                }
                else
                {
                    if (isNew = n.n0 == null)
                        n.n0 = new Node();
                    n = n.n0;
                }
            }

            if (isNew)
                n.Value = value;
            else
                throw new Exception("A node with the bit code already exists");
        }
    }
}
