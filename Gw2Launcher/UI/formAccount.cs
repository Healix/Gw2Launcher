using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Gw2Launcher.UI.Controls;
using Gw2Launcher.Windows.Native;

namespace Gw2Launcher.UI
{
    public partial class formAccount : Form
    {
        private enum ArgsState : byte
        {
            Changed,
            Loading,
            Loaded,
            Active
        }

        private class ApiRequirements
        {
            public enum ApiState
            {
                None,
                OK,
                NoPermission,
                NotEligible
            }

            public ApiRequirements(Control parent, Api.TokenInfo.Permissions[] required, Func<ApiKeyData, ApiState> onVerify)
            {
                var x = parent.Right + 2;

                var l = new Label()
                {
                    ForeColor = SystemColors.GrayText,
                    AutoSize = true,
                    AutoEllipsis = true,
                    Location = new Point(x, parent.Top + 1),
                    MaximumSize = new Size(parent.Parent.Width - x, 0),
                    Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                };

                parent.Parent.Controls.Add(l);

                this.required = required;
                this.label = l;
                this.onVerify = onVerify;

                this.State = ApiState.OK;
            }

            public Api.TokenInfo.Permissions[] required;
            public Label label;
            public Func<ApiKeyData, ApiState> onVerify;

            public void Verify(ApiKeyData data)
            {
                if (data == null)
                {
                    this.State = ApiState.OK;
                    return;
                }

                var result = ApiState.OK;

                if (!data.useAccountData)
                {
                    if (required == null)
                    {
                        result = ApiState.NoPermission;
                    }
                    else
                    {
                        foreach (var p in required)
                        {
                            if (!data.permissions.Contains(p))
                            {
                                result = ApiState.NoPermission;
                                break;
                            }
                        }
                    }
                }

                if (result == ApiState.OK && onVerify != null)
                    result = onVerify(data);

                this.State = result;
            }

            private ApiState state;
            public ApiState State
            {
                get
                {
                    return state;
                }
                set
                {
                    if (state != value)
                    {
                        state = value;

                        switch (state)
                        {
                            case ApiState.OK:

                                label.Text = GetDefaultText();
                                label.ForeColor = SystemColors.GrayText;

                                break;
                            case ApiState.NoPermission:

                                label.Text = "(no permission)";
                                label.ForeColor = Color.Maroon;

                                break;
                            case ApiState.NotEligible:

                                label.Text = "(not eligible)";
                                label.ForeColor = Color.Maroon;

                                break;
                            default:

                                label.Text = "";

                                break;
                        }
                    }
                }
            }

            public string GetDefaultText()
            {
                if (required.Length > 0)
                {
                    var sb = new StringBuilder(required.Length * 10);
                    sb.Append("api: ");
                    foreach (var p in required)
                    {
                        sb.Append(p.ToString().ToLower());
                        sb.Append(", ");
                    }
                    sb.Length -= 2;
                    return sb.ToString();
                }

                return string.Empty;
            }
        }

        private class ApiKeyData
        {
            public ApiKeyData()
            {
            }

            public bool useAccountData;

            public Api.Account account;
            public Api.TokenInfo.Permissions[] permissionsArray;
            public HashSet<Api.TokenInfo.Permissions> permissions;
        }

        private class IconValue
        {
            public IconValue(string path, Image image)
            {
                this.Path = path;
                this.Type = Settings.IconType.File;
                this.Image = image;
            }

            public IconValue(Settings.IconType type, Image image)
            {
                this.Type = type;
                this.Image = image;
            }

            public Settings.IconType Type
            {
                get;
                set;
            }

            public string Path
            {
                get;
                set;
            }

            public Image Image
            {
                get;
                set;
            }
        }

        private event EventHandler AuthenticatorKeyChanged;

        private Settings.IAccount account;
        private formBrowseLocalDat.SelectedFile selectedDatFile, selectedGfxFile;
        private Settings.IFile datFile, gfxFile;
        private CheckBox[] checkArgs;
        private ArgsState argsState;
        private Tools.Gfx.Xml xmlGfx;
        private Util.ReusableControls reuseableXml;
        private Dictionary<string, ApiKeyData> apikeys;
        private Task taskTotp;
        private bool totpChanged;
        private Rectangle boundsEmail, boundsPassword;
        private CheckBox[] checkProcessorAffinity;
        private AutoScrollContainerPanel containerGeneral, containerApi, containerLaunchOptions, containerLaunchOptionsAdvanced,
            containerLocalDat, containerGraphics, containerSecurity, containerStatistics, containerLaunchOptionsProcess;
        private ApiRequirements[] apiRequirements;
        private ApiKeyData kdCurrent;

        public formAccount()
            : this(false)
        {
        }

