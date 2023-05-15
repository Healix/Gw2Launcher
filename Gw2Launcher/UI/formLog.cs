using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI
{
    public partial class formLog : Base.BaseForm
    {
        private Queue<Util.Logging.LogEventArgs> queue;

        public formLog()
        {
            queue = new Queue<Util.Logging.LogEventArgs>();

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            AddMessage("Build " + Program.BUILD + " on " + Environment.OSVersion.Version.ToString());
            AddMessage("PID " + System.Diagnostics.Process.GetCurrentProcess().Id);
            AddMessage(Client.Launcher.GetActiveProcessCount() + " active");
            AddMessage("Logging enabled");

            Util.Logging.LogMessage += Logging_LogMessage;
            Util.Logging.Enabled = true;

            ShowMessages();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            Util.Logging.Enabled = false;
            Util.Logging.LogMessage -= Logging_LogMessage;
        }

        void Logging_LogMessage(object sender, Util.Logging.LogEventArgs e)
        {
            lock (queue)
            {
                queue.Enqueue(e);
            }
        }

        private void AddMessage(string message)
        {
            lock (queue)
            {
                queue.Enqueue(new Util.Logging.LogEventArgs(null, message));
            }
        }

        private async void ShowMessages()
        {
            await Task.Delay(100);

            var sb = new StringBuilder();

            while (this.Visible)
            {
                lock (queue)
                {
                    while (queue.Count > 0)
                    {
                        var q = queue.Dequeue();

                        sb.Append('[');
                        sb.Append(q.Timestamp.ToString("HH:mm:ss"));
                        sb.Append(']');

                        if (q.Account != null)
                        {
                            sb.Append('[');
                            sb.Append(q.Account.UID);
                            sb.Append(']');
                        }

                        sb.Append(": ");
                        sb.Append(q.Message);

                        sb.AppendLine();
                    }
                }

                if (sb.Length > 0)
                {
                    textLog.AppendText(sb.ToString());
                    sb.Length = 0;
                }

                await Task.Delay(500);
            }
        }
    }
}
