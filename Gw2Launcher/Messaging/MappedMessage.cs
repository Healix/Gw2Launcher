using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace Gw2Launcher.Messaging
{
    class MappedMessage : IDisposable
    {
        public static string BASE_ID = "Gw2Launcher-";

        protected MemoryMappedFile mf;
        protected int id;

        public MappedMessage(int id, MemoryMappedFile mf)
        {
            this.id = id;
            this.mf = mf;
        }

        public int ID
        {
            get
            {
                return id;
            }
        }

        public void Dispose()
        {
            if (mf != null)
            {
                mf.Dispose();
                mf = null;
            }
        }
    }
}
