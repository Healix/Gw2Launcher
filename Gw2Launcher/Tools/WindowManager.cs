using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Gw2Launcher.Tools
{
    public class WindowManager
    {
        private const byte MAX_ACTIVE_TEMPLATES = 254;

        public interface IWindowBounds : IDisposable
        {
            event EventHandler BoundsChanged;
            event EventHandler OptionsChanged;

            Rectangle Bounds
            {
                get;
            }

            Settings.WindowOptions Options
            {
                get;
            }

            bool IsValid
            {
                get;
            }
        }

        private class WindowBounds : IWindowBounds
        {
            public event EventHandler BoundsChanged;
            public event EventHandler OptionsChanged;

            public WindowBounds()
            {

            }

            public WindowBounds(WindowManager manager, Window window, Rectangle bounds, Settings.IAccount account)
            {
                this.Manager = manager;
                this.Window = window;
                this._Bounds = bounds;
                if (window.Settings != null)
                    this._Options = window.Settings.Options;
                this.Account = account;
            }

            ~WindowBounds()
            {
                Dispose();
            }

            public bool IsValid
            {
                get
                {
                    return Window != null;
                }
            }

            private Rectangle _Bounds;
            public Rectangle Bounds
            {
                get
                {
                    return _Bounds;
                }
                set
                {
                    if (_Bounds != value)
                    {
                        _Bounds = value;
                        if (BoundsChanged != null)
                            BoundsChanged(this, EventArgs.Empty);
                    }
                }
            }

            private Settings.WindowOptions _Options;
            public Settings.WindowOptions Options
            {
                get
                {
                    return _Options;
                }
                set
                {
                    if (_Options != value)
                    {
                        _Options = value;
                        if (OptionsChanged != null)
                            OptionsChanged(this, EventArgs.Empty);
                    }
                }
            }

            public Settings.IAccount Account
            {
                get;
                set;
            }

            public Window Window
            {
                get;
                set;
            }

            public WindowManager Manager
            {
                get;
                set;
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (this.Manager != null)
                    this.Manager.Release(this);
            }
        }

        private class Template
        {
            public Template(WindowManager m, Settings.WindowTemplate t)
            {
                this.Manager = m;
                this.Settings = t;
            }

            public WindowManager Manager
            {
                get;
                set;
            }

            private Settings.WindowTemplate _Settings;
            public Settings.WindowTemplate Settings
            {
                get
                {
                    return _Settings;
                }
                set
                {
                    if (_Settings != null)
                    {
                        _Settings.ScreensChanged -= template_ScreensChanged;
                        _Settings.SnapToEdgesChanged -= template_SnapToEdgesChanged;
                    }
                    if (value != null)
                    {
                        value.ScreensChanged += template_ScreensChanged;
                        value.SnapToEdgesChanged += template_SnapToEdgesChanged;
                    }
                    _Settings = value;
                }
            }

            void template_SnapToEdgesChanged(object sender, EventArgs e)
            {
                this.Manager.Refresh(this);
            }

            void template_ScreensChanged(object sender, EventArgs e)
            {
                this.Manager.Deactivate(this);
            }

            public Rectangle[] Bounds
            {
                get;
                set;
            }

            public byte[] Keys
            {
                get;
                set;
            }

            //public byte[] Order
            //{
            //    get;
            //    set;
            //}

            public List<Assignment> Assigned
            {
                get;
                set;
            }

            public bool Find(Settings.WindowTemplate.Assignment a, out Assignment t)
            {
                for (var i = Assigned.Count - 1; i >=0;--i)
                {
                    if (Assigned[i].Settings == a)
                    {
                        t = Assigned[i];
                        return true;
                    }
                }

                t = null;
                return false;
            }

            public Rectangle GetBounds(int index)
            {
                if (this.Settings.SnapToEdges != Gw2Launcher.Settings.WindowTemplate.SnapType.None)
                {
                    return this.Manager.Inflate(this.Bounds[index], this.Settings.SnapToEdges);
                }
                else
                {
                    return this.Bounds[index];
                }
            }

            public bool GetBounds(byte key, out Rectangle bounds)
            {
                var keys = this.Keys;

                if (keys == null)
                {
                    if (key < this.Bounds.Length)
                    {
                        bounds = GetBounds(key);
                        return true;
                    }
                }
                else
                {
                    for (var i = keys.Length - 1; i >= 0; --i)
                    {
                        if (keys[i] == key)
                        {
                            bounds = GetBounds(i);
                            return true;
                        }
                    }
                }

                bounds = Rectangle.Empty;
                return false;
            }
        }

        private class Assignment
        {
            public Assignment(Template t, Settings.WindowTemplate.Assignment a)
            {
                this.Template = t;
                this.Settings = a;
            }

            public Template Template
            {
                get;
                set;
            }

            private Settings.WindowTemplate.Assignment _Settings;
            public Settings.WindowTemplate.Assignment Settings
            {
                get
                {
                    return _Settings;
                }
                set
                {
                    if (_Settings != null)
                    {
                        _Settings.AssignedChanged -= assigned_AccountsChanged;
                        _Settings.TypeChanged += assigned_TypeChanged;
                        
                    }
                    if (value != null)
                    {
                        value.AssignedChanged += assigned_AccountsChanged;
                        value.TypeChanged += assigned_TypeChanged;
                    }
                    _Settings = value;
                }
            }

            void assigned_TypeChanged(object sender, EventArgs e)
            {
                this.Template.Manager.OnTemplatesChanged();
            }

            void assigned_AccountsChanged(object sender, EventArgs e)
            {
                this.Template.Manager.Deactivate(this);
            }

            public Window[] Windows
            {
                get;
                set;
            }
        }

        private class Window
        {
            public Window(byte key)
            {
                this.Key = key;
            }

            /// <summary>
            /// The account that this window is reserved for, if applicable
            /// </summary>
            public Settings.IAccount Reserved
            {
                get;
                set;
            }

            /// <summary>
            /// Assigned settings
            /// </summary>
            public Settings.WindowTemplate.Assigned Settings
            {
                get;
                set;
            }

            /// <summary>
            /// The account currently assigned to use this template
            /// </summary>
            public WindowBounds UsedBy
            {
                get;
                set;
            }

            public byte Key
            {
                get;
                set;
            }

            public bool IsReserved
            {
                get
                {
                    return Reserved != null;
                }
            }

            public bool IsUsed
            {
                get
                {
                    return UsedBy != null;
                }
            }

            public void Release()
            {
                if (UsedBy != null)
                    UsedBy.Window = null;
                UsedBy = null;
            }
        }

        private class Reservation
        {
            public Reservation(Assignment a, Window w)
            {
                this.Assigned = a;
                this.Window = w;
            }

            public Assignment Assigned
            {
                get;
                set;
            }

            public Window Window
            {
                get;
                set;
            }
        }

        private struct TemplateAssignment
        {
            public TemplateAssignment(Settings.WindowTemplate t, Settings.WindowTemplate.Assignment a)
            {
                Template = t;
                Assignment = a;
            }

            public Settings.WindowTemplate Template;
            public Settings.WindowTemplate.Assignment Assignment;
        }

        public event EventHandler TemplatesChanged;

        public enum BoundsResult : byte
        {
            /// <summary>
            /// A template has been returned for the account
            /// </summary>
            Success,
            /// <summary>
            /// A template that could be used is currently in use by another account
            /// </summary>
            Busy,
            /// <summary>
            /// There are no templates for the account
            /// </summary>
            None,
        }

        private Dictionary<Settings.WindowTemplate, Template> templates;
        private Dictionary<Settings.IAccount, Reservation> reserved;
        private List<Assignment> ordered;
        private byte releasing;

        private static WindowManager _Instance;
        public static WindowManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WindowManager();
                return _Instance;
            }
        }

        public static bool IsActive
        {
            get
            {
                return _Instance != null;
            }
        }

        public static void Load()
        {
            var active = new List<TemplateAssignment>();

            for (var i = Settings.WindowTemplates.Count - 1; i >= 0; --i)
            {
                var t = Settings.WindowTemplates[i];
                if (t.Assignments == null)
                    continue;
                for (var j = t.Assignments.Count - 1; j >= 0; --j)
                {
                    var a = t.Assignments[j];
                    if (a.EnabledKey > 0)
                    {
                        active.Add(new TemplateAssignment(t, a));
                    }
                }
            }

            if (active.Count > 0)
            {
                active.Sort(new Comparison<TemplateAssignment>(
                    delegate(TemplateAssignment a, TemplateAssignment b)
                    {
                        return a.Assignment.EnabledKey.CompareTo(b.Assignment.EnabledKey);
                    }));

                var wm = Instance;

                foreach (var ta in active)
                {
                    wm.Activate(ta.Template, ta.Assignment);
                }
            }
        }

        public WindowManager()
        {
            templates = new Dictionary<Settings.WindowTemplate, Template>();
            reserved = new Dictionary<Settings.IAccount, Reservation>();
            ordered = new List<Assignment>();

            Settings.WindowTemplates.ValueRemoved += WindowTemplates_ValueRemoved;
        }

        /// <summary>
        /// When a template is no longer used, templates will be re-assigned to ensure all active templates
        /// are used in order
        /// </summary>
        public bool ReorderOnRelease
        {
            get
            {
                return (Settings.WindowManagerOptions.Value & Settings.WindowManagerFlags.ReorderOnRelease) != 0;
            }
            set
            {
                if (value)
                    Settings.WindowManagerOptions.Value |= Settings.WindowManagerFlags.ReorderOnRelease;
                else
                    Settings.WindowManagerOptions.Value &= ~Settings.WindowManagerFlags.ReorderOnRelease;
            }
        }

        /// <summary>
        /// Delays launching an account when all templates that could be assigned to the account are already in use
        /// </summary>
        public bool DelayLaunchUntilAvailable
        {
            get
            {
                return (Settings.WindowManagerOptions.Value & Settings.WindowManagerFlags.DelayLaunchUntilAvailable) != 0;
            }
            set
            {
                if (value)
                    Settings.WindowManagerOptions.Value |= Settings.WindowManagerFlags.DelayLaunchUntilAvailable;
                else
                    Settings.WindowManagerOptions.Value &= ~Settings.WindowManagerFlags.DelayLaunchUntilAvailable;
            }
        }

        /// <summary>
        /// When a template becomes available, accounts that were launched without a template can obtain it
        /// </summary>
        public bool AllowActiveChanges
        {
            get
            {
                return (Settings.WindowManagerOptions.Value & Settings.WindowManagerFlags.AllowActiveChanges) != 0;
            }
            set
            {
                if (value)
                    Settings.WindowManagerOptions.Value |= Settings.WindowManagerFlags.AllowActiveChanges;
                else
                    Settings.WindowManagerOptions.Value &= ~Settings.WindowManagerFlags.AllowActiveChanges;
            }
        }

        void WindowTemplates_ValueRemoved(object sender, Settings.WindowTemplate e)
        {
            Template t;
            if (templates.TryGetValue(e, out t))
            {
                Deactivate(t);
            }
        }

        private Window FindReserved(Settings.IAccount account, out Assignment a)
        {
            for (var j = ordered.Count - 1; j >= 0; --j)
            {
                var windows = ordered[j].Windows;

                for (var i = 0; i < windows.Length; i++)
                {
                    var w = windows[i];

                    if (w.Reserved == account)
                    {
                        a = ordered[j];
                        return w;
                    }
                }
            }

            a = null;
            return null;
        }

        public void Deactivate(Settings.WindowTemplate t, Settings.WindowTemplate.Assignment a)
        {
            Template template;
            Assignment assigned;

            a.SetEnabled(0);

            lock (this)
            {
                if (this.templates.TryGetValue(t, out template) && template.Find(a, out assigned))
                {
                    Deactivate(assigned);
                }
            }
        }
        
        private void Deactivate(Template t)
        {
            lock (this)
            {
                for (var i = t.Assigned.Count - 1; i >= 0; --i)
                {
                    Deactivate(t.Assigned[i]);
                }
            }
        }

        private void Deactivate(Assignment a)
        {
            lock (this)
            {
                if (a.Settings == null)
                    return;

                a.Settings.SetEnabled(0);

                a.Template.Assigned.Remove(a);
                ordered.Remove(a);
                a.Settings = null;

                foreach (var w in a.Windows)
                {
                    Settings.IAccount account;

                    var h = w.UsedBy;

                    if (h != null)
                    {
                        h.Window = null;
                        account = w.UsedBy.Account;
                    }
                    else if (w.Reserved != null)
                    {
                        account = w.Reserved;
                    }
                    else
                    {
                        continue;
                    }

                    Reservation r;
                    if (reserved.TryGetValue(account, out r))
                    {
                        if (r.Window == w)
                        {
                            reserved.Remove(account);

                            if (w.Reserved == account)
                            {
                                Assignment _a;
                                r.Window = FindReserved(account, out _a);
                                if (r.Window != null)
                                {
                                    r.Assigned = _a;
                                    reserved[account] = r;
                                }
                            }
                        }
                    }
                }

                if (a.Template.Assigned.Count == 0)
                {
                    this.templates.Remove(a.Template.Settings);
                    a.Template.Settings = null;
                }
            }

            if (AllowActiveChanges)
                Client.Launcher.ApplyWindowedTemplate(false);

            OnTemplatesChanged();
        }

        private byte GetNextEnabledKey()
        {
            //keys are incremented up to 255, at which point they're re-indexed
            //maximum number of 254 active templates
            var count = ordered.Count;

            if (count >= MAX_ACTIVE_TEMPLATES)
            {
                do
                {
                    Deactivate(ordered[0]);
                    count = ordered.Count;
                }
                while (count >= MAX_ACTIVE_TEMPLATES);
            }
            else if (count > 0)
            {
                var k = ordered[count - 1].Settings.EnabledKey;

                if (k < 255)
                {
                    return (byte)(k + 1);
                }
            }
            else
            {
                return 1;
            }

            for (var i = 0; i < count; i++)
            {
                ordered[i].Settings.SetEnabled((byte)i);
            }

            return (byte)(ordered[count - 1].Settings.EnabledKey + 1);
        }

        public void Activate(Settings.WindowTemplate t, Settings.WindowTemplate.Assignment a)
        {
            List<Settings.IAccount> pending = null;
            
            lock (this)
            {
                Template template;

                if (!templates.TryGetValue(t, out template))
                {
                    templates[t] = template = new Template(this, t)
                    {
                        Assigned = new List<Assignment>(t.Assignments != null ? t.Assignments.Count : 1),
                        Keys = t.Keys,
                        //Order = t.Order,
                    };
                    template.Bounds = new UI.WindowPositioning.ScreenTemplate(t).GetWindows(false, false, false);
                }
                else if (a.Enabled)
                {
                    Assignment _a;
                    if (template.Find(a, out _a))
                    {
                        //already enabled
                        return;
                    }
                }

                a.SetEnabled(GetNextEnabledKey());

                var count = template.Bounds.Length;
                var windows = new Window[count];
                Dictionary<byte, Settings.WindowTemplate.Assigned> assigned = null;

                if (a.Assigned != null)
                {
                    assigned = new Dictionary<byte, Settings.WindowTemplate.Assigned>(a.Assigned.Length);
                    foreach (var v in a.Assigned)
                    {
                        assigned[v.Key] = v.Value;
                    }
                }

                var at = new Assignment(template, a)
                {
                    Windows = windows,
                };

                for (var i = 0; i < count; i++)
                {
                    var index = t.Order != null ? t.Order[i] : (byte)i;
                    var key = t.Keys != null ? t.Keys[i] : (byte)i;

                    var w = windows[index] = new Window(key);

                    if (assigned != null)
                    {
                        Settings.WindowTemplate.Assigned assign;
                        if (assigned.TryGetValue(key, out assign))
                        {
                            w.Settings = assign;

                            if (assign.Type == Settings.WindowTemplate.Assigned.AssignedType.Account)
                            {
                                foreach (var uid in assign.Accounts)
                                {
                                    var s = Settings.Accounts[uid];
                                    if (s.HasValue)
                                    {
                                        var account = s.Value;

                                        w.Reserved = account;

                                        Reservation r;
                                        if (reserved.TryGetValue(account, out r))
                                        {
                                            r.Window.Release();
                                        }
                                        reserved[account] = new Reservation(at, w);

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (w.Reserved != null && Client.Launcher.IsActive(w.Reserved))
                    {
                        if (pending == null)
                            pending = new List<Settings.IAccount>();
                        pending.Add(w.Reserved);
                    }
                }

                template.Assigned.Add(at);

                ordered.Add(at);
            }

            if (pending != null)
            {
                foreach (var account in pending)
                {
                    Client.Launcher.ApplyWindowedTemplate(account);
                }
            }

            if (AllowActiveChanges)
                Client.Launcher.ApplyWindowedTemplate(false);

            OnTemplatesChanged();
        }

        /// <summary>
        /// Updates any accounts using the template
        /// </summary>
        private void Refresh(Template t)
        {
            List<Settings.IAccount> pending = null;

            lock (this)
            {
                foreach (var r in reserved)
                {
                    if (r.Value.Assigned.Template == t)
                    {
                        var h = r.Value.Window.UsedBy;
                        if (h != null)
                        {
                            if (pending == null)
                                pending = new List<Settings.IAccount>();
                            pending.Add(r.Key);
                        }
                    }
                }
            }

            if (pending != null)
            {
                foreach (var a in pending)
                {
                    Client.Launcher.ApplyWindowedTemplate(a);
                }
            }
        }

        private void OnTemplatesChanged()
        {
            if (TemplatesChanged != null)
                TemplatesChanged(this, EventArgs.Empty);
        }

        private Rectangle Inflate(Rectangle r, Settings.WindowTemplate.SnapType type)
        {
            if (type == Settings.WindowTemplate.SnapType.None)
                return r;

            var b = Util.Dpi.GetWindowFrame(System.Windows.Forms.Screen.FromRectangle(r), type == Settings.WindowTemplate.SnapType.ClientEdge);

            if (type == Settings.WindowTemplate.SnapType.WindowEdgeOuter && b.Left > 0)
            {
                b.Left -= 1;
                b.Bottom -= 1;
                b.Right -= 1;
            }

            return new Rectangle(r.X - b.Left, r.Y - b.Top, r.Width + b.Horizontal, r.Height + b.Vertical);
        }

        private bool IsMatch(Settings.AccountType account, Settings.WindowTemplate.Assignment.AccountType assign)
        {
            switch (assign)
            {
                case Settings.WindowTemplate.Assignment.AccountType.Any:

                    return true;

                case Settings.WindowTemplate.Assignment.AccountType.GuildWars2:

                    return account == Settings.AccountType.GuildWars2;

                case Settings.WindowTemplate.Assignment.AccountType.GuildWars1:

                    return account == Settings.AccountType.GuildWars1;
            }

            return false;
        }

        private bool IsMatch(ushort account, Settings.WindowTemplate.Assigned assigned)
        {
            if (assigned == null)
                return true;

            switch (assigned.Type)
            {
                case Settings.WindowTemplate.Assigned.AssignedType.Account:
                case Settings.WindowTemplate.Assigned.AssignedType.AccountsIncluding:
                case Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding:

                    var match = false;

                    if (assigned.Accounts != null)
                    {
                        var from = assigned.Accounts.Length;

                        if (from > 1)
                        {
                            var to = from / 2;

                            if (account <= assigned.Accounts[to])
                            {
                                from = to + 1;
                                to = 0;
                            }
                            else
                            {
                                ++to;
                            }

                            for (var i = from - 1; i >= to; --i)
                            {
                                if (assigned.Accounts[i] == account)
                                {
                                    match = true;
                                    break;
                                }
                            }
                        }
                        else if (from > 0)
                        {
                            if (assigned.Accounts[0] == account)
                                match = true;
                        }
                    }

                    if (assigned.Type == Settings.WindowTemplate.Assigned.AssignedType.AccountsExcluding)
                        return !match;

                    return match;

                case Settings.WindowTemplate.Assigned.AssignedType.Disabled:

                    return false;

                case Settings.WindowTemplate.Assigned.AssignedType.Any:
                default:

                    return true;
            }
        }

        public BoundsResult TryGetBounds(Settings.IAccount account, out IWindowBounds bounds)
        {
            lock (this)
            {
                Reservation r;
                if (reserved.TryGetValue(account, out r))
                {
                    r.Window.Release();

                    Rectangle rect;
                    if (r.Assigned.Template.GetBounds(r.Window.Key, out rect)) //should always be true, otherwise it shouldn't exist (queued for removal)
                    {
                        bounds = r.Window.UsedBy = new WindowBounds(this, r.Window, rect, account);
                        return BoundsResult.Success;
                    }
                    else if (r.Window.Reserved == account)
                    {
                        Assignment a;
                        r.Window = FindReserved(account, out a);
                        if (r.Window != null)
                        {
                            r.Assigned = a;
                            if (r.Assigned.Template.GetBounds(r.Window.Key, out rect))
                            {
                                bounds = r.Window.UsedBy = new WindowBounds(this, r.Window, rect, account);
                                return BoundsResult.Success;
                            }
                        }
                    }

                    reserved.Remove(account);
                }

                var result = BoundsResult.None;

                for (int i = 0, count = ordered.Count; i < count; i++)
                {
                    if (!IsMatch(account.Type, ordered[i].Settings.Type))
                        continue;

                    var t = ordered[i].Template;
                    var windows = ordered[i].Windows;

                    for (var j = 0; j < windows.Length; j++)
                    {
                        var w = windows[j];

                        if (w.UsedBy == null)
                        {
                            if (w.Reserved == null || w.Reserved == account)
                            {
                                if (IsMatch(account.UID, w.Settings))
                                {
                                    Rectangle rect;
                                    if (t.GetBounds(w.Key, out rect))
                                    {
                                        reserved[account] = r = new Reservation(ordered[i], w);
                                        bounds = r.Window.UsedBy = new WindowBounds(this, r.Window, rect, account);
                                        return BoundsResult.Success;
                                    }
                                }
                            }
                        }
                        else if (result == BoundsResult.None && w.Reserved == null)
                        {
                            result = BoundsResult.Busy;
                        }
                    }
                }

                bounds = null;

                if (result == BoundsResult.Busy)
                {
                    //check if any of the templates could be used for this account
                    for (int i = 0, count = ordered.Count; i < count; i++)
                    {
                        if (!IsMatch(account.Type, ordered[i].Settings.Type))
                            continue;
                        var windows = ordered[i].Windows;

                        for (var j = 0; j < windows.Length; j++)
                        {
                            var w = windows[j];

                            if (w.Reserved == null && IsMatch(account.UID, w.Settings))
                            {
                                return BoundsResult.Busy;
                            }
                        }
                    }

                    return BoundsResult.None;
                }
                else
                {
                    return result;
                }
            }
        }

        private void Release(WindowBounds b)
        {
            if (b.Window == null)
                return;

            var released = false;

            lock (this)
            {
                var w = b.Window;

                if (w != null && w.UsedBy == b)
                {
                    if (w.Reserved != b.Account)
                    {
                        Reservation r;
                        if (reserved.TryGetValue(b.Account, out r) && reserved.Remove(b.Account))
                        {
                            released = true;
                        }
                    }

                    w.Release();
                }
                else
                {
                    b.Window = null;
                }
            }

            if (released)
            {
                OnReleased();
            }
        }

        private void ReorderReservations()
        {
            List<Settings.IAccount> pending = null;

            lock (this)
            {
                var remaining = reserved.Count;
                var hasFree = false;

                foreach (var o in ordered)
                {
                    foreach (var w in o.Windows)
                    {
                        if (w.IsUsed)
                        {
                            if (!w.IsReserved)
                            {
                                if (hasFree)
                                {
                                    if (pending == null)
                                        pending = new List<Settings.IAccount>(remaining);
                                    pending.Add(w.UsedBy.Account);
                                    reserved.Remove(w.UsedBy.Account);
                                    w.Release();
                                }
                            }

                            if (--remaining == 0)
                                break;
                        }
                        else if (!w.IsReserved)
                        {
                            hasFree = true;
                        }
                    }
                }
            }

            if (pending != null)
            {
                foreach (var a in pending)
                {
                    Client.Launcher.ApplyWindowedTemplate(a);
                }
            }

            if (AllowActiveChanges)
                Client.Launcher.ApplyWindowedTemplate(false);
        }

        private async void OnReleased()
        {
            lock (this)
            {
                if (++releasing != 1)
                    return;
            }

            await Task.Delay(100);

            do
            {
                var _releasing = releasing;

                if (ReorderOnRelease)
                {
                    ReorderReservations();
                }
                else if (AllowActiveChanges)
                {
                    Client.Launcher.ApplyWindowedTemplate(false);
                }

                lock (this)
                {
                    if (releasing == _releasing)
                    {
                        releasing = 0;
                        break;
                    }
                }
            }
            while (true);

            OnTemplatesChanged();
        }
    }
}
