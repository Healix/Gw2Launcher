using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formProcessSettingsPopup : Base.StackFormBase
    {
        private Settings.IAccount account;
        private System.Diagnostics.Process process;
        private Windows.Volume.VolumeControl volume;
        private long affinityMask;
        private bool delayedVolume;
        private Settings.WindowOptions initialWindowOptions;

        public formProcessSettingsPopup(Settings.IAccount account, System.Diagnostics.Process process)
        {
            this.account = account;
            this.process = process;

            affinityMask = (long)(Environment.ProcessorCount >= 64 ? ulong.MaxValue : ((ulong)1 << Environment.ProcessorCount) - 1);

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            this.Opacity = 0;

            comboPriority.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.High, "High"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.AboveNormal, "Above normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Normal, "Normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.BelowNormal, "Below normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Low, "Low"),
                });
            
            comboAffinity.Items.Add(new Util.ComboItem<long>(affinityMask, "All"));

            if (this.account.ProcessAffinity > 0)
                comboAffinity.Items.Add(new Util.ComboItem<long>(this.account.ProcessAffinity, "Account (" + Util.Bits.GetBitCount((ulong)this.account.ProcessAffinity) + ")"));
            var settings = Settings.GetSettings(account.Type);
            if (settings.ProcessAffinity.Value > 0)
                comboAffinity.Items.Add(new Util.ComboItem<long>(settings.ProcessAffinity.Value, "Settings (" + Util.Bits.GetBitCount((ulong)settings.ProcessAffinity.Value) + ")"));

            try
            {
                for (int i = 0, count = Settings.AffinityValues.Count; i < count; i++)
                {
                    var v = Settings.AffinityValues[i];
                    comboAffinity.Items.Add(new Util.ComboItem<long>(v.Affinity, v.Name));
                }
            }
            catch { }

            var wo = initialWindowOptions = Client.Launcher.GetWindowOptions(account);

            Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboPriority, Client.Launcher.GetPriority(process).ToProcessPriorityClass());

            try
            {
                afdProcess.Affinity = process.ProcessorAffinity.GetValue();
                Util.ComboItem<long>.Select(comboAffinity, afdProcess.Affinity);
            }
            catch { }

            if (comboPriority.SelectedIndex == -1)
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboPriority, Settings.ProcessPriorityClass.Normal);

            checkBlockMinimizeClose.Enabled = true;

            try
            {
                var h = Windows.FindWindow.FindMainWindow(process);
                if (h != IntPtr.Zero && Windows.WindowLong.HasValue(h, GWL.GWL_STYLE, WindowStyle.WS_MINIMIZEBOX))
                {
                    panelWindowOptions.Visible = true;

                    checkTopMost.Checked = (wo & Settings.WindowOptions.TopMost) != 0;
                    checkPreventResizing.Checked = (wo & Settings.WindowOptions.PreventChanges) != 0;
                    checkBlockMinimizeClose.Checked = (wo & Settings.WindowOptions.DisableTitleBarButtons) != 0;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }

            afdProcess.AffinityChanged += afdProcess_AffinityChanged;

            try
            {
                volume = new Windows.Volume.VolumeControl(process.Id);
                this.Disposed += formProcessSettingsPopup_Disposed;
                if (volume.Query())
                {
                    panelVolume.Visible = true;
                    sliderVolume.Value = volume.Volume;
                }
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
            }
        }

        void afdProcess_AffinityChanged(object sender, EventArgs e)
        {
            try
            {
                if (afdProcess.Tag == null)
                    comboAffinity.SelectedIndex = -1;
                process.ProcessorAffinity = new IntPtr(afdProcess.Affinity);
            }
            catch { }
        }

        void formProcessSettingsPopup_Disposed(object sender, EventArgs e)
        {
            if (volume != null)
            {
                volume.Dispose();
                volume = null;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var hasExited = false;
            
            try
            {
                hasExited = process == null || process.HasExited;
            }
            catch { }
            
            if (hasExited)
            {
                this.Dispose();
                return;
            }

            var b = new Rectangle(Cursor.Position.X - this.Width / 2, Cursor.Position.Y - Cursor.Size.Height, this.Width, this.Height);

            try
            {
                var h = Windows.FindWindow.FindMainWindow(process);
                var p = Windows.WindowSize.GetWindowPlacement(h);

                b = Util.RectangleConstraint.Constrain(p.rcNormalPosition.ToRectangle(), b);
            }
            catch { }

            this.Location = Util.RectangleConstraint.ConstrainToScreen(b).Location;

            if (!NativeMethods.SetForegroundWindow(this.Handle))
            {
                AutoClose();
            }

            FadeIn(0.9f, 100);
        }

        private async void AutoClose()
        {
            var wasActive = false;
            EventHandler onActivated = delegate
            {
                wasActive = true;
            };
            this.Activated += onActivated;

            while (true)
            {
                await Task.Delay(500);

                if (this.IsDisposed)
                    return;
                if (wasActive || !this.Visible)
                    break;
                if (!Rectangle.Inflate(this.Bounds, Cursor.Size.Width / 2, Cursor.Size.Height / 2).Contains(Cursor.Position))
                {
                    this.Dispose();
                    break;
                }
            }

            this.Activated -= onActivated;
        }

        private async void FadeIn(float o, int duration)
        {
            var t = Environment.TickCount;

            do
            {
                await Task.Delay(10);

                var e = Environment.TickCount - t;
                if (e > duration)
                    break;
                this.Opacity = o * e / duration;
            }
            while (true);

            this.Opacity = o;
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            if (panelWindowOptions.Visible)
            {
                var mask = Settings.WindowOptions.Windowed | Settings.WindowOptions.TopMost | Settings.WindowOptions.PreventChanges | Settings.WindowOptions.DisableTitleBarButtons;
                var wo = Settings.WindowOptions.Windowed;

                if (checkTopMost.Checked)
                    wo |= Settings.WindowOptions.TopMost;
                if (checkPreventResizing.Checked)
                    wo |= Settings.WindowOptions.PreventChanges;
                if (checkBlockMinimizeClose.Checked && checkBlockMinimizeClose.Enabled)
                    wo |= Settings.WindowOptions.DisableTitleBarButtons;

                if ((initialWindowOptions & mask) != wo)
                    Client.Launcher.SetWindowOptions(account, wo);
            }

            this.Dispose();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= (int)(Windows.Native.WindowStyle.WS_EX_LAYERED);
                return cp;
            }
        }

        private void comboPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;

            if (comboPriority.SelectedIndex != -1)
                Client.Launcher.SetPriority(process, Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboPriority, Settings.ProcessPriorityClass.Normal));
        }

        private void comboAffinity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;

            if (comboAffinity.SelectedIndex != -1)
            {
                afdProcess.Tag = sender;
                afdProcess.Affinity = Util.ComboItem<long>.SelectedValue(comboAffinity, affinityMask);
                afdProcess.Tag = null;
            }
        }

        private void sliderVolume_ValueChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;
            OnVolumeChanged();
        }

        private async void OnVolumeChanged()
        {
            if (delayedVolume)
                return;
            delayedVolume = true;

            await Task.Delay(100);

            delayedVolume = false;

            try
            {
                volume.Volume = sliderVolume.Value;
            }
            catch { }
        }

        private void checkPreventResizing_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkPreventResizing.Checked)
                checkBlockMinimizeClose.Checked = false;
        }

        private void checkBlockMinimizeClose_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBlockMinimizeClose.Checked)
            {
                if (!checkPreventResizing.Checked)
                    checkPreventResizing.CheckState = CheckState.Indeterminate;
            }
            else if (checkPreventResizing.CheckState == CheckState.Indeterminate)
                checkPreventResizing.Checked = false;
            
        }
    }
}
