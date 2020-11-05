using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Gw2Launcher.Net.AssetProxy
{
    class IPPool
    {
        private class Sample
        {
            public double total;
            public int count;
            public double avg;
            public int index;
            public IPAddress ip;
            public DateTime lastUsed;
        }

        private Dictionary<IPAddress, Sample> samples;
        private Sample[] sorted;
        private DateTime reset;

        public IPPool(IPAddress[] ips)
        {
            this.samples = new Dictionary<IPAddress, Sample>(ips.Length);
            sorted = new Sample[ips.Length];
            reset = DateTime.UtcNow.AddMinutes(10);

            int i = 0;
            foreach (IPAddress ip in ips)
            {
                samples[ip] = sorted[i] = new Sample()
                {
                    ip = ip,
                    index = i++
                };
            }
        }

        public void AddSample(IPAddress ip, double sample)
        {
            lock (sorted)
            {
                var s = samples[ip];
                s.total += sample;
                s.count++;
                s.avg = s.total / s.count;

                for (int i = s.index - 1; i >= 0; i--)
                {
                    var s2 = sorted[i];
                    if (s2.avg > s.avg)
                    {
                        sorted[s2.index = s.index] = s2;
                        sorted[s.index = i] = s;
                    }
                    else
                        break;
                }
                for (int i = s.index + 1, l = sorted.Length; i < l; i++)
                {
                    var s2 = sorted[i];
                    if (s2.avg < s.avg)
                    {
                        sorted[s2.index = s.index] = s2;
                        sorted[s.index = i] = s;
                    }
                    else
                        break;
                }
            }
        }

        public IPAddress GetIP()
        {
            lock (sorted)
            {
                double limit = sorted[0].avg * 1.5;
                if (limit == 0)
                    limit = double.MaxValue;
                var now = DateTime.UtcNow;

                if (now > reset)
                {
                    reset = now.AddMinutes(10);
                    for (int i = 0, l = sorted.Length; i < l; i++)
                    {
                        var s = sorted[i];
                        s.avg = s.total = s.count = 0;
                    }
                }

                for (int i = 0, l = sorted.Length; i < l; i++)
                {
                    var s = sorted[i];
                    var lu = now.Subtract(s.lastUsed).TotalSeconds;

                    if (s.count > 100 || lu > 5 || i == l - 1 || sorted[i + 1].avg > limit)
                    {
                        s.lastUsed = now;
                        return s.ip;
                    }
                }

                return sorted[0].ip;
            }
        }
    }
}
