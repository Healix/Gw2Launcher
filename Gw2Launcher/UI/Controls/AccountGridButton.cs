using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ColorNames = Gw2Launcher.UI.UiColors.Colors;

namespace Gw2Launcher.UI.Controls
{
    public class AccountGridButton : Control, UiColors.IColors
    {
        protected const float ASTRAL_LIMIT = 1300f;

        protected const string
            TEXT_ACCOUNT = "Account",
            TEXT_LAST_USED = "Last used",
            TEXT_STATUS_NEVER = "never";

        protected const int SMALL_ICON_SIZE = 24;

        protected static Image[] DefaultImage;

        public static readonly Font FONT_NAME = new Font("Segoe UI", 10.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FONT_STATUS = new Font("Segoe UI Semilight", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FONT_USER = new Font("Calibri", 7.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
        public static readonly Font FONT_LOGIN_REWARD_DAY = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);

        protected static readonly Point
            POINT_NAME = new Point(10, 10),
            POINT_ACCOUNT = new Point(10, 28),
            POINT_ACCOUNT_VALUE = new Point(62, 28),
            POINT_LAST_USED = new Point(10, 44),
            POINT_LAST_USED_VALUE = new Point(62, 44);

        protected const byte INDEX_ICON_MAIN = 0;
        protected const byte INDEX_ICON_NOTE = 1;

        protected const int BORDER_SIZE = 2;
        protected const int BORDER_HORIZONTAL = BORDER_SIZE * 2;
        protected const int BORDER_VERTICAL = BORDER_SIZE * 2;

        public event EventHandler<bool> EndPressed;
        public event EventHandler Pressed;
        public event EventHandler<float> PressedProgress;
        public event EventHandler<IconClickedEventArgs> IconClicked;

        public enum StatusColors
        {
            Default,
            Ok,
            Waiting,
            Error
        }

        public enum PressedState : byte
        {
            Pressing = 0,
            Pressed = 1,
        }

        public class IconClickedEventArgs : EventArgs
        {
            public IconClickedEventArgs(IconData.IconType type, MouseEventArgs e)
            {
                this.Type = type;
                this.Event = e;
            }

            public IconData.IconType Type
            {
                get;
                private set;
            }

            public MouseEventArgs Event
            {
                get;
                private set;
            }
        }

        public class DisplayedIconData
        {
            [Flags]
            public enum IconState : byte
            {
                None = 0,
                Enabled = 1,
                /// <summary>
                /// Icon is eligible for display
                /// </summary>
                Displayed = 2,
                /// <summary>
                /// Icon is hovered
                /// </summary>
                Hovered = 4,
                /// <summary>
                /// Icon is being clicked
                /// </summary>
                Activated = 8,
            }

            private IconState state;
            private Rectangle bounds;
            private IconData icon;
            private DisplayedIcons group;

            public DisplayedIconData(IconData icon, DisplayedIcons group)
            {
                this.icon = icon;
                this.group = group;
            }

            protected void OnEnabledChanged()
            {
                group.Refresh(DisplayedIcons.PendingRefresh.Data);
            }

            protected void OnExpiresChanged()
            {
                if (Enabled)
                {
                    group.Refresh(DisplayedIcons.PendingRefresh.Display);
                }
            }

            /// <summary>
            /// Icon is enabled
            /// </summary>
            public bool Enabled
            {
                get
                {
                    return (state & IconState.Enabled) == IconState.Enabled;
                }
                set
                {
                    if (Enabled != value)
                    {
                        SetState(IconState.Enabled, value);
                        OnEnabledChanged();
                    }
                }
            }

            /// <summary>
            /// Icon is eligible for display
            /// </summary>
            public bool Displayed
            {
                get
                {
                    return (state & IconState.Displayed) == IconState.Displayed;
                }
                set
                {
                    SetState(IconState.Displayed, value);
                }
            }

            /// <summary>
            /// Icon is visible
            /// </summary>
            public bool Visible
            {
                get
                {
                    return DisplayIndex != 0;
                }
            }

            /// <summary>
            /// Icon is hovered
            /// </summary>
            public bool Hovered
            {
                get
                {
                    return (state & IconState.Hovered) == IconState.Hovered;
                }
                set
                {
                    SetState(IconState.Hovered, value);
                }
            }

            /// <summary>
            /// Icon is being clicked
            /// </summary>
            public bool Activated
            {
                get
                {
                    return (state & IconState.Activated) == IconState.Activated;
                }
                set
                {
                    SetState(IconState.Activated, value);
                }
            }

            public IconData Icon
            {
                get
                {
                    return icon;
                }
            }

            public IconData.IconType Type
            {
                get
                {
                    return icon.Type;
                }
            }

            /// <summary>
            /// Display order starting from 1; 0 is not displaed
            /// </summary>
            public byte DisplayIndex
            {
                get;
                set;
            }

            public Rectangle[] Bounds
            {
                get;
                set;
            }

            public Rectangle DisplayBounds
            {
                get
                {
                    if (Bounds != null)
                    {
                        return Bounds[0];
                    }
                    else
                    {
                        return Rectangle.Empty;
                    }
                }
            }

            public DisplayedIcons Group
            {
                get
                {
                    return group;
                }
            }

            public void SetState(IconState state, bool value)
            {
                if (value)
                    this.state |= state;
                else
                    this.state &= ~state;
            }

            public void ClearState()
            {
                SetState(IconState.Activated | IconState.Hovered, false);
            }

            private DateTime _Expires;
            /// <summary>
            /// When the icon changes/resets
            /// </summary>
            public DateTime Expires
            {
                get
                {
                    return _Expires;
                }
                set
                {
                    if (_Expires != value)
                    {
                        _Expires = value;

                        if (value < group.nextRefresh && value != DateTime.MinValue)
                        {
                            group.nextRefresh = value;
                        }

                        OnExpiresChanged();
                    }
                }
            }

            private DateTime _Date;
            public DateTime Date
            {
                get
                {
                    return _Date;
                }
                set
                {
                    _Date = value;
                }
            }
        }

        public class DisplayedIcons
        {
            [Flags]
            public enum PendingRefresh : byte
            {
                None = 0,
                /// <summary>
                /// Icon display/order/positioning needs to be updated
                /// </summary>
                Display = 1,
                /// <summary>
                /// Icon data/order needs to be updated
                /// </summary>
                Data = 2,
                /// <summary>
                /// Dispay bounds has changed
                /// </summary>
                Bounds = 4,
                /// <summary>
                /// Order of display/data has changed
                /// </summary>
                Order = Display | Data,
                All = Display | Data | Bounds,
            }

            public event EventHandler RefreshPending;

            private Icons.IconGroup type;
            public byte[] order;
            public byte first;
            public DisplayedIconData[] icons;
            public DateTime nextRefresh;
            public PendingRefresh pending;
            public Rectangle boundary; //next position to add icon (vertical centered)

            public DisplayedIcons(Icons.IconGroup type)
            {
                this.type = type;
                this.order = Icons.DisplayOrder.GetDefault(type);
                this.pending = PendingRefresh.All;
            }

            public Icons.IconGroup Type
            {
                get
                {
                    return type;
                }
            }

            public bool Pending
            {
                get
                {
                    return pending != PendingRefresh.None || DateTime.UtcNow >= nextRefresh;
                }
            }

            public DateTime NextRefresh
            {
                get
                {
                    return nextRefresh;
                }
            }

            public void Refresh(PendingRefresh type)
            {
                if ((pending & type) != type)
                {
                    pending |= type;

                    if (RefreshPending != null)
                        RefreshPending(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Creates a sorted array of icons for display
            /// </summary>
            public DisplayedIconData[] Create(Icons icons)
            {
                int length = order.Length;

                while (order[length - 1] == 0)
                {
                    if (--length == 0)
                        break;
                }

                var _icons = new DisplayedIconData[order.Length];
                var index = 0;

                for (var i = 0; i < _icons.Length; i++)
                {
                    if (order[i] == 0)
                        break;

                    var icon = icons.GetIcon(order[i] - 1);

                    if (icon.Enabled)
                    {
                        _icons[index++] = icon;
                    }
                }

                if (index != _icons.Length)
                {
                    Array.Resize<DisplayedIconData>(ref _icons, index);
                }

                return _icons;
            }

            public DisplayedIconData FromPoint(Point p)
            {
                if (boundary.Contains(p))
                {
                    var icons = this.icons;

                    if (icons != null)
                    {
                        for (var i = icons.Length - 1; i >= 0; --i)
                        {
                            if (icons[i].Visible && icons[i].Bounds[0].Contains(p))
                                return icons[i];
                        }
                    }
                }

                return null;
            }
        }

        public class IconData
        {
            public enum IconType : byte
            {
                Login = 0,
                LoginReward = 1,
                Daily = 2,
                Weekly = 3,
                Astral = 4,
                Note = 5,
                Run = 6,

                LENGTH = 7,
                Custom = 255,
            }

            [Flags]
            public enum IconOptions : byte
            {
                None = 0,

                /// <summary>
                /// Icon supports clicking and hovering
                /// </summary>
                Clickable = 1,

                /// <summary>
                /// Icon can be displayed as a large icon
                /// </summary>
                Large = 2,
                /// <summary>
                /// Icon is offset and overlapped when displayed as a large icon
                /// </summary>
                LargeOverlap = 4 | 2,
                /// <summary>
                /// Height of the icon will fill the available area
                /// </summary>
                Fill = 8,

                /// <summary>
                /// Icon is see through
                /// </summary>
                Transparent = 16,
            };

            private IconOptions options;

            public IconData(IconType type, IconOptions options)
            {
                this.Type = type;
                this.options = options;
            }

            public IconType Type
            {
                get;
                private set;
            }

            /// <summary>
            /// Icon supports clicking and hovering
            /// </summary>
            public bool Clickable
            {
                get
                {
                    return (options & IconOptions.Clickable) == IconOptions.Clickable;
                }
                set
                {
                    SetOption(IconOptions.Clickable, value);
                }
            }

            /// <summary>
            /// Icon can be displayed as a large icon
            /// </summary>
            public bool Large
            {
                get
                {
                    return (options & IconOptions.Large) == IconOptions.Large;
                }
                set
                {
                    SetOption(IconOptions.Large, value);
                }
            }

            /// <summary>
            /// Icon is offset and overlapped when displayed as a large icon
            /// </summary>
            public bool LargeOverlap
            {
                get
                {
                    return (options & IconOptions.LargeOverlap) == IconOptions.LargeOverlap;
                }
                set
                {
                    SetOption(IconOptions.LargeOverlap, value);
                }
            }

            /// <summary>
            /// Height of the icon will fill the available area
            /// </summary>
            public bool Fill
            {
                get
                {
                    return (options & IconOptions.Fill) == IconOptions.Fill;
                }
                set
                {
                    SetOption(IconOptions.Fill, value);
                }
            }

            /// <summary>
            /// Icon is see through
            /// </summary>
            public bool Transparent
            {
                get
                {
                    return (options & IconOptions.Transparent) == IconOptions.Transparent;
                }
                set
                {
                    SetOption(IconOptions.Transparent, value);
                }
            }

            public void SetOption(IconOptions state, bool value)
            {
                if (value)
                    this.options |= state;
                else
                    this.options &= ~state;
            }

            #region Static

            private static IconData[] icons;

            static IconData()
            {
                icons = new IconData[]
                {
                    new IconData(IconData.IconType.Login, IconOptions.LargeOverlap | IconOptions.Fill),
                    new IconData(IconData.IconType.LoginReward, IconData.IconOptions.Fill),
                    new IconData(IconData.IconType.Daily, IconOptions.LargeOverlap | IconData.IconOptions.Clickable | IconOptions.Fill),
                    new IconData(IconData.IconType.Weekly, IconOptions.LargeOverlap | IconData.IconOptions.Clickable | IconOptions.Fill),
                    new IconData(IconData.IconType.Astral, IconData.IconOptions.LargeOverlap | IconOptions.Fill),
                    new IconData(IconData.IconType.Note, IconData.IconOptions.Clickable | IconOptions.Transparent),
                    new IconData(IconData.IconType.Run, IconData.IconOptions.Clickable | IconOptions.Transparent),
                };
            }

            public static IconData GetIcon(int index)
            {
                return icons[index];
            }

            public static IconData GetIcon(IconData.IconType type)
            {
                if (type == IconData.IconType.Custom)
                {
                    return null;
                }

                return icons[(int)type];
            }

            #endregion
        }

        public class Icons
        {
            public class DisplayOrder : IEquatable<DisplayOrder>
            {
                private byte[] order;
                private byte[] keys;
                private byte index;
                
                private static readonly byte[][] DEFAULTS;

                static DisplayOrder()
                {
                    DEFAULTS = new byte[][]
                    {
                        //IconGroup.Main
                        new byte[]
                        {
                            (byte)IconData.IconType.Login + 1,
                            (byte)IconData.IconType.LoginReward + 1,
                            (byte)IconData.IconType.Daily + 1,
                            (byte)IconData.IconType.Weekly + 1,
                            (byte)IconData.IconType.Astral + 1,
                        },

                        //IconGroup.Top
                        new byte[]
                        {
                            (byte)IconData.IconType.Run + 1,
                            (byte)IconData.IconType.Note + 1,
                        },
                    };
                }

                public DisplayOrder(IconGroup group)
                {
                    Group = group;

                    order = new byte[GetDefault(group).Length];
                    keys = new byte[order.Length];
                }

                public DisplayOrder(IconGroup group, Settings.DisplayIcons[] icons)
                    : this(group)
                {
                    if (icons != null)
                    {
                        Add(icons);
                    }
                }

                public static byte[] GetDefault(IconGroup group)
                {
                    return DEFAULTS[(byte)group];
                }

                public IconGroup Group
                {
                    get;
                    private set;
                }

                public void Add(IconData.IconType t)
                {
                    var k = (byte)t;

                    if (keys[k] != 0)
                    {
                        //was already added
                        return;
                    }

                    var i = (byte)(t + 1);

                    order[index] = (byte)(t + 1);
                    keys[k] = ++index;
                }

                public void Add(Settings.DisplayIcons icon)
                {
                    switch (icon)
                    {
                        case Settings.DisplayIcons.Login:

                            Add(IconData.IconType.Login);
                            Add(IconData.IconType.LoginReward);
                            Add(IconData.IconType.Daily);

                            break;
                        case Settings.DisplayIcons.Weekly:

                            Add(IconData.IconType.Weekly);

                            break;
                        case Settings.DisplayIcons.Astral:

                            Add(IconData.IconType.Astral);

                            break;
                    }
                }

                public void Add(Settings.DisplayIcons[] icons)
                {
                    foreach (var i in icons)
                    {
                        Add(i);
                    }
                }

                public byte[] ToArray()
                {
                    if (index != order.Length)
                    {
                        if (index == 0)
                        {
                            return GetDefault(Group);
                        }
                        else
                        {
                            //fill in any missing items
                            var j = index;
                            var defaults = GetDefault(Group);

                            for (var i = 0; i < defaults.Length; i++)
                            {
                                var k = defaults[i] - 1;

                                if (keys[k] == 0)
                                {
                                    order[j++] = defaults[i];
                                }
                            }
                        }
                    }

                    return order;
                }

                public bool Equals(DisplayOrder d)
                {
                    if (d.Group == this.Group && d.order.Length == order.Length)
                    {
                        for (var i =0; i < order.Length; i++)
                        {
                            if (d.order[i] != order[i])
                                return false;
                        }
                        return true;
                    }
                    return false;
                }
            }

            public enum IconGroup : byte
            {
                Main = 0,
                Top = 1,
            }

            public event EventHandler RefreshPending;

            private DisplayedIconData[] icons;
            private DisplayedIcons[] groups;
            private DisplayedIconData active;

            public Icons()
            {
                groups = new DisplayedIcons[]
                {
                    new DisplayedIcons(IconGroup.Main)
                    {
                        //order = DisplayOrder.GetDefault(IconGroup.Main),
                    },
                    new DisplayedIcons(IconGroup.Top)
                    {
                        //order = DisplayOrder.GetDefault(IconGroup.Top),
                    },
                };

                for (var i = 0; i < groups.Length; i++)
                {
                    groups[i].RefreshPending += Icons_RefreshPending;
                }

                var types = new IconData.IconType[]
                {
                    IconData.IconType.Login,
                    IconData.IconType.LoginReward,
                    IconData.IconType.Daily,
                    IconData.IconType.Weekly,
                    IconData.IconType.Astral,
                    IconData.IconType.Note,
                    IconData.IconType.Run,
                };

                icons = new DisplayedIconData[types.Length];

                for (var i = 0; i < types.Length; i++)
                {
                    icons[i] = new DisplayedIconData(IconData.GetIcon(types[i]), GetGroup(types[i]));
                }

            }

            void Icons_RefreshPending(object sender, EventArgs e)
            {
                if (RefreshPending != null)
                    RefreshPending(sender, e);
            }

            public DisplayedIconData GetIcon(int index)
            {
                return icons[index];
            }

            public DisplayedIconData GetIcon(IconData.IconType type)
            {
                if (type == IconData.IconType.Custom)
                {
                    return null;
                }

                return icons[(int)type];
            }
            public DisplayedIcons GetGroup(IconGroup group)
            {
                return groups[(byte)group];
            }

            public DisplayedIcons GetGroup(IconData.IconType type)
            {
                switch (type)
                {
                    case IconData.IconType.Note:
                    case IconData.IconType.Run:

                        return GetGroup(IconGroup.Top);
                }

                return GetGroup(IconGroup.Main);
            }

            public void Refresh(DisplayedIcons.PendingRefresh refresh)
            {
                for (var i = 0; i < groups.Length; i++)
                {
                    groups[i].Refresh(refresh);
                }
            }

            public void Refresh(IconData.IconType type, DisplayedIcons.PendingRefresh refresh)
            {
                GetGroup(type).Refresh(refresh);
            }

            /// <summary>
            /// Returns the icon at the specified point, or null if nothing is there
            /// </summary>
            public DisplayedIconData FromPoint(Point p)
            {
                if (active != null && active.DisplayBounds.Contains(p))
                {
                    if (active.DisplayIndex != 1 || active.Group.Type != IconGroup.Main) //main icon is low priority
                        return active;
                }

                for (var i = groups.Length - 1; i >= 0; --i)
                {
                    var icon = groups[i].FromPoint(p);

                    if (icon != null)
                    {
                        return icon;
                    }

                    //if (groups[i].boundary.Contains(p))
                    //{
                    //    var displayed = groups[i].icons;

                    //    for (var j = 0; j < displayed.Length; j++)
                    //    {
                    //        if (displayed[j] == null)
                    //            break;

                    //        if (displayed[j].Bounds[0].Contains(p))
                    //        {
                    //            return displayed[j];
                    //        }
                    //    }
                    //}
                }

                return null;
            }

            public DisplayedIconData Active
            {
                get
                {
                    return active;
                }
                private set
                {
                    if (active != value)
                    {
                        if (active != null)
                            active.ClearState();
                        active = value;
                        if (value != null)
                            value.Hovered = true;
                    }
                }
            }

            /// <summary>
            /// Sets the icon that is active
            /// </summary>
            /// <param name="icon">The icon to set as active, or null to reset the active icon</param>
            /// <returns>Returns true if the display state has changed</returns>
            public bool SetActive(DisplayedIconData icon)
            {
                if (active != icon)
                {
                    if (active == null)
                    {
                        active = icon;
                        icon.Hovered = true;
                        return icon.Icon.Clickable;
                    }
                    else
                    {
                        var b = active.Icon.Clickable;

                        active.ClearState();
                        active = icon;
                        if (icon != null)
                        {
                            if (!b)
                                b = icon.Icon.Clickable;
                            icon.Hovered = true;
                        }

                        return b;
                    }
                }

                return false;
            }

            public void ClearActive()
            {
                if (active != null)
                {
                    active.ClearState();
                    active = null;
                }
            }

            public void SetOrder(IconGroup group, byte[] order)
            {
                var g = GetGroup(group);

                g.order = order;
                g.Refresh(DisplayedIcons.PendingRefresh.Order);
            }

            public void SetOrder(DisplayOrder order)
            {
                SetOrder(order.Group, order.ToArray());
            }
        }

        //protected class IconGroup
        //{
        //    public DisplayedIcon[] icons;
        //    private byte[] indexes;
        //    public bool refresh;
        //    public byte first;

        //    public int GetIndex(DisplayedIcon.IconType type)
        //    {
        //        return indexes[(int)type - 1] - 1;
        //    }

        //    public DisplayedIcon GetIcon(DisplayedIcon.IconType type)
        //    {
        //        var i = GetIndex(type);

        //        if (i == -1)
        //        {
        //            return null;
        //        }

        //        return icons[i];
        //    }

        //    public void SetIcons(DisplayedIcon[] icons)
        //    {
        //        this.icons = icons;

        //        if (indexes == null)
        //        {
        //            indexes = new byte[Enum.GetValues(typeof(DisplayedIcon.IconType)).Length];
        //        }

        //        for (var i = 0; i < icons.Length; i++)
        //        {
        //            indexes[i] = (byte)(icons[i].type + 1);
        //        }
        //    }
        //}

        //protected class DisplayedIcon
        //{
        //    [Flags]
        //    public enum IconSize: byte
        //    {
        //        None = 0,

        //        Large = 1,
        //        Small = 2,

        //        Any = 3,
        //    }

        //    public enum IconType : byte
        //    {
        //        None,
        //        Login,
        //        LoginReward,
        //        Daily,
        //        Note,
        //        Weekly,
        //        Astral,
        //    }

        //    [Flags]
        //    public enum IconOptions : byte
        //    {
        //        None = 0,

        //        /// <summary>
        //        /// Icon is hovered
        //        /// </summary>
        //        Hovered = 1,
        //        /// <summary>
        //        /// Icon is being clicked
        //        /// </summary>
        //        Activated = 2,
        //        /// <summary>
        //        /// States (Hovered | Activated)
        //        /// </summary>
        //        StateFlags = 3,

        //        /// <summary>
        //        /// Icon can be large
        //        /// </summary>
        //        Large = 4,
        //        /// <summary>
        //        /// Icon can be small
        //        /// </summary>
        //        Small = 8,
        //        /// <summary>
        //        /// Icon can be shown
        //        /// </summary>
        //        Enabled = 16,
        //        /// <summary>
        //        /// Icon is visible
        //        /// </summary>
        //        Visible = 32,
        //    };

        //    public DisplayedIcon(IconType type)
        //    {
        //        this.type = type;
        //        this.options = IconOptions.Enabled;
        //    }

        //    public Rectangle bounds;
        //    public IconType type;
        //    public IconOptions options;
        //    public byte spacing;

        //    public bool Hovered
        //    {
        //        get
        //        {
        //            return (options & IconOptions.Hovered) == IconOptions.Hovered;
        //        }
        //        set
        //        {
        //            SetOption(IconOptions.Hovered, value);
        //        }
        //    }

        //    public bool Activated
        //    {
        //        get
        //        {
        //            return (options & IconOptions.Activated) == IconOptions.Activated;
        //        }
        //        set
        //        {
        //            SetOption(IconOptions.Activated, value);
        //        }
        //    }

        //    public bool Enabled
        //    {
        //        get
        //        {
        //            return (options & IconOptions.Enabled) == IconOptions.Enabled;
        //        }
        //        set
        //        {
        //            SetOption(IconOptions.Enabled, value);
        //        }
        //    }

        //    public bool Visible
        //    {
        //        get
        //        {
        public class PagingData
        {
            public event EventHandler PageChanged;

            public PagingData(Settings.PageData[] pages)
            {
                _Pages = pages;
            }

            private Settings.PageData[] _Pages;
            public Settings.PageData[] Pages
            {
                get
                {
                    return _Pages;
                }
                set
                {
                    _Pages = value;
                    _Current = null;
                    _Page = 0;
                }
            }

            private Settings.PageData _Current;
            public Settings.PageData Current
            {
                get
                {
                    return _Current;
                }
                set
                {
                    _Current = value;

                    if (value == null)
                        Page = 0;
                    else
                        Page = value.Page;
                }
            }

            private byte _Page;
            public byte Page
            {
                get
                {
                    return _Page;
                }
                set
                {
                    if (_Page != value)
                    {
                        _Page = value;

                        if (PageChanged != null)
                            PageChanged(this, EventArgs.Empty);
                    }
                }
            }

            public bool SetCurrent(byte page)
            {
                if (_Page != page)
                {
                    _Current = Find(page);
                    Page = page;
                }

                return _Current != null;
            }

            public Settings.PageData Find(byte page)
            {
                if (_Pages != null)
                {
                    for (var i = 0; i < _Pages.Length; i++)
                    {
                        if (_Pages[i].Page == page)
                            return _Pages[i];
                    }
                }

                return null;
            }

            public int IndexOf(byte page)
            {
                if (_Pages != null)
                {
                    for (var i = 0; i < _Pages.Length; i++)
                    {
                        if (_Pages[i].Page == page)
                            return i;
                    }
                }
                return -1;
            }
        }

        protected class DisplayedTotp
        {
            public DisplayedTotp(string code, int remaining)
            {
                this.code = code;
                this.limit = Environment.TickCount + remaining;
            }

            public string code;
            public int limit;
            public Size size;
        }

        protected Rectangle rectName, rectUser, rectStatus, rectImage;

        protected bool isHovered, isSelected, isPressed, isFocused, isActive, isActiveHighlight;
        protected float pressedProgress;
        protected PressedState pressedState;
        private Color pressedColor;
        //protected DisplayedIcon[] icons;
        //protected DisplayedIcon activeIcon;
        protected DisplayedTotp totp;
        protected Controls.ApiTimer apiTimer;
        protected Icons iconstore;
        protected Point cursor;

        protected ushort fontNameHeight, fontStatusHeight, fontUserHeight;
        protected Font fontName, fontStatus, fontUser;
        //protected Point pointName, pointUser, pointStatus;
        protected bool resize, redraw;
        protected ushort margin;

        protected BufferedGraphics buffer;

        public AccountGridButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.Selectable, true);

            _Colors = UiColors.GetTheme();

            this.Cursor = Windows.Cursors.Hand;

            //icons = new DisplayedIcon[]
            //{
            //    new DisplayedIcon(DisplayedIcon.IconType.None),
            //    new DisplayedIcon(DisplayedIcon.IconType.None),
                
            //    new DisplayedIcon(DisplayedIcon.IconType.None),
            //    new DisplayedIcon(DisplayedIcon.IconType.None),
            //    new DisplayedIcon(DisplayedIcon.IconType.None),
            //};

            iconstore = new Icons();
            iconstore.GetIcon(IconData.IconType.Note).Enabled = true;
            iconstore.RefreshPending += iconstore_RefreshPending;

            fontName = FONT_NAME;
            fontStatus = FONT_STATUS;
            fontUser = FONT_USER;

            redraw = true;

            _AccountName = "Example";
            _DisplayName = "example@example.com";
            _LastUsed = DateTime.MinValue;
            _ShowAccount = true;

            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                totp = null;

                if (buffer != null)
                    buffer.Dispose();
                if (_Image != null)
                    _Image.Dispose();
            }
        }

        protected virtual void ResizeLabels(Graphics g)
        {
            var scale = g.DpiX / 96f;

            fontNameHeight = (ushort)(fontName.GetHeight(g) + 0.5f);
            fontStatusHeight = (ushort)(fontStatus.GetHeight(g) + 0.5f);
            fontUserHeight = (ushort)(fontUser.GetHeight(g) + 0.5f);
            margin = (ushort)(5 * scale);

            if (totp != null)
                totp.size = Size.Empty;

            Point pointName, pointUser, pointStatus;
            var paddingStatus = (int)(1 * scale);

            int ch = fontNameHeight + fontStatusHeight;
            if (_ShowAccount)
                ch += fontUserHeight;

            int h = ch + margin * 4 + paddingStatus;
            int w = this.Width;
            if (!_ShowAccount)
                h += (int)(8 * scale);
            if (_Offsets != null && _Offsets.Height > 0)
            {
                h = _Offsets.Height;
            }

            int x;

            if (!_ShowImage)
            {
                x = BORDER_SIZE + (int)(10 * scale + 0.5f);
            }
            else
            {
                const int DEFAULT_ICON_SIZE = 26;

                if (_Image == null)
                {
                    var sz = (int)(DEFAULT_ICON_SIZE * scale);

                    if (sz > h)
                        sz = h;

                    rectImage = new Rectangle(BORDER_SIZE + margin * 2, BORDER_SIZE + (h - BORDER_VERTICAL) / 2 - sz / 2, sz, sz);
                    x = rectImage.Right + margin * 2;
                }
                else
                {
                    var sz = _Image.Size;

                    if (_ShowBackgroundImage)
                    {
                        Size max;
                        if (_ImagePlacement == Settings.ImagePlacement.Overflow)
                            max = new Size(w, h);
                        else
                            max = new Size((int)(128 * scale), h);
                        if (sz.Width > max.Width || sz.Height > max.Height)
                            sz = Util.RectangleConstraint.Scale(sz, max);
                        rectImage = new Rectangle(0, h / 2 - sz.Height / 2, sz.Width, sz.Height);
                    }
                    else
                    {
                        Size max;
                        if (_ImagePlacement == Settings.ImagePlacement.Overflow)
                            max = new Size(w - BORDER_HORIZONTAL, h - BORDER_VERTICAL);
                        else
                            max = new Size((int)(128 * scale), h - BORDER_VERTICAL);
                        if (sz.Width > max.Width || sz.Height > max.Height)
                            sz = Util.RectangleConstraint.Scale(sz, max);
                        rectImage = new Rectangle(BORDER_SIZE, BORDER_SIZE + (h - BORDER_VERTICAL) / 2 - sz.Height / 2, sz.Width, sz.Height);
                    }

                    switch (_ImagePlacement)
                    {
                        case Settings.ImagePlacement.Overflow:

                            x = BORDER_SIZE + (int)(DEFAULT_ICON_SIZE * scale) + margin * 4;

                            break;
                        case Settings.ImagePlacement.Shift:
                        default:

                            x = rectImage.Right + margin;

                            break;
                    }
                }
            }

            int y = h / 2 - ch / 2;
            int rw = w - x - margin;

            if (_ShowAccount)
            {
                if (_Offsets != null)
                {
                    pointUser = new Point(x + _Offsets[Settings.AccountGridButtonOffsets.Offsets.User].X, y + _Offsets[Settings.AccountGridButtonOffsets.Offsets.User].Y);
                }
                else
                {
                    pointUser = new Point(x, y);
                }

                rectUser = new Rectangle(pointUser, new Size(rw, fontUserHeight));
                y += fontUserHeight;
            }

            if (_Offsets != null)
            {
                pointName = new Point(x + _Offsets[Settings.AccountGridButtonOffsets.Offsets.Name].X, y + _Offsets[Settings.AccountGridButtonOffsets.Offsets.Name].Y);
                pointStatus = new Point(x + _Offsets[Settings.AccountGridButtonOffsets.Offsets.Status].X, y + fontNameHeight + paddingStatus + _Offsets[Settings.AccountGridButtonOffsets.Offsets.Status].Y);
            }
            else
            {
                pointName = new Point(x, y);
                pointStatus = new Point(x, y + fontNameHeight + paddingStatus);
            }

            rectName = new Rectangle(pointName, new Size(rw, fontNameHeight));
            rectStatus = new Rectangle(pointStatus, new Size(rw, fontStatusHeight));

            if (this.MinimumSize.Height != h)
            {
                this.MinimumSize = new Size(this.MinimumSize.Width, h);
                this.Height = h;
            }
        }

        public void ResizeLabels()
        {
            using (var g = this.CreateGraphics())
            {
                ResizeLabels(g);
            }
        }

        protected static Image GetDefaultImage(Settings.AccountType type, float scale = 1)
        {
            var i = (int)type;

            if (DefaultImage == null)
                DefaultImage = new Image[2];

            if (DefaultImage[i] == null)
            {
                using (var icon = new Icon(type == Settings.AccountType.GuildWars1 ? Properties.Resources.Gw1 : Properties.Resources.Gw2, 48, 48))
                {
                    var image = DefaultImage[i] = new Bitmap((int)(26 * scale), (int)(26 * scale), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    using (var g = Graphics.FromImage(image))
                    {
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.DrawIcon(icon, new Rectangle(0, 0, image.Width, image.Height));
                    }

                    return image;
                }
            }

            return DefaultImage[i];
        }

        /// <summary>
        /// The font used for the status
        /// </summary>
        [DefaultValue(typeof(Font), "Segoe UI Semilight, 8.25pt")]
        public Font FontStatus
        {
            get
            {
                return fontStatus;
            }
            set
            {
                if (fontStatus != value)
                {
                    fontStatus = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        /// <summary>
        /// The font used for the user name
        /// </summary>
        [DefaultValue(typeof(Font), "Calibri, 7.25pt")]
        public Font FontUser
        {
            get
            {
                return fontUser;
            }
            set
            {
                if (fontUser != value)
                {
                    fontUser = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        /// <summary>
        /// The font used for the name
        /// </summary>
        [DefaultValue(typeof(Font), "Segoe UI, 10.25pt")]
        public Font FontName
        {
            get
            {
                return fontName;
            }
            set
            {
                if (fontName != value)
                {
                    fontName = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private UiColors.ColorValues _Colors;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UiColors.ColorValues Colors
        {
            get
            {
                return _Colors;
            }
            set
            {
                _Colors = value;
                OnRedrawRequired();
            }
        }

        private Settings.AccountGridButtonOffsets _Offsets;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Settings.AccountGridButtonOffsets Offsets
        {
            get
            {
                return _Offsets;
            }
            set
            {
                _Offsets = value;
                resize = true;
                OnRedrawRequired();
            }
        }

        public async void BeginPressed(PressedState state, CancellationToken cancel, AccountGridButtonContainer.MousePressedEventArgs e)
        {
            isPressed = true;
            pressedProgress = 0;

            var start = Environment.TickCount;
            const int LIMIT = 10;
            int duration;

            switch (state)
            {
                case PressedState.Pressed:
                    pressedState = PressedState.Pressed;
                    duration = 500;
                    pressedColor = e.FlashColor;
                    break;
                case PressedState.Pressing:
                default:
                    pressedState = PressedState.Pressing;
                    duration = 500;
                    pressedColor = e.FillColor;
                    break;
            }

            try
            {
                while (true)
                {
                    int remaining = duration - (Environment.TickCount - start);
                    if (remaining > LIMIT)
                        await Task.Delay(LIMIT, cancel);
                    else if (remaining > 0)
                        await Task.Delay(remaining, cancel);

                    if (cancel.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                    else
                    {
                        pressedProgress = (float)(Environment.TickCount - start) / duration;

                        OnRedrawRequired();

                        if (pressedProgress >= 1)
                        {
                            pressedProgress = 1;

                            if (pressedState == PressedState.Pressing)
                            {
                                if (PressedProgress != null)
                                    PressedProgress(this, pressedProgress);

                                await Task.Delay(50, cancel);

                                if (cancel.IsCancellationRequested)
                                    throw new TaskCanceledException();

                                pressedState = PressedState.Pressed;
                                pressedProgress = 0;
                                duration = 500;
                                start = Environment.TickCount;
                                pressedColor = e.FlashColor;

                                OnRedrawRequired();

                                if (Pressed != null)
                                    Pressed(this, EventArgs.Empty);
                            }
                            else
                            {
                                isPressed = false;

                                break;
                            }
                        }
                        else if (pressedState == PressedState.Pressed)
                        {
                            pressedColor = Color.FromArgb((int)(e.FlashColor.A * (1 - pressedProgress)), e.FlashColor);
                        }
                        else if (PressedProgress != null)
                            PressedProgress(this, pressedProgress);
                    }
                }
            }
            catch
            {
                isPressed = false;
                OnRedrawRequired();
            }
            finally
            {
                if (EndPressed != null)
                    EndPressed(this, cancel.IsCancellationRequested);
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }

            OnSizeChanged();
        }

        protected virtual void OnSizeChanged()
        {
            int rw = this.Width - rectName.Left - margin;

            rectName = new Rectangle(rectName.X, rectName.Y, rw, rectName.Height);
            rectStatus = new Rectangle(rectStatus.X, rectStatus.Y, rw, rectStatus.Height);
            rectUser = new Rectangle(rectUser.X, rectUser.Y, rw, rectUser.Height);

            iconstore.Refresh(DisplayedIcons.PendingRefresh.Bounds);
            if (apiTimer != null)
                apiTimer.Resize = true;

            redraw = true;
            if (!resize)
                this.Invalidate();
        }

        public void Redraw()
        {
            OnRedrawRequired();
        }

        private Util.ScheduledEvents.Ticks OnDelayedRefresh()
        {
            if (this.IsDisposed)
                return Util.ScheduledEvents.Ticks.None;

            OnRedrawRequired();

            return GetNextRefresh();
        }

        private Util.ScheduledEvents.Ticks GetNextRefresh()
        {
            var ticks = DateTime.UtcNow.Ticks;
            var ms = (ticks - _LastUsed.Ticks) / 10000;

            if (ms < 0)
            {
            }
            else if (ms < 3600000) //<60m
            {
                return new Util.ScheduledEvents.Ticks(Util.ScheduledEvents.TickType.MillisecondTicks, ticks / 10000 + 60001 - ms % 60000);
            }
            else if (ms < 172800000) //<48h
            {
                return new Util.ScheduledEvents.Ticks(Util.ScheduledEvents.TickType.MillisecondTicks, ticks / 10000 + 3600001 - ms % 3600000);
            }
            else
            {
                //anything older isn't important / will be updated on daily reset
            }

            //old: displayed time is rounded, so refreshes need to occur at 30s (1m), 90s (2m), etc, until it gets to 24 hours
            //if (ms < 0)
            //{
            //}
            //else if (ms < 3600000) //<60m
            //{
            //    return 60000 - (ms + 30000) % 60000;
            //}
            //else if (ms < 82800000) //<23h
            //{
            //    return 3600000 - (ms + 1800000) % 3600000;
            //}
            //else if (ms < 86400000) //<24h
            //{
            //    return 86400000 - ms;
            //}

            return Util.ScheduledEvents.Ticks.None;
        }

        private void DelayedRefresh()
        {
            var refresh = GetNextRefresh();
            if (refresh.Value != -1)
                Util.ScheduledEvents.Register(OnDelayedRefresh, refresh);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            isHovered = true;
            OnRedrawRequired();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            cursor = Point.Empty;
            iconstore.ClearActive();
            isHovered = false;
            OnRedrawRequired();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (cursor != e.Location)
            {
                cursor = e.Location;

                var icon = iconstore.FromPoint(e.Location);

                if (iconstore.Active != icon)
                {
                    if (iconstore.SetActive(icon))
                    {
                        OnRedrawRequired();
                    }
                }
            }

        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            this.Focus();

            #region Wine

            //Wine will break enter/leave events when moving the cursor outside of the control while holding down the button
            //Use of DoDragDrop prevents this, otherwise this is needed, but enabling this will break DoDragDrop

            //if (Settings.IsRunningWine)
            //{
            //    EventHandler onLeave = null;
            //    onLeave = delegate
            //    {
            //        this.MouseLeave -= onLeave;
            //        this.MouseCaptureChanged -= onLeave;

            //        Windows.Native.NativeMethods.ReleaseCapture();
            //    };
            //    this.MouseLeave += onLeave;
            //    this.MouseCaptureChanged += onLeave;
            //}

            #endregion

            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (totp != null)
                {
                    //totp code is being clicked
                    return;
                }

                var icon = iconstore.Active;

                if (icon != null && icon.Icon.Clickable)
                {
                    //icon is being clicked
                    icon.Activated = true;
                    return;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (totp != null)
                {
                    return;
                }

                var icon = iconstore.Active;

                if (icon != null && icon.Activated)
                {
                    icon.Activated = false;
                    return;
                }
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (totp != null)
                {
                    totp = null;
                    OnRedrawRequired();
                    return;
                }

                var icon = iconstore.Active;

                if (icon != null && icon.Activated)
                {
                    if (icon.Hovered)
                    {
                        switch (icon.Type)
                        {
                            case IconData.IconType.Daily:

                                if (_AccountData != null)
                                {
                                    this.LastDailyCompletionUtc = DateTime.UtcNow;
                                    if (_AccountData.Type == Settings.AccountType.GuildWars2)
                                        ((Settings.IGw2Account)_AccountData).LastDailyCompletionUtc = this.LastDailyCompletionUtc;
                                    OnRedrawRequired();
                                }

                                break;
                            case IconData.IconType.Weekly:

                                if (_AccountData != null)
                                {
                                    this.LastWeeklyCompletionUtc = DateTime.UtcNow;
                                    if (_AccountData.Type == Settings.AccountType.GuildWars2)
                                        ((Settings.IGw2Account)_AccountData).LastWeeklyCompletionUtc = this.LastWeeklyCompletionUtc;
                                    OnRedrawRequired();
                                }

                                break;
                            case IconData.IconType.Note:
                            case IconData.IconType.Run:

                                if (IconClicked != null)
                                    IconClicked(this, new IconClickedEventArgs(icon.Type, e));

                                break;
                        }
                    }
                    return;
                }
            }

            base.OnMouseClick(e);
        }

        private string FormatLastUsed()
        {
            var elapsed = DateTime.UtcNow.Subtract(_LastUsed);
            var e = elapsed.TotalMinutes;

            if (e >= 2880) //48h
            {
                int days = (int)(e / 1440);

                if (days > 1)
                    return days + " days ago";
                else
                    return "a day ago";
            }
            if (e >= 60)
            {
                int hours = (int)(e / 60);

                if (hours > 1)
                    return hours + " hours ago";
                else
                    return "an hour ago";
            }
            else
            {
                int minutes = (int)(e);

                if (minutes > 1)
                    return minutes + " minutes ago";
                else if (minutes == 1)
                    return "a minute ago";
                else
                    return "just now";

            }

        }

        protected Color GetLoginRewardRarity(byte day)
        {
            switch (day)
            {
                case 8:
                case 9:
                case 10:
                case 12:
                case 13:
                    return _Colors[ColorNames.AccountLoginRewardRarityMasterwork];
                case 15:
                case 16:
                case 17:
                case 19:
                case 20:
                    return _Colors[ColorNames.AccountLoginRewardRarityRare];
                case 4:
                case 7:
                case 11:
                case 14:
                case 18:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                    return _Colors[ColorNames.AccountLoginRewardRarityExotic];
                case 28:
                    return _Colors[ColorNames.AccountLoginRewardRarityAscended];
                case 1:
                case 2:
                case 3:
                case 5:
                case 6:
                default:
                    return _Colors[ColorNames.AccountLoginRewardRarityFine];
            }
        }

        protected Image GetLoginRewardIcon(byte day, Settings.DailyLoginDayIconFlags flags)
        {
            switch (day)
            {
                case 1:
                case 8:
                case 15:
                case 22:
                    if ((flags & Settings.DailyLoginDayIconFlags.MysticCoins) != 0)
                        return Properties.Resources.loginreward0;
                    break;
                case 2:
                case 7:
                case 9:
                case 16:
                case 21:
                case 23:
                    if ((flags & Settings.DailyLoginDayIconFlags.Laurels) != 0)
                        return Properties.Resources.loginreward1;
                    break;
                case 3:
                case 10:
                case 17:
                case 24:
                    if ((flags & Settings.DailyLoginDayIconFlags.Luck) != 0)
                        return Properties.Resources.loginreward2;
                    break;
                case 4:
                case 11:
                case 18:
                case 25:
                    if ((flags & Settings.DailyLoginDayIconFlags.BlackLionGoods) != 0)
                        return Properties.Resources.loginreward3;
                    break;
                case 5:
                case 12:
                    if ((flags & Settings.DailyLoginDayIconFlags.CraftingMaterials) != 0)
                        return Properties.Resources.loginreward4;
                    break;
                case 6:
                case 13:
                case 20:
                case 27:
                    if ((flags & Settings.DailyLoginDayIconFlags.Experience) != 0)
                        return Properties.Resources.loginreward5;
                    break;
                case 14:
                    if ((flags & Settings.DailyLoginDayIconFlags.ExoticEquipment) != 0)
                        return Properties.Resources.loginreward6;
                    break;
                case 19:
                    if ((flags & Settings.DailyLoginDayIconFlags.CelebrationBooster) != 0)
                        return Properties.Resources.loginreward7;
                    break;
                case 26:
                    if ((flags & Settings.DailyLoginDayIconFlags.TransmutationCharge) != 0)
                        return Properties.Resources.loginreward8;
                    break;
                case 28:
                    if ((flags & Settings.DailyLoginDayIconFlags.ChestOfLoyalty) != 0)
                        return Properties.Resources.loginreward9;
                    break;
            }

            return null;
        }

        protected int DrawLoginReward(Graphics g, Rectangle bounds, byte day, Settings.DailyLoginDayIconFlags flags)
        {
            var image = GetLoginRewardIcon(day, flags);
            if (image == null && (flags & Settings.DailyLoginDayIconFlags.ShowDay) == 0)
                return 0;

            if (image != null)
            {
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(bounds);
                    g.SetClip(path);
                    g.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                    g.ResetClip();
                }
            }
            else
            {
                using (var brush = new SolidBrush(_Colors[ColorNames.AccountLoginRewardBackColor]))
                {
                    g.FillEllipse(brush, bounds);
                }

                DrawOutlinedText(g, day.ToString(), FONT_LOGIN_REWARD_DAY, _Colors[ColorNames.AccountLoginRewardText], _Colors[ColorNames.AccountLoginRewardTextOutline], (int)(3 * g.DpiX / 96f + 0.5f), StringAlignment.Center, StringAlignment.Center, bounds);
            }

            var crarity = (flags & Settings.DailyLoginDayIconFlags.ShowRarity) == 0 ? Color.Empty : GetLoginRewardRarity(day);
            var coutline = _Colors[ColorNames.AccountLoginRewardBorder];

            if (coutline.A > 0 || crarity.A > 0)
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (var pen = new Pen(_Colors[ColorNames.AccountLoginRewardBorder]))
                {
                    if (crarity.A > 0)
                    {
                        var scale = g.DpiX / 96f;

                        if (coutline.A > 0)
                        {
                            pen.Width = (int)(3 * scale + 0.5f);
                            g.DrawEllipse(pen, bounds);
                        }

                        pen.Color = crarity;
                        pen.Width = (int)(2 * scale + 0.5f);
                        g.DrawEllipse(pen, bounds);
                    }
                    else if (coutline.A > 0)
                    {
                        pen.Width = (int)(1 * g.DpiX / 96f + 0.5f);
                        g.DrawEllipse(pen, bounds);
                    }

                }
                g.SmoothingMode = SmoothingMode.None;
            }

            return 0;
        }

        //protected int DrawLoginReward(Graphics g, DisplayedIcon m, byte day, Settings.DailyLoginDayIconFlags flags)
        //{
        //    //var m = icons[INDEX_ICON_MAIN];
        //    //if (m.type != DisplayedIcon.IconType.Login || day == 0)
        //    //    return 0;
        //    var image = GetLoginRewardIcon(day, flags);
        //    if (image == null && (flags & Settings.DailyLoginDayIconFlags.ShowDay) == 0)
        //        return 0;

        //    var scale = g.DpiX / 96f;
        //    int sz = (int)(24 * scale + 0.5f);

        //    var x = m.bounds.X + (int)(m.bounds.Width * 0.1f) - sz / 2;
        //    var y = m.bounds.Y + (int)(m.bounds.Height * 0.77f) - sz / 2;
        //    if (y + sz > this.Height)
        //        y = this.Height - sz;

        //    if (image != null)
        //    {
        //        using (var path = new GraphicsPath())
        //        {
        //            path.AddEllipse(x, y, sz, sz);
        //            g.SetClip(path);
        //            g.DrawImage(image, new Rectangle(x, y, sz, sz), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
        //            g.ResetClip();
        //        }
        //    }
        //    else
        //    {
        //        using (var brush = new SolidBrush(_Colors[ColorNames.AccountLoginRewardBackColor]))
        //        {
        //            g.FillEllipse(brush, x, y, sz, sz);
        //        }

        //        DrawOutlinedText(g, day.ToString(), FONT_LOGIN_REWARD_DAY, _Colors[ColorNames.AccountLoginRewardText], _Colors[ColorNames.AccountLoginRewardTextOutline], (int)(3 * scale + 0.5f), StringAlignment.Center, StringAlignment.Center, new Rectangle(x, y, sz, sz));
        //    }

        //    var crarity = (flags & Settings.DailyLoginDayIconFlags.ShowRarity) == 0 ? Color.Empty : GetLoginRewardRarity(day);
        //    var coutline = _Colors[ColorNames.AccountLoginRewardBorder];

        //    if (coutline.A > 0 || crarity.A > 0)
        //    {
        //        g.SmoothingMode = SmoothingMode.AntiAlias;
        //        using (var pen = new Pen(_Colors[ColorNames.AccountLoginRewardBorder]))
        //        {
        //            if (crarity.A > 0)
        //            {
        //                if (coutline.A > 0)
        //                {
        //                    pen.Width = (int)(3 * scale + 0.5f);
        //                    g.DrawEllipse(pen, x, y, sz, sz);
        //                }

        //                pen.Color = crarity;
        //                pen.Width = (int)(2 * scale + 0.5f);
        //                g.DrawEllipse(pen, x, y, sz, sz);
        //            }
        //            else if (coutline.A > 0)
        //            {
        //                pen.Width = (int)(1 * scale + 0.5f);
        //                g.DrawEllipse(pen, x, y, sz, sz);
        //            }

        //        }
        //        g.SmoothingMode = SmoothingMode.None;
        //    }

        //    return x;
        //}

        protected void AddTextToGraphicsPath(Graphics g, GraphicsPath path, string text, Font font, StringAlignment alignmentHorizontal, StringAlignment alignmentVertical, Rectangle bounds)
        {
            using (var format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox))
            {
                format.Alignment = alignmentHorizontal;
                format.LineAlignment = alignmentVertical;

                path.AddString(text, font.FontFamily, (int)font.Style, g.DpiX * font.SizeInPoints / 72f, bounds, format);
            }
        }

        protected void DrawOutlinedText(Graphics g, GraphicsPath path, Color color, Color outline, int outlineSize)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingQuality = CompositingQuality.HighQuality;

            using (var pen = new Pen(outline, outlineSize))
            {
                pen.LineJoin = LineJoin.Round;
                g.DrawPath(pen, path);
            }
            using (var brush = new SolidBrush(color))
            {
                g.FillPath(brush, path);
            }

            g.SmoothingMode = SmoothingMode.None;
            g.CompositingQuality = CompositingQuality.Default;
        }

        protected void DrawOutlinedText(Graphics g, string text, Font font, Color color, Color outline, int outlineSize, StringAlignment alignmentHorizontal, StringAlignment alignmentVertical, Rectangle bounds)
        {
            using (var path = new GraphicsPath())
            {
                AddTextToGraphicsPath(g, path, text, font, alignmentHorizontal, alignmentVertical, bounds);

                DrawOutlinedText(g, path, color, outline, outlineSize);

                //using (var format = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox))
                //{
                //    g.SmoothingMode = SmoothingMode.AntiAlias;
                //    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                //    g.CompositingQuality = CompositingQuality.HighQuality;

                //    format.Alignment = alignmentHorizontal;
                //    format.LineAlignment = alignmentVertical;

                //    path.AddString(text, font.FontFamily, (int)font.Style, g.DpiX * font.SizeInPoints / 72f, bounds, format);

                //    using (var pen = new Pen(outline, outlineSize))
                //    {
                //        pen.LineJoin = LineJoin.Round;
                //        g.DrawPath(pen, path);
                //    }
                //    using (var brush = new SolidBrush(color))
                //    {
                //        g.FillPath(brush, path);
                //    }

                //    g.SmoothingMode = SmoothingMode.None;
                //    g.CompositingQuality = CompositingQuality.Default;
                //}
            }
        }

        protected Image GetImage(IconData icon, bool large)
        {
            switch (icon.Type)
            {
                case IconData.IconType.Login:

                    return Properties.Resources.login;

                case IconData.IconType.Daily:

                    return Properties.Resources.daily;

                case IconData.IconType.Weekly:

                    return Properties.Resources.weekly;

                case IconData.IconType.Astral:

                    if (large)
                        return Properties.Resources.astrallarge;
                    else
                        return Properties.Resources.astral;

                case IconData.IconType.Note:

                    return Properties.Resources.mailfull;

                case IconData.IconType.Run:

                    return Properties.Resources.run;
            }

            return null;
        }

        protected struct IconOffset
        {
            public IconOffset(sbyte x, sbyte y, byte width)
                : this()
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
            }

            public IconOffset(sbyte x, sbyte y, float width)
                : this(x, y, (byte)(255 * width + 0.5f))
            { }

            /// <summary>
            /// Icon X offset
            /// </summary>
            public sbyte X;

            /// <summary>
            /// Icon Y offset
            /// </summary>
            public sbyte Y;

            /// <summary>
            /// Percentage of the width of the icon to be displayed
            /// </summary>
            public byte Width;
        }

        protected struct IconPadding
        {
            public IconPadding(sbyte height, sbyte top, sbyte left, sbyte right)
                : this()
            {
                this.Height = height;
                this.Top = top;
                this.Left = left;
                this.Right = right;
            }

            /// <summary>
            /// Adjusts icon height
            /// </summary>
            public sbyte Height;

            /// <summary>
            /// Offsets icon Y position
            /// </summary>
            public sbyte Top;

            /// <summary>
            /// Left margin
            /// </summary>
            public sbyte Left;

            /// <summary>
            /// Right margin
            /// </summary>
            public sbyte Right;
        }

        protected void DrawIcons(Graphics g, DisplayedIcons group, Rectangle clip)
        {
            #region Refresh

            var scale = g.DpiX / 96f;
            var icons = group.icons;

            //iconstore.GetIcon(IconData.IconType.Daily).Expires = DateTime.MinValue;

            if (group.Pending)
            {
                var refresh = group.pending;

                group.pending = DisplayedIcons.PendingRefresh.None;

                byte displayIndex = 1;
                var nextRefresh = DateTime.MaxValue;
                var rh = (int)(this.Height * 0.8f);
                int anchorX, anchorY;
                var previous = 0;
                int boundaryL = int.MaxValue,
                    boundaryT = int.MaxValue,
                    boundaryR = int.MinValue,
                    boundaryB = int.MinValue;
                var changed = false;


                switch (group.Type)
                {
                    case Icons.IconGroup.Top:

                        anchorX = this.Width - (int)(8 * scale + 0.5f);
                        anchorY = (int)(2 * scale + 0.5f);

                        break;
                    case Icons.IconGroup.Main:
                    default:

                        anchorX = this.Width - (int)(SMALL_ICON_SIZE * scale + 0.5f) / 2;
                        anchorY = this.Height - (int)((SMALL_ICON_SIZE + 6) * scale + 0.5f);

                        break;
                }

                var resize = (refresh & DisplayedIcons.PendingRefresh.Bounds) != 0;

                if (icons == null || (refresh & DisplayedIcons.PendingRefresh.Data) != 0)
                {
                    if (icons != null)
                    {
                        for (var i = 0; i < icons.Length; i++)
                        {
                            if (icons[i].Visible && !icons[i].Enabled)
                            {
                                icons[i].DisplayIndex = 0;
                                icons[i].Displayed = false;
                            }
                        }
                    }

                    group.icons = icons = group.Create(iconstore);
                    //resize = false; //??? displayindex won't change if same icons are used, but control size could have changed
                }

                //group.order = new byte[]
                //        {
                //            (byte)IconData.IconType.Astral + 1,
                //            (byte)IconData.IconType.Login + 1,
                //            (byte)IconData.IconType.LoginReward + 1,
                //            (byte)IconData.IconType.Daily + 1,
                //            (byte)IconData.IconType.Weekly + 1,
                //        };

                group.first = 0;

                for (var i = 0; i < icons.Length; i++)
                {
                    //if (group.indexes[i] == 0)
                    //    break;

                    var icon = icons[i];// iconstore.GetIcon(group.indexes[i] - 1);
                    bool v, displayed;

                    //if (d == null)
                    //{
                    //    icon.Display = d = new DisplayedIconData(group.indexes[i]);
                    //}

                    switch (icon.Type)
                    {
                        case IconData.IconType.Login:

                            //displayed = v = this._LastDailyLogin < DateTime.UtcNow.Date;
                            displayed = v = DateTime.UtcNow >= icon.Expires;

                            if (!v && icon.Expires < nextRefresh)
                                nextRefresh = icon.Expires;

                            //reward icon replaces login icon if the login icon is not being displayed as the main icon
                            if (v && displayIndex != 1)
                            {
                                var reward = iconstore.GetIcon(IconData.IconType.LoginReward);

                                if (reward.Enabled && this._DailyLoginDay > 0)
                                {
                                    if (GetLoginRewardIcon(this._DailyLoginDay, this._ShowDailyLoginDay) != null || (this._ShowDailyLoginDay & Settings.DailyLoginDayIconFlags.ShowDay) != 0)
                                        v = false;
                                }
                            }

                            break;
                        case IconData.IconType.LoginReward:

                            displayed = v = this._DailyLoginDay > 0 && iconstore.GetIcon(IconData.IconType.Login).Displayed;

                            if (v)
                            {
                                if (GetLoginRewardIcon(this._DailyLoginDay, this._ShowDailyLoginDay) == null && (this._ShowDailyLoginDay & Settings.DailyLoginDayIconFlags.ShowDay) == 0)
                                    v = false;
                            }

                            //if (v)
                            //{
                            //    var login = iconstore.GetIcon(IconData.IconType.Login).Display;

                            //    if (login.DisplayIndex != 1)
                            //    {
                            //        //reward icon replaces login icon if the login icon is not being displayed as the main icon
                            //        if (d == null)
                            //        {
                            //            icon.Display = d = new DisplayedIconData(group.indexes[i]);
                            //        }
                            //        d.Displayed = true;
                            //        d.DisplayIndex = login.DisplayIndex;
                            //        login.DisplayIndex = 0;

                            //        continue;
                            //    }
                            //}

                            break;
                        case IconData.IconType.Daily:

                            //displayed = v = this._LastDailyCompletion < DateTime.UtcNow.Date;
                            displayed = v = DateTime.UtcNow >= icon.Expires;

                            if (!v && icon.Expires < nextRefresh)
                                nextRefresh = icon.Expires;

                            if (v && iconstore.GetIcon(IconData.IconType.Login).Displayed)
                            {
                                //daily icon is overriden by login icon
                                v = false;
                            }

                            break;
                        case IconData.IconType.Weekly:

                            //displayed = v = this._NextWeeklyCompletion <= DateTime.UtcNow;
                            displayed = v = DateTime.UtcNow >= icon.Expires;

                            if (!v && icon.Expires < nextRefresh)
                                nextRefresh = icon.Expires;

                            break;
                        case IconData.IconType.Note:

                            //the note icon is hidden when it expires (whereas other icons are shown when expired)
                            displayed = v = DateTime.UtcNow < icon.Expires;

                            if (v && icon.Expires < nextRefresh)
                                nextRefresh = icon.Expires;

                            break;
                        default:

                            displayed = v = true;

                            break;
                    }

                    if (icon.Displayed != displayed)
                    {
                        icon.Displayed = displayed;
                    }

                    //if (displayed)
                    //{
                    //    if (d == null)
                    //    {
                    //        icon.Display = d = new DisplayedIconData(group.indexes[i]);
                    //    }

                    //    d.Displayed = displayed;
                    //}
                    //else if (d != null)
                    //{
                    //    //clear active
                    //    icon.Display = null;
                    //}

                    if (v)
                    {
                        if (icon.DisplayIndex != displayIndex || resize)
                        {
                            changed = true;
                            icon.DisplayIndex = displayIndex;
                            //bounds may change even if displayindex doesnt - could be daily icon showing first, then daily reset causes login icon to show

                            var image = GetImage(icon.Icon, displayIndex == 1);
                            int iw, ih, w, h;

                            if (icon.Bounds == null)
                            {
                                icon.Bounds = new Rectangle[2];
                            }

                            if (image == null)
                            {
                                switch (icon.Type)
                                {
                                    case IconData.IconType.LoginReward:

                                        iw = ih = SMALL_ICON_SIZE;

                                        break;
                                    default:

                                        iw = ih = 0;

                                        break;
                                }
                            }
                            else
                            {
                                iw = image.Width;
                                ih = image.Height;
                            }

                            if (displayIndex == 1 && icon.Icon.Large)
                            {
                                group.first = group.order[i];

                                w = (int)(iw * scale + 0.5f);
                                h = (int)(ih * scale + 0.5f);

                                if (icon.Icon.LargeOverlap)
                                {
                                    var o = GetLargeOffset(icon.Type);

                                    if (h > rh)
                                    {
                                        int rw = rh * w / h;

                                        icon.Bounds[0] = new Rectangle(this.Width - rw * o.Width / 255 - BORDER_SIZE + o.X, (this.Height - rh) / 2 + o.Y, rw, rh);

                                        //rw /= 2;
                                        //icon.Bounds[0] = new Rectangle(this.Width - rw + offsetX - BORDER_SIZE, (this.Height - rh) / 2 + offsetY, rw - offsetX, rh);
                                    }
                                    else
                                    {
                                        //w /= 2;
                                        icon.Bounds[0] = new Rectangle(this.Width - w * o.Width / 255 - BORDER_SIZE + o.X, (this.Height - h) / 2 + o.Y, w, h);
                                    }

                                    icon.Bounds[1] = new Rectangle(0, 0, iw, ih);
                                    //icon.Bounds[1] = new Rectangle(0, 0, (int)(iw * overlap), ih);
                                    anchorX = this.Width - (int)(SMALL_ICON_SIZE * scale / 2f * (1 + (o.Width - 128) / 255f) + 0.5f);// *o.Width / 255 * 3 / 4; 
                                }
                                else
                                {
                                    if (h > rh)
                                    {
                                        int rw = rh * w / h;
                                        icon.Bounds[0] = new Rectangle(this.Width - rw - (int)(5 * scale + 0.5f), (this.Height - rh) / 2, rw, rh);
                                    }
                                    else
                                    {
                                        icon.Bounds[0] = new Rectangle(this.Width - w - (int)(5 * scale + 0.5f), (this.Height - h) / 2, w, h);
                                    }

                                    icon.Bounds[1] = new Rectangle(0, 0, iw, ih); 
                                    anchorX = icon.Bounds[0].X;
                                }
                            }
                            else
                            {
                                var padding = GetPadding(icon.Type);

                                if (icon.Icon.Fill || ih > SMALL_ICON_SIZE + padding.Height)
                                {
                                    h = (int)((SMALL_ICON_SIZE + padding.Height) * scale + 0.5f);
                                    if (iw == ih)
                                        w = h;
                                    else
                                        w = h * iw / ih;
                                }
                                else
                                {
                                    h = (int)(ih * scale + 0.5f);
                                    w = (int)(iw * scale + 0.5f);
                                }

                                var sH = (int)(SMALL_ICON_SIZE * scale + 0.5f);
                                int y;

                                if (h == sH)
                                    y = anchorY;
                                else
                                    y = anchorY + (sH - h) / 2;

                                anchorX -= w + padding.Right;

                                icon.Bounds[0] = new Rectangle(anchorX, y + padding.Top, w, h);
                                icon.Bounds[1] = new Rectangle(0, 0, iw, ih);

                                anchorX -= padding.Left;
                            }
                            
                            //if (icon.Hovered && iconstore.Active == icon)
                            //{
                        }
                        else if (displayIndex == 1 && icon.Icon.Large)
                        {
                            if (icon.Icon.LargeOverlap)
                            {
                                anchorX = this.Width - (int)(SMALL_ICON_SIZE * scale / 2f * (1 + (GetLargeOffset(icon.Type).Width - 128) / 255f) + 0.5f);
                            }
                            else
                            {
                                anchorX = icon.Bounds[0].X;
                            }
                        }
                        else
                        {
                            var padding = GetPadding(icon.Type);

                            anchorX -= icon.Bounds[0].Width + padding.Right;

                            if (icon.Bounds[0].X != anchorX)
                            {
                                icon.Bounds[0].X = anchorX;
                                changed = true;
                            }

                            anchorX -= padding.Left;
                        }

                        if (icon.Icon.Clickable)
                        {
                            var r = icon.Bounds[0];

                            if (r.X < boundaryL)
                                boundaryL = r.X;
                            if (r.Y < boundaryT)
                                boundaryT = r.Y;
                            if (r.Right > boundaryR)
                                boundaryR = r.Right;
                            if (r.Bottom > boundaryB)
                                boundaryB = r.Bottom;
                        }

                        //if (displayIndex == 1 && !icon.HasLargeIcon)
                        //{
                        //    anchorX = d.Bounds[0].X;
                        //}

                        //if (displayIndex == 1)
                        //{
                        //    //cache main bounds

                        //    //anchorX = d.Bounds[0].X + (int)(d.Bounds[0].Width * 0.1f) - szw / 2;
                        //    //anchorY = d.Bounds[0].Y + (int)(d.Bounds[0].Height * 0.77f) - szh / 2;

                        //    //int szh = (int)((24 + paddingH) * scale + 0.5f);
                        //    //int szw = szh * w / h;
                        //}

                        previous = i;
                        ++displayIndex;
                    }
                    else if (icon.DisplayIndex != 0)
                    {
                        icon.DisplayIndex = 0;

                        if (iconstore.Active == icon)
                        {
                            iconstore.SetActive(null);
                        }
                        icon.ClearState();
                    }
                }

                group.nextRefresh = nextRefresh;
                group.boundary = Rectangle.FromLTRB(boundaryL, boundaryT, boundaryR, boundaryB);

                if (changed && !cursor.IsEmpty)
                {
                    var active = iconstore.FromPoint(cursor);

                    if (iconstore.Active != active)
                    {
                        iconstore.SetActive(active);
                    }
                }
            }

            #endregion

            //for (var i = 0; i < group.order.Length;i++)
            //{
            //    if (group.order[i] == 0)
            //        break;

            //    var icon = iconstore.GetIcon(group.order[i] - 1);
            for (var i = 0; i < icons.Length;i++)
            {
                var icon = icons[i];

                if (icon.Visible)
                {
                    var image = GetImage(icon.Icon, icon.DisplayIndex == 1);

                    if (image == null)
                    {
                        switch (icon.Type)
                        {
                            case IconData.IconType.LoginReward:

                                DrawLoginReward(g, icon.Bounds[0], _DailyLoginDay, _ShowDailyLoginDay);

                                break;
                        }
                    }
                    else
                    {
                        var hovered = icon.Hovered && icon.Icon.Clickable;
                        var main = icon.DisplayIndex == 1 && icon.Group.Type == Icons.IconGroup.Main;

                        if (main)
                        {
                            g.SetClip(clip);
                        }

                        switch (icon.Type)
                        {
                            case IconData.IconType.Astral:
                                {
                                    var percent = _Astral / ASTRAL_LIMIT;

                                    var b0 = icon.Bounds[0];
                                    var b1 = icon.Bounds[1];

                                    int h1, h2;

                                    var cw = clip.Right - b0.X;

                                    if (main)
                                    {
                                        //large icon has blank space above/below where it'll visually fill in, so it needs to be offset
                                        //var percentAdjusted = percent * (38 / 50f) + 7 / 50f;

                                        h1 = (int)(38f / 50 * b0.Height + 0.5f);
                                        h2 = (int)(percent * h1 + 0.5f);

                                        if (h2 >= h1)
                                        {
                                            h2 = b0.Height;
                                            h1 = 0;
                                        }
                                        else if (h2 > 0)
                                        {
                                            h2 += (int)(7 / 50f * b0.Height + 0.5f);
                                            h1 = b0.Height - h2;
                                        }
                                        else
                                        {
                                            h1 = b0.Height;
                                        }
                                    }
                                    else
                                    {
                                        h2 = (int)(b0.Height * percent + 0.5f);

                                        if (h2 > b0.Height)
                                        {
                                            h2 = b0.Height;
                                            h1 = 0;
                                        }
                                        else
                                        {
                                            h1 = b0.Height - h2;
                                        }
                                    }
                                    
                                    if (cw > b0.Width)
                                        cw = b0.Width;

                                    if (h1 > 0)
                                    {
                                        //g.SetClip(Rectangle.Intersect(clip, new Rectangle(b0.X, b0.Y, b0.Width, h1)));
                                        g.SetClip(new Rectangle(b0.X, b0.Y, cw, h1));

                                        DrawImage(g, image, b0, b1, main ? (byte)102 : (byte)255, !main);
                                    }

                                    if (h2 > 0)
                                    {
                                        //g.SetClip(Rectangle.Intersect(clip, new Rectangle(b0.X, b0.Y + h1, b0.Width, h2)));
                                        g.SetClip(new Rectangle(b0.X, b0.Y + h1, cw, h2));

                                        if (main)
                                        {
                                            using (var ia = new ImageAttributes())
                                            {
                                                ia.SetColorMatrix(new ColorMatrix(new float[][] 
                                                {
                                                    new float[] {0.1f, 0.9f, 1.14f, 0, 0},
                                                    new float[] {0, 0, 0, 0, 0},
                                                    new float[] {0, 0, 0, 0, 0},
                                                    new float[] {0, 0, 0, 0.6f, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                }));

                                                g.DrawImage(image, b0, b1.X, b1.Y, b1.Width, b1.Height, GraphicsUnit.Pixel, ia);
                                            }
                                        }
                                        else
                                        {
                                            DrawImage(g, image, b0, b1, 255, false);
                                        }
                                    }

                                    g.SetClip(clip);

                                    //if (h1 > 0)
                                    //    DrawImage(g, image, new Rectangle(b0.X, b0.Y, b0.Width, h1), new Rectangle(0, 0, b1.Width, ih1), 255, true);
                                    //if (h2 > 0)
                                    //{
                                    //    DrawImage(g, image, new Rectangle(b0.X, b0.Y + h1, b0.Width, h2), new Rectangle(0, ih1, b1.Width, ih2), 255, false);
                                    //    if (false && h1 > 0)
                                    //    {
                                    //        var bmp = (Bitmap)image;
                                    //        var bxx = 0;

                                    //        for (var bx = 0; bx < b1.Width; bx++)
                                    //        {
                                    //            var px = bmp.GetPixel(bx, ih1);
                                    //            if (px.A != 0)
                                    //            {
                                    //                bxx = (int)(((float)bx / b1.Width) * b0.Width);
                                    //                break;
                                    //            }
                                    //        }

                                    //        using (var pen = new Pen(Color.FromArgb(127, 0, 0, 0), 1)) //
                                    //        {
                                    //            g.DrawLine(pen, b0.X + bxx, b0.Y + h1, b0.X+b0.Width - bxx, b0.Y + h1);
                                    //        }
                                    //    }
                                    //}

                                    if (main)
                                    {
                                        using (var path = new GraphicsPath())
                                        {
                                            AddTextToGraphicsPath(g, path, _Astral.ToString(), FONT_USER, StringAlignment.Center, StringAlignment.Center, icon.Bounds[0]);

                                            var outlineSize = (int)(4 * scale + 0.5f);
                                            var overflowR = clip.Right - path.GetBounds().Right - outlineSize / 2f;

                                            if (overflowR < -0.5)
                                            {
                                                g.TranslateTransform(overflowR, 0);
                                                DrawOutlinedText(g, path, Color.White, Color.Black, outlineSize);
                                                g.ResetTransform();
                                            }
                                            else
                                            {
                                                DrawOutlinedText(g, path, Color.White, Color.Black, outlineSize);
                                            }
                                        }
                                        //DrawOutlinedText(g, _Astral.ToString(), FONT_USER, Color.White, Color.Black, outlineSize, StringAlignment.Center, StringAlignment.Center, icon.Bounds[0]);
                                    }

                                    g.ResetClip();
                                }
                                break;
                            default:

                                if (main)
                                {
                                    g.SetClip(clip);

                                    DrawImage(g, image, icon.Bounds[0], icon.Bounds[1], main ? (byte)102 : (byte)255, hovered);

                                    g.ResetClip();
                                }
                                else
                                {
                                    DrawImage(g, image, icon.Bounds[0], icon.Bounds[1], !hovered && icon.Icon.Transparent ? (byte)204 : (byte)255, hovered);
                                }

                                break;
                        }
                    }

                    //if (icon.Type == IconData.IconType.LoginReward)
                    //{
                    //    g.FillRectangle(Brushes.White, d.Bounds[0]);
                    //}
                }
            }
        }

        protected IconOffset GetLargeOffset(IconData.IconType type)
        {
            switch (type)
            {
                case IconData.IconType.Astral:

                    return new IconOffset(0, 0, 204);

                default:

                    return new IconOffset(0, 0, 128);
            }
        }

        protected IconPadding GetPadding(IconData.IconType type)
        {
            switch (type)
            {
                case IconData.IconType.Astral:

                    return new IconPadding(0, 0, 0, 0); //0, 0, -1, -1

                case IconData.IconType.Daily:
                case IconData.IconType.Weekly:

                    return new IconPadding(0, 0, 0, 0); //2, 0, 1, 1

                case IconData.IconType.LoginReward:

                    return new IconPadding(0, -1, 2, 2);

                case IconData.IconType.Login:

                    return new IconPadding(-2, 0, 1, 1);

                case IconData.IconType.Note:
                case IconData.IconType.Run:

                    return new IconPadding(0, 0, 1, 0);
            }

            return new IconPadding();
        }

        /// <summary>
        /// Draws the image
        /// </summary>
        /// <param name="image">Image to draw</param>
        /// <param name="dest">Destination andbounds</param>
        /// <param name="src">Source bounds</param>
        /// <param name="opacity">Opacity</param>
        /// <param name="grayscale">Grayscale</param>
        protected void DrawImage(Graphics g, Image image, Rectangle dest, Rectangle src, byte opacity = 255, bool grayscale = false)
        {
            if (grayscale)
            {
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix(new float[][] 
                            {
                                new float[] {.3f, .3f, .3f, 0, 0},
                                new float[] {.6f, .6f, .6f, 0, 0},
                                new float[] {.1f, .1f, .1f, 0, 0},
                                new float[] {0, 0, 0, opacity / 255f, 0},
                                new float[] {0, 0, 0, 0, 1}
                            }));

                    g.DrawImage(image, dest, src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
            }
            else if (opacity != 255)
            {
                using (var ia = new ImageAttributes())
                {
                    ia.SetColorMatrix(new ColorMatrix()
                    {
                        Matrix33 = opacity / 255f,
                    });

                    g.DrawImage(image, dest, src.X, src.Y, src.Width, src.Height, GraphicsUnit.Pixel, ia);
                }
            }
            else
            {
                g.DrawImage(image, dest, src, GraphicsUnit.Pixel);
            }
        }

        //protected void DrawIcon(Graphics g)
        //{
        //    byte displayIndex = 1;
        //    var ofs = 0;
        //    var previous = 0;

        //    if (iconsMain != null)
        //    {
        //        if (iconsMain.refresh)
        //        {
        //            var count = 0;

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (resize)
            {
                ResizeLabels(e.Graphics);
                resize = false;
            }

            if (redraw)
            {
                if (buffer == null)
                    buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.DisplayRectangle);

                var g = buffer.Graphics;
                Color borderLight, borderDark, background, foreground;

                int width = this.Width;
                int height = this.Height;

                if (isSelected)
                {
                    background = _Colors[ColorNames.AccountBackColorSelected];
                    borderLight = _Colors[ColorNames.AccountBorderLightSelected];
                    borderDark = _Colors[ColorNames.AccountBorderDarkSelected];
                    foreground = _Colors[ColorNames.AccountForeColorSelected];
                }
                else if (isHovered)
                {
                    background = _Colors[ColorNames.AccountBackColorHovered];
                    borderLight = _Colors[ColorNames.AccountBorderLightHovered];
                    borderDark = _Colors[ColorNames.AccountBorderDarkHovered];
                    foreground = _Colors[ColorNames.AccountForeColorHovered];
                }
                else if (isActive)
                {
                    background = _Colors[ColorNames.AccountBackColorActive];
                    borderLight = _Colors[ColorNames.AccountBorderLightActive];
                    borderDark = _Colors[ColorNames.AccountBorderDarkActive];
                    foreground = _Colors[ColorNames.AccountForeColorActive];
                }
                else
                {
                    background = _Colors[ColorNames.AccountBackColorDefault];
                    borderLight = _Colors[ColorNames.AccountBorderLightDefault];
                    borderDark = _Colors[ColorNames.AccountBorderDarkDefault];
                    foreground = Color.Empty;
                }

                using (var brush = new SolidBrush(borderLight))
                {
                    if (background.A < 255)
                    {
                        g.Clear(_Colors[ColorNames.AccountBackColorDefault]);

                        if (background.A != 0)
                        {
                            brush.Color = background;
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                    }
                    else
                    {
                        g.Clear(background);
                    }

                    if (_ShowBackgroundImage)
                    {
                        try
                        {
                            var image = this.BackgroundImage;
                            if (image == null)
                                image = _DefaultBackgroundImage;
                            g.DrawImage(image, 0, 0, width, height);
                        }
                        catch { }

                        if (foreground.A != 0)
                        {
                            brush.Color = foreground;
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                    }

                    if (isPressed)
                    {
                        brush.Color = pressedColor;
                        if (pressedState == PressedState.Pressed)
                        {
                            g.FillRectangle(brush, 0, 0, width, height);
                        }
                        else
                        {
                            g.FillRectangle(brush, 0, 0, (int)(width * pressedProgress), height);
                        }
                    }

                    if (_ShowImage)
                    {
                        if (_Image == null)
                        {
                            var image = GetDefaultImage(_AccountType, g.DpiX / 96f);

                            using (var ia = new ImageAttributes())
                            {
                                ia.SetColorMatrix(new ColorMatrix()
                                {
                                    Matrix33 = 0.85f,
                                });

                                g.DrawImage(image, rectImage, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, ia);
                            }
                        }
                        else
                        {
                            try
                            {
                                g.DrawImage(_Image, rectImage);
                            }
                            catch { }
                        }
                    }

                    bool hasBorderR = false;

                    if (borderLight.A != 0)
                    {
                        brush.Color = borderLight;
                        g.FillRectangle(brush, 0, 0, width - BORDER_SIZE, BORDER_SIZE); //top
                        g.FillRectangle(brush, 0, BORDER_SIZE, BORDER_SIZE, height-BORDER_VERTICAL); //left
                    }

                    if (hasBorderR = borderDark.A != 0)
                    {
                        brush.Color = borderDark;
                        g.FillRectangle(brush, 0, height - BORDER_SIZE, width, BORDER_SIZE); //bottom
                        g.FillRectangle(brush, width - BORDER_SIZE, 0, BORDER_SIZE, height - BORDER_SIZE); //right
                    }

                    if (isFocused || isActiveHighlight && isActive)
                    {
                        var hasBorder = _Colors[isFocused ? ColorNames.AccountFocusedBorder : ColorNames.AccountActiveBorder].A != 0;
                        var hasFill = _Colors[isFocused ? ColorNames.AccountFocusedHighlight : ColorNames.AccountActiveHighlight].A != 0;

                        if (!hasBorderR)
                            hasBorderR = hasBorder;

                        if (hasBorder || hasFill)
                        {
                            var x = (width-BORDER_HORIZONTAL) / 5;
                            using (var gradient = new LinearGradientBrush(new Rectangle(x - BORDER_SIZE, 0, width, height), Color.Transparent, _Colors[isFocused ? ColorNames.AccountFocusedHighlight : ColorNames.AccountActiveHighlight], LinearGradientMode.Horizontal))
                            {
                                if (hasFill)
                                {
                                    int fw = width - x - BORDER_SIZE, fh = height - BORDER_VERTICAL, fy = BORDER_SIZE;

                                    if (!hasBorder)
                                    {
                                        if (borderLight.A == 0)
                                        {
                                            fy = 0;
                                            fh += BORDER_SIZE;
                                        }
                                        if (borderDark.A == 0)
                                        {
                                            fh += BORDER_SIZE;
                                            fw += BORDER_SIZE;
                                        }
                                    }

                                    g.FillRectangle(gradient, x, fy, fw, fh);
                                }

                                if (hasBorder)
                                {
                                    gradient.LinearColors = new Color[] { Color.Transparent, _Colors[isFocused ? ColorNames.AccountFocusedBorder : ColorNames.AccountActiveBorder] };

                                    using (var p = new Pen(gradient, BORDER_SIZE))
                                    {
                                        var bh2 = BORDER_SIZE / 2;

                                        g.DrawLines(p, new Point[]
                                        {
                                            new Point(x, bh2),
                                            new Point(width - bh2, bh2),
                                            new Point(width - bh2,height - bh2),
                                            new Point(x,height - bh2),
                                        });
                                    }
                                }
                            }
                        }
                    }

                    if (_ShowColorKey && _ColorKey.A != 0)
                    {
                        brush.Color = _ColorKey;
                        g.FillRectangle(brush, 0, 0, (int)(g.DpiX / 96f * 5), height);
                    }

                    if (_Pinned)
                    {
                        var scale = g.DpiX / 96f;
                        var sz = scale * 5;
                        var ofs = scale * 5;

                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        brush.Color = _Colors[ColorNames.AccountPinnedBorder];
                        g.FillEllipse(brush, width - BORDER_SIZE - sz - ofs - 1, BORDER_SIZE + ofs - 1, sz + 2, sz + 2);

                        brush.Color = _Colors[ColorNames.AccountPinnedColor];
                        g.FillEllipse(brush, width - BORDER_SIZE - sz - ofs, BORDER_SIZE + ofs, sz, sz);

                        g.SmoothingMode = SmoothingMode.None;
                    }

                    if (apiTimer != null && apiTimer.Pending)
                    {
                        if (apiTimer.Resize)
                        {
                            apiTimer.Resize = false;

                            var scale = g.DpiX / 96f;
                            var sz = (int)(9 * scale + 0.5f);

                            apiTimer.Bounds = new Rectangle(
                                this.Width - sz - BORDER_SIZE - 1,
                                this.Height - sz - BORDER_SIZE - 1, //- (int)((SMALL_ICON_SIZE + 6) * scale + 0.5f) + ((int)(SMALL_ICON_SIZE * scale + 0.5f) - sz / 2) / 2 - 1, 
                                sz,
                                sz);
                        }

                        apiTimer.Draw(g);
                    }

                    Rectangle clip;

                    if (hasBorderR)
                        clip = new Rectangle(BORDER_SIZE, BORDER_SIZE, width - BORDER_HORIZONTAL, height - BORDER_VERTICAL);
                    else
                        clip = new Rectangle(0, 0, width, height);

                    g.InterpolationMode = InterpolationMode.High;

                    DrawIcons(g, iconstore.GetGroup(Icons.IconGroup.Main), clip);
                    DrawIcons(g, iconstore.GetGroup(Icons.IconGroup.Top), clip);

                    g.InterpolationMode = InterpolationMode.Default;
                }
            }
        }

        protected Color GetStatusColor(StatusColors color)
        {
            switch (_StatusColor)
            {
                case StatusColors.Error:

                    return _Colors[ColorNames.AccountStatusError];

                case StatusColors.Ok:

                    return _Colors[ColorNames.AccountStatusOK];

                case StatusColors.Waiting:

                    return _Colors[ColorNames.AccountStatusWaiting];

                case StatusColors.Default:
                default:

                    return _Colors[ColorNames.AccountStatusDefault];

            }
        }

        protected virtual void OnPrePaint(PaintEventArgs e)
        {
            if (redraw)
            {
                redraw = false;

                var g = buffer.Graphics;

                if (_ShowAccount)
                {
                    TextRenderer.DrawText(g, _AccountName, fontUser, rectUser, _Colors[ColorNames.AccountUser], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                TextRenderer.DrawText(g, _DisplayName, fontName, rectName, _Colors[ColorNames.AccountName], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);

                if (_Status != null)
                {
                    TextRenderer.DrawText(g, _Status, fontStatus, rectStatus, GetStatusColor(_StatusColor), TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
                else if (this._LastUsed == DateTime.MinValue)
                {
                    TextRenderer.DrawText(g, TEXT_STATUS_NEVER, fontStatus, rectStatus, _Colors[ColorNames.AccountStatusError], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }
                else
                {
                    TextRenderer.DrawText(g, FormatLastUsed(), fontStatus, rectStatus, _Colors[ColorNames.AccountStatusDefault], TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                }

                if (totp != null)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(220, 0, 0, 0)))
                    {
                        var w = this.Width;
                        var h = this.Height;
                        var y = h - margin;
                        var bw = totp.limit - Environment.TickCount;

                        g.FillRectangle(brush, 0, 0, w, h);

                        if (bw > 0)
                        {
                            if (bw < 3000)
                            {
                                int c;
                                if (bw <= 2500)
                                    c = 0;
                                else
                                    c = (int)(90 * (bw - 2500) / 500f);
                                brush.Color = Color.FromArgb(90, c, c);
                            }
                            else
                                brush.Color = Color.FromArgb(90, 90, 90);

                            bw = (int)((bw / 30000f) * w);
                            if (bw >= w)
                            {
                                g.FillRectangle(brush, 0, y, w, margin);
                            }
                            else
                            {
                                if (bw > 0)
                                    g.FillRectangle(brush, 0, y, bw, margin);
                                brush.Color = Color.FromArgb(60, 60, 60);
                                g.FillRectangle(brush, bw, y, w - bw, margin);
                            }
                        }

                        if (totp.size.IsEmpty)
                            totp.size = TextRenderer.MeasureText(g, totp.code, fontName, Size.Empty, TextFormatFlags.NoPadding);

                        var p = new Point((w - totp.size.Width) / 2, (h - totp.size.Height) / 2);

                        TextRenderer.DrawText(g, totp.code, fontName, new Point(p.X + 1, p.Y + 1), Color.Black, TextFormatFlags.NoPadding);
                        TextRenderer.DrawText(g, totp.code, fontName, p, Color.WhiteSmoke, TextFormatFlags.NoPadding);
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            OnPrePaint(e);

            buffer.Render(e.Graphics);

            base.OnPaint(e);
        }

        void iconstore_RefreshPending(object sender, EventArgs e)
        {
            OnRedrawRequired();
        }

        protected void OnRedrawRequired()
        {
            if (!redraw)
            {
                redraw = true;
                this.Invalidate();
            }
        }

        public void SetStatus(string status, StatusColors color)
        {
            _Status = status;
            _StatusColor = color;
            OnRedrawRequired();
        }

        public int Index
        {
            get;
            set;
        }

        public ushort SortKey
        {
            get
            {
                if (_Paging != null && _Paging.Current != null)
                {
                    return _Paging.Current.SortKey;
                }
                else if (_AccountData != null)
                {
                    return _AccountData.SortKey;
                }
                else
                {
                    return ushort.MaxValue;
                }
            }
            set
            {
                if (_Paging != null && _Paging.Current != null)
                {
                    _Paging.Current.SortKey = value;
                }
                else if (_AccountData != null)
                {
                    _AccountData.SortKey = value;
                }
            }
        }

        private bool _Pinned;
        public bool Pinned
        {
            get
            {
                if (_Paging != null && _Paging.Page > 0)
                {
                    if (_Paging.Current != null)
                        return _Paging.Current.Pinned;
                }
                else if (_AccountData != null)
                {
                    return _AccountData.Pinned;
                }

                return false;
            }
            set
            {
                if (_Paging != null && _Paging.Page > 0)
                {
                    if (_Paging.Current != null)
                        _Paging.Current.Pinned = value;
                }
                else if (_AccountData != null)
                {
                    _AccountData.Pinned = value;
                }

                if (_Pinned != value)
                {
                    _Pinned = value;
                    OnRedrawRequired();
                }
            }
        }

        public ushort GridIndex
        {
            get;
            set;
        }

        public bool GridVisibility
        {
            get;
            set;
        }

        private async void UpdateTotpCode()
        {
            do
            {
                await Task.Delay(50);

                if (totp == null)
                {
                    break;
                }

                var t = Environment.TickCount;
                if (t > totp.limit)
                {
                    totp = null;
                    OnRedrawRequired();
                    break;
                }

                OnRedrawRequired();
            }
            while (true);
        }

        public void ShowTotpCode(string code, int remainingMillis)
        {
            var b = totp == null;

            if (code != null)
            {
                totp = new DisplayedTotp(code, remainingMillis);
            }
            else
            {
                totp = null;
            }
            
            if (b && code != null)
            {
                UpdateTotpCode();
            }

            OnRedrawRequired();
        }

        private string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (_Status != value)
                {
                    _Status = value;
                    OnRedrawRequired();
                }
            }
        }

        private StatusColors _StatusColor;
        public StatusColors StatusColor
        {
            get
            {
                return _StatusColor;
            }
            set
            {
                if (_StatusColor != value)
                {
                    _StatusColor = value;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowAccount;
        public bool ShowAccount
        {
            get
            {
                return _ShowAccount;
            }
            set
            {
                if (_ShowAccount != value)
                {
                    _ShowAccount = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowColorKey;
        public bool ShowColorKey
        {
            get
            {
                return _ShowColorKey;
            }
            set
            {
                if (_ShowColorKey != value)
                {
                    _ShowColorKey = value;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowImage;
        public bool ShowImage
        {
            get
            {
                return _ShowImage;
            }
            set
            {
                if (_ShowImage != value)
                {
                    _ShowImage = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private string _ImagePath;
        private Image _Image;
        public Image Image
        {
            get
            {
                return _Image;
            }
            set
            {
                if (_Image != value)
                {
                    _ImagePath = null;
                    _Image = value;
                    if (_ShowImage)
                    {
                        resize = true;
                        OnRedrawRequired();
                    }
                }
            }
        }
        private Settings.ImagePlacement _ImagePlacement;
        public Settings.ImagePlacement ImagePlacement
        {
            get
            {
                return _ImagePlacement;
            }
            set
            {
                if (_ImagePlacement != value)
                {
                    _ImagePlacement = value;
                    if (_ShowImage)
                    {
                        resize = true;
                        OnRedrawRequired();
                    }
                }
            }
        }

        private Image _DefaultBackgroundImage;
        public Image DefaultBackgroundImage
        {
            get
            {
                return _DefaultBackgroundImage;
            }
            set
            {
                if (_DefaultBackgroundImage != value)
                {
                    _ShowBackgroundImage = value != null || BackgroundImage != null;
                    _DefaultBackgroundImage = value;
                    resize = true;
                    OnRedrawRequired();
                }
            }
        }

        private bool _ShowBackgroundImage;
        private string _BackgroundImagePath;
        public override Image BackgroundImage
        {
            get
            {
                return base.BackgroundImage;
            }
            set
            {
                _BackgroundImagePath = null;
                _ShowBackgroundImage = value != null || _DefaultBackgroundImage != null;
                base.BackgroundImage = value;
                resize = true;
                OnRedrawRequired();
            }
        }

        private Settings.IAccount _AccountData;
        public Settings.IAccount AccountData
        {
            get
            {
                return _AccountData;
            }
            set
            {
                var changed = _AccountData != value;

                _AccountData = value;
                totp = null;

                if (apiTimer != null)
                {
                    if (changed || value.Type != Settings.AccountType.GuildWars2 || !object.ReferenceEquals(apiTimer.Api, ((Settings.IGw2Account)value).Api))
                    {
                        if (apiTimer.Pending)
                            OnRedrawRequired();
                        apiTimer.Dispose();
                        apiTimer = null;
                    }
                }

                if (value != null)
                {
                    this.DisplayName = value.Name;
                    this.AccountName = value.WindowsAccount;
                    this.AccountType = value.Type;
                    if (value.Pages == null)
                    {
                        this.Paging = null;
                        Paging_PageChanged(null, null);
                    }
                    else
                    {
                        this.Paging = new PagingData(value.Pages);
                        Paging_PageChanged(null, null);
                    }
                    if (changed || value.LastUsedUtc > _LastUsed)
                        this.LastUsedUtc = value.LastUsedUtc;
                    this.ShowDailyLogin = value.ShowDailyLogin;

                    if (!object.ReferenceEquals(_ImagePath, value.Image != null ? value.Image.Path : null))
                    {
                        if (_ImagePath != null && this._Image != null)
                        {
                            this._Image.Dispose();
                            this._Image = null;
                        }

                        if (value.Image != null)
                        {
                            try
                            {
                                this._ImagePlacement = value.Image.Placement;
                                this.Image = Bitmap.FromFile(value.Image.Path);
                                this.ShowImage = true;
                            }
                            catch { }

                            _ImagePath = value.Image.Path;
                        }
                        else
                        {
                            _ImagePath = null;

                            if (_ShowImage)
                            {
                                resize = true;
                                OnRedrawRequired();
                            }
                        }
                    }
                    else if (_ImagePath != null)
                    {
                        this.ImagePlacement = value.Image.Placement;
                    }

                    if (!object.ReferenceEquals(_BackgroundImagePath, value.BackgroundImage))
                    {
                        if (_BackgroundImagePath != null && this.BackgroundImage != null)
                        {
                            this.BackgroundImage.Dispose();
                            this.BackgroundImage = null;
                        }

                        if (value.BackgroundImage != null)
                        {
                            try
                            {
                                this.BackgroundImage = Bitmap.FromFile(value.BackgroundImage);
                            }
                            catch { }
                        }

                        _BackgroundImagePath = value.BackgroundImage;
                    }

                    if (value.Type == Settings.AccountType.GuildWars2)
                    {
                        var gw2 = (Settings.IGw2Account)value;
                        this.ShowDailyCompletion = gw2.ShowDailyCompletion;
                        this.LastDailyCompletionUtc = gw2.LastDailyCompletionUtc;
                        this.LastDailyLoginUtc = gw2.LastDailyLoginUtc;
                        this.ShowDailyLoginDay = Settings.StyleShowDailyLoginDay.Value;
                        this.DailyLoginDay = gw2.DailyLoginDay;
                        if (gw2.Api != null && (gw2.ApiTracking & Settings.ApiTracking.Astral) != 0)
                        {
                            this.ShowAstral = true;
                            this.Astral = gw2.Api.Data.Wallet == null ? (ushort)0 : gw2.Api.Data.Wallet.Value.Astral;
                        }
                        else
                        {
                            this.ShowAstral = false;
                            this.Astral = 0;
                        }
                        this.ShowWeeklyCompletion = gw2.ShowWeeklyCompletion;
                        this.LastWeeklyCompletionUtc = gw2.LastWeeklyCompletionUtc;
                    }
                    else if (value.Type == Settings.AccountType.GuildWars1)
                    {
                        var gw1 = (Settings.IGw1Account)value;
                        this.ShowDailyCompletion = false;
                        this.LastDailyCompletionUtc = DateTime.MinValue;
                        this.LastDailyLoginUtc = value.LastUsedUtc;
                        this.ShowDailyLoginDay = Settings.DailyLoginDayIconFlags.None;
                        this.ShowAstral = false;
                        this.ShowWeeklyCompletion = false;
                        this.LastWeeklyCompletionUtc = DateTime.MinValue;
                    }

                    this.ShowRun = Settings.StyleShowRun.Value && (HasRunAfter(value.RunAfter) || HasRunAfter(Settings.GetSettings(value.Type).RunAfter.Value));

                    if (value.Notes != null)
                        this.LastNoteUtc = value.Notes.ExpiresLast;
                    this.ColorKey = value.ColorKey;
                }
            }
        }

        private bool HasRunAfter(Settings.RunAfter[] ra)
        {
            if (ra != null)
            {
                for (var i = 0; i < ra.Length; i++)
                {
                    if (ra[i].Enabled)
                        return true;
                }
            }
            return false;
        }

        private string _AccountName;
        public string AccountName
        {
            get
            {
                return _AccountName;
            }
            set
            {
                if (_AccountName != value)
                {
                    _AccountName = value;
                    OnRedrawRequired();
                }
            }
        }

        private Settings.AccountType _AccountType;
        public Settings.AccountType AccountType
        {
            get
            {
                return _AccountType;
            }
            set
            {
                if (_AccountType != value)
                {
                    _AccountType = value;
                    OnRedrawRequired();
                }
            }
        }

        private string _DisplayName;
        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                if (_DisplayName != value)
                {
                    _DisplayName = value;
                    OnRedrawRequired();
                }
            }
        }

        private DateTime _LastUsed;
        public DateTime LastUsedUtc
        {
            get
            {
                return _LastUsed;
            }
            set
            {
                if (_LastUsed != value)
                {
                    _LastUsed = value;
                    DelayedRefresh();
                    OnRedrawRequired();
                }
            }
        }

        public bool ShowDailyLogin
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Login).Enabled;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Login).Enabled = value;
            }
        }

        private Settings.DailyLoginDayIconFlags _ShowDailyLoginDay;
        public Settings.DailyLoginDayIconFlags ShowDailyLoginDay
        {
            get
            {
                return _ShowDailyLoginDay;
            }
            set
            {
                if (_ShowDailyLoginDay != value)
                {
                    _ShowDailyLoginDay = value;
                    iconstore.GetIcon(IconData.IconType.LoginReward).Enabled = value != Settings.DailyLoginDayIconFlags.None && _DailyLoginDay != 0;
                }
            }
        }

        private byte _DailyLoginDay;
        public byte DailyLoginDay
        {
            get
            {
                return _DailyLoginDay;
            }
            set
            {
                if (_DailyLoginDay != value)
                {
                    _DailyLoginDay = value;
                    iconstore.GetIcon(IconData.IconType.LoginReward).Enabled = _ShowDailyLoginDay != Settings.DailyLoginDayIconFlags.None && value != 0;
                }
            }
        }

        public bool ShowDailyCompletion
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Daily).Enabled;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Daily).Enabled = value;
            }
        }

        public bool ShowWeeklyCompletion
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Weekly).Enabled;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Weekly).Enabled = value;
            }
        }

        public bool ShowAstral
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Astral).Enabled;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Astral).Enabled = value;
            }
        }

        private ushort _Astral;
        public ushort Astral
        {
            get
            {
                return _Astral;
            }
            set
            {
                if (_Astral != value)
                {
                    _Astral = value;
                    if (iconstore.GetIcon(IconData.IconType.Astral).Visible)
                        OnRedrawRequired();
                }
            }
        }

        public bool ShowRun
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Run).Enabled;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Run).Enabled = value;
            }
        }

        public DateTime LastNoteUtc
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Note).Expires;
            }
            set
            {
                iconstore.GetIcon(IconData.IconType.Note).Expires = value;
            }
        }

