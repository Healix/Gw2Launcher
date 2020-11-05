using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Gw2Launcher.Tools
{
    static class AutoUpdate
    {
        public static event EventHandler<int> NewBuildAvailable;
        public static event EventHandler<DateTime> NextCheckChanged;
        public static event EventHandler Checking;
        public static event EventHandler<bool> StateChanged;

        private static CancellationTokenSource cancelToken;

        static AutoUpdate()
        {
        }

        public static void Initialize()
        {
            Settings.AutoUpdate.ValueChanged += Settings_ValueChanged;
            Settings.AutoUpdateInterval.ValueChanged += AutoUpdateInterval_ValueChanged;
            Settings.BackgroundPatchingEnabled.ValueChanged += Settings_ValueChanged;

            if (IsEnabled())
            {
                if (cancelToken == null)
                {
                    cancelToken = new CancellationTokenSource();
                    DoCheck(true);
                }
            }
        }

        private static DateTime _nextCheck;
        public static DateTime NextCheck
        {
            get
            {
                return _nextCheck;
            }
            private set
            {
                _nextCheck = value;
                if (NextCheckChanged != null)
                    NextCheckChanged(null, _nextCheck);
            }
        }

        public static bool IsEnabled()
        {
            return Settings.AutoUpdate.Value || Settings.BackgroundPatchingEnabled.Value;
        }

        static void Settings_ValueChanged(object sender, EventArgs e)
        {
            if (IsEnabled())
            {
                if (cancelToken == null)
                {
                    cancelToken = new CancellationTokenSource();
                    DoCheck(false);
                }
            }
            else if (cancelToken != null)
            {
                cancelToken.Cancel();
                NextCheck = DateTime.MinValue;

                if (StateChanged != null)
                    StateChanged(null, false);
            }
        }

        static void AutoUpdateInterval_ValueChanged(object sender, EventArgs e)
        {
            if (cancelToken != null)
                cancelToken.Cancel();
        }

        private static async void DoCheck(bool initial)
        {
            if (StateChanged != null)
                StateChanged(null, true);

            bool retry = false;
            DateTime lastCheck = DateTime.MinValue;
            int lastBuild = 0;

            while (IsEnabled())
            {
                var cancel = cancelToken.Token;

                int t;
                if (retry)
                    t = 60000;
                else if (lastCheck == DateTime.MinValue)
                    t = 1000;
                else
                {
                    int interval;
                    var v = Settings.AutoUpdateInterval;
                    if (!v.HasValue)
                        interval = 600000;
                    else
                        interval = v.Value * 60 * 1000;
                    t = interval - (int)DateTime.UtcNow.Subtract(lastCheck).TotalMilliseconds;
                }

                if (t > 0)
                {
                    NextCheck = DateTime.UtcNow.AddMilliseconds(t);

                    try
                    {
                        do
                        {
                            t = (int)_nextCheck.Subtract(DateTime.UtcNow).TotalMilliseconds + 1;
                            if (t > 60000)
                                t = 60000;
                            else if (t <= 0)
                                break;
                            await Task.Delay(t, cancel);
                        }
                        while (DateTime.UtcNow < _nextCheck);
                    }
                    catch (TaskCanceledException)
                    {
                        if (IsEnabled())
                        {
                            cancelToken.Dispose();
                            cancelToken = new CancellationTokenSource();
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        break;
                    }
                }

                if (Checking != null)
                    Checking(null, null);

                var build = await Gw2Build.GetBuildAsync();

                if (!cancel.IsCancellationRequested && !(retry = build <= 0))
                {
                    lastCheck = DateTime.UtcNow;

                    if (build != lastBuild)
                    {
                        lastBuild = build;

                        if (Settings.LastKnownBuild.Value != build)
                        {
                            if (NewBuildAvailable != null)
                                NewBuildAvailable(null, build);
                        }
                    }
                }
            }

            cancelToken.Dispose();
            cancelToken = null;
        }
    }
}
