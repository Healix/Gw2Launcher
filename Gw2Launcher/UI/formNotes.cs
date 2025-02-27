using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formNotes : Base.BaseForm
    {
        public event EventHandler<Settings.Notes.Note> NoteChanged;

        private FlatVerticalButton selectedTab;
        private Util.ReusableControls reusable;
        private Settings.IAccount account;
        private Dictionary<ushort, string> strings;
        private bool isBusy;
        private bool modified;

        public formNotes(Settings.IAccount account)
        {
            InitializeComponents();

            SetStyle(ControlStyles.ResizeRedraw, true);

            panelContent.BackColor = UiColors.GetColor(UiColors.Colors.DailiesSeparator);

            _ExpiredCount = _MessageCount = -1;

            this.Opacity = 0;

            panelContainer.MouseWheel += panelContainer_MouseWheel;
            panelContent.SizeChanged += panelContent_SizeChanged;

            labelAccountName.Text = account.Name;

            this.account = account;

            buttonMessages.Selected = true;
            selectedTab = buttonMessages;

            LoadAccount(account);
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            this.Refresh();
            this.Opacity = 1;
        }

        void panelContent_SizeChanged(object sender, EventArgs e)
        {
            scrollV.Maximum = panelContent.Height - panelContainer.Height;
        }

        void panelContainer_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                scrollV.Value -= 150;
            }
            else if (e.Delta < 0)
            {
                scrollV.Value += 150;
            }

            if (e is HandledMouseEventArgs)
                ((HandledMouseEventArgs)e).Handled = true;
        }

        private int _MessageCount;
        private int MessageCount
        {
            get
            {
                return _MessageCount;
            }
            set
            {
                if (value >= 0 && _MessageCount != value)
                {
                    _MessageCount = value;
                    OnMessageCountChanged();
                }
            }
        }

        private int _ExpiredCount;
        private int ExpiredCount
        {
            get
            {
                return _ExpiredCount;
            }
            set
            {
                if (value >= 0 && _ExpiredCount != value)
                {
                    _ExpiredCount = value;
                    OnExpiredCountChanged();
                }
            }
        }

        private void OnMessageCountChanged()
        {
            buttonMessages.Text = "Notes (" + _MessageCount + ")";
        }

        private void OnExpiredCountChanged()
        {
            buttonExpired.Text = "Recent (" + _ExpiredCount + ")";
        }

        private async void LoadAccount(Settings.IAccount account)
        {
            var notes = account.Notes;

            if (notes == null || notes.Count == 0)
            {
                strings = new Dictionary<ushort, string>();
            }
            else
            {
                strings = await GetStringsAsync(notes);
            }

            SetupControls();
        }

        private void SetupControls()
        {
            var notes = account.Notes;
            var count = 0;

            if (notes != null && notes.Count > 0)
            {
                if (reusable == null)
                    reusable = new Util.ReusableControls();
                else
                    reusable.ReleaseAll();

                var controls = reusable.CreateOrAll<NoteMessage>(notes.Count,
                    delegate
                    {
                        var c = new NoteMessage()
                        {
                            Width = panelContent.Width,
                            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                        };

                        c.DeleteClick += message_DeleteClick;
                        c.EditClick += message_EditClick;

                        return c;
                    });

                var now = DateTime.UtcNow;

                var x = 0;
                var y = 0;

                var expired = selectedTab == buttonExpired;
                Settings.Notes.Note[] _notes;

                lock (notes)
                {
                    var index = notes.IndexOf(now);

                    if (expired)
                        _notes = new Settings.Notes.Note[index];
                    else
                        _notes = new Settings.Notes.Note[notes.Count - index];

                    ExpiredCount = index;
                    MessageCount = notes.Count - index;

                    if (_notes.Length > 0)
                        notes.CopyTo(expired ? 0 : index, _notes, 0, _notes.Length);
                }

                for (var i = 0; i < _notes.Length; i++)
                {
                    Settings.Notes.Note n;
                    if (expired)
                        n = _notes[_notes.Length - i - 1];
                    else
                        n = _notes[i];

                    var c = controls.GetNext();

                    c.Location = new Point(x, y);
                    c.Tag = n;
                    c.Expires = n.Expires;

                    string message;
                    strings.TryGetValue(n.SID, out message);
                    c.Message = message;

                    c.Visible = true;

                    y += c.Height + 1;

                    count++;
                }

                if (y > 0)
                    y--;

                panelContent.Height = y;

                while (controls.HasNext)
                {
                    controls.GetNext().Visible = false;
                }
                if (controls.New != null)
                {
                    panelContent.Controls.AddRange(controls.New);
                }
            }
            else
            {
                ExpiredCount = 0;
                MessageCount = 0;
            }

            if (count > 0)
            {
                panelContent.Visible = true;
                labelMessage.Visible = false;

                scrollV.Maximum = panelContent.Height - panelContainer.Height;
            }
            else
            {
                panelContent.Visible = false;
                labelMessage.Text = "No messages";
                labelMessage.MaximumSize = new Size(panelContainer.Width * 3 / 4, panelContainer.Height);
                labelMessage.Location = new Point(panelContainer.Width / 2 - labelMessage.Width / 2, panelContainer.Height / 2 - labelMessage.Height / 2);
                labelMessage.Visible = true;
            }
        }

        async void message_EditClick(object sender, EventArgs e)
        {
            if (isBusy)
                return;

            var c = (NoteMessage)sender;
            var note = (Settings.Notes.Note)c.Tag;
            var currentTab = this.selectedTab;

            using (var f = new formNote(c.Expires, c.Message, note.Notify))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    var changed = false;
                    var sid = note.SID;

                    if (!c.Message.Equals(f.Message, StringComparison.Ordinal))
                    {
                        isBusy = true;
                        try
                        {
                            using (var store = new Tools.Notes())
                            {
                                sid = await store.ReplaceAsync(note.SID, f.Message);
                                await store.CloseAsync();

                                if (changed = note.SID != sid)
                                    strings.Remove(note.SID);
                                strings[sid] = f.Message;
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);

                            MessageBox.Show(this, "An error is preventing the note from being created:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            isBusy = false;
                        }

                        var y = c.Bottom;
                        var h = c.Height;

                        c.Message = f.Message;
                        h -= c.Height;

                        if (h != 0)
                        {
                            if (y != panelContent.Height)
                            {
                                foreach (Control control in panelContent.Controls)
                                {
                                    if (control.Top > y)
                                        control.Top -= h;
                                }
                            }
                            panelContent.Height -= h;
                        }
                    }

                    if (note.Expires != f.Expires)
                    {
                        changed = true;

                        c.Expires = f.Expires;
                    }

                    if (changed)
                    {
                        var notes = account.Notes;
                        if (notes == null)
                            account.Notes = notes = new Settings.Notes();

                        notes.Remove(note);
                        var n = new Settings.Notes.Note(f.Expires, sid, f.NotifyOnExpiry);
                        notes.Add(n);
                        c.Tag = n;

                        var wasExpired = DateTime.UtcNow >= note.Expires;
                        var isExpired = DateTime.UtcNow >= f.Expires;

                        if (wasExpired != isExpired)
                        {
                            if (wasExpired)
                            {
                                ExpiredCount--;
                                MessageCount++;
                            }
                            else
                            {
                                ExpiredCount++;
                                MessageCount--;
                            }

                            Remove(c);
                        }

                        modified = true;

                        if (NoteChanged != null)
                            NoteChanged(this, n);
                    }
                    else
                    {
                        if (note.Notify != f.NotifyOnExpiry)
                        {
                            changed = true;
                            modified = true;
                            note.Notify = f.NotifyOnExpiry;
                        }

                        if (changed && NoteChanged != null)
                            NoteChanged(this, note);
                    }
                }
            }
        }

        async void message_DeleteClick(object sender, EventArgs e)
        {
            if (isBusy)
                return;

            var c = (NoteMessage)sender;
            var note = (Settings.Notes.Note)c.Tag;
            var currentTab = this.selectedTab;

            if (currentTab == buttonExpired)
            {
                isBusy = true;
                try
                {
                    await Tools.Notes.RemoveRange(note.SID);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);

                    MessageBox.Show(this, "An error is preventing the note from being deleted:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    return;
                }
                finally
                {
                    isBusy = false;
                }

                var notes = account.Notes;
                if (notes != null)
                {
                    if (notes.Remove(note))
                        modified = true;
                }

                ExpiredCount--;
            }
            else
            {
                var notes = account.Notes;
                if (notes != null && notes.Remove(note))
                {
                    var n = new Settings.Notes.Note(DateTime.UtcNow.Subtract(new TimeSpan(0, 1, 0)), note.SID, false);
                    notes.Add(n);
                    c.Tag = n;

                    modified = true;

                    ExpiredCount++;
                }

                MessageCount--;
            }

            Remove(c);
        }

        /// <summary>
        /// Hides the control and moves all other controls to fill the space
        /// </summary>
        private void Remove(NoteMessage c)
        {
            c.Visible = false;

            var y = c.Bottom;

            if (y == panelContent.Height)
            {
                if (c.Top > 0)
                    panelContent.Height -= c.Height + 1;
                else
                    SetupControls();
            }
            else
            {
                var offset = c.Height + 1;

                foreach (Control control in panelContent.Controls)
                {
                    if (control.Top > y)
                        control.Top -= offset;
                }

                panelContent.Height -= offset;
            }
        }

        private Task<Dictionary<ushort, string>> GetStringsAsync(Settings.Notes notes)
        {
            return Task<Dictionary<ushort, string>>.Run(
                delegate
                {
                    var strings = new Dictionary<ushort, string>();
                    try
                    {
                        using (var store = new Tools.Notes())
                        {
                            foreach (var n in notes)
                            {
                                strings[n.SID] = store.Get(n.SID);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                    return strings;
                });
        }

        private void buttonMessages_Click(object sender, EventArgs e)
        {
            SelectTab((FlatVerticalButton)sender);
        }

        private void buttonExpired_Click(object sender, EventArgs e)
        {
            SelectTab((FlatVerticalButton)sender);
        }

        private void SelectTab(FlatVerticalButton button)
        {
            if (selectedTab == button || isBusy)
                return;

            if (selectedTab != null)
                selectedTab.Selected = false;

            scrollV.Maximum = 0;
            panelContent.Visible = false;
            labelMessage.Visible = false;

            selectedTab = button;
            button.Selected = true;
            scrollV.Value = 0;

            SetupControls();
        }

        private async void labelAdd_Click(object sender, EventArgs e)
        {
            if (isBusy)
                return;

            labelAdd.Enabled = false;

            using (var f = new formNote())
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.Notes.Note n = null;

                    isBusy = true;
                    try
                    {
                        var sid = await Tools.Notes.AddRange(f.Message);

                        strings[sid[0]] = f.Message;

                        var notes = this.account.Notes;
                        if (notes == null)
                            this.account.Notes = notes = new Settings.Notes();

                        n = new Settings.Notes.Note(f.Expires, sid[0], f.NotifyOnExpiry);
                        notes.Add(n);
                        modified = true;

                        if (DateTime.UtcNow < n.Expires)
                        {
                            if (selectedTab == buttonMessages)
                                SetupControls();
                            else
                                MessageCount++;
                        }
                        else
                        {
                            if (selectedTab == buttonExpired)
                                SetupControls();
                            else
                                ExpiredCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);

                        MessageBox.Show(this, "An error is preventing the note from being created:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        isBusy = false;
                    }

                    if (n != null && NoteChanged != null)
                        NoteChanged(this, n);
                }
            }

            labelAdd.Enabled = true;
        }

        private void buttonMinimize_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            using (var p = new Pen(UiColors.GetColor(UiColors.Colors.MainBorder)))
            {
                e.Graphics.DrawRectangle(p, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        private void scrollV_ValueChanged(object sender, int e)
        {
            panelContent.Top = -e;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCLBUTTONDBLCLK:

                    break;
                case WindowMessages.WM_ENTERSIZEMOVE:

                    panelContainer.SuspendLayout();
                    base.WndProc(ref m);

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    panelContainer.ResumeLayout(true);
                    base.WndProc(ref m);
                    SetupControls();

                    Settings.WindowBounds[this.GetType()].Value = this.Bounds;

                    break;
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    if (m.Result == (IntPtr)HitTest.Client)
                    {
                        var p = this.PointToClient(new Point(m.LParam.GetValue32()));

                        if (p.Y > buttonExpired.Bottom && p.Y < buttonMinimize.Top)
                        {
                            if (p.X >= this.Width - 5)
                                m.Result = (IntPtr)HitTest.BottomRight;
                            else
                                m.Result = (IntPtr)HitTest.Caption;
                        }
                    }

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        public bool Modified
        {
            get
            {
                return modified;
            }
        }

        public override void RefreshColors()
        {
            panelContent.BackColor = UiColors.GetColor(UiColors.Colors.DailiesSeparator);

            base.RefreshColors();
        }
    }
}
