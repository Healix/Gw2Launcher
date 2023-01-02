using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gw2Launcher.UI.Controls.ListPanel
{
    class MarkerListPanel : ControlListPanel
    {
        public class MarkerListItem : ListItem
        {
            public MarkerListItem(ControlListPanel parent, Control c, object value)
                : base(parent, c, value)
            {
            }

            public CheckBox CheckBox
            {
                get
                {
                    return ((ItemPanel)c).CheckBox;
                }
            }

            public FlatMarkerIconButton MarkerIcon
            {
                get
                {
                    return ((ItemPanel)c).MarkerIcon;
                }
            }

            public void Update(Settings.IMarker m, Tools.Markers.MarkerIcons icons)
            {
                this.Text = m.Description;
                this.Value = m;
                this.MarkerIcon.ForeColor = m.IconColor;
                this.MarkerIcon.Marker = m.IconType;
                if (icons != null && m.IconType == Settings.MarkerIconType.Icon && !string.IsNullOrEmpty(m.IconPath))
                    this.MarkerIcon.ImageSource = new Tools.Shared.Images.SourceValue(icons.GetIcon(m.IconPath));
                else
                    this.MarkerIcon.ImageSource = null;
            }
        }

        private class ItemPanel : StackPanel
        {
            public ItemPanel()
            {
            }

            public CheckBox CheckBox
            {
                get;
                set;
            }

            public FlatMarkerIconButton MarkerIcon
            {
                get;
                set;
            }

            public LinkLabel Label
            {
                get;
                set;
            }
        }

        public event EventHandler<ListItemMouseEventArgs> MarkerMouseClick;
        public event EventHandler<ListItemMouseEventArgs> LabelMouseClick;

        public MarkerListItem Add(Settings.IMarker value, Tools.Markers.MarkerIcons icons, bool modified)
        {
            var l = (MarkerListItem)base.Add(CreateLabel(), value, modified);

            l.Update(value, icons);

            return l;
        }

        protected override void OnScaleChanged(float scale)
        {
            base.OnScaleChanged(scale);

            CheckBoxSpacing = (int)(_CheckBoxSpacing * scale + 0.5f);
            LabelSpacing = (int)(_LabelSpacing * scale + 0.5f);
        }

        protected override ControlListPanel.ListItem CreateListItem(Control c, object value)
        {
            return new MarkerListItem(this, c, value);
        }

        protected override string GetText(Control c)
        {
            return ((ItemPanel)c).Label.Text;
        }

        protected override void SetText(Control c, string value)
        {
            ((ItemPanel)c).Label.Text = value;
        }

        protected Control CreateLabel()
        {
            var p = new ItemPanel()
            {
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Margin = _ContentMargin,
                AutoSizeFill = AutoSizeFillMode.Width,
                AutoSize = true,
            };

            p.CheckBox = new CheckBox()
            {
                Anchor = AnchorStyles.Left,
                AutoSize = true,
                Margin = new Padding(0, 0, _CheckBoxSpacing, 0),
                Visible = _CheckBoxVisible,
            };

            p.MarkerIcon = new FlatMarkerIconButton()
            {
                Anchor = AnchorStyles.Left,
                Margin = Padding.Empty,
            };

            p.Label = new Controls.LinkLabel()
            {
                AutoSize = true,
                Margin = new Padding(_LabelSpacing, 0, 0, 0),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Anchor = AnchorStyles.Left,
            };

            p.Controls.AddRange(new Control[]
                {
                    p.CheckBox,
                    p.MarkerIcon,
                    p.Label,
                });

            p.MarkerIcon.MouseClick += marker_MouseClick;
            p.Label.MouseClick += label_MouseClick;
            
            return p;
        }

        void label_MouseClick(object sender, MouseEventArgs e)
        {
            if (LabelMouseClick != null)
            {
                var i = GetItem((Control)sender);
                if (i != null)
                    LabelMouseClick(this, new ListItemMouseEventArgs(i, e));
            }
        }

        void marker_MouseClick(object sender, MouseEventArgs e)
        {
            if (MarkerMouseClick != null)
            {
                var i = GetItem((Control)sender);
                if (i != null)
                    MarkerMouseClick(this, new ListItemMouseEventArgs(i, e));
            }
        }

        protected virtual void OnCheckBoxSpacingChanged()
        {
            foreach (Control c in this.Controls)
            {
                var p = (ItemPanel)c;

                p.CheckBox.Margin = new Padding(0, 0, _CheckBoxSpacing, 0);
            }
        }

        protected virtual void OnCheckBoxValueChanged()
        {
            foreach (Control c in this.Controls)
            {
                var p = (ItemPanel)c;

                p.CheckBox.Visible = _CheckBoxVisible;
            }
        }

        protected virtual void OnLabelSpacingChanged()
        {
            foreach (Control c in this.Controls)
            {
                var p = (ItemPanel)c;

                p.Label.Margin = new Padding(_LabelSpacing, 0, 0, 0);
            }
        }

        protected bool _CheckBoxVisible = false;
        [System.ComponentModel.DefaultValue(false)]
        public bool CheckBoxVisible
        {
            get
            {
                return _CheckBoxVisible;
            }
            set
            {
                if (_CheckBoxVisible != value)
                {
                    _CheckBoxVisible = value;
                    OnCheckBoxValueChanged();
                }
            }
        }

        protected int _CheckBoxSpacing = 3;
        [System.ComponentModel.DefaultValue(3)]
        public int CheckBoxSpacing
        {
            get
            {
                return _CheckBoxSpacing;
            }
            set
            {
                if (_CheckBoxSpacing != value)
                {
                    _CheckBoxSpacing = value;
                    OnCheckBoxSpacingChanged();
                }
            }
        }

        protected int _LabelSpacing = 3;
        [System.ComponentModel.DefaultValue(3)]
        public int LabelSpacing
        {
            get
            {
                return _LabelSpacing;
            }
            set
            {
                if (_LabelSpacing != value)
                {
                    _LabelSpacing = value;
                    OnLabelSpacingChanged();
                }
            }
        }
    }
}
