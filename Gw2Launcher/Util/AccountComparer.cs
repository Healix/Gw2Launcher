using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gw2Launcher.Util
{
    class AccountComparer : IComparer<Settings.IAccount>
    {
        protected Settings.SortingOptions options;
        protected Settings.SortMode sorting;
        protected Settings.GroupMode grouping;
        protected bool sortingReversed;
        protected bool groupingReversed;
        protected bool groupingEnabled;
        protected Util.AlphanumericStringComparer stringComparer;

        public AccountComparer(Settings.SortingOptions options)
        {
            this.options = options;

            OnOptionsChanged();
        }

        protected virtual int Compare(Settings.GroupMode grouping, Settings.IAccount a, Settings.IAccount b)
        {
            if ((grouping & Settings.GroupMode.Active) == Settings.GroupMode.Active)
            {
                var result = -Client.Launcher.IsActive(a).CompareTo(Client.Launcher.IsActive(b));
                if (result != 0)
                    return result;
            }

            if ((grouping & Settings.GroupMode.Type) == Settings.GroupMode.Type)
            {
                var result = a.Type.CompareTo(b.Type);
                if (result != 0)
                    return result;
            }

            return 0;
        }

        protected virtual int Compare(Settings.SortMode sorting, Settings.IAccount a, Settings.IAccount b)
        {
            switch (sorting)
            {
                case Settings.SortMode.Account:

                    return stringComparer.Compare(a.WindowsAccount, b.WindowsAccount);

                case Settings.SortMode.LastUsed:

                    return a.LastUsedUtc.CompareTo(b.LastUsedUtc);

                case Settings.SortMode.Name:

                    return stringComparer.Compare(a.Name, b.Name);

                case Settings.SortMode.CustomGrid:
                case Settings.SortMode.CustomList:

                    return a.SortKey.CompareTo(b.SortKey);

                case Settings.SortMode.None:
                default:

                    return 0;
            }
        }

        public virtual int Compare(Settings.IAccount a, Settings.IAccount b)
        {
            if (a.Pinned != b.Pinned)
            {
                if (a.Pinned)
                    return -1;
                else
                    return 1;
            }

            int result;

            if (groupingEnabled)
            {
                result = Compare(grouping, a, b);

                if (result != 0)
                {
                    if (groupingReversed)
                        return -result;
                    return result;
                }
            }

            result = Compare(sorting, a, b);

            if (result == 0)
            {
                result = a.UID.CompareTo(b.UID);
            }

            if (sortingReversed)
                return -result;

            return result;
        }

        public virtual void Clear()
        {
            if (stringComparer != null)
                stringComparer.Clear();
        }

        protected virtual void OnOptionsChanged()
        {
            sorting = options.Sorting.Mode;
            sortingReversed = options.Sorting.Descending;
            grouping = options.Grouping.Mode;
            groupingReversed = options.Grouping.Descending;

            groupingEnabled = grouping != Settings.GroupMode.None && !IsSortingCustom;

            switch (sorting)
            {
                case Settings.SortMode.Account:
                case Settings.SortMode.Name:

                    if (this.stringComparer == null)
                        this.stringComparer = new Util.AlphanumericStringComparer();

                    break;
                default:

                    this.stringComparer = null;

                    break;
            }
        }

        public bool IsSortingCustom
        {
            get
            {
                switch (options.Sorting.Mode)
                {
                    case Settings.SortMode.CustomGrid:
                    case Settings.SortMode.CustomList:

                        return true;
                }

                return false;
            }
        }

        public Settings.SortingOptions Options
        {
            get
            {
                return options;
            }
            set
            {
                if (!options.Equals(value))
                {
                    options = value;
                    OnOptionsChanged();
                }
            }
        }
    }
}
