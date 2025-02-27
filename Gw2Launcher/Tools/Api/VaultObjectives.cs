using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Launcher.Api;

namespace Gw2Launcher.Tools.Api
{
    class VaultObjectives
    {
        public event EventHandler Cleared;
        public event EventHandler<DataChangedEventArgs> DataChanged;
        public event EventHandler<DataChangedEventArgs> AccountDataChanged;

        private class DataChangedQueue
        {
            private DataChangedEventArgs[] data;

            public DataChangedQueue(int capacity)
            {
                data = new DataChangedEventArgs[capacity];
            }

            public DataChangedEventArgs[] Data
            {
                get
                {
                    return data;
                }
            }

            public void Add(Vault.VaultType type, ChangeType changed, ObjectivesGroup og, AccountObjectives ao)
            {
                int i;

                for (i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        break;
                    }

                    if (object.ReferenceEquals(data[i].Group,og))
                    {
                        changed |= data[i].Changed;

                        if (data[i].Changed != changed)
                        {
                            if (data[i].Data != null)
                            {
                                ao = data[i].Data;
                            }

                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                if (i == data.Length)
                {
                    Array.Resize<DataChangedEventArgs>(ref data, i + 3);
                }

                data[i] = new DataChangedEventArgs(type, changed, og, ao);
            }
        }

        [Flags]
        public enum RefreshOptions : byte
        {
            None = 0,
            /// <summary>
            /// API should not be requested
            /// </summary>
            NoQuery = 1,
            /// <summary>
            /// Data will be updated
            /// </summary>
            Update = 2,
            /// <summary>
            /// Delay until the next update
            /// </summary>
            Delayed = 4,
        }

        [Flags]
        public enum ChangeType : byte
        {
            None = 0,
            /// <summary>
            /// Date of the data
            /// </summary>
            Date = 1,
            /// <summary>
            /// Objective values have changed
            /// </summary>
            Values = 2,
            /// <summary>
            /// Objectives have changed
            /// </summary>
            Objectives = 4,
            /// <summary>
            /// Objectives have changed; objectives were added to an existing group
            /// </summary>
            ObjectivesAdded = 8,
            /// <summary>
            /// Accounts linked to the objectives have changed
            /// </summary>
            Accounts = 16,
            /// <summary>
            /// All objectives for the specified type were removed
            /// </summary>
            Cleared = 32,
        }

        public class DataChangedEventArgs:EventArgs
        {
            public DataChangedEventArgs(Vault.VaultType type, ChangeType change, ObjectivesGroup group, AccountObjectives account)
            {
                this.Type = type;
                this.Group = group;
                this.Data = account;
                this.Changed = change;
            }

            /// <summary>
            /// The account that was changed, if applicable
            /// </summary>
            public AccountObjectives Data
            {
                get;
                private set;
            }

            /// <summary>
            /// The objectives group that changed, if applicable
            /// </summary>
            public ObjectivesGroup Group
            {
                get;
                private set;
            }

            public ChangeType Changed
            {
                get;
                private set;
            }

            public Vault.VaultType Type
            {
                get;
                private set;
            }
        }

        private class ApiRequest : ApiRequestManager.DataRequest
        {
            public ApiRequest(ApiData.DataType type, Settings.IAccount account, Settings.ApiDataKey key, RequestOptions options = RequestOptions.None)
                : base(type, account, key, options)
            {

            }

            public bool EnsureLatest
            {
                get;
                set;
            }
        }

        public class RefreshStatus : IDisposable
        {
            public event EventHandler Complete;

            private ApiRequestManager.DataRequest[] requests;
            private int remaining;

            public RefreshStatus(Vault.VaultType type, ApiRequestManager.DataRequest[] requests)
            {
                this.Type = type;

                this.requests = requests;
                this.remaining = requests.Length;

                for (var i = 0; i < requests.Length; i++)
                {
                    if (requests[i] == null)
                    {
                        this.remaining = i;

                        break;
                    }

                    requests[i].Complete += r_Complete;
                }
            }

            public Vault.VaultType Type
            {
                get;
                private set;
            }

            void r_Complete(object sender, EventArgs e)
            {
                ((ApiRequestManager.DataRequest)sender).Complete -= r_Complete;

                if (--remaining == 0)
                {
                    if (Complete != null)
                    {
                        Complete(this, EventArgs.Empty);
                    }
                }
            }

            public bool IsComplete
            {
                get
                {
                    return remaining == 0;
                }
            }

            public void Dispose()
            {
                Complete = null;
            }
        }

        //need to have list of objectives with accounts that share that same list
        //may want option to condense into a single list of objectices

        //need to get objectives grouped by daily/weekly - multiple daily/weekly groups for the different groupings of dailies/weeklies
        //need to get objectives by account, showing their progress

        public class AccountObjective : IComparable<AccountObjective>
        {
            public AccountObjective(Vault.Objective o)
            {
                this.ID = o.ID;
                this.Claimed = o.Claimed;
                this.ProgressCurrent = o.ProgressCurrent;
            }

            public AccountObjective(ushort id)
            {
                this.ID = id;
            }

            public ushort ID
            {
                get;
                set;
            }

            public bool Claimed
            {
                get;
                set;
            }

            public ushort ProgressCurrent
            {
                get;
                set;
            }

            public int CompareTo(AccountObjective o)
            {
                return this.ID.CompareTo(o.ID);
            }
        }

        public class AccountObjectives
        {
            private AccountObjective[][] objectives;
            private ObjectivesGroup[] groups;
            private bool[] claimed;
            private byte pending;
            private DateTime[] date;

            public AccountObjectives(Settings.IAccount account)
            {
                this.Account = account;

                objectives = new AccountObjective[3][];
                groups = new ObjectivesGroup[3];
                claimed = new bool[3];
                date = new DateTime[3];
            }

            public Settings.IAccount Account
            {
                get;
                private set;
            }

            public DateTime LastPending
            {
                get;
                set;
            }

            public bool IsPending(Vault.VaultType type)
            {
                var bit = 1 << GetIndex(type);

                return (pending & bit) == 0;
            }

            public void SetPending(Vault.VaultType type, bool value)
            {
                var bit = 1 << GetIndex(type);

                if (value)
                {
                    pending &= (byte)~bit;
                }
                else
                {
                    pending |= (byte)bit;
                }
            }

            public void SetPending()
            {
                LastPending = DateTime.UtcNow;
                pending = 0;
            }

            public static int GetIndex(Vault.VaultType type)
            {
                switch (type)
                {
                    case Vault.VaultType.Daily:

                        return 0;

                    case Vault.VaultType.Weekly:

                        return 1;

                    case Vault.VaultType.Special:

                        return 2;
                }

                return 0;
            }

            public AccountObjective[][] Objectives
            {
                get
                {
                    return objectives;
                }
            }

            public ObjectivesGroup[] Groups
            {
                get
                {
                    return groups;
                }
            }

            public ObjectivesGroup GetGroup(Vault.VaultType type)
            {
                return groups[GetIndex(type)];
            }

            public AccountObjective[] GetObjectives(Vault.VaultType type)
            {
                return objectives[GetIndex(type)];
            }

            public AccountObjective GetObjective(Vault.VaultType type, ushort id, int index = -1)
            {
                var o = objectives[GetIndex(type)];

                if (o != null)
                {
                    if (index != -1 && index < o.Length && o[index].ID == id)
                    {
                        return o[index];
                    }

                    for (var i = 0; i < o.Length; i++)
                    {
                        if (o[i].ID == id)
                        {
                            return o[i];
                        }
                    }
                }

                return null;
            }

            public bool GetClaimed(Vault.VaultType type)
            {
                return claimed[GetIndex(type)];
            }

            public DateTime GetDate(Vault.VaultType type)
            {
                return date[GetIndex(type)];
            }

            public bool IsComplete(Vault.VaultType type)
            {
                var ao = objectives[GetIndex(type)];

                if (ao != null && ao.Length > 0)
                {
                    for (var i = 0; i < ao.Length; i++)
                    {
                        if (ao[i] == null || !ao[i].Claimed)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public bool Equals(Vault.VaultType type, uint key, AccountObjective[] o)
            {
                var g = groups[GetIndex(type)];

                return g != null && g.Summary == key && g.Equals(o);
            }

            public bool IsEmpty
            {
                get
                {
                    for (var i = 0; i < objectives.Length; i++)
                    {
                        if (objectives[i] != null)
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }

            /// <summary>
            /// Updates objectives, keeping existing account values
            /// </summary>
            public ChangeType Update(Vault.VaultType type, ObjectivesGroup g)
            {
                var r = ChangeType.None;
                var gi = GetIndex(type);
                var objectives = this.objectives[gi];
                var length = g.Objectives != null ? g.Objectives.Length : 0;
                var existing = objectives != null ? objectives.Length : 0;

                if (existing != length)
                {
                    r |= ChangeType.Values;
                }

                if (length > 0)
                {
                    var objectives2 = new AccountObjective[length];
                    var startAt = 0;

                    for (var i = 0; i < length; i++)
                    {
                        for (; startAt < existing; startAt++)
                        {
                            if (objectives[startAt].ID >= g.Objectives[i].ID)
                            {
                                if (g.Objectives[i].ID == objectives[startAt].ID)
                                {
                                    objectives2[i] = objectives[startAt];
                                    ++startAt;
                                }
                                break;
                            }
                        }

                        if (objectives2[i] == null)
                        {
                            objectives2[i] = new AccountObjective(g.Objectives[i].ID);
                            r |= ChangeType.Values;
                        }
                    }

                    this.objectives[gi] = objectives2;
                }
                else
                {
                    this.objectives[gi] = null;
                }

                if ((r & ChangeType.Values) != 0)
                {
                    this.claimed[gi] = false;
                }

                if (this.groups[gi] != g)
                {
                    this.groups[gi] = g;

                    if (g.Add(this.Account))
                    {
                        r |= ChangeType.Accounts;
                    }

                    r |= ChangeType.Objectives;
                }

                return r;
            }

            public ChangeType Update(Vault.VaultType type, AccountObjective[] o, ObjectivesGroup g, Vault data)
            {
                var gi = GetIndex(type);
                var objectives = this.objectives[gi];

                if (o == null)
                {
                    this.objectives[gi] = null;
                    this.groups[gi] = null;
                    this.claimed[gi] = false;
                    this.date[gi] = DateTime.MinValue;

                    SetPending(type, true);

                    if (objectives != null)
                    {
                        return ChangeType.Values | ChangeType.Objectives | ChangeType.Date;
                    }
                    else
                    {
                        return ChangeType.None;
                    }
                }

                var r = ChangeType.None;

                if (this.date[gi] != g.LastModified)
                {
                    r |= ChangeType.Date;

                    this.date[gi] = g.LastModified;
                }

                if (g.LastModified > LastPending || DateTime.UtcNow.Subtract(LastPending).TotalMinutes > 10)
                {
                    SetPending(type, false);
                }

                if (objectives == null || objectives.Length != o.Length)
                {
                    r |= ChangeType.Values;

                    this.objectives[gi] = o;

                    if (this.groups[gi] != g)
                    {
                        this.groups[gi] = g;

                        r |= ChangeType.Objectives;
                    }

                    if (g.Add(this.Account))
                    {
                        r |= ChangeType.Accounts;
                    }
                }
                else
                {
                    for (var i = 0; i < o.Length; i++)
                    {
                        if (objectives[i].ID != o[i].ID)
                        {
                            r |= ChangeType.Values;

                            this.objectives[gi] = o;

                            if (this.groups[gi] != g)
                            {
                                this.groups[gi] = g;

                                r |= ChangeType.Objectives;
                            }

                            if (g.Add(this.Account))
                            {
                                r |= ChangeType.Accounts;
                            }

                            break;
                        }
                        else if (objectives[i].ProgressCurrent != o[i].ProgressCurrent || objectives[i].Claimed != o[i].Claimed)
                        {
                            r |= ChangeType.Values;

                            objectives[i] = o[i];
                        }
                    }
                }

                if (data != null)
                {
                    if (this.claimed[gi] != data.Claimed)
                    {
                        this.claimed[gi] = data.Claimed;
                        r |= ChangeType.Values;
                    }
                }

                if (IsPending(type) && IsComplete(type))
                {
                    SetPending(type, false);
                }

                return r;
            }
        }

        public class ObjectivesGroup
        {
            private Settings.IAccount[] accounts;
            private ObjectiveData[] objectives;

            public ObjectivesGroup(Vault.VaultType type, byte id, uint sum, ObjectiveData[] objectives, Settings.IAccount[] accounts = null)
            {
                this.Type = type;
                this.ID = id;
                this.Summary = sum;
                this.objectives = objectives;
                this.accounts = accounts;
            }

            public DateTime LastModified
            {
                get;
                set;
            }

            public int LastModifiedInMinutes
            {
                get
                {
                    return (int)DateTime.UtcNow.Subtract(LastModified).TotalMinutes;
                }
            }

            public byte ID
            {
                get;
                set;
            }

            public uint Summary
            {
                get;
                private set;
            }

            public Vault.VaultType Type
            {
                get;
                private set;
            }

            public ObjectiveData[] Objectives
            {
                get
                {
                    return objectives;
                }
            }

            /// <summary>
            /// Array may contain nulls; first null is end of array
            /// </summary>
            public Settings.IAccount[] Accounts
            {
                get
                {
                    return accounts;
                }
            }

            public bool HasAccounts
            {
                get
                {
                    return accounts != null && accounts[0] != null;
                }
            }

            /// <summary>
            /// Number of objectives
            /// </summary>
            public int Count
            {
                get
                {
                    if (objectives != null)
                    {
                        return objectives.Length;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }

            public bool Add(Settings.IAccount account)
            {
                lock (this)
                {
                    if (accounts == null)
                    {
                        accounts = new Settings.IAccount[] { account };
                    }
                    else
                    {
                        int i;

                        for (i = 0; i < accounts.Length; i++)
                        {
                            if (accounts[i] == null)
                            {
                                accounts[i] = account;
                                return true;
                            }
                            else if (accounts[i].UID == account.UID)
                            {
                                return false;
                            }
                        }

                        Array.Resize<Settings.IAccount>(ref accounts, i + 1);

                        accounts[i] = account;
                    }

                    return true;
                }
            }

            public void Remove(Settings.IAccount account)
            {
                lock (this)
                {
                    if (accounts != null)
                    {
                        for (var i = 0; i < accounts.Length; i++)
                        {
                            if (accounts[i] == null)
                            {
                                return;
                            }
                            else if (accounts[i].UID == account.UID)
                            {
                                var j = accounts.Length - 1;

                                if (i != j)
                                {
                                    Array.Copy(accounts, i + 1, accounts, i, j - i);
                                }

                                accounts[j] = null;

                                return;
                            }
                        }
                    }
                }
            }

            public bool Equals(AccountObjective[] o)
            {
                if (objectives.Length == o.Length)
                {
                    for (var i = 0; i < o.Length; i++)
                    {
                        if (objectives[i].ID != o[i].ID)
                        {
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }

            public void Update(uint sum, ObjectiveData[] o)
            {
                this.Summary = sum;
                this.objectives = o;
            }

            public void SetPending(ApiRequestManager.DataRequest r)
            {
                r.Complete += r_Complete;

                this.Pending = true;
            }

            void r_Complete(object sender, EventArgs e)
            {
                ((ApiRequestManager.DataRequest)sender).Complete -= r_Complete;

                this.Pending = false;
            }

            public bool Pending
            {
                get;
                set;
            }

        }

        public class ObjectiveData
        {
            public ObjectiveData(Vault.Objective o)
            {
                this.ID = o.ID;
                this.Type = o.Type;
                this.Title = o.Title;
                this.ProgressComplete = o.ProgressComplete;
            }

            public ushort ID
            {
                get;
                set;
            }

            public Vault.ObjectiveType Type
            {
                get;
                set;
            }

            public string Title
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public ushort ProgressComplete
            {
                get;
                set;
            }
        }

        private Dictionary<ushort, AccountObjectives> accounts; //objectives per account
        private Dictionary<ushort, ObjectiveData> objectives; //objectives by ID
        private ObjectivesGroup[][] groups; //objectives by what was offered
        private KeyValuePair<byte,uint>[][] history;
        private ApiRequestManager apiManager;

        public VaultObjectives(ApiRequestManager apiManager)
        {
            groups = new ObjectivesGroup[3][];
            accounts = new Dictionary<ushort, AccountObjectives>();
            objectives = new Dictionary<ushort, ObjectiveData>();

            this.apiManager = apiManager;

            apiManager.DataAvailable += apiManager_DataAvailable;
        }

        public ApiRequestManager ApiManager
        {
            get
            {
                return apiManager;
            }
        }

        private byte GetIndex(Vault.VaultType type)
        {
            switch (type)
            {
                case Vault.VaultType.Daily:
                    return 0;
                case Vault.VaultType.Weekly:
                    return 1;
                case Vault.VaultType.Special:
                    return 2;
            }
            return 0;
        }

        public byte GetNewGroupKey(Vault.VaultType type)
        {
            if (type == Vault.VaultType.Special)
                return 0;

            var keys = new bool[byte.MaxValue];

            foreach (var a in Util.Accounts.GetGw2Accounts())
            {
                var k = GetGroupKey(type, a);

                if (k > 0)
                {
                    keys[k - 1] = true;
                }
            }

            for (var i = 0; i < keys.Length; i++)
            {
                if (!keys[i])
                {
                    return (byte)(i + 1);
                }
            }

            return 0;
        }

        public void Add(Settings.IGw2Account account, Vault data, DateTime date)
        {
            var objectives = new AccountObjective[data.Objectives.Length];
            uint sum = 0;
            ushort previous = 0;
            var sorted = true;
            var indexes = new byte[objectives.Length];
            //var indexes = new int[objectives.Length];

            if (objectives.Length == 0)
            {
                return;
            }

            //for (var i = 0; i < objectives.Length;i++)
            //{
            //    objectives[i] = new AccountObjective(data.Objectives[i]);
            //    indexes[i] = i;
            //    sum += objectives[i].ID;
            //}

            for (var i = 0; i < objectives.Length; i++)
            {
                objectives[i] = new AccountObjective(data.Objectives[i]);
                sum += objectives[i].ID;
                indexes[i] = (byte)i;

                if (sorted)
                {
                    if (objectives[i].ID < previous)
                    {
                        sorted = false;
                    }
                    else
                    {
                        previous = objectives[i].ID;
                    }
                }
            }

            //lock (this)
            //{
            //    for (var i = 0; i < objectives.Length; i++)
            //    {
            //        objectives[i] = new AccountObjective(data.Objectives[i]);
            //        sum += objectives[i].ID;

            //        if (sorted)
            //        {
            //            if (objectives[i].ID < previous)
            //            {
            //                sorted = false;
            //            }
            //            else
            //            {
            //                previous = objectives[i].ID;
            //            }
            //        }

            //        ObjectiveData d;
            //        if (!this.objectives.TryGetValue(objectives[i].ID, out d))
            //        {
            //            this.objectives[objectives[i].ID] = d = new ObjectiveData()
            //            {
            //                ProgressComplete = data.Objectives[i].ProgressComplete,
            //                Title = data.Objectives[i].Title,
            //            };
            //        }
            //    }
            //}

            if (!sorted)
            {
                Array.Sort<AccountObjective, byte>(objectives, indexes);
            }

            ChangeType r;
            ChangeType rd = ChangeType.None;
            AccountObjectives ao;
            ObjectivesGroup og;
            Settings.IAccount[] _accounts = null;
            ChangeType[] _changes = null;

            lock (this)
            {
                var k = GetGroupKey(data.Type, account);
                var index = GetIndex(data.Type);
                //var g = FindGroup(index, sum, objectives);
                //var g = FindGroup(index, k, sum, objectives);
                int g;

                if (k != 0 && (g = FindGroup(index, k)) != -1)
                {
                    og = groups[index][g];

                    if (og.Summary == 0)
                    {
                        var od = new ObjectiveData[objectives.Length];

                        for (var i = 0; i < objectives.Length; i++)
                        {
                            if (!this.objectives.TryGetValue(objectives[i].ID, out od[i]))
                            {
                                this.objectives[objectives[i].ID] = od[i] = new ObjectiveData(data.Objectives[indexes[i]]);
                            }
                        }

                        //groups[index][g] = new ObjectivesGroup(og.Type, og.ID, sum, od);
                        og.Update(sum, od);
                        rd = ChangeType.Objectives;
                    }
                    else if (og.Summary != sum || !og.Equals(objectives))
                    {
                        //objectives don't match, account has changed groups
                        g = FindGroup(index, sum, objectives);
                        k = 0;

                    }
                }
                else
                {
                    g = FindGroup(index, sum, objectives);
                }

                if (g == -1)
                {
                    if (data.Type != Vault.VaultType.Special)
                    {
                        //find a new key for this group if it doesn't have one or another group is already using the key (objectives for this account is no longer shared with other accounts)
                        if (k == 0)
                        {
                            k = GetNewGroupKey(data.Type);

                            SetGroupKey(data.Type, account, k);
                        }
                        else
                        {
                            var keys = Settings.ApiKeys.GetValues();
                            var sdate = GetStartingDate(GetApiType(data.Type));

                            _accounts = new Settings.IAccount[keys.Length];
                            var j = 0;

                            foreach (var key in keys)
                            {
                                if (GetGroupKey(data.Type, key.Value) == k)
                                {
                                    var kaccounts = key.Value.Accounts;

                                    if (kaccounts != null)
                                    {
                                        for (var i = 0; i < kaccounts.Length; i++)
                                        {
                                            if (kaccounts[i].LastUsedUtc >= sdate && kaccounts[i].UID != account.UID)
                                            {
                                                if (j == _accounts.Length)
                                                {
                                                    Array.Resize<Settings.IAccount>(ref _accounts, j + 3);
                                                }

                                                _accounts[j++] = kaccounts[i];

                                                AccountObjectives _ao;

                                                if (!accounts.TryGetValue(kaccounts[i].UID, out _ao))
                                                {
                                                    accounts[kaccounts[i].UID] = new AccountObjectives(kaccounts[i]);
                                                }

                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (j > 0)
                            {
                                rd |= ChangeType.Accounts;

                                if (j < _accounts.Length - 3)
                                {
                                    Array.Resize<Settings.IAccount>(ref _accounts, j + 1);
                                }
                            }
                            else
                            {
                                _accounts = null;
                            }
                        }
                    }
                    else if (groups[index] != null)
                    {
                        //check for differences in special objectives

                        if (groups[index][0].LastModified >= date)
                        {
                            //data is different, but cache is newer - assuming this data is expired

                            if (!accounts.TryGetValue(account.UID, out ao))
                            {
                                accounts[account.UID] = ao = new AccountObjectives(account);
                            }

                            var existing = ao.Objectives[2];

                            if (existing == null || existing.Length != groups[index][0].Objectives.Length)
                            {

                            }

                            return;
                        }
                        else
                        {
                            //special objectives have changed, remove old objectives from cache

                            _accounts = groups[index][0].Accounts;

                            var hs = new HashSet<ushort>();

                            foreach (var o in groups[index][0].Objectives)
                            {
                                hs.Add(o.ID);
                            }

                            var added = false;
                            var count = hs.Count;

                            for (var i = 0; i < objectives.Length; i++)
                            {
                                if (!hs.Remove(objectives[i].ID))
                                {
                                    added = true;
                                }
                            }

                            if (added && count > 0)
                            {
                                rd |= ChangeType.ObjectivesAdded;
                            }

                            foreach (var id in hs)
                            {
                                this.objectives.Remove(id);
                            }
                        }
                    }

                    var od = new ObjectiveData[objectives.Length];

                    for (var i = 0; i < objectives.Length;i++)
                    {
                        if (!this.objectives.TryGetValue(objectives[i].ID, out od[i]))
                        {
                            this.objectives[objectives[i].ID] = od[i] = new ObjectiveData(data.Objectives[indexes[i]]);
                        }
                    }

                    if (groups[index] == null)
                    {
                        g = 0;
                        groups[index] = new ObjectivesGroup[1];
                    }
                    else
                    {
                        if (data.Type == Vault.VaultType.Special)
                        {
                            g = 0;
                        }
                        else
                        {
                            g = groups[index].Length;
                            Array.Resize<ObjectivesGroup>(ref groups[index], g + 1);
                        }
                    }

                    groups[index][g] = og = new ObjectivesGroup(data.Type, k, sum, od, _accounts);
                    
                    og.LastModified = date;

                    rd |= ChangeType.Objectives | ChangeType.Date;
                }
                else
                {
                    og = groups[index][g];

                    if (og.ID != k)
                    {
                        k = og.ID;

                        if (data.Type != Vault.VaultType.Special)
                        {
                            SetGroupKey(data.Type, account, k);
                        }
                    }

                    if (date > og.LastModified)
                    {
                        og.LastModified = date;
                        rd |= ChangeType.Date;
                    }
                }

                if (_accounts != null)
                {
                    _changes = new ChangeType[_accounts.Length];

                    for (var i = 0; i < _accounts.Length; i++)
                    {
                        if (_accounts[i] == null)
                        {
                            break;
                        }
                        else
                        {
                            AccountObjectives _ao;

                            if (_accounts[i].UID != account.UID && accounts.TryGetValue(_accounts[i].UID, out _ao))
                            {
                                _changes[i] = _ao.Update(data.Type, og);
                            }
                        }
                    }
                }

                if (!accounts.TryGetValue(account.UID, out ao))
                {
                    accounts[account.UID] = ao = new AccountObjectives(account);
                }
                else
                {
                    var aog = ao.GetGroup(data.Type);

                    if (aog != null && aog != og)
                    {
                        aog.Remove(ao.Account);
                    }
                }

                r = ao.Update(data.Type, objectives, og, data);

                if ((r & ChangeType.Accounts) != 0)
                {
                    rd |= ChangeType.Accounts;
                }
            }

            if (r != ChangeType.None && AccountDataChanged != null)
            {
                OnAccountDataChanged(data.Type, r, og, ao);

                if (_changes != null)
                {
                    for (var i = 0; i < _accounts.Length; i++)
                    {
                        if (_accounts[i] == null)
                        {
                            break;
                        }
                        else if (_changes[i] != ChangeType.None)
                        {
                            AccountObjectives _ao;

                            if (accounts.TryGetValue(_accounts[i].UID, out _ao))
                            {
                                OnAccountDataChanged(data.Type, _changes[i], og, _ao);
                            }
                        }
                    }
                }
            }

            if (rd != ChangeType.None)
            {
                OnDataChanged(data.Type, rd, og, ao);
            }
        }

        /// <summary>
        /// Finds a group with the specified grouping key
        /// </summary>
        private int FindGroup(byte index, byte key)
        {
            var groups = this.groups[index];

            if (groups != null)
            {
                for (var i = 0; i < groups.Length; i++)
                {
                    if (groups[i].ID == key)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds a group that shares the objectives
        /// </summary>
        private int FindGroup(byte index, uint sum, AccountObjective[] objectives)
        {
            var groups = this.groups[index];

            if (groups != null)
            {
                for (var i = 0; i < groups.Length; i++)
                {
                    if (groups[i].Summary == sum && objectives.Length == groups[i].Objectives.Length)
                    {
                        var b = true;

                        for (var j = 0; j < objectives.Length; j++)
                        {
                            if (objectives[j].ID != groups[i].Objectives[j].ID)
                            {
                                b = false;
                                break;
                            }
                        }

                        if (b)
                        {
                            return i;
                        }
                    }
                }
            }

            return -1;
        }

        public uint GetHistory(Vault.VaultType type, byte id)
        {
            if (type != Vault.VaultType.Special)
            {
                lock (this)
                {
                    if (this.history != null)
                    {
                        var sums = this.history[GetIndex(type)];

                        if (sums != null)
                        {
                            for (var i = 0; i < sums.Length; i++)
                            {
                                if (sums[i].Key == id)
                                {
                                    return sums[i].Value;
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }

        public void Clear()
        {
            var types = new Vault.VaultType[] 
            { 
                Vault.VaultType.Daily, 
                Vault.VaultType.Weekly, 
                Vault.VaultType.Special 
            };
            var changed = new bool[types.Length];

            lock (this)
            {
                this.objectives.Clear();
                this.accounts.Clear();
                this.history = null;

                for (var i = 0; i < types.Length; i++)
                {
                    var k = GetIndex(types[i]);

                    if (this.groups[k] != null)
                    {
                        this.groups[k] = null;
                        changed[i] = true;
                    }
                }
            }

            for (var i = 0; i < types.Length; i++)
            {
                OnDataChanged(types[i], ChangeType.Cleared, null, null);
            }

            if (Cleared != null)
            {
                try
                {
                    Cleared(this, EventArgs.Empty);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public void Clear(Vault.VaultType type)
        {
            var changed = false;

            lock(this)
            {
                var index = GetIndex(type);
                var groups = this.groups[index];

                if (groups != null)
                {
                    this.groups[index] = null;

                    var empty = true;
                    var sums = new KeyValuePair<byte, uint>[groups.Length];

                    //check if all types are empty
                    for (var i = 0; i < this.groups.Length; i++)
                    {
                        if (this.groups[i] != null)
                        {
                            empty = false;
                            break;
                        }
                    }

                    if (empty)
                    {
                        this.accounts.Clear();
                        this.objectives.Clear();

                        for (var i = 0; i < groups.Length; i++)
                        {
                            sums[i] = new KeyValuePair<byte, uint>(groups[i].ID, groups[i].Summary);
                        }
                    }
                    else
                    {
                        var accounts = 0;

                        for (var i = 0; i < groups.Length; i++)
                        {
                            sums[i] = new KeyValuePair<byte, uint>(groups[i].ID, groups[i].Summary);

                            foreach (var o in groups[i].Objectives)
                            {
                                objectives.Remove(o.ID);
                            }

                            if (groups[i].Accounts != null)
                            {
                                accounts += groups[i].Accounts.Length;
                            }
                        }

                        var uids = new List<ushort>(accounts);

                        foreach (var a in this.accounts.Values)
                        {
                            if (a.Update(type, null, null, null) != ChangeType.None && a.IsEmpty)
                            {
                                uids.Add(a.Account.UID);
                            }
                        }

                        foreach (var uid in uids)
                        {
                            this.accounts.Remove(uid);
                        }
                    }

                    if (type != Vault.VaultType.Special)
                    {
                        if (this.history == null)
                        {
                            history = new KeyValuePair<byte, uint>[2][];
                        }
                        this.history[index] = sums;
                    }

                    changed = true;

                }
                else if (this.history != null)
                {
                    this.history[index] = null;
                }
            }

            if (changed)
            {
                OnDataChanged(type, ChangeType.Cleared, null, null);
            }
        }

        private void OnAccountDataChanged(Vault.VaultType type, ChangeType changed, ObjectivesGroup og, AccountObjectives ao)
        {
            if (AccountDataChanged != null)
            {
                try
                {
                    AccountDataChanged(this, new DataChangedEventArgs(type, changed, og, ao));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        private void OnDataChanged(Vault.VaultType type, ChangeType changed, ObjectivesGroup og, AccountObjectives ao)
        {
            if (DataChanged != null)
            {
                try
                {
                    DataChanged(this, new DataChangedEventArgs(type, changed, og, ao));
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }
        }

        public ObjectivesGroup[] GetObjectives(Vault.VaultType type)
        {
            return groups[GetIndex(type)];
        }

        public AccountObjectives GetObjectives(Settings.IAccount account)
        {
            lock (this)
            {
                AccountObjectives ao;

                accounts.TryGetValue(account.UID, out ao);

                return ao;
            }
        }

        public ObjectivesGroup GetObjectives(Vault.VaultType type, Settings.IAccount account)
        {
            lock (this)
            {
                AccountObjectives ao;

                if (accounts.TryGetValue(account.UID, out ao))
                {
                    return ao.GetGroup(type);
                }

                return null;
            }
        }

        /// <summary>
        /// Returns the key used for grouping accounts with the same objectives or 0 for no key
        /// </summary>
        private byte GetGroupKey(Vault.VaultType type, Settings.IGw2Account a)
        {
            return GetGroupKey(type, a.Api);
        }

        /// <summary>
        /// Returns the key used for grouping accounts with the same objectives or 0 for no key
        /// </summary>
        private byte GetGroupKey(Vault.VaultType type, Settings.ApiDataKey api)
        {
            if (type != Vault.VaultType.Special && api != null)
            {
                switch (type)
                {
                    case Vault.VaultType.Daily:

                        return api.Data.VaultGroup.Daily;

                    case Vault.VaultType.Weekly:

                        return api.Data.VaultGroup.Weekly;

                    case Vault.VaultType.Special:
                    default:

                        return 0;
                }
            }

            return 0;
        }

        private void SetGroupKey(Vault.VaultType type, Settings.IGw2Account a, byte key)
        {
            if (type != Vault.VaultType.Special)
            {
                if (Util.Logging.Enabled)
                {
                    Util.Logging.LogEvent(a, "Vault grouping for [" + type + "] changed to [" + key + "]");
                }

                var api = a.Api;

                if (api != null)
                {
                    var g = api.Data.VaultGroup;

                    switch (type)
                    {
                        case Vault.VaultType.Daily:

                            g.Daily = key;

                            break;
                        case Vault.VaultType.Weekly:

                            g.Weekly = key;

                            break;
                    }

                    api.Data.VaultGroup = g;
                }
            }
        }

        private ApiData.DataType GetApiType(Vault.VaultType type)
        {
            switch (type)
            {
                case Vault.VaultType.Daily:

                    return ApiData.DataType.VaultDaily;

                case Vault.VaultType.Weekly:

                    return ApiData.DataType.VaultWeekly;

                case Vault.VaultType.Special:

                    return ApiData.DataType.VaultSpecial;

                default:

                    throw new NotSupportedException();
            }
        }

        private Vault.VaultType GetVaultType(ApiData.DataType type)
        {
            switch (type)
            {
                case ApiData.DataType.VaultDaily:

                    return Vault.VaultType.Daily;

                case ApiData.DataType.VaultWeekly:

                    return Vault.VaultType.Weekly;

                case ApiData.DataType.VaultSpecial:

                    return Vault.VaultType.Special;

                default:

                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Marks that the objectives for the account need to be refreshed
        /// </summary>
        public void Refresh(Settings.IGw2Account account)
        {
            lock (this)
            {
                AccountObjectives ao;

                if (accounts.TryGetValue(account.UID, out ao))
                {
                    ao.SetPending();
                }
            }
        }

        /// <summary>
        /// Refreshes the account if no data is cached
        /// </summary>
        /// <param name="required">Forces a refresh</param>
        /// <param name="delayed">If refreshing, delay until the next update</param>
        /// <param name="query">If the api should be queried (if needed)</param>
        public RefreshStatus Refresh(Vault.VaultType type, Settings.IGw2Account account, RefreshOptions options) //, bool required, bool delayed, bool query)
        {
            return Refresh(type, account, options, OnDataChanged);
        }

        private RefreshStatus Refresh(Vault.VaultType type, Settings.IGw2Account account, RefreshOptions options, Action<Vault.VaultType,ChangeType,ObjectivesGroup,AccountObjectives> onDataChanged) //, bool required, bool delayed, bool query)
        {
            RefreshStatus rs = null;

            var api = account.Api;

            if (api != null && (api.Permissions & TokenInfo.Permissions.Progression) != 0)
            {
                var rd = ChangeType.None;
                var r = ChangeType.None;
                var refresh = (options & RefreshOptions.Update) != 0;
                
                AccountObjectives ao;
                ObjectivesGroup og = null;

                lock (this)
                {
                    accounts.TryGetValue(account.UID, out ao);

                    //if (accounts.TryGetValue(account.UID, out ao))
                    //{
                    //    if (ao.GetGroup(type) != null)
                    //    {
                    //        if (!required || !ao.IsPending(type))
                    //        {
                    //            return rs;
                    //        }
                    //    }
                    //}
                    
                    if (ao == null || (og = ao.GetGroup(type)) == null)
                    {
                        //add the account to its last known group if available
                        var k = GetGroupKey(type, account);

                        if (k != 0 || type == Vault.VaultType.Special)
                        {
                            var index = GetIndex(type);
                            var i = FindGroup(index, k);

                            if (ao == null)
                            {
                                ao = new AccountObjectives(account);
                                accounts[account.UID] = ao;
                            }

                            if (i == -1)
                            {
                                og = new ObjectivesGroup(type, k, 0, new ObjectiveData[0]);

                                if (groups[index] == null)
                                {
                                    i = 0;
                                    groups[index] = new ObjectivesGroup[1];
                                }
                                else
                                {
                                    if (type == Vault.VaultType.Special)
                                    {
                                        i = 0;
                                    }
                                    else
                                    {
                                        i = groups[index].Length;
                                        Array.Resize<ObjectivesGroup>(ref groups[index], i + 1);

                                    }
                                }

                                groups[index][i] = og;

                                ao.Update(type, og);
                                refresh = true;
                            }
                            else
                            {
                                og = groups[index][i];

                                if (og.Summary == 0)
                                {
                                    ao.Update(type, og);

                                    if (!og.Pending)
                                    {
                                        refresh = true;
                                    }
                                }
                                else
                                {
                                    r = ao.Update(type, og);
                                    rd = ChangeType.Accounts;
                                }
                            }

                            //if (i != -1)
                            //{
                            //    og = groups[index][i];

                            //    if (ao == null)
                            //    {
                            //        ao = new AccountObjectives(account);
                            //        accounts[account.UID] = ao;
                            //    }

                            //    r = ao.Update(type, og);

                            //    rd = ChangeType.Accounts;
                            //}
                        }
                        else
                        {
                            refresh = true;
                        }
                    }
                    else if (!refresh || !ao.IsPending(type))
                    {
                        if (og.Summary == 0 && !og.Pending || refresh && DateTime.UtcNow.Subtract(ao.GetDate(type)).TotalMinutes > 5 && !ao.IsComplete(type))
                        {
                        }
                        else
                        {
                            return rs;
                        }
                    }
                    else if (refresh && ao.IsComplete(type))
                    {
                        refresh = false;
                    }
                }

                if (og != null && og.Summary != 0)
                {
                    if (r != ChangeType.None)
                    {
                        OnAccountDataChanged(type, r, og, ao);
                    }

                    if (rd != ChangeType.None && onDataChanged != null)
                    {
                        onDataChanged(type, rd, og, ao);
                    }
                }

                if ((options & RefreshOptions.NoQuery) == 0)
                {
                    if (refresh)
                    {
                        var t = GetApiType(type);
                        var o = ApiData.DataRequest.RequestOptions.NoCache;
                        var delay = DateTime.MinValue;

                        if ((options & RefreshOptions.Delayed) != 0)
                        {
                            o |= ApiData.DataRequest.RequestOptions.Delay | ApiData.DataRequest.RequestOptions.IgnoreDelayIfModified;
                            delay = apiManager.DataSource.GetNextEstimatedUpdate(api.Key);
                            if (delay == DateTime.MinValue)
                                delay = DateTime.UtcNow.AddMinutes(2);
                        }

                        var requests = new ApiRequestManager.DataRequest[]
                        {
                            new ApiRequest(t, account, account.Api, o)
                            {
                                Date = DateTime.UtcNow,
                                Reason = ApiRequestManager.RequestReason.None,
                                Delay = delay,
                                EnsureLatest = (options & RefreshOptions.Update) != 0,
                            },
                        };

                        rs = new RefreshStatus(type, requests);

                        if (og != null)
                        {
                            og.SetPending(requests[0]);
                        }

                        apiManager.Queue(requests);
                    }
                }
            }

            return rs;
        }

        public RefreshStatus Refresh(Vault.VaultType type, bool includeInactive = false, bool includeActive = true)
        {
            //queue all active accounts
            //use custom request - if requestDate-lastmodified < 10m, retry request

            //if (type == Vault.VaultType.Special)
            //{
            //    var g = groups[GetIndex(type)];

            //    if (g != null)
            //    {
            //        for (var i = 0; i < g.Length; i++)
            //        {
            //            if (g[i].HasAccounts && g[i].LastModifiedInMinutes < 60)
            //            {
            //                return null;
            //            }
            //        }
            //    }
            //}

            //List<ObjectivesGroup> queue = null;
            //IList<Settings.IAccount> accounts;
            //RefreshStatus rs = null;
            //var count = 0;
            DataChangedQueue cqueue = null;

            if (includeInactive)
            {
                var keys = Settings.ApiKeys.GetValues();
                var date = GetStartingDate(GetApiType(type));
                var index = GetIndex(type);
                cqueue = new DataChangedQueue(keys.Length);

                foreach (var key in keys)
                {
                    var api = key.Value;
                    if (api == null || (api.Permissions & TokenInfo.Permissions.Progression) == 0)
                        continue;
                    var _accounts = api.Accounts;
                    if (_accounts == null)
                        continue;
                    for (var i = 0; i < _accounts.Length; i++)
                    {
                        if (_accounts[i].LastUsedUtc >= date)
                        {
                            Refresh(type, (Settings.IGw2Account)_accounts[i], RefreshOptions.NoQuery, cqueue.Add);

                            //note there could be multiple accounts sharing using key, but only one needs to be refreshed now, since the others will be added on completion regardless
                            break;
                        }
                    }
                }

            }
            else if (includeActive)
            {
                var accounts = Client.Launcher.GetActiveProcessesWithState(Client.Launcher.AccountState.ActiveGame);
                cqueue = new DataChangedQueue(accounts.Count);

                foreach (var a in accounts)
                {
                    if (a.Type == Settings.AccountType.GuildWars2)
                    {
                        Refresh(type, (Settings.IGw2Account)a, RefreshOptions.NoQuery, cqueue.Add);
                    }
                }
            }

            if (cqueue != null && DataChanged != null)
            {
                var data = cqueue.Data;

                for (var i = 0; i < data.Length; i++)
                {
                    if (data[i] == null)
                    {
                        break;
                    }

                    try
                    {
                        DataChanged(this, data[i]);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                    }
                }

                cqueue = null;
            }

            List<ObjectivesGroup> queue = null;

            lock (this)
            {
                var og = groups[GetIndex(type)];

                if (og != null)
                {
                    for (var i = 0; i < og.Length; i++)
                    {
                        if (og[i] == null)
                        {
                            break;
                        }
                        else if (og[i].Summary == 0 && !og[i].Pending)
                        {
                            if (queue== null)
                            {
                                queue = new List<ObjectivesGroup>(og.Length - i);
                            }

                            queue.Add(og[i]);

                            //if (ids == null || count == 0 && og[i].ID == 0 || og[i].ID != 0 && !ids[og[i].ID - 1])
                            //{
                            //    var _accounts = og[i].Accounts;

                            //    if (_accounts != null)
                            //    {
                            //        for (var j = 0; j < _accounts.Length; j++)
                            //        {
                            //            if (_accounts[j] == null)
                            //            {
                            //                break;
                            //            }

                            //            if (_accounts[j].Type == Settings.AccountType.GuildWars2)
                            //            {
                            //                var api = ((Settings.IGw2Account)_accounts[j]).Api;

                            //                if (api != null && (api.Permissions & TokenInfo.Permissions.Progression) != 0)
                            //                {
                            //                    if (accounts == null)
                            //                    {
                            //                        accounts = new List<Settings.IAccount>();
                            //                    }

                            //                    if (count < accounts.Count)
                            //                        accounts[count] = _accounts[j];
                            //                    else
                            //                        accounts.Add(_accounts[j]);

                            //                    ++count;

                            //                    break;
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }

            if (queue != null)
            {
                var count = 0;
                var t = GetApiType(type);
                var requests = new ApiRequestManager.DataRequest[queue.Count];
                var d = DateTime.UtcNow;

                for (var i = 0; i < requests.Length; i++)
                {
                    var og = queue[i];
                    var account = Select(og.Accounts);

                    if (account != null)
                    {
                        var api = ((Settings.IGw2Account)account).Api;

                        if (api != null && (api.Permissions & TokenInfo.Permissions.Progression) != 0)
                        {
                            var r = new ApiRequest(t, account, api, ApiData.DataRequest.RequestOptions.None)
                            {
                                Date = d,
                                Reason = ApiRequestManager.RequestReason.None,
                                EnsureLatest = false,
                            };

                            r.DataAvailable += r_DataAvailable;
                            r.Complete += r_Complete;

                            og.SetPending(r);

                            requests[count++] = r;
                        }
                    }


                }

                if (count > 0)
                {
                    var rs = new RefreshStatus(type, requests);

                    apiManager.Queue(requests);

                    return rs;
                }
            }

            return null;

        }

        /// <summary>
        /// Returns an account to use for API requests, prioritizing active accounts, then last used accounts
        /// </summary>
        private Settings.IAccount Select(Settings.IAccount[] accounts)
        {
            if (accounts == null)
            {
                return null;
            }

            var d = DateTime.MinValue;
            var index = -1;

            for (var i = 0; i < accounts.Length; i++)
            {
                if (accounts[i] == null)
                {
                    break;
                }

                if (accounts[i].Type != Settings.AccountType.GuildWars2)
                {
                    continue;
                }

                var api = ((Settings.IGw2Account)accounts[i]).Api;

                if (api != null && (api.Permissions & TokenInfo.Permissions.Progression) != 0)
                {
                    if (i == 0 && (accounts.Length == 1 || accounts[1] == null))
                    {
                        return accounts[i];
                    }
                    else
                    {
                        var m = Client.Launcher.GetMumbleLink(accounts[i]);

                        if (m != null && m.IsVerified)
                        {
                            return accounts[i];
                        }

                        if (accounts[i].LastUsedUtc > d)
                        {
                            d = accounts[i].LastUsedUtc;
                            index = i;
                        }
                    }
                }
            }

            if (index == -1)
            {
                return null;
            }
            else
            {
                return accounts[index];
            }
        }

        void r_Complete(object sender, EventArgs e)
        {
        }

        void r_DataAvailable(object sender, ApiData.RequestDataAvailableEventArgs e)
        {
        }

        private DateTime GetStartingDate(ApiData.DataType type)
        {
            if (type == ApiData.DataType.VaultWeekly)
            {
                return Util.Date.GetWeek(DateTime.UtcNow);
            }
            else
            {
                return DateTime.UtcNow.Date;
            }
        }

        void apiManager_DataAvailable(object sender, ApiRequestManager.DataAvailableEventArgs e)
        {
            if (e.Status == ApiData.DataStatus.Error)
                return;

            switch (e.Type)
            {
                case ApiData.DataType.VaultDaily:
                case ApiData.DataType.VaultWeekly:
                case ApiData.DataType.VaultSpecial:
                    
                    var a = (Settings.IGw2Account)e.Account;

                    if (a != null)
                    {
                        var data = (Vault)e.Data.Value;
                        var date = GetStartingDate(e.Type);
                        var elapsed = e.LastModified.Subtract(date).TotalMinutes;
                        var expired = elapsed < 3;

                        //warning: the api can have the previous objectives with a last modified date after reset (have seen 00:01 UTC with yesterday's objectives)
                        if (expired && elapsed > 0)
                        {
                            //compare current sum to previous
                            //if previous isn't availabe, assume it's invalid
                            var k = GetGroupKey(data.Type, a);

                            if (k != 0)
                            {
                                var sum = GetHistory(data.Type, k);

                                if (sum != 0)
                                {
                                    var objectives = data.Objectives;

                                    for (var i = 0; i < objectives.Length; i++)
                                    {
                                        sum -= objectives[i].ID;
                                    }

                                    expired = sum == 0;
                                }
                            }
                        }

                        if (!expired)
                        {
                            Add(a, data, e.LastModifiedInLocalTime);
                        }

                        //((Vault)e.Data.Value).Objectives

                        for (var i = e.Requests.Count - 1; i >= 0; --i)
                        {
                            if (e.Requests[i] is ApiRequest)
                            {
                                var r = (ApiRequest)e.Requests[i];

                                if (expired || r.EnsureLatest && e.LastModifiedInLocalTime < r.Date)
                                {
                                    if (r.RepeatCount == 0 || e.Status == ApiData.DataStatus.Changed && r.RepeatCount == 1)
                                    {
                                        e.Repeat(r);
                                    }
                                }

                                break;
                            }
                        }
                    }

                    break;
            }
        }
    }
}
