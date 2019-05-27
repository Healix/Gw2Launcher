using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Tools.Dat.Compression.Tree
{
    class TreeBuilder
    {
        private List<ushort>[] values;
        private ushort count;
        private byte bits;

        public TreeBuilder()
        {
            values = new List<ushort>[31];
        }

        public ushort Value
        {
            get;
            set;
        }

        public void Add(byte bits, ushort value)
        {
            if (bits == 0)
                return;

            var values = this.values[bits - 1];
            if (values == null)
            {
                this.values[bits - 1] = values = new List<ushort>();
                if (bits > this.bits)
                    this.bits = bits;
            }
            values.Add(value);
            ++count;
        }

        public BinaryTree Build()
        {
            var tree = new BinaryTree();

            if (count > 0)
            {
                ushort code = 1;

                for (byte bit = 1; bit <= this.bits; bit++)
                {
                    var values = this.values[bit - 1];

                    if (values != null)
                    {
                        for (var i = values.Count - 1; i >= 0; i--)
                        {
                            tree.Add(bit, code, values[i]);
                            --code;
                        }
                    }

                    code = (ushort)(code << 1 | 1);
                }
            }
            else
            {
                tree.Root.Value = this.Value;
            }

            return tree;
        }
    }
}
