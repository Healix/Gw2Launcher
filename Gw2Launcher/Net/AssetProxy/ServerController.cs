using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;

namespace Gw2Launcher.Net.AssetProxy
{
    static class ServerController
    {
        private static ProxyServer server;
        private static bool isEnabled;

        public static event EventHandler<ProxyServer> Created;
        public static event EventHandler<bool> EnabledChanged;

        static ServerController()
        {
            Settings.GW2Arguments.ValueChanged += GW2Arguments_ValueChanged;
            GW2Arguments_ValueChanged(Settings.GW2Arguments, null);
        }

        static void GW2Arguments_ValueChanged(object sender, EventArgs e)
        {
            string args = ((Settings.ISettingValue<string>)sender).Value;

            bool isEnabled;
            if (!string.IsNullOrEmpty(args))
            {
                isEnabled = args.IndexOf("-l:assetsrv") != -1;

                if (isEnabled)
                {
                    if (server == null)
                    {
                        server = new ProxyServer();
                        if (Created != null)
                            Created(null, server);
                    }

                    IPEndPoint remoteEp;
                    string assetsrv = Util.Args.GetValue(args, "assetsrv");
                    if (!string.IsNullOrEmpty(assetsrv))
                    {
                        Util.IPEndPoint.TryParse(assetsrv, 80, out remoteEp);
                    }
                    else
                        remoteEp = null;

                    server.RemoteEP = remoteEp;
                }
            }
            else
            {
                isEnabled = false;
            }

            if (server != null && !isEnabled && isEnabled != ServerController.isEnabled)
            {
                server.Stop();
                //server = null;
            }

            if (ServerController.isEnabled != isEnabled)
            {
                ServerController.isEnabled = isEnabled;

                if (EnabledChanged != null)
                    EnabledChanged(null, isEnabled);
            }
        }

        public static bool Enabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (isEnabled != value)
                {
                    string args;
                    if (Settings.GW2Arguments.HasValue)
                    {
                        args = Settings.GW2Arguments.Value;
                        if (args==null)
                            args="";
                    }
                    else
                        args="";

                    Settings.GW2Arguments.Value = Util.Args.AddOrReplace(args, "l:assetsrv", value ? "-l:assetsrv" : "");

                    //isEnabled = value;
                }
            }
        }

        public static ProxyServer Server
        {
            get
            {
                return server;
            }
        }

        public static ProxyServer Active
        {
            get
            {
                if (isEnabled)
                {
                    server.Start();
                    return server;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
