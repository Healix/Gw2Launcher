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
    public partial class formAccount : Base.BaseForm
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

            public ApiRequirements(Label l, Api.TokenInfo.Permissions required, Func<ApiKeyData, ApiState> onVerify)
            {
                //var x = parent.Right + 2;

                //var l = new Label()
                //{
                //    ForeColor = SystemColors.GrayText,
                //    AutoSize = true,
                //    AutoEllipsis = true,
                //    Location = new Point(x, parent.Top + 1),
                //    MaximumSize = new Size(parent.Parent.Width - x, 0),
                //    Font = new System.Drawing.Font("Segoe UI Semilight", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                //    Margin = new Padding(6, 1, 0, 0),
                //};

                //parent.Parent.Controls.Add(l);

                this.required = required;
                this.label = l;
                this.onVerify = onVerify;

                this.State = ApiState.OK;
            }

            public Api.TokenInfo.Permissions required;
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
                    if (required == Api.TokenInfo.Permissions.None || (data.permissions & required) != required)
                    {
                        result = ApiState.NoPermission;
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

                                label.Text = GetDefaultText();//"(no permission)";
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
                if (required != Api.TokenInfo.Permissions.None)
                {
                    var sb = new StringBuilder(Util.Bits.GetBitCount((uint)required) * 10 + 5);
                    var c = 0;

                    sb.Append("api: ");

                    foreach (Api.TokenInfo.Permissions p in Enum.GetValues(typeof(Api.TokenInfo.Permissions)))
                    {
                        if ((required & p) == p && p != 0 && p != Api.TokenInfo.Permissions.Account)
                        {
                            sb.Append(p.ToString().ToLower());
                            sb.Append(", ");
                            ++c;
                        }
                    }

                    if (c > 0)
                    {
                        sb.Length -= 2;
                        return sb.ToString();
                    }
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
            public Api.TokenInfo.Permissions permissions;
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

        private class ButtonImages
        {
            public ButtonImages()
            {
                paths = new string[2];
            }

            public string[] paths;
            public Settings.ImagePlacement imagePlacement;

            public string Image
            {
                get
                {
                    return paths[0];
                }
                set
                {
                    paths[0] = value;
                }
            }

            public string Background
            {
                get
                {
                    return paths[1];
                }
                set
                {
                    paths[1] = value;
                }
            }
        }

        private event EventHandler AuthenticatorKeyChanged;

        private Settings.IAccount account;
        private IList<Settings.IAccount> accounts;
        private Settings.AccountType accountType;
        private formBrowseLocalDat.SelectedFile selectedDatFile, selectedGfxFile;
        private formBrowseGwDat.SelectedFile selectedGwDatFile;
        private Settings.IFile[] _files;
        private CheckBox[] checkArgs;
        private ArgsState argsState;
        private Tools.Gfx.Xml xmlGfx;
        private Util.ReusableControls reuseableXml;
        private Dictionary<string, ApiKeyData> apikeys;
        private Task taskTotp;
        private bool totpChanged;
        private ApiRequirements[] apiRequirements;
        private ApiKeyData kdCurrent;
        private TextBox[] textAccountNames;
        private Dictionary<string, object> iconsRunAfter;
        private Tools.Shared.Images imagesLoginRewards;
        private bool hasTempSettings;

        public formAccount(Settings.AccountType type)
            : this(type, false)
        {
        }

        private formAccount(Settings.AccountType type, bool hasData)
        {
            InitializeComponents();

            this.accountType = type;

            var sbounds = Settings.WindowBounds[typeof(formSettings)];
            if (sbounds.HasValue)
                this.Size = sbounds.Value.Size;

            apikeys = new Dictionary<string, ApiKeyData>(StringComparer.OrdinalIgnoreCase);
            iconsRunAfter = new Dictionary<string, object>();

            progressTotpTime.Value = 0;

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

            if (!Client.FileManager.IsFolderLinkingSupported)
            {
                checkScreenshotsLocation.Enabled = false;
                buttonScreenshotsLocation.Enabled = false;
            }

            checkArgs = formSettings.InitializeArguments(type, panelArgs, labelArgsTemplateHeader, checkArgsTemplate, labelArgsTemplateSwitch, labelArgsTemplateDesc, checkArgs_CheckedChanged);

            if (type == Settings.AccountType.GuildWars1)
            {
                labelShowDailyLoginApi.Visible = false;
                label96.Visible = false;
                label59.Visible = false;
                checkShowDailyCompletion.Parent.Visible = false;
                checkResetDailyCompletion.Parent.Visible = false;
                panelAutoLoginGw2.Visible = false;
                panelAccountTypeGw2.Visible = false;
                checkMuteMusic.Visible = false;
                checkMuteVoices.Visible = false;
                panelClientPortGw2.Visible = false;
                panelMumbleNameGw2.Visible = false;
                panelAutomaticLauncherLoginGw2.Visible = false;
                labelAutologinConfigure.Visible = false;
                panelTrackLoginRewardsDay.Visible = false;
                panelLaunchSteamGw2.Visible = false;
                panelLaunchOptionsProcessBrowser.Visible = false;
                label93.Visible = false;
                label92.Visible = false;
                checkShowWeeklyCompletion.Parent.Visible = false;
                checkResetWeeklyCompletion.Parent.Visible = false;

                panelAutoLoginGw1.Visible = true;
                labelAutoLoginCharacter.Visible = true;
                textAutoLoginCharacter.Visible = true;
            }
            else
            {
                apiRequirements = new ApiRequirements[]
                {
                    new ApiRequirements(labelTrackDailyCompletionApi, 
                        Api.TokenInfo.Permissions.Account | Api.TokenInfo.Permissions.Progression,
                        delegate(ApiKeyData kd)
                        {
                            return ApiRequirements.ApiState.OK;
                        }),
                    new ApiRequirements(labelTrackPlayedApi,
                        Api.TokenInfo.Permissions.Account, 
                        null),
                    new ApiRequirements(labelTrackWeeklyCompletionApi,
                        Api.TokenInfo.Permissions.Account | Api.TokenInfo.Permissions.Progression,
                        null),
                    new ApiRequirements(labelTrackAstralApi,
                        Api.TokenInfo.Permissions.Account | Api.TokenInfo.Permissions.Wallet, 
                        null),
                };

                LinkCheckBox(checkShowDailyCompletion, checkTrackDailyCompletionApi);
                LinkCheckBox(checkShowDailyLogin, checkTrackPlayedApi);
                LinkCheckBox(checkShowWeeklyCompletion, checkTrackWeeklyCompletionApi);

                panelGraphics.PreVisiblePropertyChanged += panelGraphics_PreVisiblePropertyChanged;
            }
            
            panelSecurity.PreVisiblePropertyChanged += panelSecurity_PreVisiblePropertyChanged;
            panelLaunchOptionsAdvanced.PreVisiblePropertyChanged += panelLaunchOptionsAdvanced_PreVisiblePropertyChanged;

            buttonLaunchOptions.Panels = new Panel[] { panelLaunchOptions, panelLaunchOptionsProcess, panelLaunchOptionsAdvanced };
            buttonSecurity.Panels = new Panel[] { panelSecurity };
            buttonStatistics.Panels = new Panel[] { panelStatistics };

            buttonLaunchOptions.SubItems = new string[] { "Processor" };

            if (type == Settings.AccountType.GuildWars1)
            {
                buttonGeneral.Panels = new Panel[] { panelGeneral, panelHotkeys };
                buttonLocalDat.Panels = new Panel[] { panelGwDat };

                buttonGeneral.SubItems = new string[] { "Hotkeys" };
                buttonLocalDat.Text = "Gw.dat";
            }
            else
            {
                buttonGeneral.Panels = new Panel[] { panelGeneral, panelApi, panelHotkeys };
                buttonLocalDat.Panels = new Panel[] { panelLocalDat, panelGraphics };

                buttonGeneral.SubItems = new string[] { "API usage", "Hotkeys" };
                buttonLocalDat.SubItems = new string[] { "Graphics" };
            }

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

            var priorityValues = new object[]
                {
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.High, "High"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.AboveNormal, "Above normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Normal, "Normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.BelowNormal, "Below normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Low, "Low"),
                };

            comboProcessPriority.Items.AddRange(priorityValues);
            comboBrowserPriority.Items.AddRange(priorityValues);

            Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriorityClass.Normal);
            Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboBrowserPriority, Settings.ProcessPriorityClass.Normal);

            if (!hasData)
            {
                panelIdentifierColor.BackColor = Util.Color.FromUID(Settings.GetNextUID());
                SetIcon(CreateIcon(Settings.IconType.None, type));
            }

            textEmail.TextBox.TextChanged += credentials_TextChanged;
            textPassword.PasswordBox.TextChanged += credentials_TextChanged;
            textAutoLoginEmail.TextBox.TextChanged += credentials_TextChanged;
            textAutoLoginPassword.PasswordBox.TextChanged += credentials_TextChanged;
            textAuthenticatorKey.TextBox.TextChanged += textAuthenticatorKey_TextChanged;

            if (!hasData)
            {
                textEmail.TextVisible = true;
                textAutoLoginEmail.TextVisible = true;
                textPassword.TextVisible = true;
                textAutoLoginPassword.TextVisible = true;
                textAuthenticatorKey.TextVisible = true;
                textAutoLoginCharacter.TextVisible = true;
            }

            if (Settings.FontName.HasValue)
                buttonSample.FontName = Settings.FontName.Value;
            else
                buttonSample.FontName = UI.Controls.AccountGridButton.FONT_NAME;

            if (Settings.FontStatus.HasValue)
                buttonSample.FontStatus = Settings.FontStatus.Value;
            else
                buttonSample.FontStatus = UI.Controls.AccountGridButton.FONT_STATUS;

            if (Settings.FontUser.HasValue)
                buttonSample.FontUser = Settings.FontUser.Value;
            else
                buttonSample.FontUser = UI.Controls.AccountGridButton.FONT_USER;

            buttonSample.ShowAccount = !Settings.StyleShowAccount.HasValue || Settings.StyleShowAccount.Value;
            buttonSample.ShowColorKey = Settings.StyleShowColor.Value;
            buttonSample.AccountType = type;

            labelDisableRunAfterGlobal.Visible = Settings.DisableRunAfter.Value;
            Settings.DisableRunAfter.ValueChanged += DisableRunAfter_ValueChanged;
        }

        public formAccount(Settings.IAccount account)
            : this(account.Type, true)
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

            if (!string.IsNullOrEmpty(account.Email))
            {
                textEmail.TextBox.Text = account.Email;
                textAutoLoginEmail.TextBox.Text = account.Email;
            }
            else
            {
                textEmail.TextVisible = true;
                textAutoLoginEmail.TextVisible = true;
            }
            if (account.Password == null)
            {
                textPassword.TextVisible = true;
                textAutoLoginPassword.TextVisible = true;
            }

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
                textWindowed.Text = account.WindowBounds.Width > 0 ? ToString(account.WindowBounds) : "";

                checkWindowedPreventChanges.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.PreventChanges);
                checkWindowedDisableTitleButtons.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.DisableTitleBarButtons);
                checkWindowedUpdateOnChange.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.RememberChanges);
                checkWindowedTopMost.Checked = account.WindowOptions.HasFlag(Settings.WindowOptions.TopMost);
            }

            checkShowDailyLogin.Checked = account.ShowDailyLogin;

            if (account.Type == Settings.AccountType.GuildWars1)
            {
                var gw1 = (Settings.IGw1Account)account;

                checkAutomaticLoginGw1.Checked = gw1.AutomaticLogin;

                if (!string.IsNullOrEmpty(gw1.CharacterName))
                {
                    textAutoLoginCharacter.TextBox.Text = gw1.CharacterName;
                }
                else
                {
                    textAutoLoginCharacter.TextVisible = true;
                }

                if (gw1.DatFile != null)
                {
                    textGwDat.Text = gw1.DatFile.Path;
                    textGwDat.Select(textGwDat.TextLength, 0);
                }
            }
            else if (account.Type == Settings.AccountType.GuildWars2)
            {
                var gw2 = (Settings.IGw2Account)account;

                checkAutomaticLogin.Checked = gw2.AutomaticLogin;
                checkAutomaticLoginPlay.Checked = gw2.AutomaticPlay;

                if (gw2.DatFile != null)
                {
                    textLocalDat.Text = gw2.DatFile.Path;
                    textLocalDat.Select(textLocalDat.TextLength, 0);
                }

                if (gw2.GfxFile != null)
                {
                    textGfxSettings.Text = gw2.GfxFile.Path;
                    textGfxSettings.Select(textGfxSettings.TextLength, 0);
                    checkGfxSettingsReadOnly.Checked = gw2.GfxFile.IsLocked;
                }

                checkAutomaticLauncherLogin.Checked = gw2.AutomaticRememberedLogin;
                checkPort80.Checked = gw2.ClientPort == 80;
                checkPort443.Checked = gw2.ClientPort == 443;

                checkShowDailyCompletion.Checked = gw2.ShowDailyCompletion;
                checkShowWeeklyCompletion.Checked = gw2.ShowWeeklyCompletion;

                if (gw2.Api != null)
                {
                    textApiKey.Text = gw2.Api.Key;

                    checkTrackPlayedApi.Checked = (gw2.ApiTracking & Settings.ApiTracking.Login) != 0;
                    checkTrackDailyCompletionApi.Checked = (gw2.ApiTracking & Settings.ApiTracking.Daily) != 0;
                    checkTrackWeeklyCompletionApi.Checked = (gw2.ApiTracking & Settings.ApiTracking.Weekly) != 0;
                    checkTrackAstralApi.Checked = (gw2.ApiTracking & Settings.ApiTracking.Astral) != 0;

                    var kd = new ApiKeyData();
                    kd.useAccountData = true;

                    kdCurrent = kd;
                    foreach (var ar in apiRequirements)
                    {
                        ar.Verify(kd);
                    }

                    apikeys[gw2.Api.Key] = kd;
                }

                if (gw2.MumbleLinkName != null)
                {
                    checkGw2MumbleName.Checked = true;
                    textGw2MumbleName.Text = gw2.MumbleLinkName;
                }

                if (gw2.DailyLoginDay > 0)
                {
                    checkTrackLoginRewardsDay.Checked = true;
                    textTrackLoginRewardsDay.Value = gw2.DailyLoginDay;
                }

                if (gw2.Provider == Settings.AccountProvider.Steam)
                    radioAccountTypeSteam.Checked = true;

                if (gw2.Proxy == Settings.LaunchProxy.Steam)
                    checkLaunchSteam.Checked = true;

                if (!gw2.DisableMumbleLinkDailyLogin)
                    checkDailyLoginMumbleLink.CheckState = Settings.Tweaks.DisableMumbleLinkDailyLogin.Value ? CheckState.Indeterminate : CheckState.Checked;
                else
                    checkDailyLoginMumbleLink.Checked = false;

                if (gw2.ChromiumPriority != Settings.ProcessPriorityClass.None)
                {
                    Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboBrowserPriority, gw2.ChromiumPriority);
                    checkBrowserPriority.Checked = true;
                }

                if (gw2.ChromiumAffinity != 0)
                {
                    adBrowserAffinity.Affinity = gw2.ChromiumAffinity;
                    checkBrowserAffinityAll.Checked = false;
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            if (account.VolumeEnabled)
            {
                checkVolume.Checked = true;
                sliderVolume.Value = account.Volume;
            }
            else
                sliderVolume.Value = 1f;

            checkMuteAll.Checked = account.Mute.HasFlag(Settings.MuteOptions.All);
            checkMuteMusic.Checked = account.Mute.HasFlag(Settings.MuteOptions.Music);
            checkMuteVoices.Checked = account.Mute.HasFlag(Settings.MuteOptions.Voices);

            checkScreenshotsBmp.Checked = account.ScreenshotsFormat == Settings.ScreenshotFormat.Bitmap;

            if (Client.FileManager.IsFolderLinkingSupported)
            {
                if (checkScreenshotsLocation.Checked = account.ScreenshotsLocation != null)
                {
                    textScreenshotsLocation.Text = account.ScreenshotsLocation;
                    textScreenshotsLocation.Select(textScreenshotsLocation.TextLength, 0);
                }
            }

            checkEnableNetworkAuthorization.Checked = (account.NetworkAuthorization & Settings.NetworkAuthorizationOptions.Enabled) != 0;
            checkEnableNetworkAuthorizationRemember.Checked = (account.NetworkAuthorization & Settings.NetworkAuthorizationOptions.Remember) != 0;

            if (account.TotpKey != null)
                textAuthenticatorKey.Tag = account.TotpKey;
            else
                textAuthenticatorKey.TextVisible = true;

            if (account.ProcessPriority != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, account.ProcessPriority);
                checkProcessPriority.Checked = true;
            }

            if (account.ProcessAffinity != 0)
            {
                adProcessAffinity.Affinity = account.ProcessAffinity;
                checkProcessAffinityAll.Checked = false;
            }

            if (!account.ColorKeyIsDefault)
            {
                SetColor(account.ColorKey);
                menuColorUseDefaultToolStripMenuItem.Checked = false;
            }
            else
                panelIdentifierColor.BackColor = account.ColorKey;

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

                    icon = CreateIcon(account.IconType, account.Type);

                    break;
            }

            if (icon == null)
                icon = CreateIcon(Settings.IconType.None, account.Type);

            SetIcon(icon);

            checkDisableJumpList.Checked = account.JumpListPinning == Settings.JumpListPinning.Disabled;

            var images = new ButtonImages();

            if (account.Image != null)
            {
                buttonSample.ImagePlacement = account.Image.Placement;

                try
                {
                    buttonSample.Image = Bitmap.FromFile(account.Image.Path);
                }
                catch 
                {
                    buttonSample.Image = Util.Bitmap.CreateErrorImage(32, 32);
                }

                images.Image = account.Image.Path;
                images.imagePlacement = account.Image.Placement;

                shiftImageToolStripMenuItem.Checked = images.imagePlacement == Settings.ImagePlacement.Shift;
                overflowImageToolStripMenuItem.Checked = images.imagePlacement == Settings.ImagePlacement.Overflow;

                menuImageDefaultToolStripMenuItem.Checked = false;
            }

            if (account.BackgroundImage != null)
            {
                try
                {
                    buttonSample.BackgroundImage = Bitmap.FromFile(account.BackgroundImage);
                }
                catch
                {
                    buttonSample.BackgroundImage = Util.Bitmap.CreateErrorImage(buttonSample.Width, buttonSample.Height);
                }

                images.Background = account.BackgroundImage;
                menuImageBackgroundDefaultToolStripMenuItem.Checked = false;
            }

            buttonSample.Tag = images;

            if (account.RunAfter != null)
            {
                panelRunAfterPrograms.SuspendLayout();
                foreach (var r in account.RunAfter)
                {
                    CreateRunAfterButton(r, panelRunAfterPrograms, labelRunAfterProgramsAdd, labelRunAfterProgram_Click);
                }
                panelRunAfterPrograms.ResumeLayout();
            }

            panelHotkeysHotkeys.HotkeyClick += panelHotkeysHotkeys_HotkeyClick;
            panelHotkeysHotkeys.TemplateHeader = label77;
            panelHotkeysHotkeys.TemplateText = labelHotkeysAdd;
            panelHotkeysHotkeys.TemplateKey = label214;
            panelHotkeysHotkeys.EnableGrouping = false;
            labelHotkeysDisabled.Visible = !Settings.HotkeysEnabled.Value;
            
            if (account.Hotkeys != null)
            {
                panelHotkeysHotkeys.SuspendLayout();
                foreach (var h in account.Hotkeys)
                {
                    CreateHotkeyButton(h);
                }
                panelHotkeysHotkeys.ResumeLayout();
                panelHotkeysHotkeys.ResetModified();
            }

            checkLaunchSteam.Checked = account.Proxy == Settings.LaunchProxy.Steam;

            checkDisableRunAfter.Checked = account.DisableRunAfter;






            if (accountType == Settings.AccountType.GuildWars2 && Settings.GuildWars2.ProfileMode.HasValue && Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic)
            {
                labelScreenshotsLocationBasicWarning.Visible = true;
            }
        }

        public formAccount(IList<Settings.IAccount> accounts)
            : this(accounts[0])
        {
            var count = accounts.Count;

            if (count > 1)
            {
                this.accounts = accounts;

                automaticApplyAllToolStripMenuItem.Checked = true;

                this.textAccountNames = new TextBox[count];
                this.textAccountNames[0] = textAccountName;

                var panelAccountNames = textAccountName.Parent;
                panelAccountNames.SuspendLayout();
                for (var i = 1; i < count; i++)
                {
                    var t = new TextBox()
                    {
                        Margin = textAccountName.Margin,
                        Size = textAccountName.Size,
                        Font = textAccountName.Font,
                        Text = accounts[i].Name,
                    };
                    this.textAccountNames[i] = t;
                    panelAccountNames.Controls.Add(t);
                }
                panelAccountNames.ResumeLayout();

                foreach (var c in EnumerateApplyAllCheckBoxes(this))
                {
                    c.Enabled = true;
                    c.Visible = true;
                    c.MouseDown += applyAll_MouseDown;

                    EventHandler e = delegate
                    {
                        OnAutoApplyValueChanged(c);
                    };

                    if (c.DefaultState)
                    {
                        foreach (var c1 in EnumerateControls(c.Parent))
                        {
                            if (c1 is CheckBox)
                            {
                                ((CheckBox)c1).CheckedChanged += e;
                            }
                            else if (c1 is RadioButton)
                            {
                                ((RadioButton)c1).CheckedChanged += e;
                            }
                            else if (c1 is TextBox)
                            {
                                ((TextBox)c1).TextChanged += e;
                            }
                            else if (c1 is ComboBox)
                            {
                                ((ComboBox)c1).SelectedIndexChanged += e;
                            }
                            else if (c1 is Controls.FlatSlider)
                            {
                                ((FlatSlider)c1).ValueChanged += e;
                            }
                        }
                    }
                }

                panelIdentifierColor.BackColorChanged += delegate
                {
                    OnAutoApplyValueChanged(aaIdentifierColor);
                };

                panelIdentifierIcon.BackgroundImageChanged += delegate
                {
                    OnAutoApplyValueChanged(aaIdentifierIcon);
                };
            }
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();
            MarkTemporarySettings();

            sidebarPanel1.BackColor = Util.Color.Lighten(this.BackColor, Util.Color.Luminance(this.ForeColor) <= 127 ? 0.9f : 0.1f);

            if (Settings.IsRunningWine)
            {
                ctimerTotpTime.Visible = false;
            }
        }

        protected void MarkTemporarySettings()
        {
            if (!Settings.HAS_TEMP_SETTINGS)
                return;

            var controls = new Control[]
            {
                checkShowWeeklyCompletion,
                checkTrackAstralApi,
                checkTrackWeeklyCompletionApi,
            };

            foreach (var c in controls)
            {
                MarkTemporary(c);
            }
        }

        protected void MarkTemporary(Control c)
        {
            if (!hasTempSettings)
            {
                hasTempSettings = true;
                if (!Settings.NotifiedTemporarySettings)
                    this.Shown += hasTempSettings_Shown;
            }
            c.ForeColor = Color.Purple;
        }

        void hasTempSettings_Shown(object sender, EventArgs e)
        {
            MessageBox.Show(this, "This version contains purple marked settings are temporary and may reset in future versions.\n\nThis will reappear after a reset.", "Temporary settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            Settings.NotifiedTemporarySettings = true;
        }

        private void OnAutoApplyValueChanged(ApplyAllCheckBox c)
        {
            if (automaticApplyAllToolStripMenuItem.Checked && !c.Clicked)
                c.Checked = true;
        }

        private IEnumerable<Control> EnumerateControls(Control parent)
        {
            var queue = new Queue<Control>();

            queue.Enqueue(parent);

            do
            {
                var p = queue.Dequeue();

                if (p.Controls.Count == 0)
                {
                    yield return p;
                }
                else
                {
                    foreach (Control c in p.Controls)
                    {
                        queue.Enqueue(c);
                    }
                }
            }
            while (queue.Count > 0);
        }

        private IEnumerable<ApplyAllCheckBox> EnumerateApplyAllCheckBoxes(Control parent)
        {
            var queue = new Queue<Control>();

            queue.Enqueue(parent);

            do
            {
                var p = queue.Dequeue();

                if (p is ApplyAllCheckBox)
                {
                    yield return (ApplyAllCheckBox)p;
                }

                foreach (Control c in p.Controls)
                {
                    queue.Enqueue(c);
                }
            }
            while (queue.Count > 0);
        }

        public AccountGridButton SampleButton
        {
            get
            {
                return buttonSample;
            }
        }

        private void OnAccountTypeChanged(Settings.AccountType type)
        {
            var b = type == Settings.AccountType.GuildWars2;
        }

        void credentials_TextChanged(object sender, EventArgs e)
        {
            var t = (TextBox)sender;

            if (t.ContainsFocus)
            {
                t.TextChanged -= credentials_TextChanged;
                t.LostFocus += credentials_LostFocus;
            }
        }

        void credentials_LostFocus(object sender, EventArgs e)
        {
            var t = (TextBox)sender;

            t.TextChanged += credentials_TextChanged;
            t.LostFocus -= credentials_LostFocus;

            if (sender is PasswordBox)
            {
                var t1 = (PasswordBox)sender;
                var t2 = textPassword.PasswordBox;

                if (t1 == t2)
                {
                    t2 = textAutoLoginPassword.PasswordBox;
                }

                t2.Password = t1.Password;
            }
            else
            {
                var t1 = t;
                var t2 = textEmail.TextBox;

                if (t1 == t2)
                {
                    t2 = textAutoLoginEmail.TextBox;
                }

                t2.Text = t1.Text;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (textAccountName.TextLength == 0)
                textAccountName.Focus();
        }

        private IconValue CreateIcon(Settings.IconType type, Settings.AccountType accountType)
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
                        if (accountType == Settings.AccountType.GuildWars1)
                        {
                            using (var iconGw1 = Properties.Resources.Gw1)
                            {
                                return new IconValue(Settings.IconType.None, iconGw1.ToBitmap());
                            }
                        }
                        else
                        {
                            using (var iconGw2 = Properties.Resources.Gw2)
                            {
                                return new IconValue(Settings.IconType.None, iconGw2.ToBitmap());
                            }
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

        void panelLaunchOptionsAdvanced_PreVisiblePropertyChanged(object sender, bool e)
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

        void panelSecurity_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e && textAuthenticatorKey.Tag != null)
                taskTotp = RefreshTotp();
        }

        void panelGraphics_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e && xmlGfx == null)
            {
                var gw2 = (Settings.IGw2Account)this.account;

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
                else if (gw2 != null && gw2.GfxFile != null)
                {
                    path = gw2.GfxFile.Path;
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

            checkDX11.Checked = Util.Args.Contains(textArguments.Text, "dx11");
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

        private void checkAutomaticLogin_CheckedChanged(object sender, EventArgs e)
        {
            tableLogin.Parent.SuspendLayout();

            var b = checkAutomaticLogin.Checked;

            textAutoLoginEmail.Enabled = textAutoLoginPassword.Enabled = b;
            panelAutomaticLauncherLoginGw2.Visible = panelAutomaticLauncherLoginGw2.Enabled = !b && radioAccountTypeGw2.Checked;
            tableLogin.Visible = tableLogin.Enabled = b && radioAccountTypeGw2.Checked;

            tableLogin.Parent.ResumeLayout();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            ReleaseFiles();
        }

        private void ReleaseFiles()
        {
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

            if (selectedGwDatFile != null)
            {
                selectedGwDatFile.Cancel();
                selectedGwDatFile = null;
            }

            if (_files != null)
            {
                foreach (var f in _files)
                {
                    if (f == null)
                        continue;
                    try
                    {
                        File.Delete(f.Path);
                    }
                    catch { }

                    if (f is Settings.IDatFile)
                        Settings.DatFiles[f.UID].Clear();
                    else if (f is Settings.IGfxFile)
                        Settings.GfxFiles[f.UID].Clear();
                    else if (f is Settings.IGwDatFile)
                        Settings.GwDatFiles[f.UID].Clear();
                }
                _files = null;
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
            Settings.IFile file;

            if (accountType == Settings.AccountType.GuildWars2)
            {
                var gw2 = (Settings.IGw2Account)this.account;
                if (_files == null)
                    _files = new Settings.IFile[2];
                int _index = -1;
                formBrowseLocalDat.SelectedFile selectedFile;
                string ext;

                switch (type)
                {
                    case Client.FileManager.FileType.Dat:

                        selectedFile = selectedDatFile;
                        file = _files[_index = 0];
                        ext = ".dat";

                        break;
                    case Client.FileManager.FileType.Gfx:

                        selectedFile = selectedGfxFile;
                        file = _files[_index = 1];
                        ext = ".xml";

                        break;
                    default:
                        return null;
                }

                if (selectedFile == null)
                {
                    if (file != null)
                        return file;

                    if (gw2 != null)
                    {
                        switch (type)
                        {
                            case Client.FileManager.FileType.Dat:
                                return gw2.DatFile;
                            case Client.FileManager.FileType.Gfx:
                                return gw2.GfxFile;
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

                            _files[_index] = file = Settings.CreateDatFile();

                            break;
                        case Client.FileManager.FileType.Gfx:

                            _files[_index] = file = Settings.CreateGfxFile();

                            break;
                    }
                }

                var wasMoved = false;
                var path = Path.Combine(DataPath.AppDataAccountData, file.UID + ext);

                try
                {
                    Util.FileUtil.MoveFile(selectedFile.Path, path, false, true);
                    selectedFile.Update(path);
                    wasMoved = true;
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);

                }

                if (!wasMoved)
                {
                    try
                    {
                        File.Copy(selectedFile.Path, path, true);
                    }
                    catch (Exception e)
                    {
                        Util.Logging.Log(e);
                        throw;
                    }

                    try
                    {
                        File.Delete(selectedFile.Path);
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }

                file.Path = path;
            }
            else if (accountType == Settings.AccountType.GuildWars1)
            {
                var gw1 = (Settings.IGw1Account)this.account;
                if (_files == null)
                    _files = new Settings.IFile[1];
                int _index = -1;
                formBrowseGwDat.SelectedFile selectedFile;

                switch (type)
                {
                    case Client.FileManager.FileType.GwDat:
                        selectedFile = selectedGwDatFile;
                        file = _files[_index = 0]; 
                        break;
                    default:
                        return null;
                }

                if (selectedFile == null)
                {
                    if (file != null)
                        return file;

                    if (gw1 != null)
                    {
                        switch (type)
                        {
                            case Client.FileManager.FileType.GwDat:
                                return gw1.DatFile;
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

                var path = selectedFile.Path;

                if (file == null)
                {
                    _files[_index] = file = Settings.CreateGwDatFile();
                }

                if (selectedFile.Flags.HasFlag(formBrowseGwDat.SelectedFile.FileFlags.Created))
                {
                    try
                    {
                        var n = Path.GetFileName(path);
                        var d = Path.GetDirectoryName(path);
                        var newpath = Path.Combine(Path.GetDirectoryName(d), file.UID.ToString());

                        Directory.Move(d, newpath);

                        path = Path.Combine(newpath, n);
                        selectedFile.Cancel();
                    }
                    catch { }
                }

                file.Path = path;
            }
            else
            {
                file = null;
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
                        sliderText.Text = ofloat.Value.ToString("0.##");
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
                if (!panelGraphics.Visible)
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
                        var gw2 = (Settings.IGw2Account)this.account;

                        bool isShared = false,
                             isDefault = path.Equals(Client.FileManager.GetDefaultPath(Client.FileManager.FileType.Gfx), StringComparison.OrdinalIgnoreCase);

                        if (selectedGfxFile != null)
                        {
                            isShared = selectedGfxFile.File != null && selectedGfxFile.File.References > 0;    
                        }
                        else if (gw2 != null && gw2.GfxFile != null)
                        {
                            isShared = gw2.GfxFile.References > 1;
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

                        //if (labelGfxFileInfo.Visible)
                        //    panelGraphicsXml.Top = labelGfxFileInfo.Bottom + 10;
                        //else
                        //    panelGraphicsXml.Top = labelGfxSettingsTitleSub.Bottom + 10;
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

        void sliderGfx_ValueChanged(object sender, EventArgs e)
        {
            if (!panelGraphicsXml.Visible)
                return;
            var o = (object[])((Control)sender).Tag;
            var ofloat = (Tools.Gfx.Xml.Option.Float)o[0];
            var text = (TextBox)o[1];
            var slider = (UI.Controls.FlatSlider)sender;

            if (!text.ContainsFocus)
            {
                var value = ofloat.MinValue + slider.Value * (ofloat.MaxValue - ofloat.MinValue);

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

        private IEnumerable<Settings.IAccount> GetAccounts()
        {
            yield return account;

            if (accounts != null)
            {
                foreach (var a in accounts)
                {
                    if (!object.ReferenceEquals(a, this.account))
                        yield return a;
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            foreach (var t in textAccountNames != null ? textAccountNames : new TextBox[] { textAccountName })
            {
                if (t.TextLength == 0)
                {
                    buttonGeneral.Selected = true;
                    t.Focus();
                    MessageBox.Show(this, "An identifier / account or display name is required", "Identifier / name required", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            Settings.IDatFile datFile;
            Settings.IGfxFile gfxFile;
            Settings.IGwDatFile gwdatFile;

            if (accountType == Settings.AccountType.GuildWars2)
            {
                gwdatFile = null;

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
                            if (a.Type == Settings.AccountType.GuildWars2 && ((Settings.IGw2Account)a).GfxFile == gfxFile)
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
                                if (a.Type == Settings.AccountType.GuildWars2 && ((Settings.IGw2Account)a).GfxFile == gfxFile)
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
                    bool gfx, dat;
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
                                buttonLocalDat.SelectPanel(panelLocalDat);
                            else
                                buttonLocalDat.SelectPanel(panelGraphics);
                            return;
                        }
                    }
                }
            }
            else if (accountType == Settings.AccountType.GuildWars1)
            {
                datFile = null;
                gfxFile = null;

                gwdatFile = (Settings.IGwDatFile)GetFile(Client.FileManager.FileType.GwDat);

                if (gwdatFile == null)
                {
                    buttonLocalDat.SelectPanel(panelGwDat);
                    MessageBox.Show(this, "Choose a Gw.dat file to use with this account", "Gw.dat not selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                datFile = null;
                gfxFile = null;
                gwdatFile = null;
            }

            selectedDatFile = null;
            selectedGfxFile = null;
            selectedGwDatFile = null;
            _files = null;

            try
            {
                if (!Util.Users.IsCurrentUser(textWindowsAccount.Text))
                    Util.FileUtil.AllowFileAccess(System.Reflection.Assembly.GetExecutingAssembly().Location, System.Security.AccessControl.FileSystemRights.Modify);
            }
            catch { }

            var accounts = this.accounts;
            var master = this.account;

            if (master == null)
            {
                this.account = master = Settings.CreateAccount(accountType);
                //this.account.LastUsedUtc = DateTime.MinValue;
                //this.account.CreatedUtc = DateTime.UtcNow;

                if (accounts != null)
                {
                    //this shouldn't be possible (creating a new account while editing others)

                    accounts = new Settings.IAccount[accounts.Count + 1];
                    accounts[0] = master;

                    int i = 0;
                    foreach (var a in this.accounts)
                    {
                        accounts[++i] = a;
                    }
                }
            }

            if (accounts == null)
                accounts = new Settings.IAccount[] { master };

            for (int i = 0, count = accounts.Count; i < count; i++)
            {
                var a = accounts[i];
                var isMaster = i == 0;

                if (isMaster)
                {
                    if (checkWindowed.Checked)
                    {
                        var windowed = Settings.WindowOptions.Windowed;
                        if (checkWindowedPreventChanges.Checked)
                            windowed |= Settings.WindowOptions.PreventChanges;
                        if (checkWindowedDisableTitleButtons.Checked)
                            windowed |= Settings.WindowOptions.DisableTitleBarButtons;
                        if (checkWindowedUpdateOnChange.Checked)
                            windowed |= Settings.WindowOptions.RememberChanges;
                        if (checkWindowedTopMost.Checked)
                            windowed |= Settings.WindowOptions.TopMost;

                        a.WindowOptions = windowed;
                    }
                    else
                    {
                        a.WindowOptions = Settings.WindowOptions.None;
                    }
                }
                else if (aaWindowedOptions.Checked)
                {
                    a.WindowOptions = master.WindowOptions;
                }

                if (textAccountNames != null)
                {
                    a.Name = textAccountNames[i].Text;
                }
                else
                {
                    a.Name = textAccountName.Text;
                }

                if (isMaster || aaWindowsAccount.Checked)
                {
                    a.WindowsAccount = textWindowsAccount.Text;
                }

                if (isMaster || aaArguments.Checked)
                {
                    a.Arguments = textArguments.Text;
                }

                if (checkWindowed.Checked)
                {
                    if (isMaster)
                    {
                        a.WindowBounds = !string.IsNullOrWhiteSpace(textWindowed.Text) ? FixSize(ParseWindowSize(textWindowed.Text)) : Rectangle.Empty;
                    }
                    else if (aaWindowed.Checked)
                    {
                        a.WindowBounds = master.WindowBounds;
                    }
                }

                if (isMaster || aaRecordLaunch.Checked)
                {
                    a.RecordLaunches = checkRecordLaunch.Checked;
                }

                if (isMaster || aaEmail.Checked)
                {
                    a.Email = textEmail.TextBox.TextLength > 0 ? textEmail.TextBox.Text : null;
                }

                if (isMaster || aaPassword.Checked)
                {
                    if (textPassword.TextVisible)
                    {
                        if (a.Password != null)
                            a.Password.Dispose();
                        a.Password = textPassword.PasswordBox.TextLength > 0 ? Settings.PasswordString.Create(textPassword.PasswordBox.Password) : null;
                    }
                }

                if (isMaster || aaVolume.Checked)
                {
                    if (checkVolume.Checked)
                        a.Volume = sliderVolume.Value;
                    else
                        a.VolumeEnabled = false;
                }

                if (isMaster)
                {
                    var mute = Settings.MuteOptions.None;
                    if (checkMuteAll.Checked)
                        mute |= Settings.MuteOptions.All;
                    if (a.Type == Settings.AccountType.GuildWars2)
                    {
                        if (checkMuteMusic.Checked)
                            mute |= Settings.MuteOptions.Music;
                        if (checkMuteVoices.Checked)
                            mute |= Settings.MuteOptions.Voices;
                    }
                    a.Mute = mute;
                }
                else if (aaMute.Checked)
                {
                    if (a.Type != master.Type && a.Type == Settings.AccountType.GuildWars1)
                    {
                        a.Mute = master.Mute & Settings.MuteOptions.All;
                    }
                    else
                    {
                        a.Mute = master.Mute;
                    }
                }

                if (isMaster || aaScreenshotsBmp.Checked)
                {
                    if (checkScreenshotsBmp.Checked)
                        a.ScreenshotsFormat = Settings.ScreenshotFormat.Bitmap;
                    else
                        a.ScreenshotsFormat = Settings.ScreenshotFormat.Default;
                }

                if (isMaster)
                {
                    if (checkScreenshotsLocation.Checked && Directory.Exists(textScreenshotsLocation.Text))
                        a.ScreenshotsLocation = Util.FileUtil.GetTrimmedDirectoryPath(textScreenshotsLocation.Text);
                    else
                        a.ScreenshotsLocation = null;
                }
                else if (aaScreenshotsLocation.Checked)
                {
                    a.ScreenshotsLocation = master.ScreenshotsLocation;
                }

                if (isMaster || aaShowDailyLogin.Checked)
                {
                    a.ShowDailyLogin = checkShowDailyLogin.Checked;
                }

                if (a.Type == accountType)
                {
                    switch (a.Type)
                    {
                        case Settings.AccountType.GuildWars2:

                            var gw2 = (Settings.IGw2Account)a;

                            if (isMaster || aaLocalDat.Checked)
                            {
                                gw2.DatFile = datFile;
                            }

                            if (isMaster || aaGfxSettings.Checked)
                            {
                                gw2.GfxFile = gfxFile;

                                if (isMaster && gfxFile != null)
                                {
                                    gfxFile.IsLocked = checkGfxSettingsReadOnly.Checked;
                                }
                            }

                            if (isMaster || aaAutoLoginGw2.Checked)
                            {
                                gw2.AutomaticLogin = checkAutomaticLogin.Checked && a.HasCredentials;
                                gw2.AutomaticPlay = checkAutomaticLoginPlay.Checked;
                            }

                            if (isMaster || aaAutomaticLauncherLogin.Checked)
                            {
                                if (panelAutomaticLauncherLoginGw2.Enabled)
                                    gw2.AutomaticRememberedLogin = checkAutomaticLauncherLogin.Checked;
                            }

                            if (isMaster || aaPort.Checked)
                            {
                                if (checkPort80.Checked)
                                    gw2.ClientPort = 80;
                                else if (checkPort443.Checked)
                                    gw2.ClientPort = 443;
                                else
                                    gw2.ClientPort = 0;
                            }

                            #region API

                            var apiTracking = Settings.ApiTracking.None;
                            var apiTrackingPrevious = gw2.ApiTracking;

                            if (isMaster || aaShowDailyLogin.Checked)
                            {
                                gw2.ShowDailyLogin = checkShowDailyLogin.Checked;
                            }

                            if (gw2.ShowDailyLogin)
                            {
                                if (isMaster || aaTrackPlayedApi.Checked)
                                {
                                    if (checkTrackPlayedApi.Checked)
                                    {
                                        apiTracking |= Settings.ApiTracking.Login;
                                    }
                                }
                                else
                                {
                                    apiTracking |= (apiTrackingPrevious & Settings.ApiTracking.Login);
                                }
                            }

                            if (isMaster || aaShowDailyCompletion.Checked)
                            {
                                gw2.ShowDailyCompletion = checkShowDailyCompletion.Checked;
                            }

                            if (gw2.ShowDailyCompletion)
                            {
                                if (isMaster || aaTrackDailyCompletionApi.Checked)
                                {
                                    if (checkTrackDailyCompletionApi.Checked)
                                    {
                                        apiTracking |= Settings.ApiTracking.Daily;
                                    }
                                }
                                else
                                {
                                    apiTracking |= (apiTrackingPrevious & Settings.ApiTracking.Daily);
                                }
                            }

                            if (isMaster || aaShowWeeklyCompletion.Checked)
                            {
                                gw2.ShowWeeklyCompletion = checkShowWeeklyCompletion.Checked;
                            }

                            if (gw2.ShowWeeklyCompletion)
                            {
                                if (isMaster || aaTrackWeeklyCompletionApi.Checked)
                                {
                                    if (checkTrackWeeklyCompletionApi.Checked)
                                    {
                                        apiTracking |= Settings.ApiTracking.Weekly;
                                    }
                                }
                                else
                                {
                                    apiTracking |= (apiTrackingPrevious & Settings.ApiTracking.Weekly);
                                }
                            }

                            if (isMaster || aaTrackAstralApi.Checked)
                            {
                                if (checkTrackAstralApi.Checked)
                                {
                                    apiTracking |= Settings.ApiTracking.Astral;
                                }
                            }
                            else
                            {
                                apiTracking |= (apiTrackingPrevious & Settings.ApiTracking.Astral);
                            }

                            bool keyChanged;

                            if (isMaster || aaApiKey.Checked)
                            {
                                var apiKey = textApiKey.Text;

                                if (string.IsNullOrWhiteSpace(apiKey))
                                {
                                    if (keyChanged = gw2.Api != null)
                                    {
                                        gw2.Api = null;
                                    }
                                }
                                else if (keyChanged = gw2.Api == null || !gw2.Api.Equals(apiKey))
                                {
                                    var p = Api.TokenInfo.Permissions.Unknown;

                                    if (textApiKey.Tag != null)
                                    {
                                        var kd = (ApiKeyData)textApiKey.Tag;

                                        p = kd.permissions;
                                    }

                                    gw2.Api = Settings.ApiDataKey.Create(apiKey, p);
                                }
                                else if (gw2.Api.Permissions == Api.TokenInfo.Permissions.None)
                                {
                                    if (textApiKey.Tag != null)
                                    {
                                        var kd = (ApiKeyData)textApiKey.Tag;

                                        gw2.Api.Permissions = kd.permissions;
                                    }
                                }
                            }
                            else
                            {
                                keyChanged = false;
                            }

                            gw2.ApiTracking = apiTracking;

                            if (gw2.Api != null)
                            {
                                var p = gw2.Api.Permissions;
                                var d = gw2.Api.Data;
                                var t = gw2.Api.Tracking;

                                if (t != 0)
                                {
                                    if (d.Account == null)
                                    {
                                        d.Account = d.CreateValue<Settings.ApiData.AccountValues>();
                                    }

                                    if (d.Wallet == null && (p & (Api.TokenInfo.Permissions.Wallet | Api.TokenInfo.Permissions.Unknown)) != 0 && (t & (Settings.ApiTracking.Astral | Settings.ApiTracking.Weekly | Settings.ApiTracking.Daily)) != 0)
                                    {
                                        d.Wallet = d.CreateValue<Settings.ApiData.WalletValues>();
                                    }

                                    if (textApiKey.Tag != null && (isMaster || aaApiKey.Checked))
                                    {
                                        var kd = (ApiKeyData)textApiKey.Tag;

                                        if (kd.account != null)
                                        {
                                            if (d.Account != null && d.Account.State == Settings.ApiCacheState.None)
                                            {
                                                d.Account.Value = new Settings.ApiData.AccountValues(kd.account);
                                                d.Account.State = Settings.ApiCacheState.OK;
                                                d.Account.LastChange = kd.account.LastModifiedServer;
                                            }
                                        }
                                    }
                                }
                            }

                            #endregion



                            if (isMaster)
                            {
                                if (checkGw2MumbleName.Checked && !string.IsNullOrWhiteSpace(textGw2MumbleName.Text))
                                    gw2.MumbleLinkName = textGw2MumbleName.Text;
                                else
                                    gw2.MumbleLinkName = null;
                            }
                            else if (aaGw2MumbleName.Checked)
                            {
                                gw2.MumbleLinkName = ((Settings.IGw2Account)master).MumbleLinkName;
                            }

                            if (isMaster || aaTrackLoginRewardsDay.Checked)
                            {
                                gw2.DailyLoginDay = checkTrackLoginRewardsDay.Checked ? (byte)textTrackLoginRewardsDay.Value : (byte)0;
                            }

                            if (isMaster || aaAccountTypeGw2.Checked)
                            {
                                gw2.Provider = radioAccountTypeSteam.Checked ? Settings.AccountProvider.Steam : Settings.AccountProvider.ArenaNet;
                            }

                            if (isMaster || aaLaunchSteam.Checked)
                            {
                                gw2.Proxy = checkLaunchSteam.Checked ? Settings.LaunchProxy.Steam : Settings.LaunchProxy.None;
                            }

                            if (isMaster || aaDailyLoginMumbleLink.Checked)
                            {
                                gw2.DisableMumbleLinkDailyLogin = !checkDailyLoginMumbleLink.Checked;
                            }

                            if (isMaster || aaBrowserPriority.Checked)
                            {
                                if (checkBrowserPriority.Checked && comboBrowserPriority.SelectedIndex >= 0)
                                    gw2.ChromiumPriority = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboBrowserPriority);
                                else
                                    gw2.ChromiumPriority = Settings.ProcessPriorityClass.None;
                            }

                            if (isMaster)
                            {
                                gw2.ChromiumAffinity = checkBrowserAffinityAll.Checked ? 0 : adBrowserAffinity.Affinity;
                            }
                            else if (aaBrowserAffinity.Checked)
                            {
                                gw2.ChromiumAffinity = ((Settings.IGw2Account)master).ChromiumAffinity;
                            }

                            if (checkResetDailyCompletion.Checked && (isMaster || aaResetDailyCompletion.Checked) && gw2.ShowDailyCompletion)
                            {
                                gw2.LastDailyCompletionUtc = DateTime.MinValue;
                            }

                            if (checkResetWeeklyCompletion.Checked && (isMaster || aaResetWeeklyCompletion.Checked) && gw2.ShowWeeklyCompletion)
                            {
                                gw2.LastWeeklyCompletionUtc = DateTime.MinValue;
                            }

                            break;
                        case Settings.AccountType.GuildWars1:

                            var gw1 = (Settings.IGw1Account)a;

                            if (isMaster || aaGwDat.Checked)
                            {
                                gw1.DatFile = gwdatFile;
                            }

                            if (isMaster || aaEmail.Checked)
                            {
                                gw1.CharacterName = textAutoLoginCharacter.TextBox.Text;
                            }

                            if (isMaster || aaAutoLoginGw1.Checked)
                            {
                                gw1.AutomaticLogin = checkAutomaticLoginGw1.Checked && gw1.HasCredentials && !string.IsNullOrEmpty(gw1.CharacterName);
                            }

                            break;
                    }
                }

                if (isMaster || aaEnableNetworkAuthorization.Checked)
                {
                    if (checkEnableNetworkAuthorization.Checked)
                    {
                        var na = Settings.NetworkAuthorizationOptions.Enabled;
                        if (checkEnableNetworkAuthorizationRemember.Checked)
                            na |= Settings.NetworkAuthorizationOptions.Remember;
                        a.NetworkAuthorization = na;
                    }
                    else
                        a.NetworkAuthorization = Settings.NetworkAuthorizationOptions.None;
                }

                if (isMaster)
                {
                    if (totpChanged)
                    {
                        byte[] totp;
                        if (Tools.Totp.IsValid(textAuthenticatorKey.TextBox.Text))
                            totp = Tools.Totp.Decode(textAuthenticatorKey.TextBox.Text);
                        else
                            totp = null;
                        a.TotpKey = totp;
                    }
                }
                else if (aaAuthenticatorKey.Checked)
                {
                    a.TotpKey = master.TotpKey;
                }

                if (isMaster || aaProcessPriority.Checked)
                {
                    if (checkProcessPriority.Checked && comboProcessPriority.SelectedIndex >= 0)
                        a.ProcessPriority = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboProcessPriority);
                    else
                        a.ProcessPriority = Settings.ProcessPriorityClass.None;
                }

                if (isMaster)
                {
                    a.ProcessAffinity = checkProcessAffinityAll.Checked ? 0 : adProcessAffinity.Affinity;
                }
                else if (aaProcessAffinity.Checked)
                {
                    a.ProcessAffinity = master.ProcessAffinity;
                }

                if (isMaster || aaIdentifierIcon.Checked)
                {
                    if (panelIdentifierIcon.Tag is IconValue)
                    {
                        var icon = (IconValue)panelIdentifierIcon.Tag;
                        a.Icon = icon.Path;
                        a.IconType = icon.Type;
                    }
                    else
                    {
                        a.Icon = null;
                        a.IconType = Settings.IconType.None;
                    }
                }

                if (isMaster || aaIdentifierColor.Checked)
                {
                    if (panelIdentifierColor.Tag != null)
                    {
                        a.ColorKey = (Color)panelIdentifierColor.Tag;
                    }
                    else
                    {
                        a.ColorKey = Color.Empty;
                    }
                }

                if (isMaster || aaDisableJumpList.Checked)
                {
                    a.JumpListPinning = checkDisableJumpList.Checked ? Settings.JumpListPinning.Disabled : Settings.JumpListPinning.None;
                }

                if (isMaster || aaImages.Checked)
                {
                    if (buttonSample.Tag != null)
                    {
                        var images = (ButtonImages)buttonSample.Tag;
                        a.Image = images.Image != null ? new Settings.ImageOptions(images.Image, images.imagePlacement) : null;
                        a.BackgroundImage = images.Background;
                    }
                    else
                    {
                        a.Image = null;
                        a.BackgroundImage = null;
                    }
                }

                if (isMaster)
                {
                    if (panelRunAfterPrograms.Controls.Count > 0)
                    {
                        var controls = panelRunAfterPrograms.Controls;
                        var ra = new Settings.RunAfter[controls.Count];
                        var existing = a.RunAfter;
                        var changed = existing == null || ra.Length != existing.Length;

                        for (var j = 0; j < ra.Length; j++)
                        {
                            ra[j] = (Settings.RunAfter)controls[j].Tag;
                            if (!changed)
                                changed = !existing[j].Equals(ra[j]);
                        }

                        if (changed)
                            a.RunAfter = ra;
                    }
                    else
                        a.RunAfter = null;
                }
                else if (aaRunAfterPrograms.Checked)
                {
                    a.RunAfter = master.RunAfter;
                }

                if (isMaster && panelHotkeysHotkeys.Modified)
                {
                    foreach (var m in panelHotkeysHotkeys.GetModified())
                    {
                        var hotkeys = m.Hotkeys;

                        a.Hotkeys = hotkeys;

                        break;
                    }
                }

                if (isMaster || aaDisableRunAfter.Checked)
                {
                    a.DisableRunAfter = checkDisableRunAfter.Checked;
                }




            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkWindowed_CheckedChanged(object sender, EventArgs e)
        {
            var b = checkWindowed.Checked;
            textWindowed.Enabled = b;
            buttonWindowed.Enabled = b;
            panelWindowOptions.Enabled = b;
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
            if (r.Width < 150)
                r.Width = 150;
            if (r.Height < 100)
                r.Height = 100;

            return r;
        }

        private void buttonWindowed_Click(object sender, EventArgs e)
        {
            using (var f = new WindowPositioning.formWindowSize(true, accountType, account, textAccountName.Text))
            {
                using (var o = new formCompassOverlay(this, f))
                {
                    Rectangle r = FixSize(ParseWindowSize(textWindowed.Text));

                    f.SetBounds(r.X, r.Y, r.Width, r.Height);
                    o.Show(this);
                    if (f.ShowDialog() == DialogResult.OK)
                        textWindowed.Text = ToString(f.Bounds);

                    this.Focus();
                }
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

        private void checkVolume_CheckedChanged(object sender, EventArgs e)
        {
            sliderVolume.Enabled = checkVolume.Checked;
        }

        private void sliderVolume_ValueChanged(object sender, EventArgs e)
        {
            labelVolume.Text = (int)(((FlatSlider)sender).Value * 100 + 0.5f) + "%";
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

            var container = buttonApiVerify.Parent;

            container.SuspendLayout();

            labelApiPermissions.Text = "...";

            buttonApiVerify.Enabled = false;
            labelApiPermissions.Visible = true;
            buttonApiVerify.Visible = false;

            container.ResumeLayout();

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
                    kd.permissions = await Api.TokenInfo.GetPermissionsAsync(key);
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    kd.permissions = Api.TokenInfo.Permissions.None;
                }

                int totalAp;

                if (kd.permissions == Api.TokenInfo.Permissions.None)
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

                showVerify = kd.permissions == Api.TokenInfo.Permissions.None;
            }

            textApiKey.Tag = kd;

            var container = buttonApiVerify.Parent;

            container.SuspendLayout();

            if (!showVerify)
            {
                var sb = new StringBuilder(Util.Bits.GetBitCount((uint)kd.permissions) * 10 + 50);

                if (kd.account != null)
                    sb.AppendLine(kd.account.Name);

                sb.Append("Permissions: ");
                if (kd.permissions == Api.TokenInfo.Permissions.None)
                {
                    sb.Append("none");
                }
                else
                {
                    foreach (Api.TokenInfo.Permissions p in Enum.GetValues(typeof(Api.TokenInfo.Permissions)))
                    {
                        if ((kd.permissions & p) == p && p != 0)
                        {
                            sb.Append(p.ToString().ToLower());
                            sb.Append(", ");
                        }
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

            container.ResumeLayout();
        }

        private Util.ScheduledEvents.Ticks OnApiKeyTextChangedCallback()
        {
            ApiKeyData kd;
            apikeys.TryGetValue(textApiKey.Text, out kd);

            OnApiKeyDataChanged(kd);

            return Util.ScheduledEvents.Ticks.None;
        }

        private void textApiKey_TextChanged(object sender, EventArgs e)
        {
            if (textApiKey.ContainsFocus)
                Util.ScheduledEvents.Register(OnApiKeyTextChangedCallback, 500);
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
                if (!this.Visible || !panelSecurity.Visible)
                    isActive = false;
            };

            EventHandler onKeyChanged = delegate
            {
                update = DateTime.MinValue;
            };

            this.AuthenticatorKeyChanged += onKeyChanged;
            this.Disposed += onEvent;
            panelSecurity.VisibleChanged += onEvent;

            ctimerTotpTime.Start();

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

            ctimerTotpTime.Stop();

            this.AuthenticatorKeyChanged -= onKeyChanged;
            this.Disposed -= onEvent;
            panelSecurity.VisibleChanged -= onEvent;
        }

        private Util.ScheduledEvents.Ticks OnAuthenticatorKeyTextChanged()
        {
            if (Tools.Totp.IsValid(textAuthenticatorKey.TextBox.Text))
            {
                var key = Tools.Totp.Decode(textAuthenticatorKey.TextBox.Text);
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

            return Util.ScheduledEvents.Ticks.None;
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

                Settings.DisableRunAfter.ValueChanged -= DisableRunAfter_ValueChanged;

                ReleaseFiles();

                using (panelIdentifierIconLarge.BackgroundImage) { }
                using (panelIdentifierIcon.BackgroundImage) { }
                using (buttonSample.Image) { }
                using (buttonSample.BackgroundImage) { }
                using (imagesLoginRewards) { }

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                foreach (object o in iconsRunAfter.Values)
                {
                    if (o is Icon)
                    {
                        ((Icon)o).Dispose();
                    }
                }
            }
        }

        void DisableRunAfter_ValueChanged(object sender, EventArgs e)
        {
            Util.Invoke.Required(this, delegate
            {
                labelDisableRunAfterGlobal.Visible = Settings.DisableRunAfter.Value;
            });
        }

        private void labelShowDailyCompletionApi_Click(object sender, EventArgs e)
        {
            buttonGeneral.SelectPanel(panelApi);
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
            buttonGeneral.SelectPanel(panelApi);
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
            panelProcessAffinityContainer.Visible = !checkProcessAffinityAll.Checked;
        }

        private void panelIdentifierColor_Click(object sender, EventArgs e)
        {
            contextColor.Show(Cursor.Position);
        }

        private void panelIdentifierIcon_Click(object sender, EventArgs e)
        {
            contextIcon.Show(Cursor.Position);
        }

        protected Size GetScaledDimensions(Image image, int maxW, int maxH)
        {
            return Util.RectangleConstraint.Scale(image.Size, new Size(maxW, maxH));
        }

        private void panelIdentifierIcon_MouseEnter(object sender, EventArgs e)
        {
            var image = panelIdentifierIcon.BackgroundImage;

            if (panelIdentifierIconLarge.BackgroundImage != image)
            {
                using (panelIdentifierIconLarge.BackgroundImage) { }
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
            {
                panelIdentifierIconLarge.BringToFront();
                var p1 = panelIdentifierIcon.PointToScreen(Point.Empty);
                var p2 = this.PointToScreen(Point.Empty);

                panelIdentifierIconLarge.Location = new Point(p1.X - p2.X - 1 + panelIdentifierIcon.Width + panelIdentifierIconLarge.Margin.Left, p1.Y - p2.Y - 1);
                panelIdentifierIconLarge.Visible = true;
            }
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
                    
                    //switch ((HitTest)m.Result.GetValue())
                    //{
                    //    case HitTest.BottomLeft:
                    //    case HitTest.BottomRight:

                    //        m.Result = (IntPtr)HitTest.Bottom;

                    //        break;
                    //    case HitTest.Left:
                    //    case HitTest.Right:
                    //    case HitTest.TopLeft:
                    //    case HitTest.TopRight:

                    //        m.Result = (IntPtr)HitTest.Border;

                    //        break;
                    //}

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
            SetIcon(CreateIcon(Settings.IconType.None, accountType));
        }

        private void menuIconUseColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetIcon(CreateIcon(Settings.IconType.Gw2LauncherColorKey, accountType));
        }

        private void menuIconUseSolidColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetIcon(CreateIcon(Settings.IconType.ColorKey, accountType));
        }

        private void labelAutologinConfigure_Click(object sender, EventArgs e)
        {
            Settings.Point<ushort> empty, play;

            if (Settings.GuildWars2.LauncherAutologinPoints.HasValue)
            {
                var v = Settings.GuildWars2.LauncherAutologinPoints.Value;
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
                    Settings.GuildWars2.LauncherAutologinPoints.Clear();
                }
                else
                {
                    Settings.GuildWars2.LauncherAutologinPoints.Value = new Settings.LauncherPoints()
                    {
                        EmptyArea = f.EmptyLocation,
                        PlayButton = f.PlayLocation,
                    };
                }
            }

            this.Owner.Visible = true;
            this.Visible = true;

            this.Select();
        }

        private void checkWindowedPreventChanges_CheckedChanged(object sender, EventArgs e)
        {
            checkWindowedDisableTitleButtons.Enabled = checkWindowedPreventChanges.Checked;
        }

        private void radioTypeGw2_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                OnAccountTypeChanged(Settings.AccountType.GuildWars2);
            }
        }

        private void radioTypeGw1_CheckedChanged(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                OnAccountTypeChanged(Settings.AccountType.GuildWars1);
            }
        }

        private void buttonGwDat_Click(object sender, EventArgs e)
        {
            Settings.IGwDatFile dat = null;
            string path = null;

            if (selectedGwDatFile != null)
            {
                if (selectedGwDatFile.File != null)
                {
                    dat = selectedGwDatFile.File;
                }
                else
                {
                    path = selectedGwDatFile.Path;
                }
            }
            else if (account != null)
            {
                dat = ((Settings.IGw1Account)account).DatFile;
            }
            else
            {
                dat = null;
            }

            using (var f = new formBrowseGwDat(path, dat))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    if (selectedGwDatFile != null)
                    {
                        selectedGwDatFile.Cancel();
                    }

                    selectedGwDatFile = f.Result;

                    if (selectedGwDatFile.File != null)
                        path = selectedGwDatFile.File.Path;
                    else
                        path = selectedGwDatFile.Path;
                    textGwDat.Text = path;
                    textGwDat.Select(textGwDat.TextLength, 0);
                }
            }
        }

        private void checkAutomaticLoginGw1_CheckedChanged(object sender, EventArgs e)
        {
            textAutoLoginCharacter.Enabled = textAutoLoginEmail.Enabled = textAutoLoginPassword.Enabled = checkAutomaticLoginGw1.Checked;
            tableLogin.Visible = tableLogin.Enabled = checkAutomaticLoginGw1.Checked;
        }

        private void checkGw2MumbleName_CheckedChanged(object sender, EventArgs e)
        {
            textGw2MumbleName.Enabled = checkGw2MumbleName.Checked;
            labelGw2MumbleNameVariables.Enabled = checkGw2MumbleName.Checked;
        }

        private void ShowVariables(TextBox text, object label, AnchorStyles anchor, params Client.Variables.VariableType[] type)
        {
            var f = new formVariables(Client.Variables.GetVariables(type));

            f.VariableSelected += delegate(object o, Client.Variables.Variable v)
            {
                text.SelectedText = v.Name;
            };

            f.Show(this, (Control)label, anchor);
        }

        private void labelGw2MumbleNameVariables_Click(object sender, EventArgs e)
        {
            ShowVariables(textGw2MumbleName, sender, AnchorStyles.Top, Client.Variables.VariableType.Account);
        }

        private void labelLaunchOptionsAdvanced_Click(object sender, EventArgs e)
        {
            buttonLaunchOptions.SelectPanel(panelLaunchOptions, panelLaunchOptionsAdvanced);
        }

        private void labelLaunchOptionsAdvancedBack_Click(object sender, EventArgs e)
        {
            buttonLaunchOptions.SelectPanel(panelLaunchOptions);
        }

        private void buttonSample_Click(object sender, EventArgs e)
        {
            contextImage.Show(Cursor.Position);
        }

        private void menuImageBrowseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnStyleImageClicked(0);
        }

        private void menuImageDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!menuImageDefaultToolStripMenuItem.Checked)
            {
                menuImageDefaultToolStripMenuItem.Checked = true;
                using (buttonSample.Image) { }
                buttonSample.Image = null;
                var images = (ButtonImages)buttonSample.Tag;
                if (images != null)
                {
                    images.Image = null;
                }
                if (aaImages.Visible)
                    OnAutoApplyValueChanged(aaImages);
            }
        }

        private void panelIdentifierColor_BackColorChanged(object sender, EventArgs e)
        {
            buttonSample.ColorKey = panelIdentifierColor.BackColor;
        }

        private void SetPasswordVisibility(bool visible)
        {
            textAutoLoginPassword.Parent.SuspendLayout();
            textAutoLoginPassword.TextVisible = visible;
            labelAutoLoginPasswordRevert.Visible = visible;
            textAutoLoginPassword.Parent.ResumeLayout();

            textPassword.Parent.SuspendLayout();
            textPassword.TextVisible = visible;
            labelPasswordRevert.Visible = visible;
            textPassword.Parent.ResumeLayout();
        }

        private void labelPasswordRevert_Click(object sender, EventArgs e)
        {
            SetPasswordVisibility(false);
            ((Control)sender).Parent.Select();
        }

        private void textPassword_BeforeShowTextBoxClicked(object sender, EventArgs e)
        {
            var p = ((BoxedLabelPasswordBox)sender).PasswordBox;
            SetPasswordVisibility(true);
        }

        private void textEmail_BeforeShowTextBoxClicked(object sender, EventArgs e)
        {
            textEmail.TextBox.Select(textEmail.TextBox.TextLength, 0);
            textEmail.TextVisible = true;

            textAutoLoginEmail.TextBox.Select(textAutoLoginEmail.TextBox.TextLength, 0);
            textAutoLoginEmail.TextVisible = true;
        }

        private void textAutoLoginCharacter_BeforeShowTextBoxClicked(object sender, EventArgs e)
        {
            var t = ((BoxedLabelTextBox)sender).TextBox;

            t.Select(t.TextLength, 0);
        }

        private void textAuthenticatorKey_BeforeShowTextBoxClicked(object sender, EventArgs e)
        {
            var p = (BoxedLabelTextBox)sender;
            var t = p.TextBox;

            try
            {
                t.Text = Tools.Totp.Encode((byte[])p.Tag);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }

            t.Select(t.TextLength, 0);
            p.TextVisible = true;
        }
        
        private void OnStyleImageClicked(int index)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png";

                var images = (ButtonImages)buttonSample.Tag;
                if (images == null)
                    buttonSample.Tag = images = new ButtonImages();
                if (!string.IsNullOrEmpty(images.paths[index]))
                    f.FileName = images.paths[index];

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var i = Bitmap.FromFile(f.FileName);

                        switch (index)
                        {
                            case 0:

                                using (buttonSample.Image) { }
                                buttonSample.Image = i;
                                menuImageDefaultToolStripMenuItem.Checked = false;

                                break;
                            case 1:

                                using (buttonSample.BackgroundImage) { }
                                buttonSample.BackgroundImage = i;
                                menuImageBackgroundDefaultToolStripMenuItem.Checked = false;

                                break;
                            default:

                                i.Dispose();
                                return;
                        }

                        if (aaImages.Visible)
                            OnAutoApplyValueChanged(aaImages);

                        images.paths[index] = f.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuImageBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnStyleImageClicked(1);
        }

        private void menuImageBackgroundDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!menuImageBackgroundDefaultToolStripMenuItem.Checked)
            {
                menuImageBackgroundDefaultToolStripMenuItem.Checked = true;
                using (buttonSample.BackgroundImage) { }
                buttonSample.BackgroundImage = null;
                var images = (ButtonImages)buttonSample.Tag;
                if (images != null)
                {
                    images.Background = null;
                }
                if (aaImages.Visible)
                    OnAutoApplyValueChanged(aaImages);
            }
        }

        private void applyAll_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                var b = ((ApplyAllCheckBox)sender).Checked;

                applyAllApplyToolStripMenuItem.Checked = b;
                applyPageApplyToolStripMenuItem.Checked = b;

                contextApplyAll.Tag = sender;
                contextApplyAll.Show(Cursor.Position);
            }
        }

        private Control FindContainer(Control c)
        {
            while (true)
            {
                var p = c.Parent;
                if (p == this || p == null)
                    break;
                c = p;
            }
            return c;
        }

        private void applyPageInvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var c in EnumerateApplyAllCheckBoxes(FindContainer(((Control)contextApplyAll.Tag).Parent)))
            {
                c.Checked = !c.Checked;
            }
        }

        private void applyPageResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var c in EnumerateApplyAllCheckBoxes(FindContainer(((Control)contextApplyAll.Tag).Parent)))
            {
                c.Checked = false;
            }
        }

        private void applyAllInvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var c in EnumerateApplyAllCheckBoxes(this))
            {
                c.Checked = !c.Checked;
            }
        }

        private void applyAllResetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var c in EnumerateApplyAllCheckBoxes(this))
            {
                c.Checked = false;
            }
        }

        private void applyAllApplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var b = !applyPageApplyToolStripMenuItem.Checked;
            applyPageApplyToolStripMenuItem.Checked = b;
            applyAllApplyToolStripMenuItem.Checked = b;

            foreach (var c in EnumerateApplyAllCheckBoxes(this))
            {
                c.Checked = b;
            }
        }

        private void applyPageApplyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var b = !applyPageApplyToolStripMenuItem.Checked;
            applyPageApplyToolStripMenuItem.Checked = b;
            applyAllApplyToolStripMenuItem.Checked = b;

            foreach (var c in EnumerateApplyAllCheckBoxes(FindContainer(((Control)contextApplyAll.Tag).Parent)))
            {
                c.Checked = b;
            }
        }

        private void SetColor(Color c)
        {
            panelIdentifierColor.Tag = c;
            if (c.IsEmpty)
            {
                try
                {
                    if (account != null)
                        c = Util.Color.FromUID(account.UID);
                    else
                        c = Util.Color.FromUID(Settings.GetNextUID());
                }
                catch { }
            }
            panelIdentifierColor.BackColor = c;

            if (panelIdentifierIcon.Tag is IconValue)
            {
                var type = ((IconValue)panelIdentifierIcon.Tag).Type;
                switch (type)
                {
                    case Settings.IconType.ColorKey:
                    case Settings.IconType.Gw2LauncherColorKey:

                        SetIcon(CreateIcon(type, accountType));

                        break;
                }
            }
        }

        private void menuColorUseDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!menuColorUseDefaultToolStripMenuItem.Checked)
            {
                menuColorUseDefaultToolStripMenuItem.Checked = true;
                SetColor(Color.Empty);
            }
        }

        private void menuColorBrowseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new ColorPicker.formColorDialog())
            {
                f.AllowAlphaTransparency = false;
                f.Color = panelIdentifierColor.BackColor;

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    menuColorUseDefaultToolStripMenuItem.Checked = false;
                    SetColor(f.Color);
                }
            }
        }

        private void automaticApplyAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (automaticApplyAllToolStripMenuItem.Checked = !automaticApplyAllToolStripMenuItem.Checked)
            {
                foreach (var c in EnumerateApplyAllCheckBoxes(this))
                {
                    c.Clicked = false;
                }
            }
        }

        private void menuImageDefaultToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            iconPlacementToolStripMenuItem.Enabled = !menuImageDefaultToolStripMenuItem.Checked;
        }

        private void OnStyleImagePlacementChanged(Settings.ImagePlacement placement)
        {
            if (buttonSample.Tag == null)
                return;

            var images = (ButtonImages)buttonSample.Tag;
            images.imagePlacement = placement;

            buttonSample.ImagePlacement = placement;

            shiftImageToolStripMenuItem.Checked = placement == Settings.ImagePlacement.Shift;
            overflowImageToolStripMenuItem.Checked = placement == Settings.ImagePlacement.Overflow;

            if (aaImages.Visible)
                OnAutoApplyValueChanged(aaImages);
        }

        private void shiftImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnStyleImagePlacementChanged(Settings.ImagePlacement.Shift);
        }

        private void overflowImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnStyleImagePlacementChanged(Settings.ImagePlacement.Overflow);
        }

        private async void LoadIconAsync(UI.Controls.LinkLabel l, Settings.RunAfter r)
        {
            var path = r.GetPath();
            Icon icon;
            Size sz;

            if (!string.IsNullOrEmpty(path))
            {
                object o;

                sz = Windows.ShellIcons.GetSize(Windows.ShellIcons.IconSize.Small);

                l.Padding = new Padding(sz.Width + Scale(3), 0, 0, 0);
                l.MinimumSize = new Size(0, sz.Height + 2);

                if (!iconsRunAfter.TryGetValue(path, out o))
                {
                    var t = Task.Run<Icon>(
                        delegate
                        {
                            try
                            {
                                return Windows.ShellIcons.GetIcon(path, Windows.ShellIcons.IconSize.Small);
                            }
                            catch
                            {
                                return null;
                            }
                        });

                    using (t)
                    {
                        iconsRunAfter[path] = t;

                        icon = await t;

                        if (this.IsDisposed && icon != null)
                        {
                            icon.Dispose();
                            return;
                        }

                        iconsRunAfter[path] = icon;
                    }
                }
                else if (o is Task<Icon>)
                {
                    icon = await (Task<Icon>)o;

                    if (this.IsDisposed)
                    {
                        return;
                    }
                }
                else
                {
                    icon = (Icon)o;
                }
            }
            else
            {
                icon = null;
                sz = Size.Empty;
            }

            if (l.Icon != icon && l.Tag == r)
            {
                l.Icon = icon;

                if (icon != null)
                {
                    if (icon.Size != sz)
                    {
                        l.Padding = new Padding(icon.Width + Scale(3), 0, 0, 0);
                        l.MinimumSize = new Size(0, icon.Height + 2);
                    }
                }
                else
                {
                    l.Padding = Padding.Empty;
                }
            }
        }

        private Label CreateRunAfterButton(Settings.RunAfter r, Panel container, Label template, EventHandler onClick)
        {
            var l = new Controls.LinkLabel()
            {
                AutoSize = template.AutoSize,
                Margin = new Padding(0, 0, 0, panelRunAfterPrograms.Margin.Left),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            l.Click += onClick;
            container.Controls.Add(l);

            if (container.Controls.Count == 1)
            {
                panelRunAfterProgramsAddSeparator.Visible = true;
                container.Visible = true;
            }

            UpdateRunAfterButton(l, r);

            return l;
        }

        private void UpdateRunAfterButton(UI.Controls.LinkLabel l, Settings.RunAfter r)
        {
            l.Text = r.GetName();
            l.Tag = r;

            if (!r.Enabled)
                l.ForeColor = SystemColors.GrayText;
            else
                l.ResetForeColor();

            LoadIconAsync(l, r);
        }

        private void labelRunAfterProgram_Click(object sender, EventArgs e)
        {
            contextRunAfterProgram.Tag = sender;
            contextRunAfterProgram.Show(Cursor.Position);
        }

        private void editRunAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = (UI.Controls.LinkLabel)contextRunAfterProgram.Tag;
            var r = (Settings.RunAfter)l.Tag;

            using (var f = new formRunAfter(r, accountType))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    UpdateRunAfterButton(l, f.Result);
                    OnAutoApplyValueChanged(aaRunAfterPrograms);
                }
            }
        }

        private void deleteRunAfterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var label = (Label)contextRunAfterProgram.Tag;
            var r = (Settings.RunAfter)label.Tag;
            var panel = (Panel)label.Parent;

            panel.Controls.Remove(label);
            if (panel.Controls.Count == 0)
            {
                panel.Parent.SuspendLayout();
                panelRunAfterProgramsAddSeparator.Visible = false;
                panel.Visible = false;
                panel.Parent.ResumeLayout();
            }
            label.Dispose();

            OnAutoApplyValueChanged(aaRunAfterPrograms);
        }

        private void labelRunAfterProgramsAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formRunAfter(accountType))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    CreateRunAfterButton(f.Result, panelRunAfterPrograms, labelRunAfterProgramsAdd, labelRunAfterProgram_Click);
                    OnAutoApplyValueChanged(aaRunAfterPrograms);

                    panelLaunchOptions.ScrollControlIntoView(labelRunAfterProgramsAdd);
                }
            }
        }

        private void checkDX11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkDX11.Focused)
                textArguments.Text = Util.Args.AddOrReplace(textArguments.Text, "dx11", checkDX11.Checked ? "-dx11" : "");
        }

        private void CreateHotkeyButton(Settings.Hotkey h)
        {
            panelHotkeysHotkeys.Add(h, panelHotkeysHotkeys.GetText(h), account);

            if (panelHotkeysHotkeys.Count == 1)
            {
                panelHotkeysAddSeparator.Visible = true;
                panelHotkeysHotkeys.Visible = true;
            }
        }

        private void labelHotkeysAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formHotkey(true))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    CreateHotkeyButton(f.Result);
                    panelHotkeys.ScrollControlIntoView((Control)sender);
                }
            }
        }

        void panelHotkeysHotkeys_HotkeyClick(object sender, EventArgs e)
        {
            contextHotkey.Tag = sender;
            contextHotkey.Show(Cursor.Position);
        }

        private void editHotkeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var h = (HotkeyContainerPanel.HotkeyValue)contextHotkey.Tag;

            using (var f = new formHotkey(h.Hotkey, account, true))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panelHotkeysHotkeys.Update(f.Result, h.Hotkey, panelHotkeysHotkeys.GetText(f.Result), account);
                }
            }
        }

        private void deleteHotkeyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var h = (HotkeyContainerPanel.HotkeyValue)contextHotkey.Tag;

            panelHotkeysHotkeys.Remove(h.Hotkey);

            if (panelHotkeysHotkeys.Count == 0)
            {
                panelHotkeysAddSeparator.Visible = false;
                panelHotkeysHotkeys.Visible = false;
            }
        }

        private void checkTrackLoginRewardsDay_CheckedChanged(object sender, EventArgs e)
        {
            if (checkTrackLoginRewardsDay.Checked)
            {
                var sz = textTrackLoginRewardsDay.Height;
                imageTrackLoginRewardsDay.Size = new Size(sz, sz);
            }
            imageTrackLoginRewardsDay.Visible = checkTrackLoginRewardsDay.Checked;
        }

        private Image GetLoginRewardIcon(int day)
        {
            switch (day)
            {
                case 1:
                case 8:
                case 15:
                case 22:
                    return Properties.Resources.loginreward0;
                case 2:
                case 7:
                case 9:
                case 16:
                case 21:
                case 23:
                    return Properties.Resources.loginreward1;
                case 3:
                case 10:
                case 17:
                case 24:
                    return Properties.Resources.loginreward2;
                case 4:
                case 11:
                case 18:
                case 25:
                    return Properties.Resources.loginreward3;
                case 5:
                case 12:
                    return Properties.Resources.loginreward4;
                case 6:
                case 13:
                case 20:
                case 27:
                    return Properties.Resources.loginreward5;
                case 14:
                    return Properties.Resources.loginreward6;
                case 19:
                    return Properties.Resources.loginreward7;
                case 26:
                    return Properties.Resources.loginreward8;
                case 28:
                    return Properties.Resources.loginreward9;
            }

            return null;
        }

        private void SetLoginRewardIcon()
        {
            var i = GetLoginRewardIcon(textTrackLoginRewardsDay.Value);
            if (i != null)
                imageTrackLoginRewardsDay.Image = i;
        }

        private void textTrackLoginRewardsDay_ValueChanged(object sender, EventArgs e)
        {
            if (imageTrackLoginRewardsDay.Visible)
                SetLoginRewardIcon();

            if (panelLoginRewards.Visible)
            {
                SelectLoginRewardButton(GetLoginRewardButton(textTrackLoginRewardsDay.Value));
            }

            checkTrackLoginRewardsDay.Checked = true;
        }

        private FlatMarkerIconButton GetLoginRewardButton(int day)
        {
            if (panelLoginRewardsContainer.Tag != null && object.Equals(((Control)panelLoginRewardsContainer.Tag).Tag, day))
            {
                return (FlatMarkerIconButton)panelLoginRewardsContainer.Tag;
            }

            foreach (Control p in panelLoginRewardsContainer.Controls)
            {
                foreach (Control c in p.Controls)
                {
                    if (object.Equals(c.Tag, day))
                    {
                        return (FlatMarkerIconButton)c;
                    }
                }
            }
            return null;
        }

        private void imageTrackLoginRewardsDay_VisibleChanged(object sender, EventArgs e)
        {
            if (imageTrackLoginRewardsDay.Visible)
                SetLoginRewardIcon();
        }

        private void CreateLoginRewardsButtons()
        {
            const int DAYS = 28;
            const int COLUMNS = 7;
            const int ROWS = (DAYS - 1) / COLUMNS + 1;

            panelLoginRewardsContainer.Controls.Clear();

            var day = 0;
            var selected = textTrackLoginRewardsDay.Value;

            panelLoginRewards.Size = Size.Empty;
            panelLoginRewardsContainer.Size = Size.Empty;

            for (var row = 0; row < ROWS; row++)
            {
                StackPanel panel = null;

                for (var col = 0; col < COLUMNS; col++)
                {
                    if (++day > DAYS)
                        break;

                    if (panel == null)
                    {
                        panel = new StackPanel()
                        {
                            FlowDirection = FlowDirection.LeftToRight,
                            Margin = Padding.Empty,
                            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink,
                            AutoSize = true,
                        };
                        panelLoginRewardsContainer.Controls.Add(panel);
                    }

                    var img = GetLoginRewardIcon(day);
                    var source = imagesLoginRewards.GetValue(img);

                    source.BeginLoad(delegate
                    {
                        return img;
                    });

                    var b = new FlatMarkerIconButton()
                    {
                        Size = buttonLoginRewardsDayTemplate.Size,
                        Margin = buttonLoginRewardsDayTemplate.Margin,
                        Padding = buttonLoginRewardsDayTemplate.Padding,
                        ImageSource = new Tools.Shared.Images.SourceValue(source),
                        Marker = Settings.MarkerIconType.Icon,
                        BackColorSelected = Color.LightSteelBlue,
                        BorderColorSelected = Color.SteelBlue,
                        BorderStyle = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                        Cursor = Windows.Cursors.Hand,
                        Tag = day,
                        PrioritizeSelectedColoring = true,
                        ImageOpacity = 0.5f,
                        ImageGrayscale = true,
                    };

                    b.Click += buttonLoginRewardsDay_Click;

                    panel.Controls.Add(b);
                }
            }

            panelLoginRewardsContainer.PerformLayout(true);
        }

        void buttonLoginRewardsDay_Click(object sender, EventArgs e)
        {
            SelectLoginRewardButton((FlatMarkerIconButton)sender);
        }

        private void SelectLoginRewardButton(FlatMarkerIconButton b)
        {
            if (b != null && b.Selected)
                return;

            if (panelLoginRewardsContainer.Tag != null)
            {
                var bp = ((FlatMarkerIconButton)panelLoginRewardsContainer.Tag);
                bp.Selected = false;
                bp.ImageOpacity = 0.5f;
                bp.ImageGrayscale = true;
            }

            panelLoginRewardsContainer.Tag = b;

            if (b != null)
            {
                b.ImageOpacity = 1;
                b.Selected = true;
                b.ImageGrayscale = false;

                textTrackLoginRewardsDay.Value = (int)b.Tag;
            }
        }

        private void textTrackLoginRewardsDay_Enter(object sender, EventArgs e)
        {
            if (imagesLoginRewards == null)
            {
                imagesLoginRewards = new Tools.Shared.Images(false);
                CreateLoginRewardsButtons();
                panelLoginRewards.SizeChanged += panelLoginRewards_SizeChanged;
            }

            if (!panelLoginRewards.Visible)
            {
                panelLoginRewards.SendToBack();
                panelTrackLoginRewardsDay.LocationChanged += panelTrackLoginRewardsDay_LocationChanged;
                stackPanel18.LocationChanged += panelTrackLoginRewardsDay_LocationChanged;
                SelectLoginRewardButton(GetLoginRewardButton(textTrackLoginRewardsDay.Value));
            }

            MoveLoginRewardsPanel();
            panelLoginRewards.BringToFront();
            panelLoginRewards.Visible = true;
        }

        void panelTrackLoginRewardsDay_LocationChanged(object sender, EventArgs e)
        {
            MoveLoginRewardsPanel();
        }

        void panelLoginRewards_SizeChanged(object sender, EventArgs e)
        {
            if (panelLoginRewards.Visible)
                MoveLoginRewardsPanel();
        }

        private void MoveLoginRewardsPanel()
        {
            var p = stackPanel18.PointFromChild(textTrackLoginRewardsDay);
            var y = p.Y - (panelLoginRewards.Height - textTrackLoginRewardsDay.Height) / 2;
            var top = -panelGeneral.AutoScrollPosition.Y;
            var bottom = top + panelGeneral.Height;

            if (y < top)
            {
                if (top > p.Y)
                    y = p.Y;
                else
                    y = top;
            }
            else if (y + panelLoginRewards.Height > bottom)
            {
                y = p.Y + textTrackLoginRewardsDay.Height;
                if (bottom > y)
                    y = bottom - panelLoginRewards.Height;
                else
                    y -= panelLoginRewards.Height;
            }

            panelLoginRewards.Location = new Point(p.X + textTrackLoginRewardsDay.Width + imageTrackLoginRewardsDay.Margin.Left / 2, y + panelGeneral.AutoScrollPosition.Y);
        }

        private void textTrackLoginRewardsDay_Leave(object sender, EventArgs e)
        {
            panelLoginRewards.Visible = false;
            panelTrackLoginRewardsDay.LocationChanged -= panelTrackLoginRewardsDay_LocationChanged;
            stackPanel18.LocationChanged -= panelTrackLoginRewardsDay_LocationChanged;
        }

        private void checkEnableNetworkAuthorization_CheckedChanged(object sender, EventArgs e)
        {
            checkEnableNetworkAuthorizationRemember.Enabled = checkEnableNetworkAuthorization.Checked;
        }

        private void checkAccountTypeSteam_CheckedChanged(object sender, EventArgs e)
        {
            var b = radioAccountTypeSteam.Checked;

            checkAutomaticLogin.Enabled = !b;
            panelAutomaticLauncherLoginGw2.Visible = panelAutomaticLauncherLoginGw2.Enabled = !b && !checkAutomaticLogin.Checked;
            tableLogin.Visible = tableLogin.Enabled = !b && checkAutomaticLogin.Checked;
        }

        private void checkLaunchSteam_CheckedChanged(object sender, EventArgs e)
        {
            panelLaunchSteamGw2.SuspendLayout();

            labelLaunchSteamBasicWarning.Visible = checkLaunchSteam.Checked;
            labelLaunchSteamFeatureWarning.Visible = checkLaunchSteam.Checked;

            panelLaunchSteamGw2.ResumeLayout();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new UI.Affinity.formAffinitySelectDialog(((AffinityDisplay)contextAffinity.Tag).Affinity))
            {
                f.ShowDialog(this);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new UI.Affinity.formAffinitySelectDialog(Affinity.formAffinitySelectDialog.DialogMode.Select))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    ((AffinityDisplay)contextAffinity.Tag).Affinity = f.SelectedAffinity.Affinity;
                }
            }
        }

        private void adProcessAffinity_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextAffinity.Tag = sender;
                contextAffinity.Show(Cursor.Position);
            }
        }

        private void checkDailyLoginMumbleLink_CheckedChanged(object sender, EventArgs e)
        {
            if (checkDailyLoginMumbleLink.Checked && Settings.Tweaks.DisableMumbleLinkDailyLogin.Value && checkDailyLoginMumbleLink.Focused)
            {
                if (MessageBox.Show(this, "Tracking daily logins using MumbleLink is required and has been disabled under the settings.\n\nEnable the use of MumbleLink?", "Enable use of MumbleLink?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                {
                    Settings.Tweaks.DisableMumbleLinkDailyLogin.Value = false;
                }
                else
                {
                    checkDailyLoginMumbleLink.CheckState = CheckState.Indeterminate;
                }
            }
        }

        private void adBrowserAffinity_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextAffinity.Tag = sender;
                contextAffinity.Show(Cursor.Position);
            }
        }

        private void checkBrowserPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboBrowserPriority.Enabled = checkBrowserPriority.Checked;
        }

        private void checkBrowserAffinityAll_CheckedChanged(object sender, EventArgs e)
        {
            panelBrowserAffinityContainer.Visible = !checkBrowserAffinityAll.Checked;
        }

        private void labelShowWeeklyCompletionApi_Click(object sender, EventArgs e)
        {
            buttonGeneral.SelectPanel(panelApi);
            checkTrackWeeklyCompletionApi.Focus();
        }

        protected override void OnSystemColorsChanged(EventArgs e)
        {
            base.OnSystemColorsChanged(e);

            sidebarPanel1.BackColor = Util.Color.Lighten(this.BackColor, Util.Color.Luminance(this.ForeColor) <= 127 ? 0.9f : 0.1f);
        }
    }
}