        public formAccount(bool hasData)
        {
            InitializeComponent();

            this.MaximumSize = new Size(this.MinimumSize.Width, int.MaxValue);

            var sbounds = Settings.WindowBounds[typeof(formSettings)];
            if (sbounds.HasValue)
                this.Size = sbounds.Value.Size;

            apikeys = new Dictionary<string, ApiKeyData>(StringComparer.OrdinalIgnoreCase);

            progressTotpTime.BackColor = Util.Color.Darken(this.BackColor, 0.05f);
            progressTotpTime.ForeColor = Util.Color.Darken(this.BackColor, 0.15f);
            progressTotpTime.Value = 0;

            textAutoLoginEmail.Visible = textAutoLoginPassword.Visible = false;

            boundsEmail = textEmail.Bounds;
            boundsPassword = textPassword.Bounds;

            labelCreatedDate.Text = "n/a";
            labelLastLaunch.Text = "never";
            labelLaunchCount.Text = "0";
            labelAccountId.Text = "n/a";

            Rectangle r = new Rectangle();
            Rectangle bounds = Screen.PrimaryScreen.Bounds;

            r.Width = bounds.Width / 3;
            r.Height = bounds.Height / 3;
            r.X = bounds.X + bounds.Width / 2 - r.Width / 2;
            r.Y = bounds.Y + bounds.Height / 2 - r.Height / 2;

            textWindowed.Text = ToString(r);

            if (!Client.FileManager.IsDataLinkingSupported)
            {
                checkScreenshotsLocation.Enabled = false;
                buttonScreenshotsLocation.Enabled = false;
            }

            checkArgs = formSettings.InitializeArguments(panelLaunchOptionsAdvanced, labelArgsTemplateHeader, checkArgsTemplate, labelArgsTemplateSwitch, labelArgsTemplateDesc, checkArgs_CheckedChanged);

            apiRequirements = new ApiRequirements[]
            {
                new ApiRequirements(checkTrackDailyCompletionApi,
                    new Api.TokenInfo.Permissions[]
                    {
                        Api.TokenInfo.Permissions.Account,
                        Api.TokenInfo.Permissions.Progression
                    }, 
                    delegate(ApiKeyData kd)
                    {
                        int totalAp = -1;
                        if (kd.useAccountData)
                        {
                            if (account.ApiData != null && account.ApiData.DailyPoints != null)
                                totalAp = account.ApiData.DailyPoints.Value;
                        }
                        else
                            totalAp = kd.account.DailyAP + kd.account.MonthlyAP;
                        if (totalAp >= Api.Account.MAX_AP)
                            return ApiRequirements.ApiState.NotEligible;
                        return ApiRequirements.ApiState.OK;
                    }),
                new ApiRequirements(checkTrackPlayedApi,
                    new Api.TokenInfo.Permissions[]
                    {
                        Api.TokenInfo.Permissions.Account,
                    }, 
                    null),
            };

            LinkCheckBox(checkShowDailyCompletion, checkTrackDailyCompletionApi);
            LinkCheckBox(checkShowDailyLogin, checkTrackPlayedApi);

            containerGeneral = CreateContainer(panelGeneral);
            containerApi = CreateContainer(panelApi);
            containerLaunchOptions = CreateContainer(panelLaunchOptions);
            containerLaunchOptionsAdvanced = CreateContainer(panelLaunchOptionsAdvanced);
            containerLocalDat = CreateContainer(panelLocalDat);
            containerGraphics = CreateContainer(panelGraphics);
            containerSecurity = CreateContainer(panelSecurity);
            containerStatistics = CreateContainer(panelStatistics);
            containerLaunchOptionsProcess = CreateContainer(panelLaunchOptionsProcess);

            buttonGeneral.Panels = new Panel[] { containerGeneral, containerApi };
            buttonGeneral.SubItems = new string[] { "API usage" };

            buttonLaunchOptions.Panels = new Panel[] { containerLaunchOptions, containerLaunchOptionsAdvanced, containerLaunchOptionsProcess };
            buttonLaunchOptions.SubItems = new string[] { "Advanced", "Processor" };

            buttonLocalDat.Panels = new Panel[] { containerLocalDat, containerGraphics };
            buttonLocalDat.SubItems = new string[] { "Graphics" };

            buttonSecurity.Panels = new Panel[] { containerSecurity };

            buttonStatistics.Panels = new Panel[] { containerStatistics };

            sidebarPanel1.Initialize(new SidebarButton[]
                {
                    buttonGeneral,
                    buttonLaunchOptions,
                    buttonLocalDat,
                    buttonSecurity,
                    buttonStatistics
                });

            buttonGeneral.Selected = true;

            argsState = ArgsState.Changed;

            containerGraphics.PreVisiblePropertyChanged += containerGraphics_PreVisiblePropertyChanged;
            containerSecurity.PreVisiblePropertyChanged += containerSecurity_PreVisiblePropertyChanged;
            containerLaunchOptions.PreVisiblePropertyChanged += containerLaunchOptions_PreVisiblePropertyChanged;
            containerLaunchOptionsAdvanced.PreVisiblePropertyChanged += containerLaunchOptionsAdvanced_PreVisiblePropertyChanged;

            comboProcessPriority.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.High, "High"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.AboveNormal, "Above normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Normal, "Normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.BelowNormal, "Below normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Low, "Low"),
                });

            Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriorityClass.Normal);

            checkProcessorAffinity = formSettings.InitializeProcessorAffinity(panelProcessAffinity, checkProcessAffinityAll, label76);
            checkProcessAffinityAll.Checked = true;

            if (!hasData)
            {
                panelIdentifierColor.BackColor = Util.Color.FromUID(Settings.GetNextUID());
                SetIcon(CreateIcon(Settings.IconType.None));
            }

            labelAutologin_SizeChanged(null, null);
        }

        public formAccount(Settings.IAccount account)
            : this(true)
        {
            this.account = account;

            if (account.TotalUses == 1)
            {
                labelLaunchCount.Text = "once";
                labelLaunchCountEnd.Visible = false;
            }
            else
                labelLaunchCount.Text = account.TotalUses.ToString();

            if (account.LastUsedUtc != DateTime.MinValue)
            {
                var local = account.LastUsedUtc.ToLocalTime();
                labelLastLaunch.Text = local.ToLongDateString() + " " + local.ToLongTimeString();
            }

            if (account.CreatedUtc != DateTime.MinValue)
            {
                var local = account.CreatedUtc.ToLocalTime();
                labelCreatedDate.Text = local.ToLongDateString() + " " + local.ToLongTimeString();
            }

            labelAccountId.Text = account.UID.ToString();

            labelExportRecordedLaunch.Visible = File.Exists(Path.Combine(DataPath.AppData, "statistics.dat"));
            checkRecordLaunch.Checked = account.RecordLaunches;

            checkAutomaticLogin.Checked = account.AutomaticLogin;
            checkAutomaticLoginPlay.Checked = account.AutomaticPlay;

            if (!string.IsNullOrEmpty(account.Email))
                textEmail.Text = account.Email;
            if (account.Password != null)
                textPassword.Password = account.Password;

            textAccountName.Text = account.Name;
            textWindowsAccount.Text = account.WindowsAccount;
            textArguments.Text = account.Arguments;

            checkWindowed.Checked = account.Windowed;
            if (!account.Windowed && account.WindowBounds.IsEmpty)
            {
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                Rectangle r = new Rectangle();
                r.Width = bounds.Width / 3;
                r.Height = bounds.Height / 3;
                r.X = bounds.X + bounds.Width / 2 - r.Width / 2;
                r.Y = bounds.Y + bounds.Height / 2 - r.Height / 2;
                textWindowed.Text = ToString(r);
            }
            else
            {
                textWindowed.Text = ToString(account.WindowBounds);

                checkWindowedPreventChanges.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.PreventChanges);
                checkWindowedUpdateOnChange.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.RememberChanges);
                checkWindowedTopMost.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.TopMost);
            }

            checkShowDailyLogin.Checked = account.ShowDailyLogin;

            if (account.DatFile != null)
            {
                textLocalDat.Text = account.DatFile.Path;
                textLocalDat.Select(textLocalDat.TextLength, 0);
            }
            else
                textLocalDat.Text = "";

            if (account.GfxFile != null)
            {
                textGfxSettings.Text = account.GfxFile.Path;
                textGfxSettings.Select(textGfxSettings.TextLength, 0);
            }
            else
                textGfxSettings.Text = "";

            if (account.VolumeEnabled)
            {
                checkVolume.Checked = true;
                sliderVolume.Value = account.Volume;
            }
            else
                sliderVolume.Value = 1f;

            if (!string.IsNullOrEmpty(account.RunAfterLaunching))
                textRunAfterLaunch.Text = account.RunAfterLaunching;

            checkAutomaticLauncherLogin.Checked = account.AutomaticRememberedLogin;

            checkMuteAll.Checked = account.Mute.HasFlag(Settings.MuteOptions.All);
            checkMuteMusic.Checked = account.Mute.HasFlag(Settings.MuteOptions.Music);
            checkMuteVoices.Checked = account.Mute.HasFlag(Settings.MuteOptions.Voices);

            checkPort80.Checked = account.ClientPort == 80;
            checkPort443.Checked = account.ClientPort == 443;
            checkScreenshotsBmp.Checked = account.ScreenshotsFormat == Settings.ScreenshotFormat.Bitmap;

            if (Client.FileManager.IsDataLinkingSupported)
            {
                if (checkScreenshotsLocation.Checked = account.ScreenshotsLocation != null)
                {
                    textScreenshotsLocation.Text = account.ScreenshotsLocation;
                    textScreenshotsLocation.Select(textScreenshotsLocation.TextLength, 0);
                }
            }

            checkShowDailyCompletion.Checked = account.ShowDailyCompletion;
            textApiKey.Text = account.ApiKey;

            if (account.ApiData != null)
            {
                if (!string.IsNullOrEmpty(account.ApiKey))
                {
                    var kd = new ApiKeyData();
                    kd.useAccountData = true;

                    kdCurrent = kd;
                    foreach (var ar in apiRequirements)
                    {
                        ar.Verify(kd);
                    }

                    apikeys[account.ApiKey] = kd;
                }

                if (account.ApiData.DailyPoints != null)
                    checkTrackDailyCompletionApi.Checked = true;

                if (account.ApiData.Played != null)
                    checkTrackPlayedApi.Checked = true;
            }
            checkEnableNetworkAuthorization.Checked = account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled;

            if (account.TotpKey != null)
            {
                try
                {
                    textAuthenticatorKey.Text = Tools.Totp.Encode(account.TotpKey);
                    textAuthenticatorKey.Tag = account.TotpKey;
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                }
            }

            if (account.ProcessPriority != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, account.ProcessPriority);
                checkProcessPriority.Checked = true;
            }

            if (account.ProcessAffinity != 0)
            {
                var bits = account.ProcessAffinity;
                var isSet = false;
                var count = checkProcessorAffinity.Length;
                if (count > 64)
                    count = 64;

                for (var i = 0; i < count; i++)
                {
                    if (checkProcessorAffinity[i].Checked = (bits & 1) == 1)
                        isSet = true;
                    bits >>= 1;
                }
                checkProcessAffinityAll.Checked = !isSet;
            }

            if (account.ColorKey != Color.Empty)
                panelIdentifierColor.BackColor = account.ColorKey;
            else
                panelIdentifierColor.BackColor = Util.Color.FromUID(account.UID);

            IconValue icon = null;
            switch (account.IconType)
            {
                case Settings.IconType.File:

                    try
                    {
                        if (!string.IsNullOrEmpty(account.Icon) && File.Exists(account.Icon))
                        {
                            var image = Image.FromFile(account.Icon);
                            icon = new IconValue(account.Icon, image);
                        }
                    }
                    catch { }

                    break;
                case Settings.IconType.ColorKey:
                case Settings.IconType.Gw2LauncherColorKey:

                    icon = CreateIcon(account.IconType);

                    break;
            }

            if (icon == null)
                icon = CreateIcon(Settings.IconType.None);

            SetIcon(icon);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (textAccountName.TextLength == 0)
                textAccountName.Focus();
        }

        private IconValue CreateIcon(Settings.IconType type)
        {
            switch (type)
            {
                case Settings.IconType.ColorKey:

                    try
                    {
                        using (var icons = Tools.Icons.From(panelIdentifierColor.BackColor, false))
                        {
                            return new IconValue(type, icons.Small.ToBitmap());
                        }
                    }
                    catch
                    {
                        return null;
                    }

                case Settings.IconType.Gw2LauncherColorKey:

                    try
                    {
                        using (var icons = Tools.Icons.From(panelIdentifierColor.BackColor, true))
                        {
                            return new IconValue(type, icons.Small.ToBitmap());
                        }
                    }
                    catch
                    {
                        return null;
                    }

                default:

                    try
                    {
                        using (var iconGw2 = Properties.Resources.Gw2)
                        {
                            return new IconValue(Settings.IconType.None, iconGw2.ToBitmap());
                        }
                    }
                    catch
                    {
                        return new IconValue(Settings.IconType.None, null);
                    }
            }
        }

        private void SetIcon(IconValue icon)
        {
            using (panelIdentifierIcon.BackgroundImage)
            {
                if (icon == null)
                    panelIdentifierIcon.BackgroundImage = null;
                else
                    panelIdentifierIcon.BackgroundImage = icon.Image;
                panelIdentifierIcon.Tag = icon;
                OnIconChanged();
            }
        }

        public Settings.IAccount Account
        {
            get
            {
                return this.account;
            }
        }

        private AutoScrollContainerPanel CreateContainer(Panel panel)
        {
            var container = new AutoScrollContainerPanel(panel);
            
            this.Controls.Add(container);

            return container;
        }

        void containerLaunchOptionsAdvanced_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e)
            {
                if (argsState == ArgsState.Changed)
                {
                    argsState = ArgsState.Loading;

                    foreach (var check in checkArgs)
                    {
                        var arg = check.Tag as string;
                        if (arg != null)
                            check.Checked = Util.Args.Contains(textArguments.Text, arg.Substring(1));
                    }

                    argsState = ArgsState.Active;
                }
            }
            else if (argsState == ArgsState.Active)
            {
                argsState = ArgsState.Loaded;
                textArguments.TextChanged += textArguments_TextChanged;
            }
        }

        void containerSecurity_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e)
            {
                textEmail.Bounds = boundsEmail;
                textPassword.Bounds = boundsPassword;
                textEmail.Enabled = textPassword.Enabled = true;

                panelSecurity.Controls.AddRange(new Control[]
                    {
                        textEmail,
                        textPassword
                    });

                if (textAuthenticatorKey.Tag != null)
                    taskTotp = RefreshTotp();
            }
        }

        void containerGraphics_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e && xmlGfx == null)
            {
                string path;
                if (selectedGfxFile != null)
                {
                    if (selectedGfxFile.File != null)
                        path = selectedGfxFile.File.Path;
                    else if (selectedGfxFile.Path != null)
                        path = selectedGfxFile.Path;
                    else
                        path = null;
                }
                else if (account != null && account.GfxFile != null)
                {
                    path = account.GfxFile.Path;
                }
                else
                {
                    path = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx);
                }

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    LoadGraphics(path);
                }
            }
        }

        void containerLaunchOptions_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e)
            {
                textEmail.Bounds = textAutoLoginEmail.Bounds;
                textPassword.Bounds = textAutoLoginPassword.Bounds;
                textEmail.Enabled = textPassword.Enabled = checkAutomaticLogin.Checked;

                panelLaunchOptions.Controls.AddRange(new Control[]
                    {
                        textEmail,
                        textPassword
                    });
            }
        }

        void textArguments_TextChanged(object sender, EventArgs e)
        {
            textArguments.TextChanged -= textArguments_TextChanged;
            argsState = ArgsState.Changed;
        }

        void checkArgs_CheckedChanged(object sender, EventArgs e)
        {
            if (argsState == ArgsState.Loading)
                return;

            var check = (CheckBox)sender;
            var arg = check.Tag as string;

            if (arg != null)
            {
                string _arg;
                if (check.Checked)
                    _arg = arg;
                else
                    _arg = string.Empty;
                textArguments.Text = Util.Args.AddOrReplace(textArguments.Text, arg.Substring(1), _arg);
            }
        }

        private void formAccount_Load(object sender, EventArgs e)
        {
            textAccountName.Focus();
        }

        private void checkAutomaticLogin_CheckedChanged(object sender, EventArgs e)
        {
            textEmail.Enabled = textPassword.Enabled = checkAutomaticLogin.Checked;
        }

        private void labelLaunchCount_SizeChanged(object sender, EventArgs e)
        {
            labelLaunchCountEnd.Location = new Point(labelLaunchCount.Location.X + labelLaunchCount.Width + 6, labelLaunchCountEnd.Location.Y);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (selectedDatFile != null)
            {
                selectedDatFile.Cancel();
                selectedDatFile = null;
            }

            if (selectedGfxFile != null)
            {
                selectedGfxFile.Cancel();
                selectedGfxFile = null;
            }
        }

        private void buttonUsername_Click(object sender, EventArgs e)
        {
            using (formWindowsAccount f = new formWindowsAccount(textWindowsAccount.Text))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                    textWindowsAccount.Text = f.AccountName;
            }
        }

        private Settings.IFile GetFile(Client.FileManager.FileType type)
        {
            formBrowseLocalDat.SelectedFile selectedFile;
            Settings.IFile file;
            string ext;

            switch (type)
            {
                case Client.FileManager.FileType.Dat:
                    selectedFile = selectedDatFile;
                    file = datFile;
                    ext = ".dat";
                    break;
                case Client.FileManager.FileType.Gfx:
                    selectedFile = selectedGfxFile;
                    file = gfxFile;
                    ext = ".xml";
                    break;
                default:
                    return null;
            }

            if (selectedFile == null)
            {
                if (account != null)
                {
                    switch (type)
                    {
                        case Client.FileManager.FileType.Dat:
                            return account.DatFile;
                        case Client.FileManager.FileType.Gfx:
                            return account.GfxFile;
                        default:
                            return null;
                    }
                }
                else
                    return null;
            }
            else if (selectedFile.File != null)
            {
                return selectedFile.File;
            }
            else if (selectedFile.Path == null)
            {
                return null;
            }

            #region No longer using other user folders

            //Security.Impersonation.IImpersonationToken impersonation = null;

            //string username = Util.Users.GetUserName(textWindowsAccount.Text);
            //bool isCurrent = Util.Users.IsCurrentUser(username);
            //bool userExists = isCurrent;
            //bool wasMoved = false;

            //string path = null;

            //if (!isCurrent)
            //{
            //    try
            //    {
            //        using (var user = Util.Users.GetPrincipal(username))
            //        {
            //            userExists = user != null;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.Logging.Log(ex);
            //    }

            //    if (userExists)
            //    {
            //        var password = Security.Credentials.GetPassword(username);

            //        while (true)
            //        {
            //            if (password == null)
            //            {
            //                using (formPassword f = new formPassword("Password for " + username))
            //                {
            //                    if (f.ShowDialog(this) == DialogResult.OK)
            //                    {
            //                        password = f.Password;
            //                        Security.Credentials.SetPassword(username, password);
            //                    }
            //                    else
            //                    {
            //                        break;
            //                    }
            //                }
            //            }

            //            try
            //            {
            //                impersonation = Security.Impersonation.Impersonate(username, Security.Credentials.GetPassword(username));
            //                break;
            //            }
            //            catch (Win32Exception ex)
            //            {
            //                Util.Logging.Log(ex);

            //                if (ex.NativeErrorCode == 1326)
            //                {
            //                    password = null;
            //                    continue;
            //                }

            //                break;
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.Logging.Log(ex);
            //                break;
            //            }
            //        }
            //    }
            //}

            //if (isCurrent || impersonation != null)
            //{
            //    try
            //    {
            //        string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            //        if (!string.IsNullOrWhiteSpace(folder))
            //        {
            //            folder = Path.Combine(folder, "Guild Wars 2");
            //            if (!Directory.Exists(folder))
            //                Directory.CreateDirectory(folder);

            //            path = Util.FileUtil.GetTemporaryFileName(folder);

            //            try
            //            {
            //                File.Move(selectedDatFile.Path, path);
            //                wasMoved = true;
            //            }
            //            catch (Exception ex)
            //            {
            //                Util.Logging.Log(ex);

            //                try
            //                {
            //                    File.Copy(selectedDatFile.Path, path, true);
            //                }
            //                catch (Exception e)
            //                {
            //                    Util.Logging.Log(e);
            //                    throw;
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //the user exists, but the account hasn't been set up yet
            //        }
            //    }
            //    finally
            //    {
            //        if (impersonation != null)
            //            impersonation.Dispose();
            //    }
            //}

            //string folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create);
            //if (!string.IsNullOrWhiteSpace(folder))
            //{
            //    folder = Path.Combine(folder, "Guild Wars 2");
            //    if (!Directory.Exists(folder))
            //        Directory.CreateDirectory(folder);

            //    path = Util.FileUtil.GetTemporaryFileName(folder);

            //    try
            //    {
            //        File.Move(selectedDatFile.Path, path);
            //        wasMoved = true;
            //    }
            //    catch (Exception ex)
            //    {
            //        Util.Logging.Log(ex);

            //        try
            //        {
            //            File.Copy(selectedDatFile.Path, path, true);
            //        }
            //        catch (Exception e)
            //        {
            //            Util.Logging.Log(e);
            //            throw;
            //        }
            //    }
            //}
            //else
            //{
            //    //the user exists, but the account hasn't been set up yet
            //}

            #endregion

            if (file == null)
            {
                switch (type)
                {
                    case Client.FileManager.FileType.Dat:
                        file = datFile = Settings.CreateDatFile();
                        break;
                    case Client.FileManager.FileType.Gfx:
                        file = gfxFile = Settings.CreateGfxFile();
                        break;
                }
            }

            var wasMoved = false;
            var path = Path.Combine(DataPath.AppDataAccountData, file.UID + ext);

            try
            {
                File.Move(selectedFile.Path, path);
                wasMoved = true;
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);

                try
                {
                    File.Copy(selectedFile.Path, path, true);
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);
                    throw;
                }
            }

            file.Path = path;

            if (!wasMoved)
            {
                try
                {
                    File.Delete(selectedFile.Path);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            return file;
        }

        private void LoadGraphics(Tools.Gfx.Xml xmlGfx)
        {
            var names = new Tools.Gfx.Xml.DisplayName();
            var y = 0;
            var avgWidth = 0;
            var avgCount = 0;
            int countCheck = 0,
                countText = 0,
                countSlider = 0,
                countLabel = 2,
                countCombo = 0;

            var options = new Dictionary<string, Tools.Gfx.Xml.Option>();

            foreach (var o in xmlGfx.Options)
            {
                if (o.Type == Tools.Gfx.Xml.Option.OptionType.Resolution)
                {
                    continue;
                }
                else
                {
                    string name;
                    options[name = ((Tools.Gfx.Xml.Option.OptionValue)o).Name] = o;

                    avgWidth += TextRenderer.MeasureText(names.GetName(name), checkGfxTemplate.Font).Width;
                    avgCount++;
                    
                    switch (o.Type)
                    {
                        case Tools.Gfx.Xml.Option.OptionType.Boolean:
                            
                            countCheck++;

                            break;
                        case Tools.Gfx.Xml.Option.OptionType.Enum:

                            countLabel++;
                            countCombo++;

                            break;
                        case Tools.Gfx.Xml.Option.OptionType.Float:

                            countLabel++;
                            countText++;
                            countSlider++;

                            break;
                        default:

                            Util.Logging.Log("Unknown GFXSettings type for " + name);

                            break;
                    }
                }
            }

            if (reuseableXml == null)
                reuseableXml = new Util.ReusableControls();
            else
                reuseableXml.ReleaseAll();

            var labels = reuseableXml.Create<Label>(countLabel,
                delegate
                {
                    return new Label()
                    {
                        AutoSize = true,
                    };
                });

            var checks = reuseableXml.Create<CheckBox>(countCheck,
                delegate
                {
                    var c = new CheckBox()
                    {
                        Font = checkGfxTemplate.Font,
                        AutoSize = true,
                    };
                    c.CheckedChanged += checkGfx_CheckedChanged;
                    return c;
                });

            var combos = reuseableXml.Create<ComboBox>(countCombo,
                delegate
                {
                    var c = new ComboBox()
                    {
                        Font = comboGfxTemplate.Font,
                        DropDownStyle = comboGfxTemplate.DropDownStyle
                    };
                    c.SelectedValueChanged += comboGfx_SelectedValueChanged;
                    return c;
                });

            var texts = reuseableXml.Create<TextBox>(countText,
                delegate
                {
                    var c = new TextBox()
                    {
                        Font = textSliderGfxTemplate.Font,
                        Multiline=false,
                    };
                    c.TextChanged += textGfx_TextChanged;
                    return c;
                });

            var sliders = reuseableXml.Create<FlatSlider>(countSlider,
                delegate
                {
                    var c = new FlatSlider()
                    {
                        Font = comboGfxTemplate.Font,
                        Size = sliderGfxTemplate.Size,
                    };
                    c.ValueChanged += sliderGfx_ValueChanged;
                    return c;
                });

            if (avgCount > 0)
                avgWidth /= avgCount;

            Action<Tools.Gfx.Xml.Option> createOption = delegate(Tools.Gfx.Xml.Option o)
            {
                Label l;

                switch (o.Type)
                {
                    case Tools.Gfx.Xml.Option.OptionType.Boolean:

                        var obool = (Tools.Gfx.Xml.Option.Boolean)o;
                        var checkbox = checks.GetNext();

                        checkbox.Checked = obool.Value;
                        checkbox.Text = names.GetName(obool.Name);
                        checkbox.Location = new Point(checkGfxTemplate.Left, y);
                        checkbox.Tag = obool;

                        y += checkGfxTemplate.Height + 10;

                        break;
                    case Tools.Gfx.Xml.Option.OptionType.Enum:

                        var oenum = (Tools.Gfx.Xml.Option.Enum)o;
                        if (oenum.EnumValues.Count == 0)
                            break;

                        l = labels.GetNext();

                        l.Font = labelGfxTemplate.Font;
                        l.Text = names.GetName(oenum.Name);
                        l.Location = new Point(labelGfxTemplate.Left, y);

                        var labelSize = l.GetPreferredSize(Size.Empty);
                        var comboWidth = textSliderGfxTemplate.Right - labelGfxTemplate.Left - 20 - avgWidth;

                        if (labelGfxTemplate.Left + labelSize.Width + comboWidth + 10 > textSliderGfxTemplate.Right)
                            comboWidth = textSliderGfxTemplate.Right - labelGfxTemplate.Left - labelSize.Width - 10;

                        var combo = combos.GetNext();

                        combo.Items.Clear();
                        combo.Location = new Point(textSliderGfxTemplate.Right - comboWidth, y - 3);
                        combo.Width = comboWidth;
                        combo.Tag = oenum;

                        foreach (var v in oenum.EnumValues)
                        {
                            var text = names.GetName(v);
                            combo.Items.Add(text);
                        }

                        combo.SelectedItem = names.GetName(oenum.Value);

                        y += comboGfxTemplate.Height + 10;

                        break;
                    case Tools.Gfx.Xml.Option.OptionType.Float:

                        var ofloat = (Tools.Gfx.Xml.Option.Float)o;

                        l = labels.GetNext();
                        l.Font = labelGfxTemplate.Font;
                        l.Text = names.GetName(ofloat.Name);
                        l.Location = new Point(labelGfxTemplate.Left, y);

                        y += labelGfxTemplate.Height + 5;

                        var sliderText = texts.GetNext();

                        sliderText.Size = textSliderGfxTemplate.Size;
                        sliderText.Text = string.Format("{0:0.##}", ofloat.Value);
                        sliderText.Location = new Point(textSliderGfxTemplate.Left, y + textSliderGfxTemplate.Top - sliderGfxTemplate.Top);

                        var slider = sliders.GetNext();

                        slider.Location = new Point(sliderGfxTemplate.Left, y);
                        slider.Tag = new object[] { ofloat, sliderText };

                        sliderText.Tag = new object[] { ofloat, slider };
                        slider.Value = (ofloat.Value - ofloat.MinValue) / (ofloat.MaxValue - ofloat.MinValue);

                        y += sliderGfxTemplate.Height + 10;

                        break;
                }
            };


            foreach (var name in new string[] { "screenMode", "frameLimit", "dpiScaling", "gamma" })
            {
                Tools.Gfx.Xml.Option o;
                if (options.TryGetValue(name, out o))
                {
                    options.Remove(name);

                    if (y == 0)
                    {
                        y = 10;

                        var l = labels.GetNext();
                        l.Font = labelGfxTemplateHeader.Font;
                        l.Location = new Point(labelGfxTemplateHeader.Left, y);
                        l.Text = "Display";

                        y += labelGfxTemplateHeader.Height + 20;
                    }

                    createOption(o);
                    //y = AddGfxOption(o, names, controls, y, avgWidth);
                }
            }

            if (y > 0)
            {
                y += 10;

                var l = labels.GetNext();
                l.Font = labelGfxTemplateHeader.Font;
                l.Location = new Point(labelGfxTemplateHeader.Left, y);
                l.Text = "Advanced";

                y += labelGfxTemplate.Height + 20;

                labelGfxPreset.Location = new Point(textSliderGfxTemplate.Right - labelGfxPreset.Width, l.Top);
                labelGfxPreset.Visible = true;
            }
            else
                labelGfxPreset.Visible = false;

            foreach (var name in new string[] { "animation", "antiAliasing", "environment", "lodDistance", "reflections", "textureDetail", "sampling", "shadows", "shaders", "postProc", "charModelLimit", "charModelQuality" })
            {
                Tools.Gfx.Xml.Option o;
                if (options.TryGetValue(name, out o))
                {
                    options.Remove(name);

                    createOption(o);
                    //y = AddGfxOption(o, names, controls, y, avgWidth);
                }
            }

            var keys = options.Keys.ToArray();
            Array.Sort(keys);

            foreach (var k in keys)
            {
                createOption(options[k]);
                //y = AddGfxOption(options[k], names, controls, y, avgWidth);
            }

            foreach (Util.ReusableControls.IResult r in new Util.ReusableControls.IResult[] { combos, checks, labels, sliders, texts })
            {
                foreach (var c in r)
                {
                    c.Visible = false;
                    c.Tag = null;
                }
                if (r.New != null)
                    panelGraphicsXml.Controls.AddRange(r.New);
            }

            if (y > 10)
            {
                panelGraphicsXml.Height = y - 10;
                panelGraphicsXml.Visible = true;
            }
        }

        private async void LoadGraphics(string path)
        {
            if (xmlGfx != null && xmlGfx.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                return;

            panelGraphicsXml.Visible = false;
            labelGfxSettingsNotAvailable.Visible = false;

            labelGfxFileInfo.Text = "...";
            labelGfxFileInfo.Visible = true;

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                xmlGfx = null;
            }
            else
            {
                if (!containerGraphics.Visible)
                    await Task.Delay(100);
                xmlGfx = await Task.Run<Tools.Gfx.Xml>(
                    delegate
                    {
                        try
                        {
                            return Tools.Gfx.Xml.Load(path);
                        }
                        catch (Exception e)
                        {
                            Util.Logging.Log(e);
                        }
                        return null;
                    });
            }

            labelGfxFileInfo.Visible = false;

            if (xmlGfx == null)
            {
                labelGfxSettingsNotAvailable.Visible = true;
            }
            else
            {
                try
                {
                    LoadGraphics(xmlGfx);

                    if (panelGraphicsXml.Visible)
                    {
                        bool isShared = false,
                             isDefault = path.Equals(Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx), StringComparison.OrdinalIgnoreCase);

                        if (selectedGfxFile != null)
                        {
                            isShared = selectedGfxFile.File != null && selectedGfxFile.File.References > 0;    
                        }
                        else if (account != null && account.GfxFile != null)
                        {
                            isShared = account.GfxFile.References > 1;
                        }

                        if (isDefault)
                        {
                            labelGfxFileInfo.Text = "Currently modifying the default file used by Guild Wars 2";
                            labelGfxFileInfo.Visible = true;
                        }
                        else if (isShared)
                        {
                            labelGfxFileInfo.Text = "These settings are shared with other accounts";
                            labelGfxFileInfo.Visible = true;
                        }

                        if (labelGfxFileInfo.Visible)
                            panelGraphicsXml.Top = labelGfxFileInfo.Bottom + 10;
                        else
                            panelGraphicsXml.Top = labelGfxSettingsTitleSub.Bottom + 10;
                    }
                }
                catch (Exception e)
                {
                    Util.Logging.Log(e);

                    panelGraphicsXml.Visible = false;
                    labelGfxSettingsNotAvailable.Visible = true;
                }
            }
        }

        private enum GraphicsPreset
        {
            Lowest,
            Low,
            Medium,
            High,
            Highest,
            Ultra
        }

        private GraphicsPreset ParseGfxEnumValue(string value)
        {
            switch (value)
            {
                case "lowest":
                    return GraphicsPreset.Lowest;
                case "low":
                    return GraphicsPreset.Low;
                case "medium":
                    return GraphicsPreset.Medium;
                case "high":
                    return GraphicsPreset.High;
                case "highest":
                    return GraphicsPreset.Highest;
                case "ultra":
                    return GraphicsPreset.Ultra;
                case "subsample":
                    return GraphicsPreset.Low;
                case "native":
                    return GraphicsPreset.Medium;
                case "supersample":
                    return GraphicsPreset.Highest;
                case "none":
                    return GraphicsPreset.Lowest;
                case "fxaa":
                    return GraphicsPreset.Low;
                case "smaa_low":
                    return GraphicsPreset.Medium;
                case "smaa_high":
                    return GraphicsPreset.High;
                case "terrain":
                    return GraphicsPreset.Medium;
                case "all":
                    return GraphicsPreset.High;
            }

            return (GraphicsPreset)(-1);
        }

        private int FindGfxEnum(List<string> values, GraphicsPreset preset)
        {
            var match = preset.ToString().ToLower();
            var l = values.Count - 1;

            if (preset <= GraphicsPreset.Medium)
            {
                for (var i = 0; i <= l; i++)
                {
                    var v = ParseGfxEnumValue(values[i]);
                    if (preset < v && i > 0)
                        return i - 1;
                    else if (preset <= v)
                        return i;
                }
            }
            else
            {
                for (var i = l; i >= 0; i--)
                {
                    var v = ParseGfxEnumValue(values[i]);
                    if (preset > v && i < l)
                        return i + 1;
                    else if (preset >= v)
                        return i;
                }
            }

            return -1;
        }

        private void SetGraphicsPreset(GraphicsPreset preset)
        {
            foreach (Control c in panelGraphicsXml.Controls)
            {
                if (c.Tag == null || !(c.Tag is Tools.Gfx.Xml.Option.OptionValue))
                    continue;

                switch (((Tools.Gfx.Xml.Option.OptionValue)c.Tag).Type)
                {
                    case Tools.Gfx.Xml.Option.OptionType.Boolean:
                        
                        var obool = (Tools.Gfx.Xml.Option.Boolean)c.Tag;
                        var checkbox = (CheckBox)c;

                        switch (obool.Name)
                        {
                            case "effectLod":
                                checkbox.Checked = preset != GraphicsPreset.Ultra;
                                break;
                            case "depthBlur":
                            case "bestTextureFiltering":
                            case "highResCharacter":
                                checkbox.Checked = preset >= GraphicsPreset.High;
                                break;
                        }

                        break;
                    case Tools.Gfx.Xml.Option.OptionType.Enum:

                        var oenum = (Tools.Gfx.Xml.Option.Enum)c.Tag;
                        var combo = (ComboBox)c;

                        switch (oenum.Name)
                        {
                            case "shaders":
                            case "sampling":
                            case "postProc":
                            case "lodDistance":
                            case "environment":
                            case "charModelQuality":
                            case "animation":
                            case "textureDetail":
                            case "antiAliasing":
                            case "reflections":
                            case "charModelLimit":
                            case "shadows":

                                var i = FindGfxEnum(oenum.EnumValues, preset);
                                if (i != -1)
                                    combo.SelectedIndex = i;

                                break;
                        }

                        break;
                    case Tools.Gfx.Xml.Option.OptionType.Float:

                        var ofloat = (Tools.Gfx.Xml.Option.Float)c.Tag;

                        break;
                }
            }
        }

        void textGfx_TextChanged(object sender, EventArgs e)
        {
            if (!panelGraphicsXml.Visible)
                return;
            var o = (object[])((Control)sender).Tag;
            var ofloat = (Tools.Gfx.Xml.Option.Float)o[0];
            var slider = (UI.Controls.FlatSlider)o[1];
            var text = (TextBox)sender;

            if (text.ContainsFocus)
            {
                float value;
                if (float.TryParse(text.Text, out value))
                {
                    if (value < ofloat.MinValue)
                        value = ofloat.MinValue;
                    else if (value > ofloat.MaxValue)
                        value = ofloat.MaxValue;

                    ofloat.Value = value;
                    slider.Value = (value - ofloat.MinValue) / (ofloat.MaxValue - ofloat.MinValue);
                }
            }
        }

        void sliderGfx_ValueChanged(object sender, float e)
        {
            if (!panelGraphicsXml.Visible)
                return;
            var o = (object[])((Control)sender).Tag;
            var ofloat = (Tools.Gfx.Xml.Option.Float)o[0];
            var text = (TextBox)o[1];
            var slider = (UI.Controls.FlatSlider)sender;

            if (!text.ContainsFocus)
            {
                var value = ofloat.MinValue + e * (ofloat.MaxValue - ofloat.MinValue);

                text.Text = string.Format("{0:0.##}", value);
                ofloat.Value = value;
            }
        }

        void comboGfx_SelectedValueChanged(object sender, EventArgs e)
        {
            if (!panelGraphicsXml.Visible)
                return;
            var oenum = (Tools.Gfx.Xml.Option.Enum)((Control)sender).Tag;
            var combo = (ComboBox)sender;
            var i = combo.SelectedIndex;

            if (i >= 0)
                oenum.Value = oenum.EnumValues[i];
        }

        void checkGfx_CheckedChanged(object sender, EventArgs e)
        {
            if (!panelGraphicsXml.Visible)
                return;
            var obool = (Tools.Gfx.Xml.Option.Boolean)((Control)sender).Tag;
            var checkbox = (CheckBox)sender;

            obool.Value = checkbox.Checked;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textAccountName.Text.Length == 0)
            {
                buttonGeneral.Selected = true;
                textAccountName.Focus();
                MessageBox.Show(this, "An identifier / account or display name is required", "Identifier / name required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            Rectangle windowBounds;

            if (checkWindowed.Checked)
            {
                Rectangle r = ParseWindowSize(textWindowed.Text);
                windowBounds = FixSize(r);
            }
            else
            {
                if (account != null)
                    windowBounds = account.WindowBounds;
                else
                    windowBounds = Rectangle.Empty;
            }

            Settings.IDatFile datFile;

            try
            {
                datFile = (Settings.IDatFile)GetFile(Client.FileManager.FileType.Dat);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                MessageBox.Show(this, "An error occured while handling Local.dat.\n\n" + ex.Message, "Failed to handle Local.dat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Settings.IGfxFile gfxFile;

            try
            {
                gfxFile = (Settings.IGfxFile)GetFile(Client.FileManager.FileType.Gfx);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                MessageBox.Show(this, "An error occured while handling GFXSettings.xml.\n\n" + ex.Message, "Failed to handle GFXSettings.xml", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (xmlGfx != null && xmlGfx.IsModified)
            {
                //if the xml was modified, save it to a new location to prevent it from being overwritten
                // - this file will be moved to the proper location on the next launch

                var defaultPath = Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx);
                string path;
                if (gfxFile != null)
                    path = gfxFile.Path;
                else
                {
                    path = defaultPath;
                    gfxFile = (Settings.IGfxFile)Client.FileManager.FindFile(Client.FileManager.FileType.Gfx, defaultPath);
                }
                bool isDefault;

                if (isDefault = defaultPath.Equals(path))
                {
                    #region ask to overwrite default file? (disabled)

                    //bool copy = false;

                    //switch (MessageBox.Show(this, "You are about to modify the default GFXSettings.xml file used by Guild Wars 2.\n\nWould you like to make a copy instead?", "Make a copy instead?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation))
                    //{
                    //    case System.Windows.Forms.DialogResult.Yes:
                    //        copy = true;
                    //        break;
                    //    case System.Windows.Forms.DialogResult.No:
                    //        copy = false;
                    //        break;
                    //    case System.Windows.Forms.DialogResult.Cancel:
                    //        return;
                    //}

                    //if (copy)
                    //{
                    //    if (gfxFile == null || gfxFile.References == 0 || account != null && account.GfxFile == gfxFile && gfxFile.References == 1)
                    //    {
                    //        //this file is not being used
                    //    }
                    //    else
                    //    {
                    //        gfxFile = Settings.CreateGfxFile();
                    //    }

                    //    gfxFile.Path = path = Path.Combine(DataPath.AppDataAccountData, gfxFile.UID + ".xml");
                    //}

                    #endregion
                }

                var inUse = false;

                if (gfxFile != null && gfxFile.References > 0)
                {
                    foreach (var a in Client.Launcher.GetActiveProcesses())
                    {
                        if (a.GfxFile == gfxFile)
                        {
                            inUse = true;
                            break;
                        }
                    }
                }

                if (inUse && isDefault)
                {
                    while (inUse)
                    {
                        var r = MessageBox.Show(this, "The default GFXSettings.xml file is currently in use and cannot be modified", "File in use", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error, MessageBoxDefaultButton.Button2);
                        if (r == System.Windows.Forms.DialogResult.Ignore)
                        {
                            inUse = false;
                            break;
                        }
                        else if (r == System.Windows.Forms.DialogResult.Cancel)
                            return;

                        inUse = false;

                        foreach (var a in Client.Launcher.GetActiveProcesses())
                        {
                            if (a.GfxFile == gfxFile)
                            {
                                inUse = true;
                                break;
                            }
                        }
                    }
                }

                try
                {
                    if (inUse)
                    {
                        //will be temporarily saved as #.tmp.xml
                        var name = Path.GetFileNameWithoutExtension(path);
                        var ext = ".tmp";
                        if (!name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                            path = Path.Combine(Path.GetDirectoryName(path), name + ext + Path.GetExtension(path));
                        xmlGfx.SaveTo(path);

                        gfxFile.Path = path;
                    }
                    else
                    {
                        xmlGfx.SaveTo(path);
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }

            if (gfxFile == null || datFile == null)
            {
                bool gfx,dat;
                using (var f = new formBrowseLocalDatQuick(dat = datFile == null, gfx = gfxFile == null))
                {
                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        if (dat)
                            datFile = f.DatFile;
                        if (gfx)
                            gfxFile = f.GfxFile;
                    }
                    else
                    {
                        if (dat)
                            buttonLocalDat.SelectPanel(containerLocalDat);
                        else
                            buttonLocalDat.SelectPanel(containerGraphics);
                        return;
                    }
                }
            }

            selectedDatFile = null;
            selectedGfxFile = null;

            if (this.account == null)
            {
                this.account = Settings.CreateAccount();
                this.account.LastUsedUtc = DateTime.MinValue;
                this.account.CreatedUtc = DateTime.UtcNow;
            }

            if (checkWindowed.Checked)
            {
                var windowed = Settings.WindowOptions.Windowed;
                if (checkWindowedPreventChanges.Checked)
                    windowed |= Settings.WindowOptions.PreventChanges;
                if (checkWindowedUpdateOnChange.Checked)
                    windowed |= Settings.WindowOptions.RememberChanges;
                if (checkWindowedTopMost.Checked)
                    windowed |= Settings.WindowOptions.TopMost;
                this.account.WindowOptions = windowed;
            }
            else
            {
                this.account.WindowOptions = Settings.WindowOptions.None;
            }

            this.account.Name = textAccountName.Text;
            this.account.WindowsAccount = textWindowsAccount.Text;
            this.account.Arguments = textArguments.Text;
            this.account.DatFile = datFile;
            this.account.GfxFile = gfxFile;
            this.account.WindowBounds = windowBounds;
            this.account.RecordLaunches = checkRecordLaunch.Checked;

            try
            {
                if (!Util.Users.IsCurrentUser(textWindowsAccount.Text))
                    Util.FileUtil.AllowFileAccess(System.Reflection.Assembly.GetExecutingAssembly().Location, System.Security.AccessControl.FileSystemRights.Modify);
            }
            catch { }

            if (textEmail.TextLength > 0)
                this.account.Email = textEmail.Text;
            else
                this.account.Email = null;

            if (textEmail.TextLength > 0 && textPassword.TextLength > 0)
            {
                var pold = this.account.Password;
                var pnew = textPassword.Password;

                if (!Security.Credentials.Compare(pnew, pold))
                {
                    if (pold != null)
                        pold.Dispose();
                    this.account.Password = pnew;
                }
                else
                {
                    pnew.Dispose();
                }
            }
            else
            {
                var pold = this.account.Password;
                if (pold != null)
                    pold.Dispose();
                this.account.Password = null;
            }

            this.account.AutomaticLogin = checkAutomaticLogin.Checked && this.account.HasCredentials;
            this.account.AutomaticPlay = checkAutomaticLoginPlay.Checked;

            if (checkVolume.Checked)
                this.account.Volume = sliderVolume.Value;
            else
                this.account.VolumeEnabled = false;

            if (!string.IsNullOrEmpty(textRunAfterLaunch.Text))
                this.account.RunAfterLaunching = textRunAfterLaunch.Text;
            else
                this.account.RunAfterLaunching = null;

            this.account.AutomaticRememberedLogin = checkAutomaticLauncherLogin.Checked;

            var mute = Settings.MuteOptions.None;
            if (checkMuteAll.Checked)
                mute |= Settings.MuteOptions.All;
            if (checkMuteMusic.Checked)
                mute |= Settings.MuteOptions.Music;
            if (checkMuteVoices.Checked)
                mute |= Settings.MuteOptions.Voices;
            this.account.Mute = mute;

            if (checkPort80.Checked)
                this.account.ClientPort = 80;
            else if (checkPort443.Checked)
                this.account.ClientPort = 443;
            else
                this.account.ClientPort = 0;

            if (checkScreenshotsBmp.Checked)
                this.account.ScreenshotsFormat = Settings.ScreenshotFormat.Bitmap;
            else
                this.account.ScreenshotsFormat = Settings.ScreenshotFormat.Default;

            if (checkScreenshotsLocation.Checked && Directory.Exists(textScreenshotsLocation.Text))
            {
                var path = Util.FileUtil.GetTrimmedDirectoryPath(textScreenshotsLocation.Text);
                if (string.IsNullOrEmpty(path))
                    Settings.ScreenshotsLocation.Clear();
                else
                    Settings.ScreenshotsLocation.Value = path;
                this.account.ScreenshotsLocation = path;
            }
            else
                this.account.ScreenshotsLocation = null;

            var apiKeyChanged = !textApiKey.Text.Equals(account.ApiKey, StringComparison.OrdinalIgnoreCase);

            this.account.ShowDailyCompletion = checkShowDailyCompletion.Checked;

            if (checkShowDailyCompletion.Checked && checkTrackDailyCompletionApi.Checked && !string.IsNullOrEmpty(textApiKey.Text))
            {
                if (account.ApiData == null)
                    account.ApiData = account.CreateApiData();

                if (account.ApiData.DailyPoints == null)
                {
                    var points = account.ApiData.DailyPoints = account.ApiData.CreateValue<ushort>();
                    points.State = Settings.ApiCacheState.None;
                }
                else if (apiKeyChanged)
                    account.ApiData.DailyPoints.State = Settings.ApiCacheState.None;

                if (account.ApiData.DailyPoints.State == Settings.ApiCacheState.None && textApiKey.Tag != null)
                {
                    var kd = (ApiKeyData)textApiKey.Tag;
                    if (kd.account != null)
                    {
                        var total = kd.account.DailyAP + kd.account.MonthlyAP;
                        if (total < 0)
                            total = ushort.MaxValue;
                        var points = account.ApiData.DailyPoints;
                        points.Value = (ushort)total;
                        points.State = Settings.ApiCacheState.OK;
                        points.LastChange = account.LastUsedUtc;
                        account.LastDailyCompletionUtc = account.LastUsedUtc;
                    }
                }
            }
            else if (account.ApiData != null && account.ApiData.DailyPoints != null)
                account.ApiData.DailyPoints = null;

            this.account.ShowDailyLogin = checkShowDailyLogin.Checked;

            if (checkShowDailyLogin.Checked && checkTrackPlayedApi.Checked && !string.IsNullOrEmpty(textApiKey.Text))
            {
                if (account.ApiData == null)
                    account.ApiData = account.CreateApiData();

                if (account.ApiData.Played == null)
                {
                    var played = account.ApiData.Played = account.ApiData.CreateValue<int>();
                    played.State = Settings.ApiCacheState.None;
                }
                else if (apiKeyChanged)
                    account.ApiData.Played.State = Settings.ApiCacheState.None;

                if (account.ApiData.Played.State == Settings.ApiCacheState.None && textApiKey.Tag != null)
                {
                    var kd = (ApiKeyData)textApiKey.Tag;
                    if (kd.account != null)
                    {
                        var played = account.ApiData.Played;
                        played.Value = kd.account.Age;
                        played.State = Settings.ApiCacheState.Pending;
                        played.LastChange = account.LastUsedUtc;
                    }
                }
            }
            else if (account.ApiData != null && account.ApiData.Played != null)
                account.ApiData.Played = null;

            if (account.ApiData != null)
            {
                if (account.ApiData.Played == null && account.ApiData.DailyPoints == null)
                    account.ApiData = null;
            }

            account.ApiKey = textApiKey.Text;

            if (checkEnableNetworkAuthorization.Checked)
            {
                if (account.NetworkAuthorizationState == Settings.NetworkAuthorizationState.Disabled)
                    account.NetworkAuthorizationState = Settings.NetworkAuthorizationState.Unknown;
            }
            else if (account.NetworkAuthorizationState != Settings.NetworkAuthorizationState.Disabled)
                account.NetworkAuthorizationState = Settings.NetworkAuthorizationState.Disabled;

            if (totpChanged)
            {
                byte[] totp;
                if (Tools.Totp.IsValid(textAuthenticatorKey.Text))
                    totp = Tools.Totp.Decode(textAuthenticatorKey.Text);
                else
                    totp = null;
                account.TotpKey = totp;
            }

            if (checkProcessPriority.Checked && comboProcessPriority.SelectedIndex >= 0)
                account.ProcessPriority = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboProcessPriority);
            else
                account.ProcessPriority = Settings.ProcessPriorityClass.None;

            if (!checkProcessAffinityAll.Checked)
            {
                long bits = 0;
                var count = checkProcessorAffinity.Length;
                if (count > 64)
                    count = 64;
                for (int i = 0; i < count; i++)
                {
                    if (checkProcessorAffinity[i].Checked)
                        bits |= ((long)1 << i);
                }

                account.ProcessAffinity = bits;
            }
            else
                account.ProcessAffinity = 0;

            if (panelIdentifierIcon.Tag is IconValue)
            {
                var icon = (IconValue)panelIdentifierIcon.Tag;
                account.Icon = icon.Path;
                account.IconType = icon.Type;
            }
            else
            {
                account.Icon = null;
                account.IconType = Settings.IconType.None;
            }

            if (panelIdentifierColor.Tag != null)
            {
                var c = (Color)panelIdentifierColor.Tag;
                try
                {
                    if (c == Util.Color.FromUID(account.UID))
                        c=Color.Empty;
                }
                catch { }
                account.ColorKey = c;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkWindowed_CheckedChanged(object sender, EventArgs e)
        {
            var b = checkWindowed.Checked;
            textWindowed.Enabled = b;
            buttonWindowed.Enabled = b;
            checkWindowedPreventChanges.Enabled = b;
            checkWindowedUpdateOnChange.Enabled = b;
            checkWindowedTopMost.Enabled = b;
        }

        private string ToString(Rectangle r)
        {
            return string.Format("{0}x, {1}y, {2}w, {3}h", r.X, r.Y, r.Width, r.Height);
        }

        private Rectangle ParseWindowSize(string s)
        {
            try
            {
                Rectangle r = new Rectangle();
                char current = char.MinValue;
                int last = 0;

                for (int i = s.Length - 1; i >= 0; i--)
                {
                    char c = s[i];

                    if (c == ',' || i == 0)
                    {
                        if (current != char.MinValue)
                        {
                            int n = 0;
                            int j = i > 0 ? i + 1 : i;

                            if (last > j && Int32.TryParse(s.Substring(j, last - j), out n))
                            {
                                switch (current)
                                {
                                    case 'w':
                                        r.Width = n;
                                        break;
                                    case 'h':
                                        r.Height = n;
                                        break;
                                    case 'x':
                                        r.X = n;
                                        break;
                                    case 'y':
                                        r.Y = n;
                                        break;
                                }
                            }
                        }

                        current = char.MinValue;
                    }
                    else if (char.IsNumber(c) || c == '-' || c == '.')
                    {

                    }
                    else
                    {
                        switch (c)
                        {
                            case 'w':
                            case 'h':
                            case 'x':
                            case 'y':
                                current = s[i];
                                last = i;
                                break;
                        }
                    }
                }

                return r;
            }
            catch (Exception e)
            {
                Util.Logging.Log(e);
                return Rectangle.Empty;
            }
        }

        private Rectangle FixSize(Rectangle r)
        {
            if (r.Width < 50)
                r.Width = 50;
            if (r.Height < 50)
                r.Height = 50;

            return r;
        }

        private void buttonWindowed_Click(object sender, EventArgs e)
        {
            using (formWindowSize f = new formWindowSize(true, account, textAccountName.Text))
            {
                Rectangle r = FixSize(ParseWindowSize(textWindowed.Text));

                f.SetBounds(r.X, r.Y, r.Width, r.Height);
                if (f.ShowDialog(this) == DialogResult.OK)
                    textWindowed.Text = ToString(f.Bounds);

                this.Focus();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void buttonBrowseLocalDat_Click(object sender, EventArgs e)
        {
            using (formBrowseLocalDat f = new formBrowseLocalDat(Client.FileManager.FileType.Dat, account))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (selectedDatFile != null)
                        selectedDatFile.Cancel();

                    selectedDatFile = f.Result;
                    if (selectedDatFile == null)
                        textLocalDat.Text = "";
                    else if (selectedDatFile.File != null)
                        textLocalDat.Text = selectedDatFile.File.Path;
                    else
                        textLocalDat.Text = selectedDatFile.Path;
                    textLocalDat.Select(textLocalDat.TextLength, 0);
                }
            }
        }

        private async void labelExportRecordedLaunch_Click(object sender, EventArgs e)
        {
            labelExportRecordedLaunch.Enabled = false;
            Color c = labelExportRecordedLaunch.ForeColor;
            labelExportRecordedLaunch.ForeColor = SystemColors.GrayText;

            string path = await Task.Factory.StartNew<string>(new Func<string>(
                delegate
                {
                    return Tools.Statistics.Export(this.account);
                }));

            if (path == null)
            {
                MessageBox.Show(this, this.account.Name + " has no data to export", "Nothing to export", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                using (SaveFileDialog f = new SaveFileDialog())
                {
                    f.Filter = "CSV|*.csv";

                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            if (File.Exists(f.FileName))
                                File.Delete(f.FileName);

                            File.Move(path, f.FileName);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                            MessageBox.Show(this, "An error occured while trying to save the file:\n\n" + ex.Message, "Unable to save file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Util.Logging.Log(ex);
                        }
                    }
                }
            }

            labelExportRecordedLaunch.ForeColor = c;
            labelExportRecordedLaunch.Enabled = true;
        }

        private void labelViewRecordedLaunch_Click(object sender, EventArgs e)
        {

        }

        private void label30_Click(object sender, EventArgs e)
        {
            textRunAfterLaunch.SelectedText = ((Label)sender).Text;
        }

        private void checkVolume_CheckedChanged(object sender, EventArgs e)
        {
            sliderVolume.Enabled = checkVolume.Checked;
        }

        private void sliderVolume_ValueChanged(object sender, float e)
        {
            labelVolume.Text = (int)(e * 100 + 0.5f) + "%";
        }

        private void buttonGfxSettings_Click(object sender, EventArgs e)
        {
            using (formBrowseLocalDat f = new formBrowseLocalDat(Client.FileManager.FileType.Gfx, account))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (selectedGfxFile != null)
                        selectedGfxFile.Cancel();

                    string path;
                    selectedGfxFile = f.Result;
                    if (selectedGfxFile == null)
                        path = "";
                    else if (selectedGfxFile.File != null)
                        path = selectedGfxFile.File.Path;
                    else
                        path = selectedGfxFile.Path;

                    textGfxSettings.Text = path;
                    textGfxSettings.Select(textGfxSettings.TextLength, 0);

                    LoadGraphics(path);
                }
            }
        }

        private void checkScreenshotsLocation_CheckedChanged(object sender, EventArgs e)
        {
            textScreenshotsLocation.Enabled = buttonScreenshotsLocation.Enabled = checkScreenshotsLocation.Checked;
        }

        private void checkPort80_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                checkPort443.Checked = false;
        }

        private void checkPort443_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                checkPort80.Checked = false;
        }

        private void buttonScreenshotsLocation_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.OpenFolderDialog();

            if (textScreenshotsLocation.Text.Length > 0)
            {
                f.SetPath(textScreenshotsLocation.Text, false);
            }

            try
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    textScreenshotsLocation.Text = f.FileName;
                    textScreenshotsLocation.Select(textScreenshotsLocation.TextLength, 0);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private async void buttonApiVerify_Click(object sender, EventArgs e)
        {
            if (textApiKey.TextLength == 0)
                return;

            labelApiPermissions.Text = "...";

            buttonApiVerify.Enabled = false;
            labelApiPermissions.Visible = true;
            buttonApiVerify.Visible = false;

            bool keyChanged;
            EventHandler onChanged = delegate
            {
                keyChanged = true;
            };
            textApiKey.TextChanged += onChanged;

            do
            {
                keyChanged = false;

                var key = textApiKey.Text;
                var kd = new ApiKeyData();

                try
                {
                    kd.permissionsArray = await Api.TokenInfo.GetPermissionsAsync(key);
                    kd.permissions = new HashSet<Api.TokenInfo.Permissions>(kd.permissionsArray);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    kd.permissions = null;
                }

                int totalAp;

                if (kd.permissions == null)
                {
                    if (!keyChanged)
                    {
                        labelApiPermissions.Text = "Unable to verify key";
                        textApiKey.Tag = null;

                        break;
                    }
                }
                else
                {
                    try
                    {
                        kd.account = await Api.Account.GetAccountAsync(key);
                        totalAp = kd.account.DailyAP + kd.account.MonthlyAP;
                    }
                    catch (Exception ex)
                    {
                        //if (ex is Api.Exceptions.PermissionRequiredException)
                        //    totalAp = -2;
                        //else
                        //    totalAp = -1;
                        Util.Logging.Log(ex);
                        kd.account = null;
                    }

                    apikeys[key] = kd;

                    if (!keyChanged)
                    {
                        OnApiKeyDataChanged(kd);

                        break;
                    }
                }
            }
            while (keyChanged);

            textApiKey.TextChanged -= onChanged;

            buttonApiVerify.Enabled = true;
        }

        private void OnApiKeyDataChanged(ApiKeyData kd)
        {
            bool showVerify;

            if (kd == null)
            {
                showVerify = true;
            }
            else
            {
                if (textApiKey.Tag == kd)
                    return;

                showVerify = kd.permissions == null;
            }

            textApiKey.Tag = kd;

            if (!showVerify)
            {
                var sb = new StringBuilder(kd.permissionsArray.Length * 5 + 50);

                if (kd.account != null)
                    sb.AppendLine(kd.account.Name);

                sb.Append("Permissions: ");
                if (kd.permissionsArray.Length == 0)
                {
                    sb.Append("none");
                }
                else
                {
                    foreach (var p in kd.permissions)
                    {
                        sb.Append(p.ToString().ToLower());
                        sb.Append(", ");
                    }
                    sb.Length -= 2;
                }

                labelApiPermissions.Text = sb.ToString();
            }

            buttonApiVerify.Visible = showVerify;
            labelApiPermissions.Visible = !showVerify;

            if (kdCurrent != kd)
            {
                kdCurrent = kd;
                foreach (var ar in apiRequirements)
                {
                    ar.Verify(kd);
                }
            }
        }

        private int OnApiKeyTextChangedCallback()
        {
            ApiKeyData kd;
            apikeys.TryGetValue(textApiKey.Text, out kd);

            OnApiKeyDataChanged(kd);

            return -1;
        }

        private void textApiKey_TextChanged(object sender, EventArgs e)
        {
            if (textApiKey.ContainsFocus)
                Util.ScheduledEvents.Register(OnApiKeyTextChangedCallback, 500);
        }

        private void labelApiPermissions_SizeChanged(object sender, EventArgs e)
        {
            var b = labelApiPermissions.Bottom;
            var b2 = buttonApiVerify.Bottom;
            if (b2 > b)
                b = b2;
            panelApiContent.Top = b + 13;
        }

        private void checkEnableNetworkAuthorization_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void textAuthenticatorKey_TextChanged(object sender, EventArgs e)
        {
            if (textAuthenticatorKey.ContainsFocus)
            {
                totpChanged = true;
                Util.ScheduledEvents.Register(OnAuthenticatorKeyTextChanged, 500);
            }
        }

        private async Task RefreshTotp()
        {
            var update = DateTime.MinValue;
            var isActive = true;

            EventHandler onEvent = delegate
            {
                if (this.IsDisposed || !panelSecurity.Visible)
                    isActive = false;
            };

            EventHandler onKeyChanged = delegate
            {
                update = DateTime.MinValue;
            };

            this.AuthenticatorKeyChanged += onKeyChanged;
            this.Disposed += onEvent;
            panelSecurity.VisibleChanged += onEvent;

            do
            {
                var now = DateTime.UtcNow;
                var s = 30000 - (int)((now.Ticks / 10000) % 30000);
                if (s <= 0)
                    s = 0;

                if (now > update)
                {
                    var key = (byte[])textAuthenticatorKey.Tag;
                    if (key == null)
                    {
                        progressTotpTime.Value = 0;
                        break;
                    }
                    textTotpCurrent.Text = new string(Tools.Totp.Generate(key, now.Ticks));
                    textTotpNext.Text = new string(Tools.Totp.Generate(key, now.Ticks + Tools.Totp.OTP_LIFESPAN_TICKS));

                    update = now.AddMilliseconds(s);
                }

                progressTotpTime.Value = s;

                await Task.Delay(100);
            }
            while (isActive);

            this.AuthenticatorKeyChanged -= onKeyChanged;
            this.Disposed -= onEvent;
            panelSecurity.VisibleChanged -= onEvent;
        }

        private int OnAuthenticatorKeyTextChanged()
        {
            if (Tools.Totp.IsValid(textAuthenticatorKey.Text))
            {
                var key = Tools.Totp.Decode(textAuthenticatorKey.Text);
                textAuthenticatorKey.Tag = key;
                if (AuthenticatorKeyChanged != null)
                    AuthenticatorKeyChanged(this, EventArgs.Empty);
                if (taskTotp == null || taskTotp.IsCompleted)
                    taskTotp = RefreshTotp();
            }
            else if (textAuthenticatorKey.Tag != null)
            {
                textAuthenticatorKey.Tag = null;
                if (AuthenticatorKeyChanged != null)
                    AuthenticatorKeyChanged(this, EventArgs.Empty);
                textTotpCurrent.Text = textTotpNext.Text = "invalid";
            }

            return -1;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Util.ScheduledEvents.Unregister(OnApiKeyTextChangedCallback);
                Util.ScheduledEvents.Unregister(OnAuthenticatorKeyTextChanged);

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void labelShowDailyCompletionApi_Click(object sender, EventArgs e)
        {
            buttonGeneral.SelectPanel(containerApi);
            checkTrackDailyCompletionApi.Focus();
        }

        /// <summary>
        /// Links two checkboxes, where b requires a
        /// </summary>
        private void LinkCheckBox(CheckBox a, CheckBox b)
        {
            a.CheckedChanged += delegate
            {
                if (a.ContainsFocus)
                {
                    if (b.Tag == a)
                    {
                        b.Checked = a.Checked;
                    }
                    else if (!a.Checked && b.Checked)
                    {
                        b.Tag = a;
                        b.Checked = false;
                    }
                    a.Tag = null;
                }
            };

            a.VisibleChanged += delegate
            {
                if (a.Visible)
                    a.Tag = null;
            };

            b.CheckedChanged += delegate
            {
                if (b.ContainsFocus)
                {
                    if (a.Tag == b)
                    {
                        a.Checked = b.Checked;
                    }
                    else if (b.Checked && !a.Checked)
                    {
                        a.Tag = b;
                        a.Checked = true;
                    }
                    b.Tag = null;
                }
            };

            b.VisibleChanged += delegate
            {
                if (b.Visible)
                    b.Tag = null;
            };
        }

        private void labelShowDailyLoginApi_Click(object sender, EventArgs e)
        {
            buttonGeneral.SelectPanel(containerApi);
            checkTrackPlayedApi.Focus();
        }

        private void labelGfxPreset_Click(object sender, EventArgs e)
        {
            var items = new MenuItem[5];
            var i = 0;

            foreach (var preset in new GraphicsPreset[] { GraphicsPreset.Lowest, GraphicsPreset.Low, GraphicsPreset.Medium, GraphicsPreset.High, GraphicsPreset.Ultra })
            {
                items[i++] = new MenuItem(preset.ToString(), 
                    delegate
                    {
                        SetGraphicsPreset(preset);
                    });
            }

            var menu = new System.Windows.Forms.ContextMenu(items);
            menu.Collapse += delegate
            {
                menu.Dispose();
            };
            menu.Show(labelGfxPreset, Point.Empty);
        }

        private void checkProcessPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboProcessPriority.Enabled = checkProcessPriority.Checked;
        }

        private void checkProcessAffinityAll_CheckedChanged(object sender, EventArgs e)
        {
            panelProcessAffinity.Visible = !checkProcessAffinityAll.Checked;
        }

        private void panelIdentifierColor_Click(object sender, EventArgs e)
        {
            using (var f = new ColorDialog())
            {
                f.AnyColor = true;
                f.FullOpen = true;

                try
                {
                    Color c;
                    if (account != null)
                        c = Util.Color.FromUID(account.UID);
                    else
                        c = Util.Color.FromUID(Settings.GetNextUID());
                    f.CustomColors = new int[] { c.R | c.G << 8 | c.B << 16 };
                }
                catch { }

                f.Color = panelIdentifierColor.BackColor;

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panelIdentifierColor.BackColor = f.Color;
                    panelIdentifierColor.Tag = f.Color;

                    if (panelIdentifierIcon.Tag is IconValue)
                    {
                        var type = ((IconValue)panelIdentifierIcon.Tag).Type;
                        switch (type)
                        {
                            case Settings.IconType.ColorKey:
                            case Settings.IconType.Gw2LauncherColorKey:

                                SetIcon(CreateIcon(type));

                                break;
                        }
                    }
                }
            }
        }

        private void panelIdentifierIcon_Click(object sender, EventArgs e)
        {
            contextIcon.Show(Cursor.Position);
        }

        protected Size GetScaledDimensions(Image image, int maxW, int maxH)
        {
            int w = image.Width,
                h = image.Height;

            float rw = (float)maxW / w,
                  rh = (float)maxH / h,
                  r = rw < rh ? rw : rh;

            if (r < 1)
            {
                w = (int)(w * r + 0.5f);
                h = (int)(h * r + 0.5f);
            }

            return new Size(w, h);
        }

        private void panelIdentifierIcon_MouseEnter(object sender, EventArgs e)
        {
            var image = panelIdentifierIcon.BackgroundImage;

            if (panelIdentifierIconLarge.BackgroundImage != image)
            {
                panelIdentifierIconLarge.Size = Size.Empty;
                panelIdentifierIconLarge.BackgroundImage = image;

                if (image != null)
                {
                    var sz = GetScaledDimensions(image, panelIdentifierIconLarge.MaximumSize.Width, panelIdentifierIconLarge.MaximumSize.Height);

                    if (sz.Width > panelIdentifierIcon.Width * 2 || sz.Height > panelIdentifierIcon.Height * 2)
                    {
                        panelIdentifierIconLarge.BringToFront();
                        panelIdentifierIconLarge.Size = sz;
                    }
                }
            }

            if (!panelIdentifierIconLarge.Size.IsEmpty && panelIdentifierIconLarge.BackgroundImage != null)
                panelIdentifierIconLarge.Visible = true;
        }

        private void panelIdentifierIcon_MouseLeave(object sender, EventArgs e)
        {
            panelIdentifierIconLarge.Visible = false;
        }

        protected override void WndProc(ref Message m)
        {
            switch ((WindowMessages)m.Msg)
            {
                case WindowMessages.WM_NCHITTEST:

                    base.WndProc(ref m);

                    switch ((HitTest)m.Result)
                    {
                        case HitTest.BottomLeft:
                        case HitTest.BottomRight:

                            m.Result = (IntPtr)HitTest.Bottom;

                            break;
                        case HitTest.Left:
                        case HitTest.Right:
                        case HitTest.TopLeft:
                        case HitTest.TopRight:

                            m.Result = (IntPtr)HitTest.Border;

                            break;
                    }

                    break;
                case WindowMessages.WM_EXITSIZEMOVE:

                    base.WndProc(ref m);

                    Settings.WindowBounds[typeof(formSettings)].Value = this.Bounds;

                    break;
                default:

                    base.WndProc(ref m);

                    break;
            }
        }

        private void menuIconBrowseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png;*.ico";

                if (panelIdentifierIcon.Tag is IconValue)
                {
                    var icon = (IconValue)panelIdentifierIcon.Tag;
                    if (!string.IsNullOrEmpty(icon.Path))
                        f.FileName = icon.Path;
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    using (panelIdentifierIcon.BackgroundImage)
                    {
                        try
                        {
                            SetIcon(new IconValue(f.FileName, Image.FromFile(f.FileName)));
                        }
                        catch (Exception ex)
                        {
                            panelIdentifierIcon.BackgroundImage = null;
                            panelIdentifierIcon.Tag = null;
                            OnIconChanged();

                            MessageBox.Show(this, "The selected image could not be used\n\n" + ex.Message, "Unable to use image", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void OnIconChanged()
        {
            if (panelIdentifierIcon.Tag is IconValue)
            {
                var icon = (IconValue)panelIdentifierIcon.Tag;
                menuIconUseColorToolStripMenuItem.Checked = icon.Type == Settings.IconType.Gw2LauncherColorKey;
                menuIconUseDefaultToolStripMenuItem.Checked = icon.Type == Settings.IconType.None;
                menuIconUseSolidColorToolStripMenuItem.Checked = icon.Type == Settings.IconType.ColorKey;
            }
            else
            {
                menuIconUseColorToolStripMenuItem.Checked = false;
                menuIconUseDefaultToolStripMenuItem.Checked = false;
                menuIconUseSolidColorToolStripMenuItem.Checked = false;
            }
        }

        private void menuIconUseDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetIcon(CreateIcon(Settings.IconType.None));
        }

        private void menuIconUseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetIcon(CreateIcon(Settings.IconType.Gw2LauncherColorKey));
        }

        private void menuIconUseSolidColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetIcon(CreateIcon(Settings.IconType.ColorKey));
        }

        private void labelAutologinConfigure_Click(object sender, EventArgs e)
        {
            Settings.Point<ushort> empty, play;

            if (Settings.LauncherAutologinPoints.HasValue)
            {
                var v = Settings.LauncherAutologinPoints.Value;
                empty = v.EmptyArea;
                play = v.PlayButton;
            }
            else
            {
                empty = play = new Settings.Point<ushort>();
            }

            var c = Cursor.Position;
            var f = new formAutologinConfig(empty, play)
            {
                StartPosition = FormStartPosition.Manual,
                TopMost = true,
            };

            f.Location = new Point(this.Location.X + (this.Width - f.Width) / 2, this.Location.Y);

            this.Visible = false;
            this.Owner.Visible = false;

            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                if (f.EmptyLocation.IsEmpty && f.PlayLocation.IsEmpty)
                {
                    Settings.LauncherAutologinPoints.Clear();
                }
                else
                {
                    Settings.LauncherAutologinPoints.Value = new Settings.LauncherPoints()
                    {
                        EmptyArea = f.EmptyLocation,
                        PlayButton = f.PlayLocation,
                    };
                }
            }

            this.Visible = true;
            this.Owner.Visible = true;
        }

        private void labelAutologin_SizeChanged(object sender, EventArgs e)
        {
            labelAutologinConfigure.Location = new Point(labelAutologin.Right + 1, labelAutologin.Top + (labelAutologin.Height - labelAutologinConfigure.Height) / 2);
        }
    }
}
