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
            Settings.LocalAssetServerEnabled.ValueChanged += LocalAssetServerEnabled_ValueChanged;
            Settings.GuildWars2.Arguments.ValueChanged += GW2Arguments_ValueChanged;
            Settings.PatchingPort.ValueChanged += PatchingPort_ValueChanged;
            Settings.PatchingOptions.ValueChanged += PatchingOptions_ValueChanged;

            LocalAssetServerEnabled_ValueChanged(Settings.LocalAssetServerEnabled, null);
        }

        static void PatchingOptions_ValueChanged(object sender, EventArgs e)
        {
            SetPort();
        }

        static void PatchingPort_ValueChanged(object sender, EventArgs e)
        {
            SetPort();
        }

        private static void SetPort()
        {
            if (server == null)
                return;

            var options = Settings.PatchingOptions.Value;
            var port = Settings.PatchingPort.Value;
            ushort p;

            if (options.HasFlag(Settings.PatchingFlags.OverrideHosts))
            {
                p = 80;
            }
            else
            {
                p = port;
            }

            server.DefaultPort = p;
        }

        static void LocalAssetServerEnabled_ValueChanged(object sender, EventArgs e)
        {
            bool isEnabled = ((Settings.ISettingValue<bool>)sender).Value;

            if (isEnabled)
            {
                if (server == null)
                {
                    server = new ProxyServer();
                    SetPort();
                    if (Created != null)
                        Created(null, server);
                }

                GW2Arguments_ValueChanged(Settings.GuildWars2.Arguments, null);
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

        static void GW2Arguments_ValueChanged(object sender, EventArgs e)
        {
            string args = ((Settings.ISettingValue<string>)sender).Value;

            if (!string.IsNullOrEmpty(args) && server != null)
            {
                string assetsrv = Util.Args.GetValue(args, "assetsrv");
                EndPoint remoteEp = null;

                if (!string.IsNullOrEmpty(assetsrv))
                {
                    IPEndPoint ipEp;
                    if (Util.IPEndPoint.TryParse(assetsrv, 0, out ipEp))
                    {
                        remoteEp = ipEp;
                    }
                    else
                    {
                        DnsEndPoint dnsEp;
                        if (Util.DnsEndPoint.TryParse(assetsrv, 0, out dnsEp))
                        {
                            remoteEp = dnsEp;
                        }
                    }
                }

                server.RemoteEP = remoteEp;
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
                    //string args;
                    //if (Settings.GuildWars2.Arguments.HasValue)
                    //{
                    //    args = Settings.GuildWars2.Arguments.Value;
                    //    if (args==null)
                    //        args="";
                    //}
                    //else
                    //    args="";

                    //Settings.GuildWars2.Arguments.Value = Util.Args.AddOrReplace(args, "l:assetsrv", value ? "-l:assetsrv" : "");

                    Settings.LocalAssetServerEnabled.Value = value;

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
