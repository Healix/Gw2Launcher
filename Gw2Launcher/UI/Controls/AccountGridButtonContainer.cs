using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Gw2Launcher.UI.Controls
{
    public partial class AccountGridButtonContainer : UserControl, IMessageFilter
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        public event EventHandler AddAccountClick;
        public event MouseEventHandler AccountMouseClick;
        public event EventHandler ContentHeightChanged;
        public event MouseEventHandler AccountBeginDrag;

        private List<AccountGridButton> buttons;
        private Size gridSize;
        private int lastSelected = -1;

        private bool isDirty;
        private bool isNewAccountVisible;
        private NewAccountGridButton buttonNewAccount;

        private Font fontLarge, fontSmall;
        private bool showAccount;

        private Rectangle dragBounds;
        private bool dragging;

        public AccountGridButtonContainer()
        {
            buttons = new List<AccountGridButton>();

            InitializeComponent();

            fontLarge = AccountGridButton.FONT_LARGE;
            fontSmall = AccountGridButton.FONT_SMALL;
            showAccount = true;

            this.GridSize = new Size(200, 67);

            panelContents.Size = Size.Empty;

            buttonNewAccount = new NewAccountGridButton();
            buttonNewAccount.Visible = false;
            buttonNewAccount.Click += buttonNewAccount_Click;
            buttonNewAccount.VisibleChanged += buttonNewAccount_VisibleChanged;
            buttonNewAccount.LostFocus += buttonNewAccount_LostFocus;

            panelContents.Controls.Add(buttonNewAccount);
            isNewAccountVisible = false;

            Application.AddMessageFilter(this);
            this.Disposed += AccountGridButtonContainer_Disposed;
            
            isDirty = true;
            this.Invalidate();
        }

        void buttonNewAccount_LostFocus(object sender, EventArgs e)
        {
            if (this.ParentForm.ContainsFocus)
            {
                if (isNewAccountVisible)
                    buttonNewAccount.Focus();
            }
            else
            {
                ShowNewAccountButton(false);
            }
        }

        void buttonNewAccount_VisibleChanged(object sender, EventArgs e)
        {
            if (buttonNewAccount.Visible)
                buttonNewAccount.Focus();
        }

        void buttonNewAccount_Click(object sender, EventArgs e)
        {
            if (AddAccountClick != null)
                AddAccountClick(this, EventArgs.Empty);
        }

        void AccountGridButtonContainer_Disposed(object sender, EventArgs e)
        {
            Application.RemoveMessageFilter(this);
        }

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x20a:

                    var pos = new Point(m.LParam.ToInt32());
                    var wParam = m.WParam.ToInt32();
                    Func<int, MouseButtons, MouseButtons> getButton =
                        (flag, button) => ((wParam & flag) == flag) ? button : MouseButtons.None;

                    var buttons = getButton(wParam & 0x0001, MouseButtons.Left)
                                | getButton(wParam & 0x0010, MouseButtons.Middle)
                                | getButton(wParam & 0x0002, MouseButtons.Right)
                                | getButton(wParam & 0x0020, MouseButtons.XButton1)
                                | getButton(wParam & 0x0040, MouseButtons.XButton2)
                                ; // Not matching for these /*MK_SHIFT=0x0004;MK_CONTROL=0x0008*/

                    var delta = wParam >> 16;
                    var e = new MouseEventArgs(buttons, 0, pos.X, pos.Y, delta);
                    OnMsgMouseWheel(e);

                    break;
                case 256:

                    ShowNewAccountButton(false);

                    break;
                case 257:

                    OnMsgKeyUp(new KeyEventArgs((Keys)m.WParam));

                    break;

                case 0x0104: //WM_SYSKEYDOWN

                    if (m.WParam == (IntPtr)Keys.Menu)
                    {
                        ShowNewAccountButton(true);
                        return true;
                    }
                    else
                        ShowNewAccountButton(false);

                    break;

                case 0x0105: //WM_SYSKEYUP

                    if (isNewAccountVisible && m.WParam == (IntPtr)Keys.Menu)
                        ShowNewAccountButton(false);

                    break;
            }

            return false;
        }

        private void ShowNewAccountButton(bool visible)
        {
            if (visible)
            {
                if (!isNewAccountVisible && this.ParentForm.ContainsFocus)
                {
                    isNewAccountVisible = true;
                    isDirty = true;
                    this.Invalidate();
                }
            }
            else if (isNewAccountVisible)
            {
                isNewAccountVisible = false;
                isDirty = true;
                this.Invalidate();
            }
        }

        void OnMsgKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                lastSelected = -1;
            }
        }

        void OnMsgMouseWheel(MouseEventArgs e)
        {
            if (verticalScroll.Visible)
            {
                int value;
                int scroll = verticalScroll.SmallChange;

                if (e.Delta > 0)
                {
                    value = verticalScroll.Value - scroll;
                    if (value < 0)
                        value = 0;
                }
                else
                {
                    value = verticalScroll.Value + scroll;
                    int max = verticalScroll.Maximum - verticalScroll.LargeChange + 1;
                    if (value > max)
                        value = max;
                }
                ((HandledMouseEventArgs)e).Handled = true;
                if (verticalScroll.Value != value)
                    verticalScroll.Value = value;
            }
        }

        private void AccountGridButtonContainer_Load(object sender, EventArgs e)
        {
        }

        void panelContents_Click(object sender, EventArgs e)
        {
        }

        public Size GridSize
        {
            get
            {
                return this.gridSize;
            }
            set
            {
                this.gridSize = value;
                isDirty = true;
                this.Invalidate();
            }
        }

        public void ClearSelected()
        {
            foreach (var button in buttons)
            {
                if (button.Selected)
                {
                    button.Selected = false;
                    button.Invalidate();
                }
            }

            lastSelected = -1;
        }

        public IList<AccountGridButton> GetSelected()
        {
            List<AccountGridButton> selected = new List<AccountGridButton>();

            foreach (var button in buttons)
            {
                if (button.Selected)
                    selected.Add(button);
            }

            return selected;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            isDirty = true;
            this.Invalidate();
        }

        public int Count
        {
            get
            {
                bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
                return buttons.Count + (isNewAccountVisible ? 1 : 0);
            }
        }

        public int ContentHeight
        {
            get
            {
                if (this.gridSize.IsEmpty)
                    return 0;

                bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
                int count = buttons.Count + (isNewAccountVisible ? 1 : 0);

                int w = this.Width - 5;
                int columns = w / this.gridSize.Width;
                if (columns < 1)
                    columns = 1;
                int rows = (count - 1) / columns + 1;
                int rowHeight = gridSize.Height + 5;

                return rowHeight * rows + 5;
            }
        }

        public void Sort(Settings.SortMode mode, Settings.SortOrder order)
        {
            buttons.Sort(new Comparison<AccountGridButton>(
                delegate(Controls.AccountGridButton a, Controls.AccountGridButton b)
                {
                    int result;

                    switch (mode)
                    {
                        case Settings.SortMode.Account:
                            result = a.AccountName.CompareTo(b.AccountName);
                            break;
                        case Settings.SortMode.LastUsed:
                            result = a.LastUsedUtc.CompareTo(b.LastUsedUtc);
                            break;
                        case Settings.SortMode.Name:
                            result = a.DisplayName.CompareTo(b.DisplayName);
                            break;
                        case Settings.SortMode.None:
                            try
                            {
                                result = a.AccountData.UID.CompareTo(b.AccountData.UID);
                            }
                            catch (Exception e)
                            {
                                Util.Logging.Log(e);
                                if (a.AccountData == b.AccountData)
                                    result = 0;
                                else if (a.AccountData == null)
                                    result = -1;
                                else
                                    result = 1;
                            }
                            break;
                        default:
                            result = 0;
                            break;
                    }

                    if (order == Settings.SortOrder.Ascending)
                        return result;

                    return -result;
                }));

            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].Index = i;
            }

            isDirty = true;
            this.Invalidate();
        }

        public void Add(AccountGridButton button)
        {
            button.FontLarge = fontLarge;
            button.FontSmall = fontSmall;
            button.ShowAccount = showAccount;

            panelContents.Controls.Add(button);
            button.Index = buttons.Count;
            buttons.Add(button);
            isDirty = true;
            this.Invalidate();

            button.MouseClick += button_MouseClick;
            button.MouseDown += button_MouseDown;
            button.MouseUp += button_MouseUp;
        }

        void button_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var c = (Control)sender;
                c.MouseMove += button_MouseMove;
                c.MouseUp += button_MouseUp;

                var size = SystemInformation.DragSize;
                dragBounds = new Rectangle(e.X - size.Width / 2, e.Y - size.Height / 2, size.Width, size.Height);
            }
        }

        void button_MouseUp(object sender, MouseEventArgs e)
        {
            var c = (Control)sender;
            c.MouseMove -= button_MouseMove;
            c.MouseUp -= button_MouseUp;
        }

        void button_MouseMove(object sender, MouseEventArgs e)
        {
            if (!dragBounds.Contains(e.Location))
            {
                var c = (Control)sender;
                c.MouseMove -= button_MouseMove;
                c.MouseUp -= button_MouseUp;

                dragging = true;

                if (AccountBeginDrag != null)
                    AccountBeginDrag(sender, e);
            }
        }

        void button_MouseClick(object sender, MouseEventArgs e)
        {
            var button = (AccountGridButton)sender;
            int index = button.Index;

            if (dragging)
            {

            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                button.Selected = !button.Selected;
                lastSelected = index;
            }
            else if (ModifierKeys.HasFlag(Keys.Shift))
            {
                if (lastSelected == -1)
                {
                    for (int i = 0; i < buttons.Count; i++)
                    {
                        button=buttons[i];
                        button.Selected = i == index;
                    }
                    lastSelected = index;
                }
                else
                {
                    for (int i = 0; i < buttons.Count;i++)
                    {
                        button=buttons[i];
                        if (lastSelected > index)
                            button.Selected = (i >= index && i <= lastSelected);
                        else
                            button.Selected = (i >= lastSelected && i <= index);
                    }
                }
            }
            else
            {
                if (AccountMouseClick != null)
                    AccountMouseClick(sender, e);
            }
        }

        public void Remove(AccountGridButton button)
        {
            if (buttons.Remove(button))
            {
                panelContents.Controls.Remove(button);
                isDirty = true;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (isDirty)
            {
                isDirty = false;
                ArrangeGrid();
            }
        }

        private void ArrangeGrid()
        {
            if (this.gridSize.IsEmpty)
                return;

            bool isNewAccountVisible = buttonNewAccount != null && (buttons.Count == 0 || this.isNewAccountVisible);
            int count = buttons.Count + (isNewAccountVisible ? 1 : 0);

            buttonNewAccount.Visible = isNewAccountVisible;

            int w = this.Width - 5;
            int columns = w / this.gridSize.Width;
            if (columns < 1)
                columns = 1;
            int columnWidth = w / columns;
            int rows = (count - 1) / columns + 1;
            int rowHeight = gridSize.Height + 5;

            int panelWidth = columnWidth * columns + 5;
            int panelHeight = rowHeight * rows + 5;

            if (ContentHeightChanged != null && panelContents.Height != panelHeight)
                ContentHeightChanged(this, EventArgs.Empty);

            if (panelHeight > this.Height)
            {
                verticalScroll.Visible = true;

                w -= verticalScroll.Width;
                columns = w / this.gridSize.Width;
                if (columns < 1)
                    columns = 1;
                columnWidth = w / columns;
                rows = (count - 1) / columns + 1;

                panelWidth = columnWidth * columns + 5;
                panelHeight = rowHeight * rows + 5;

                int largeChange = gridSize.Height + 5;
                int smallChange = largeChange / 2;

                verticalScroll.Maximum = panelHeight - this.Height + largeChange - 1;
                verticalScroll.LargeChange = largeChange;
                verticalScroll.SmallChange = verticalScroll.LargeChange / 2;
            }
            else
            {
                verticalScroll.Visible = false;
                verticalScroll.Value = 0;
            }

            panelContents.Size = new Size(panelWidth, panelHeight);

            for (int i = 0; i < count; i++)
            {
                int column = i % columns;
                int row = i / columns;

                AccountGridButton b;
                if (i == count - 1 && isNewAccountVisible)
                    b = buttonNewAccount;
                else
                    b = buttons[i];

                b.Size = new Size(columnWidth - 5, rowHeight - 5);
                b.Location = new Point(columnWidth * column + 5, rowHeight * row + 5);
            }
        }

        private void panelContents_Paint(object sender, PaintEventArgs e)
        {

        }

        private void verticalScroll_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void verticalScroll_ValueChanged(object sender, EventArgs e)
        {
            panelContents.Location = new Point(0, -verticalScroll.Value);
        }

        public void SetStyle(Font fontLarge, Font fontSmall, bool showAccount)
        {
            if (this.fontLarge == fontLarge && this.fontSmall == fontSmall && this.showAccount == showAccount)
                return;

            this.fontLarge = fontLarge;
            this.fontSmall = fontSmall;
            this.showAccount = showAccount;

            if (buttons.Count > 0)
            {
                foreach (var b in buttons)
                {
                    b.FontLarge = fontLarge;
                    b.FontSmall = fontSmall;
                    b.ShowAccount = showAccount;
                }
            }

            buttonNewAccount.FontLarge = fontLarge;
            buttonNewAccount.FontSmall = fontSmall;
            buttonNewAccount.ShowAccount = showAccount;
            buttonNewAccount.ResizeLabels();

            this.GridSize = new Size(fontLarge.Height * 30 / 2, buttonNewAccount.MinimumSize.Height);
        }
    }
}
