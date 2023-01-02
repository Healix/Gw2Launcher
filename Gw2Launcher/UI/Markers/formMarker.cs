using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gw2Launcher.Tools.Markers;
using Gw2Launcher.UI.Controls.ListPanel;

namespace Gw2Launcher.UI.Markers
{
    public partial class formMarker : Base.BaseForm
    {
        private MarkerIcons icons;
        private Tooltip.FloatingTooltip tooltip;
        private Settings.IMarker source;

        public formMarker(MarkerIcons icons)
            : this(icons, null)
        {
        }

        public formMarker(MarkerIcons icons, Settings.IMarker source)
        {
            this.icons = icons;
            this.source = source;

            InitializeComponents();
        }

        protected override void OnInitializeComponents()
        {
            InitializeComponent();

            panelIcons.SuspendLayout();

            panelIcons.Controls.Add(CreateMarkerButton(Settings.MarkerIconType.Circle, panelColor.BackColor));
            panelIcons.Controls.Add(CreateMarkerButton(Settings.MarkerIconType.Square, panelColor.BackColor));

            if (icons.Count > 0)
            {
                var keys = icons.Keys;
                foreach (var k in keys)
                {
                    Tools.Shared.Images.IValueSource i;
                    if (k is string && icons.TryGetIcon(k, out i))
                    {
                        var b = CreateMarkerButton(Settings.MarkerIconType.Icon, Color.Red, i);
                        panelIcons.Controls.Add(b);
                    }
                }
            }

            panelIcons.ResumeLayout();

            if (source != null)
            {
                textDescription.Text = source.Description;
                checkClickToHide.Checked = source.HideOnClick;

                switch (source.IconType)
                {
                    case Settings.MarkerIconType.Circle:
                    case Settings.MarkerIconType.Square:

                        panelColor.BackColor = source.IconColor;

                        break;
                }
                
                SelectedIconButton = GetMarkerButton(source.IconType, source.IconPath);

                if (source.TriggersHide != null)
                {
                    foreach (var t in source.TriggersHide)
                    {
                        panelHideTriggerContainer.Add(GetText(t), t, false);
                    }
                }

                if (source.TriggersShow != null)
                {
                    foreach (var t in source.TriggersShow)
                    {
                        panelShowTriggerContainer.Add(GetText(t), t, false);
                    }
                }
            }
        }

        private string GetText(Settings.MarkerTriggerCondition t)
        {
            return string.IsNullOrEmpty(t.Description) ? t.ToString() : t.Description;
        }

        private Controls.FlatMarkerIconButton GetMarkerButton(Settings.MarkerIconType type, string path = null)
        {
            foreach (Controls.FlatMarkerIconButton b in panelIcons.Controls)
            {
                if (b.Marker == type)
                {
                    if (type == Settings.MarkerIconType.Icon && b.ImageSource.Source.Key is string && (string)b.ImageSource.Source.Key == path)
                    {
                        return b;
                    }
                }
            }

            if (type == Settings.MarkerIconType.Icon && path != null)
            {
                //this shouldn't be possible (icons weren't pre-loaded)
                return CreateMarkerButton(type, Color.Empty, this.icons.GetIcon(path));
            }

            return null;
        }

        private Controls.FlatMarkerIconButton CreateMarkerButton(Settings.MarkerIconType type, Color color, Tools.Shared.Images.IValueSource source = null)
        {
            const int PADDING = 2;

            var b = new Controls.FlatMarkerIconButton()
            {
                Padding = new Padding(PADDING),
                Margin = new Padding(0),
                BackColorHovered = Color.LightGray,
                BackColorSelected = Color.LightSteelBlue,
                BorderColorSelected = Color.SteelBlue,
                PrioritizeSelectedColoring = true,
                ForeColor = color,
                Size = new Size(icons.IconSize.Width + PADDING * 2, icons.IconSize.Height + PADDING * 2)
            };

            b.Marker = type;

            switch (type)
            {
                case Settings.MarkerIconType.Icon:

                    b.ImageSource = new Tools.Shared.Images.SourceValue(source);
                    b.MouseHover += marker_MouseHover;

                    break;
            }

            b.Click += marker_Click;

            return b;
        }

        void marker_MouseHover(object sender, EventArgs e)
        {
            if (tooltip == null)
                tooltip=new Tooltip.FloatingTooltip();

            var b = (Controls.FlatMarkerIconButton)sender;
            var i = b.ImageSource;

            if (i != null && i.Source.Key is string)
            {
                tooltip.ShowTooltip((Control)sender, System.IO.Path.GetFileName((string)i.Source.Key));
            }
        }

        void marker_BackgroundImageChanged(object sender, EventArgs e)
        {
            var c = (Control)sender;

            c.BackgroundImageChanged -= marker_BackgroundImageChanged;
            c.Visible = true;
        }

        void marker_Click(object sender, EventArgs e)
        {
            SelectedIconButton = (Controls.FlatMarkerIconButton)sender;
        }