        public DateTime LastDailyCompletionUtc
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Daily).Date;
            }
            set
            {
                var i = iconstore.GetIcon(IconData.IconType.Daily);

                if (i.Date != value)
                {
                    i.Date = value;
                    i.Expires = value.Date.AddDays(1);
                }
            }
        }

        public DateTime LastWeeklyCompletionUtc
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Weekly).Date;
            }
            set
            {
                var i = iconstore.GetIcon(IconData.IconType.Weekly);

                if (i.Date != value)
                {
                    i.Date = value;
                    i.Expires = Util.Date.GetNextWeek(value, DayOfWeek.Monday, 7, 30);
                }
            }
        }

        public DateTime LastDailyLoginUtc
        {
            get
            {
                return iconstore.GetIcon(IconData.IconType.Login).Date;
            }
            set
            {
                var i = iconstore.GetIcon(IconData.IconType.Login);

                if (i.Date != value)
                {
                    i.Date = value;
                    i.Expires = value.Date.AddDays(1);
                }
            }
        }

        public void SetOrder(Icons.DisplayOrder order)
        {
            if (order == null)
                iconstore.SetOrder(Icons.IconGroup.Main, Icons.DisplayOrder.GetDefault(Icons.IconGroup.Main));
            else
                iconstore.SetOrder(order);
        }

        private ApiTimer GetApiTimer()
        {
            if (apiTimer == null)
            {
                apiTimer = new ApiTimer(_AccountData);
                apiTimer.Tick += apiTimer_Tick;
            }
            return apiTimer;
        }

        void apiTimer_Tick(object sender, EventArgs e)
        {
            if (apiTimer != null)
            {
                if (apiTimer.Pending)
                {
                    OnRedrawRequired();
                }
                //else if (!apiTimer.Active)
                //{
                //    apiTimer.Dispose();
                //    apiTimer = null;
                //}
            }

        }

        public void SetApiRequestDelay(ApiTimer.DelayType t, DateTime d)
        {
            if (DateTime.UtcNow < d)
            {
                GetApiTimer().SetTimer(t, d);
            }
            else if (apiTimer != null)
            {
                apiTimer.SetTimer(t, d);
            }

        }

        public bool ApiPending
        {
            get
            {
                return apiTimer != null && apiTimer.Pending;
            }
            set
            {
                if (value || apiTimer != null)
                {
                    var t = GetApiTimer();

                    if (t.Pending != value)
                    {
                        t.Pending = value;

                        if (value)
                        {
                            t.Restart();
                        }
                        else if (!t.Ticking)
                        {
                            apiTimer.Dispose();
                            apiTimer = null;
                        }

                        OnRedrawRequired();
                    }
                }
            }
        }

        private Color _ColorKey;
        public Color ColorKey
        {
            get
            {
                return _ColorKey;
            }
            set
            {
                if (_ColorKey != value)
                {
                    _ColorKey = value;
                    OnRedrawRequired();
                }
            }
        }

        private PagingData _Paging;
        public PagingData Paging
        {
            get
            {
                return _Paging;
            }
            set
            {
                if (!object.ReferenceEquals(_Paging, value))
                {
                    if (_Paging != null)
                        _Paging.PageChanged -= Paging_PageChanged;

                    _Paging = value;

                    if (value != null)
                        value.PageChanged += Paging_PageChanged;
                }
            }
        }

        void Paging_PageChanged(object sender, EventArgs e)
        {
            var b = Pinned;

            if (_Pinned != b)
            {
                _Pinned = b;
                OnRedrawRequired();
            }
        }

        public bool IsHovered
        {
            get
            {
                return isHovered;
            }
            set
            {
                if (isHovered != value)
                {
                    isHovered = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool IsFocused
        {
            get
            {
                return isFocused;
            }
            set
            {
                if (isFocused != value)
                {
                    isFocused = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool Selected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    OnRedrawRequired();
                }
            }
        }

        public bool IsActiveHighlight
        {
            get
            {
                return isActiveHighlight;
            }
            set
            {
                if (isActiveHighlight != value)
                {
                    isActiveHighlight = value;
                    if (isActive)
                        OnRedrawRequired();
                }
            }
        }

        public Rectangle GetIconBounds(AccountGridButton.IconData.IconType type)
        {
            var i = iconstore.GetIcon(type);

            if (i != null && i.Visible)
            {
                return i.DisplayBounds;
            }

            return Rectangle.Empty;
        }

        public virtual void RefreshColors()
        {
            _Colors = UiColors.GetTheme();
            OnRedrawRequired();
        }
    }
}