        private void AddTrigger(LabelListPanel panel)
        {
            using (var f = new formMarkerTrigger())
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK && f.Result != null)
                {
                    panel.Add(GetText(f.Result), f.Result, true);
                }
            }
        }

        private void labelAddShowTrigger_Click(object sender, EventArgs e)
        {
            AddTrigger(panelShowTriggerContainer);
            panelScroll.ScrollControlIntoView((Control)sender);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = (ControlListPanel.ListItem)contextTrigger.Tag;
            var t = (Settings.MarkerTriggerCondition)l.Value;

            using (var f = new formMarkerTrigger((Settings.MarkerTriggerCondition)l.Value))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK && !t.Equals(f.Result))
                {
                    l.Value = f.Result;
                    l.Text = GetText(f.Result);
                }
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = (ControlListPanel.ListItem)contextTrigger.Tag;
            l.Remove();
        }

        private void panelTriggerContainer_LabelClick(object sender, ControlListPanel.ListItemEventArgs e)
        {
            contextTrigger.Tag = e.Item;
            contextTrigger.Show(Cursor.Position);
        }

        private void panelTriggerContainer_CountChanged(object sender, EventArgs e)
        {
            var p = (LabelListPanel)sender;
            var c = p.Controls.Count;

            if (c == 1 || c == 0)
            {
                p.Visible = c == 1;
            }
        }

        private void panelShowTriggerContainer_VisibleChanged(object sender, EventArgs e)
        {
            lineShowTriggerSeparator.Visible = ((Control)sender).Visible;
        }

        private void panelHideTriggerContainer_VisibleChanged(object sender, EventArgs e)
        {
            //lineHideTriggerSeparator.Visible = ((Control)sender).Visible;
        }

        private void labelAddHideTrigger_Click(object sender, EventArgs e)
        {
            AddTrigger(panelHideTriggerContainer);
            panelScroll.ScrollControlIntoView((Control)sender);
        }

        private Settings.MarkerTriggerCondition[] GetTriggers(LabelListPanel panel)
        {
            var items = panel.GetItems();
            var triggers = new Settings.MarkerTriggerCondition[items.Length];

            for (var i = 0; i < triggers.Length; i++)
            {
                triggers[i] = (Settings.MarkerTriggerCondition)items[i].Value;
            }

            return triggers;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            var b = SelectedIconButton;
            if (b == null)
                return;

            var t = b.Marker;

            switch (t)
            {
                case Settings.MarkerIconType.Icon:
                    if (!(b.ImageSource.Source.Key is string))
                        return;
                    break;
                case Settings.MarkerIconType.Circle:
                case Settings.MarkerIconType.Square:
                    break;
                default:
                    return;
            }

            var marker = Settings.CreateMarker();

            marker.IconColor = panelColor.BackColor;
            marker.IconType = t;

            if (t == Settings.MarkerIconType.Icon)
            {
                marker.IconPath = (string)b.ImageSource.Source.Key;
            }

            if (panelShowTriggerContainer.Modified)
                marker.TriggersShow = GetTriggers(panelShowTriggerContainer);
            if (panelHideTriggerContainer.Modified)
                marker.TriggersHide = GetTriggers(panelHideTriggerContainer);

            marker.Description = textDescription.Text;
            marker.HideOnClick = checkClickToHide.Checked;

            this.Result = marker;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void labelIconBrowse_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var b = CreateMarkerButton(Settings.MarkerIconType.Icon, panelColor.BackColor, icons.GetIcon(f.FileName));
                    panelIcons.Controls.Add(b);
                    SelectedIconButton = b;
                }
            }
        }

        private Controls.FlatMarkerIconButton _SelectedIconButton;
        private Controls.FlatMarkerIconButton SelectedIconButton
        {
            get
            {
                return _SelectedIconButton;
            }
            set
            {
                if (_SelectedIconButton != value)
                {
                    if (_SelectedIconButton != null)
                        _SelectedIconButton.Selected = false;
                    _SelectedIconButton = value;
                    if (value != null)
                    {
                        value.Selected = true;
                        panelColorContainer.Visible = value.Marker != Settings.MarkerIconType.Icon;
                    }
                    else
                    {
                        panelColorContainer.Visible = false;
                    }
                }
            }
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            using (var f = new ColorPicker.formColorDialog())
            {
                f.Color = panelColor.BackColor;
                f.AllowAlphaTransparency = false;

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panelColor.BackColor = f.Color;
                }
            }
        }

        private void panelColor_BackColorChanged(object sender, EventArgs e)
        {
            foreach (Controls.FlatMarkerIconButton b in panelIcons.Controls)
            {
                switch (b.Marker)
                {
                    case Settings.MarkerIconType.Circle:
                    case Settings.MarkerIconType.Square:

                        b.ForeColor = panelColor.BackColor;

                        break;
                }
            }
        }

        public Settings.IMarker Result
        {
            get;
            private set;
        }
    }
}
