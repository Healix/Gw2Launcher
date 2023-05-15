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
using System.Threading;
using Gw2Launcher.UI.Controls.ListPanel;
using ColorNames = Gw2Launcher.UI.UiColors.Colors;

namespace Gw2Launcher.UI
{
    public partial class formSettings : Base.BaseForm
    {
        private class UiColorsPreviewPanel : ColorPicker.Controls.ColorPreviewPanel
        {
            public UiColors.Colors ColorName
            {
                get;
                set;
            }

            public string Tooltip
            {
                get;
                set;
            }
        }

        private class ColorInfo
        {
            private UiColors.Colors[][] shared;
            private UiColors.ColorValues values;
            private UiColors.ColorValues defaults;
            private bool previewed;

            public UiColors.ColorValues Values
            {
                get
                {
                    if (values == null)
                    {
                        var cv = UiColors.GetTheme();
                        values = new UiColors.ColorValues(cv.BaseTheme, cv.Decompress());
                    }
                    return values;
                }
                set
                {
                    values = value;
                }
            }

            public Dictionary<UiColors.Colors, UiColorsPreviewPanel> Panels
            {
                get;
                set;
            }

            public DataGridViewRow[] Rows
            {
                get;
                set;
            }

            public UiColors.ColorValues Original
            {
                get;
                set;
            }

            public UiColors.ColorValues Defaults
            {
                get
                {
                    if (defaults == null)
                        defaults = UiColors.GetTheme(UiColors.Theme.Settings);
                    return defaults;
                }
                set
                {
                    defaults = value;
                }
            }

            public void SetColor(ColorNames c, Color v, bool applyToPanel = true, bool applyToRows = true)
            {
                Values[c] = v;

                if (applyToPanel)
                {
                    UiColorsPreviewPanel p;
                    if (Panels.TryGetValue(c, out p))
                    {
                        p.Color1 = v;
                    }
                }

                if (applyToRows && Rows != null)
                {
                    Rows[(int)c].Cells[0].Value = v;
                }

                Modified = true;
            }

            public ColorNames[] GetShared(ColorNames c)
            {
                var i = (int)c;

                if (shared == null)
                    shared = UiColors.GetTheme(UiColors.Theme.Light).GetShared();

                if (i < 0 || i > shared.Length || shared[i] == null)
                    return new ColorNames[0];

                return shared[i];
            }

            public void FromPanels()
            {
                if (values == null)
                    return;
                foreach (var p in this.Panels.Values)
                {
                    Values[p.ColorName] = p.Color1;
                }
            }

            /// <summary>
            /// Copies the current values to any attached controls
            /// </summary>
            public void ToControls()
            {
                if (values == null)
                    return;
                
                foreach (var p in this.Panels.Values)
                {
                    p.Color1 = Values[p.ColorName];
                }

                if (Rows != null)
                {
                    for (var i = 0; i < Rows.Length; i++)
                    {
                        Rows[i].Cells[0].Value = Values[(ColorNames)i];
                    }
                }
            }

            /// <summary>
            /// Applies the current colors to the current theme
            /// </summary>
            public void Preview()
            {
                if (values == null)
                    return;
                if (Original == null)
                    Original = UiColors.GetTheme().Clone();
                previewed = true;
                UiColors.SetColors(Values);
            }

            /// <summary>
            /// Resets the current theme to its original values
            /// </summary>
            public void Reset()
            {
                if (previewed && Original != null)
                {
                    UiColors.SetColors(Original);
                }
            }

            public void Save()
            {
                if (values == null)
                    return;
                Original = null;

                if (values.Equals(UiColors.GetTheme(UiColors.Theme.Light)))
                {
                    Settings.StyleColors.Clear();
                }
                else if (values.Equals(UiColors.GetTheme(UiColors.Theme.Dark)))
                {
                    Settings.StyleColors.Value = new Settings.UiColors()
                    {
                        Theme = UiColors.Theme.Dark,
                    };
                }
                else
                {
                    Settings.StyleColors.Value = new Settings.UiColors()
                    {
                        Colors = UiColors.Compress(values),
                        Theme = UiColors.Theme.Settings,
                    };
                }
            }

            public bool Modified
            {
                get;
                set;
            }
        }

        public enum Panels
        {
            General,
            GuildWars2,
            GuildWars1,
            Security,
            Style,
            Tools,
            Updates,
            Backup,
        }

        private enum ArgsState : byte
        {
            Changed,
            Loading,
            Loaded,
            Active
        }

        private Form activeWindow;
        private CheckBox[] checkArgsGw2, checkArgsGw1;
        private ArgsState argsStateGw2, argsStateGw1, encryptionState;
        private ToolTip tooltipColors;
        private CancellationTokenSource cancelPressed;
        private Point locationColorDialog;
        private Dictionary<string, object> iconsRunAfter;
        private formMain owner;
        private Tools.Markers.MarkerIcons markerIcons;
        private UI.Tooltip.FloatingTooltip ftooltip;
        private ColorInfo colorInfo;

        public formSettings(formMain owner)
        {
            InitializeComponents();

            this.owner = owner;
            locationColorDialog = new Point(int.MinValue, int.MinValue);
            iconsRunAfter = new Dictionary<string, object>();

            Util.CheckedButton.Group(radioNetworkVerifyAutomatic, radioNetworkVerifyManual);
            Util.CheckedButton.Group(checkRemoveAllNetworks, checkRemovePreviousNetworks);
            Util.CheckedButton.Group(radioGw2ModeAdvanced, radioGw2ModeBasic);
            Util.CheckedButton.Group(radioLocalizedExecutionFull, radioLocalizedExecutionBinaries);
            Util.CheckedButton.Group(radioLocalizedExecutionAutoSyncBasic, radioLocalizedExecutionAutoSyncAll);
            Util.CheckedButton.Group(checkJumpListOnlyShowDaily, checkJumpListHideActive);
            Util.CheckedButton.Group(checkPreventRelaunchingExit, checkPreventRelaunchingRelaunch);
            Util.CheckedButton.Group(radioLocalizedExecutionAccountsExclude, radioLocalizedExecutionAccountsInclude);

            labelVersionBuild.Text = Program.BUILD + (Environment.Is64BitProcess ? " (64-bit)" : " (32-bit)");
            labelVersionRelease.Text = Program.RELEASE_VERSION.ToString();
            try
            {
                labelVersionDate.Text = string.Format(labelVersionDate.Text, DateTime.FromBinary(Program.RELEASE_TIMESTAMP));
            }
            catch
            {
                labelVersionDate.Text = "-";
            }

            var sbounds = Settings.WindowBounds[typeof(formSettings)];
            if (sbounds.HasValue)
                this.Size = sbounds.Value.Size;

            checkArgsGw2 = InitializeArguments(Settings.AccountType.GuildWars2, panelArgsGw2, labelArgsTemplateHeader, checkArgsTemplate, null, labelArgsTemplateDesc, checkArgsGw2_CheckedChanged);
            checkArgsGw1 = InitializeArguments(Settings.AccountType.GuildWars1, panelArgsGw1, labelArgsTemplateHeader, checkArgsTemplate, null, labelArgsTemplateDesc, checkArgsGw1_CheckedChanged);

            buttonGeneral.Panels = new Panel[] { panelGeneral, panelLaunchConfiguration, panelWindows, panelHotkeys };
            buttonGeneral.SubItems = new string[] { "Launching", "Windows", "Hotkeys" };

            buttonGuildWars2.Panels = new Panel[] { panelGw2, panelLaunchOptionsGw2, panelLaunchConfigurationGw2, panelSteamGw2, panelTweaksGw2,
                                                    panelLaunchOptionsAdvancedGw2 };
            buttonGuildWars2.SubItems = new string[] { "Launch options", "Management", "Steam", "Tweaks" };

            buttonGuildWars1.Panels = new Panel[] { panelGw1, panelLaunchOptionsGw1,
                                                    panelLaunchOptionsAdvancedGw1 };
            buttonGuildWars1.SubItems = new string[] { "Launch options" };

            buttonSecurity.Panels = new Panel[] { panelSecurity, panelPasswords };
            buttonSecurity.SubItems = new string[] { "Windows" };

            buttonStyle.Panels = new Panel[] { panelStyle, panelActions, panelColors };
            buttonStyle.SubItems = new string[] { "Actions", "Colors" };

            buttonTools.Panels = new Panel[] { panelTools, panelAccountBar, panelLocalDat, panelScreenshots };
            buttonTools.SubItems = new string[] { "Account bar", "Local.dat", "Screenshots" };

            buttonUpdates.Panels = new Panel[] { panelUpdates };

            buttonBackup.Panels = new Panel[] { panelBackup };

            sidebarPanel1.Initialize(new SidebarButton[]
                {
                    buttonGeneral,
                    buttonGuildWars2,
                    buttonGuildWars1,
                    buttonSecurity,
                    buttonStyle,
                    buttonTools,
                    buttonUpdates,
                    buttonBackup,
                });

            buttonGeneral.Selected = true;

            InitializeActions();

            if (Settings.GuildWars2.Path.HasValue)
            {
                textGw2Path.Text = Settings.GuildWars2.Path.Value;
            }

            if (Settings.GuildWars2.PathSteam.HasValue)
            {
                textGw2PathSteam.Text = Settings.GuildWars2.PathSteam.Value;
                checkGw2PathSteam.Checked = true;
            }

            if (Settings.GuildWars1.Path.HasValue)
            {
                textGw1Path.Text = Settings.GuildWars1.Path.Value;
            }

            if (Settings.GuildWars2.Arguments.HasValue)
                textGw2Arguments.Text = Settings.GuildWars2.Arguments.Value;

            if (Settings.GuildWars1.Arguments.HasValue)
                textGw1Arguments.Text = Settings.GuildWars1.Arguments.Value;

            checkMinimizeToTray.Checked = Settings.MinimizeToTray.Value;
            checkCloseToTray.Checked = Settings.CloseToTray.Value;
            checkShowTrayIcon.Checked = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            checkBringToFrontOnExit.Checked = Settings.BringToFrontOnExit.Value;
            checkStoreCredentials.Checked = Settings.StoreCredentials.Value;
            checkShowUser.Checked = !Settings.StyleShowAccount.HasValue || Settings.StyleShowAccount.Value;
            checkShowColor.Checked = Settings.StyleShowColor.Value;
            checkStyleMarkFocused.Checked = Settings.StyleHighlightFocused.Value;
            checkStyleShowIcon.Checked = Settings.StyleShowIcon.Value;

            if (Settings.FontName.HasValue)
            {
                buttonSample.FontName = Settings.FontName.Value;
                labelFontTitleReset.Visible = true;
                labelFontTitleResetSep.Visible = true;
            }
            else
                buttonSample.FontName = UI.Controls.AccountGridButton.FONT_NAME;

            if (Settings.FontStatus.HasValue)
            {
                buttonSample.FontStatus = Settings.FontStatus.Value;
                labelFontStatusReset.Visible = true;
                labelFontStatusResetSep.Visible = true;
            }
            else
                buttonSample.FontStatus = UI.Controls.AccountGridButton.FONT_STATUS;

            if (Settings.FontUser.HasValue)
            {
                buttonSample.FontUser = Settings.FontUser.Value;
                labelFontUserReset.Visible = true;
                labelFontUserResetSep.Visible = true;
            }
            else
                buttonSample.FontUser = UI.Controls.AccountGridButton.FONT_USER;

            buttonFontTitle.Text = GetFontName(buttonSample.FontName);
            buttonFontStatus.Text = GetFontName(buttonSample.FontStatus);
            buttonFontUser.Text = GetFontName(buttonSample.FontUser);

            if (Settings.StyleBackgroundImage.HasValue)
            {
                checkStyleBackgroundImage.Tag = Settings.StyleBackgroundImage.Value;
                checkStyleBackgroundImage.Checked = true;
                menuImageBackgroundDefaultToolStripMenuItem.Checked = false;

                try
                {
                    buttonSample.BackgroundImage = Bitmap.FromFile(Settings.StyleBackgroundImage.Value);
                }
                catch
                {
                    buttonSample.BackgroundImage = Util.Bitmap.CreateErrorImage(buttonSample.Width, buttonSample.Height);
                }
            }

            checkCheckBuildOnLaunch.Checked = Settings.CheckForNewBuilds.Value;
            checkAutoUpdate.Checked = Settings.AutoUpdate.Value;
            checkAutoUpdateDownload.Checked = Settings.BackgroundPatchingEnabled.Value;
            if (Settings.BackgroundPatchingNotifications.HasValue)
            {
                checkAutoUpdateDownloadNotifications.Checked = true;
                checkAutoUpdateDownloadNotifications.Tag = Settings.BackgroundPatchingNotifications.Value;
            }
            else
            {
                checkAutoUpdateDownloadNotifications.Tag = new Settings.ScreenAttachment(0, Settings.ScreenAnchor.BottomRight);
            }
            if (Settings.AutoUpdateInterval.HasValue)
                Util.NumericUpDown.SetValue(numericUpdateInterval, Settings.AutoUpdateInterval.Value);

            checkGw2Volume.Checked = Settings.GuildWars2.Volume.HasValue;
            if (Settings.GuildWars2.Volume.HasValue)
                sliderGw2Volume.Value = Settings.GuildWars2.Volume.Value;
            else
                sliderGw2Volume.Value = 1f;

            checkGw1Volume.Checked = Settings.GuildWars1.Volume.HasValue;
            if (Settings.GuildWars1.Volume.HasValue)
                sliderGw1Volume.Value = Settings.GuildWars1.Volume.Value;
            else
                sliderGw1Volume.Value = 1f;

            if (Settings.LastProgramVersion.HasValue)
            {
                checkCheckVersionOnStart.Checked = true;
                labelVersionUpdate.Visible = Settings.LastProgramVersion.Value.Version > Program.RELEASE_VERSION;
            }

            if (Settings.WindowCaption.HasValue)
            {
                textWindowCaption.Text = Settings.WindowCaption.Value;
                checkWindowCaption.Checked = true;
            }

            checkWindowIcon.Checked = Settings.WindowIcon.Value;

            checkPreventTaskbarGrouping.Checked = Settings.PreventTaskbarGrouping.Value;
            checkForceTaskbarGrouping.Checked = Settings.ForceTaskbarGrouping.Value;
            checkTopMost.Checked = Settings.TopMost.Value;

            if (Client.FileManager.IsFolderLinkingSupported)
            {
                if (Settings.GuildWars2.VirtualUserPath.HasValue)
                {
                    checkCustomUsername.Checked = true;
                    textCustomUsername.Text = Settings.GuildWars2.VirtualUserPath.Value;
                }
                else
                {
                    var n = Settings.GuildWars2.VirtualUserPath.ValueCommit;
                    if (!string.IsNullOrEmpty(n))
                        textCustomUsername.Text = n;
                }
            }
            else
            {
                checkCustomUsername.Enabled = false;
                buttonCustomUsername.Enabled = false;
            }

            if (Settings.ActionActiveLClick.HasValue)
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveLClick, Settings.ActionActiveLClick.Value);
            else
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveLClick, Settings.ButtonAction.Focus);

            if (Settings.ActionActiveLPress.HasValue)
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveLPress, Settings.ActionActiveLPress.Value);
            else
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveLPress, Settings.ButtonAction.Close);

            if (Settings.ActionInactiveLClick.HasValue)
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionInactiveLClick, Settings.ActionInactiveLClick.Value);
            else
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionInactiveLClick, Settings.ButtonAction.Launch);

            if (Settings.ActionActiveMClick.HasValue)
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveMClick, Settings.ActionActiveMClick.Value);
            else
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionActiveMClick, Settings.ButtonAction.ShowAuthenticator);

            if (Settings.ActionInactiveMClick.HasValue)
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionInactiveMClick, Settings.ActionInactiveMClick.Value);
            else
                Util.ComboItem<Settings.ButtonAction>.Select(comboActionInactiveMClick, Settings.ButtonAction.ShowAuthenticator);

            checkGw2AutomaticLauncherLogin.Checked = Settings.GuildWars2.AutomaticRememberedLogin.Value;

            if (Settings.GuildWars2.Mute.HasValue)
            {
                var v = Settings.GuildWars2.Mute.Value;
                checkGw2MuteAll.Checked = v.HasFlag(Settings.MuteOptions.All);
                checkGw2MuteMusic.Checked = v.HasFlag(Settings.MuteOptions.Music);
                checkGw2MuteVoices.Checked = v.HasFlag(Settings.MuteOptions.Voices);
            }

            if (Settings.GuildWars1.Mute.HasValue)
            {
                var v = Settings.GuildWars1.Mute.Value;
                checkGw1MuteAll.Checked = v.HasFlag(Settings.MuteOptions.All);
            }

            checkGw2Port80.Checked = Settings.GuildWars2.ClientPort.Value == 80;
            checkGw2Port443.Checked = Settings.GuildWars2.ClientPort.Value == 443;

            checkGw2ScreenshotsBmp.Checked = Settings.GuildWars2.ScreenshotsFormat.Value == Settings.ScreenshotFormat.Bitmap;
            checkGw1ScreenshotsBmp.Checked = Settings.GuildWars1.ScreenshotsFormat.Value == Settings.ScreenshotFormat.Bitmap;

            if (Client.FileManager.IsFolderLinkingSupported)
            {
                if (checkGw2ScreenshotsLocation.Checked = Settings.GuildWars2.ScreenshotsLocation.HasValue)
                {
                    textGw2ScreenshotsLocation.Text = Settings.GuildWars2.ScreenshotsLocation.Value;
                }
                if (checkGw1ScreenshotsLocation.Checked = Settings.GuildWars1.ScreenshotsLocation.HasValue)
                {
                    textGw1ScreenshotsLocation.Text = Settings.GuildWars1.ScreenshotsLocation.Value;
                }
            }
            else
            {
                checkGw2ScreenshotsLocation.Enabled = false;
                buttonGw2ScreenshotsLocation.Enabled = false;
                checkGw1ScreenshotsLocation.Enabled = false;
                buttonGw1ScreenshotsLocation.Enabled = false;
            }

            checkDeleteCacheOnLaunch.Checked = Settings.DeleteCacheOnLaunch.Value;
            checkShowDailies.Checked = Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.Show);
            checkShowDailiesAuto.Checked = Settings.ShowDailies.Value.HasFlag(Settings.DailiesMode.AutoLoad);

            if (Settings.BackgroundPatchingProgress.HasValue)
            {
                checkAutoUpdateDownloadProgress.Checked = true;
                checkAutoUpdateDownloadProgress.Tag = Settings.BackgroundPatchingProgress.Value;
            }
            else
            {
                var screen = Screen.PrimaryScreen.Bounds;
                checkAutoUpdateDownloadProgress.Tag = new Rectangle(screen.Right - 210, 10, 200, 9);
            }

            //if (Settings.NetworkAuthorization.HasValue)
            //{
            //    checkEnableNetworkAuthorization.Checked = true;
            //    var v = Settings.NetworkAuthorization.Value;
            //    switch (v & Settings.NetworkAuthorizationFlags.VerificationModes)
            //    {
            //        case Settings.NetworkAuthorizationFlags.Manual:
            //            radioNetworkVerifyManual.Checked = true;
            //            break;
            //        case Settings.NetworkAuthorizationFlags.Automatic:
            //        default:
            //            radioNetworkVerifyAutomatic.Checked = true;
            //            break;
            //    }
            //    checkRemovePreviousNetworks.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized);
            //    checkRemoveAllNetworks.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll);
            //    checkNetworkAbortOnCancel.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail);
            //    checkNetworkVerifyAutomaticIP.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.VerifyIP);
            //}

            if (Settings.ScreenshotNaming.HasValue)
            {
                checkScreenshotNameFormat.Checked = true;
                comboScreenshotNameFormat.Text = Settings.ScreenshotNaming.Value;
            }
            else
                comboScreenshotNameFormat.SelectedIndex = 0;

            comboScreenshotImageFormat.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.ScreenshotConversionOptions.ImageFormat>(Settings.ScreenshotConversionOptions.ImageFormat.Jpg,"JPEG (*.jpg)"),
                    new Util.ComboItem<Settings.ScreenshotConversionOptions.ImageFormat>(Settings.ScreenshotConversionOptions.ImageFormat.Png,"PNG (*.png)"),
                });

            if (Settings.ScreenshotConversion.HasValue)
            {
                checkScreenshotImageFormat.Checked = true;

                var v = Settings.ScreenshotConversion.Value;
                switch (v.Format)
                {
                    case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                        comboScreenshotImageFormat.SelectedIndex = 0;
                        Util.NumericUpDown.SetValue(numericScreenshotImageFormatJpgQuality, v.Options);
                        break;
                    case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                        comboScreenshotImageFormat.SelectedIndex = 1;
                        if (v.Options == 16)
                            radioScreenshotImageFormat16.Checked = true;
                        else
                            radioScreenshotImageFormat24.Checked = true;
                        break;
                    default:
                        comboScreenshotImageFormat.SelectedIndex = 0;
                        break;
                }
                checkScreenshotDeleteOriginal.Checked = v.DeleteOriginal;
            }
            else
                comboScreenshotImageFormat.SelectedIndex = 0;

            checkDeleteCrashLogsOnLaunch.Checked = Settings.DeleteCrashLogsOnLaunch.Value;

            var priorityValues = new object[]
                {
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.High, "High"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.AboveNormal, "Above normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Normal, "Normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.BelowNormal, "Below normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Low, "Low"),
                };
            comboGw2ProcessPriority.Items.AddRange(priorityValues);
            comboGw2ProcessPriorityDx.Items.AddRange(priorityValues);
            comboGw1ProcessPriority.Items.AddRange(priorityValues);
            comboProcessPriority.Items.AddRange(priorityValues);

            if (Settings.GuildWars2.ProcessPriority.HasValue && Settings.GuildWars2.ProcessPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriority, Settings.GuildWars2.ProcessPriority.Value);
                checkGw2ProcessPriority.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriority, Settings.ProcessPriorityClass.Normal);

            if (Settings.GuildWars2.DxLoadingPriority.HasValue && Settings.GuildWars2.DxLoadingPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriorityDx, Settings.GuildWars2.DxLoadingPriority.Value);
                checkGw2ProcessPriorityDx.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriorityDx, Settings.ProcessPriorityClass.Normal);

            if (Settings.GuildWars1.ProcessPriority.HasValue && Settings.GuildWars1.ProcessPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw1ProcessPriority, Settings.GuildWars1.ProcessPriority.Value);
                checkGw1ProcessPriority.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw1ProcessPriority, Settings.ProcessPriorityClass.Normal);

            if (Settings.ProcessPriority.HasValue && Settings.ProcessPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriority.Value);
                checkProcessPriority.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriorityClass.Normal);

            if (Settings.GuildWars2.ProcessAffinity.HasValue)
            {
                adGw2ProcessAffinity.Affinity = Settings.GuildWars2.ProcessAffinity.Value;
                checkGw2ProcessAffinityAll.Checked = false;
            }
            if (Settings.GuildWars1.ProcessAffinity.HasValue)
            {
                adGw1ProcessAffinity.Affinity = Settings.GuildWars1.ProcessAffinity.Value;
                checkGw1ProcessAffinityAll.Checked = false;
            }

            checkGw2ProcessBoostBrowser.Checked = Settings.GuildWars2.PrioritizeCoherentUI.Value;

            if (Settings.NotesNotifications.HasValue)
            {
                var v = Settings.NotesNotifications.Value;
                checkNoteNotifications.Tag = v;
                checkNoteNotifications.Checked = true;
                checkNoteNotificationsOnlyWhileActive.Checked = v.OnlyWhileActive;
            }
            else
            {
                checkNoteNotifications.Tag = new Settings.NotificationScreenAttachment(0, Settings.ScreenAnchor.BottomRight, false);
            }

            if (Settings.MaxPatchConnections.HasValue)
            {
                checkMaxPatchConnections.Checked = true;
                Util.NumericUpDown.SetValue(numericMaxPatchConnections, Settings.MaxPatchConnections.Value);
            }

            checkAccountBarEnable.Checked = Settings.AccountBar.Enabled.Value;

            if (Settings.LimitActiveAccounts.HasValue)
            {
                checkLimitActiveAccount.Checked = true;
                Util.NumericUpDown.SetValue(numericLimitActiveAccount, Settings.LimitActiveAccounts.Value);
            }

            if (Settings.DelayLaunchSeconds.HasValue)
            {
                checkDelaySeconds.Checked = true;
                Util.NumericUpDown.SetValue(numericDelaySeconds, Settings.DelayLaunchSeconds.Value);
            }

            checkDelayUntilLoaded.Checked = Settings.DelayLaunchUntilLoaded.Value;

            if (Settings.GuildWars2.LocalizeAccountExecution.HasValue)
            {
                var v = Settings.GuildWars2.LocalizeAccountExecution.Value;
                checkLocalizedExecution.Checked = v.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled);
                if (v.HasFlag(Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders))
                    radioLocalizedExecutionBinaries.Checked = true;
                else
                    radioLocalizedExecutionFull.Checked = true;
                checkLocalizedExecutionExcludeUnknown.Checked = v.HasFlag(Settings.LocalizeAccountExecutionOptions.ExcludeUnknownFiles);
                radioLocalizedExecutionAutoSyncAll.Checked = v.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSync);
                checkLocalizedExecutionAutoSyncDelete.Checked = v.HasFlag(Settings.LocalizeAccountExecutionOptions.AutoSyncDeleteUnknowns);
            } 
            radioLocalizedExecutionBinaries.Enabled = BitConverter.IsLittleEndian;

            checkUseDefaultShortcutIcon.Checked = Settings.UseDefaultIconForShortcuts.Value;
            
            checkLocalDatDirectUpdates.Enabled = checkLocalDatCache.Enabled = BitConverter.IsLittleEndian;
            checkLocalDatDirectUpdates.Checked = Settings.GuildWars2.DatUpdaterEnabled.Value;
            checkLocalDatCache.Checked = Settings.GuildWars2.UseCustomGw2Cache.Value;

            checkPreventDefaultCoherentUI.Checked = Settings.GuildWars2.PreventDefaultCoherentUI.Value;
            checkStyleShowCloseAll.Checked = Settings.ShowKillAllAccounts.Value;

            checkRepaintInitialWindow.Checked = Settings.RepaintInitialWindow.Value;
            checkHideInitialWindow.Checked = Settings.HideInitialWindow.Value;

            if (Settings.GuildWars2.MumbleLinkName.HasValue)
            {
                checkGw2MumbleName.Checked = true;
                textGw2MumbleName.Text = Settings.GuildWars2.MumbleLinkName.Value;
            }

            if (Settings.GuildWars2.ProfileMode.HasValue)
            {
                switch (Settings.GuildWars2.ProfileMode.Value)
                {
                    case Settings.ProfileMode.Advanced:

                        radioGw2ModeAdvanced.Checked = true;

                        break;
                    case Settings.ProfileMode.Basic:

                        radioGw2ModeBasic.Checked = true;

                        if (Settings.GuildWars2.ProfileOptions.HasValue)
                        {
                            var o = Settings.GuildWars2.ProfileOptions.Value;
                            checkGw2ModeBasicRestorePath.Checked = o.HasFlag(Settings.ProfileModeOptions.RestoreOriginalPath);
                            checkGw2ModeBasicClearTemporary.Checked = o.HasFlag(Settings.ProfileModeOptions.ClearTemporaryFiles);
                        }

                        break;
                }
            }

            if (!Client.FileManager.IsVirtualModeSupported)
            {
                labelGw2ModeAdvancedWarning1.Visible = true;
                labelGw2ModeAdvancedWarning2.Visible = true;

                if (!Settings.GuildWars2.ProfileMode.HasValue && Client.FileManager.IsBasicModeSupported)
                    radioGw2ModeBasic.Checked = true;
            }

            if (!Client.FileManager.IsBasicModeSupported)
            {
                labelGw2ModeBasicWarning1.Visible = true;
                labelGw2ModeBasicWarning2.Visible = false;
            }

            if (Settings.ScreenshotNotifications.HasValue)
            {
                checkScreenshotNotification.Checked = true;
                checkScreenshotNotification.Tag = Settings.ScreenshotNotifications.Value;
            }
            else
            {
                checkScreenshotNotification.Tag = new Settings.ScreenAttachment(0, Settings.ScreenAnchor.BottomRight);
            }

            if (Settings.StyleColumns.HasValue)
            {
                checkStyleColumns.Checked = true;
                Util.NumericUpDown.SetValue(numericStyleColumns, Settings.StyleColumns.Value);
            }

            buttonSample.SetStatus("sample", AccountGridButton.StatusColors.Default);

            comboEncryptionScope.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.EncryptionScope>(Settings.EncryptionScope.CurrentUser,"User"),
                    new Util.ComboItem<Settings.EncryptionScope>(Settings.EncryptionScope.LocalMachine,"PC"),
                    new Util.ComboItem<Settings.EncryptionScope>(Settings.EncryptionScope.Portable,"Portable"),
                    new Util.ComboItem<Settings.EncryptionScope>(Settings.EncryptionScope.Unencrypted,"Unencrypted"),
                });

            Util.ComboItem<Settings.EncryptionScope>.Select(comboEncryptionScope, Settings.Encryption.HasValue ? Settings.Encryption.Value.Scope : Settings.EncryptionScope.CurrentUser);
            encryptionState = ArgsState.Loading;

            checkLaunchBehindWindows.Checked = Settings.LaunchBehindOtherAccounts.Value;

            if (Settings.LaunchLimiter.HasValue)
            {
                var v = Settings.LaunchLimiter.Value;
                if (v.IsAutomatic)
                {
                    radioLimiterAutomatic.Checked = true;
                }
                else
                {
                    radioLimiterManual.Checked = true;
                    Util.NumericUpDown.SetValue(numericLaunchLimiterAmount, v.Count);
                    Util.NumericUpDown.SetValue(numericLaunchLimiterRechargeAmount, v.RechargeCount);
                    Util.NumericUpDown.SetValue(numericLaunchLimiterRechargeTime, v.RechargeTime);
                }
                checkLaunchLimiter.Checked = true;
            }

            checkStyleShowLaunchAll.Checked = Settings.ShowLaunchAllAccounts.Value;

            if (Settings.LaunchTimeout.HasValue)
            {
                checkTimeoutRelaunch.Checked = true;
                Util.NumericUpDown.SetValue(numericTimeoutRelaunch, Settings.LaunchTimeout.Value);
            }

            checkJumpList.Enabled = Windows.JumpList.IsSupported;
            if (Settings.JumpList.HasValue && checkJumpList.Enabled)
            {
                var v = Settings.JumpList.Value;
                if (v.HasFlag(Settings.JumpListOptions.Enabled))
                {
                    checkJumpList.Checked = true;
                    if (v.HasFlag(Settings.JumpListOptions.OnlyShowDaily))
                        checkJumpListOnlyShowDaily.Checked = true;
                    else if (v.HasFlag(Settings.JumpListOptions.OnlyShowInactive))
                        checkJumpListHideActive.Checked = true;
                }
            }

            checkPreventMinimizing.Checked = Settings.PreventTaskbarMinimize.Value;

            if (Settings.GuildWars2.PreventRelaunching.HasValue)
            {
                switch (Settings.GuildWars2.PreventRelaunching.Value)
                {
                    case Settings.RelaunchOptions.Exit:
                        checkPreventRelaunchingExit.Checked = true;
                        break;
                    case Settings.RelaunchOptions.Relaunch:
                        checkPreventRelaunchingRelaunch.Checked = true;
                        break;
                }
            }

            checkManualAuthenticator.Checked = Settings.AuthenticatorPastingEnabled.Value;

            if (Settings.GuildWars2.RunAfter.HasValue)
            {
                var ra = Settings.GuildWars2.RunAfter.Value;
                panelGw2RunAfterPrograms.SuspendLayout();
                foreach (var r in ra)
                {
                    CreateRunAfterButton(r, panelGw2RunAfterPrograms, labelGw2RunAfterProgramsAdd, labelRunAfterProgram_Click);
                }
                panelGw2RunAfterPrograms.ResumeLayout();
            }

            if (Settings.GuildWars1.RunAfter.HasValue)
            {
                var ra = Settings.GuildWars1.RunAfter.Value;
                panelGw1RunAfterPrograms.SuspendLayout();
                foreach (var r in ra)
                {
                    CreateRunAfterButton(r, panelGw1RunAfterPrograms, labelGw1RunAfterProgramsAdd, labelRunAfterProgram_Click);
                }
                panelGw1RunAfterPrograms.ResumeLayout();
            }

            panelHotkeysHotkeys.HotkeyClick += panelHotkeysHotkeys_HotkeyClick;
            panelHotkeysHotkeys.TemplateHeader = label28;
            panelHotkeysHotkeys.TemplateText = labelHotkeysAdd;
            panelHotkeysHotkeys.TemplateKey = label214;
            panelHotkeysHotkeys.EnableGrouping = true;

            checkHotkeysEnable.Checked = Settings.HotkeysEnabled.Value;

            if (Settings.StyleOffsets.HasValue)
            {
                var offsets = Settings.StyleOffsets.Value.Clone();

                Util.NumericUpDown.SetValue(numericStyleOffsetNameX, offsets[Settings.AccountGridButtonOffsets.Offsets.Name].X);
                Util.NumericUpDown.SetValue(numericStyleOffsetNameY, offsets[Settings.AccountGridButtonOffsets.Offsets.Name].Y);
                Util.NumericUpDown.SetValue(numericStyleOffsetStatusX, offsets[Settings.AccountGridButtonOffsets.Offsets.Status].X);
                Util.NumericUpDown.SetValue(numericStyleOffsetStatusY, offsets[Settings.AccountGridButtonOffsets.Offsets.Status].Y);
                Util.NumericUpDown.SetValue(numericStyleOffsetUserX, offsets[Settings.AccountGridButtonOffsets.Offsets.User].X);
                Util.NumericUpDown.SetValue(numericStyleOffsetUserY, offsets[Settings.AccountGridButtonOffsets.Offsets.User].Y);

                if (offsets.Height > 0)
                {
                    checkStyleHeight.Checked = true;
                    Util.NumericUpDown.SetValue(numericStyleHeight, offsets.Height);
                }

                if (Settings.StyleGridSpacing.HasValue)
                {
                    checkStyleSpacing.Checked = true;
                    Util.NumericUpDown.SetValue(numericStyleSpacing, Settings.StyleGridSpacing.Value);
                }

                buttonSample.Offsets = offsets;
                checkStyleCustomizeOffsets.Checked = true;
            }

            checkStyleDisableShadows.Checked = Settings.StyleDisableWindowShadows.Value;
            checkStyleDisableSearchOnKeyPress.Checked = Settings.StyleDisableSearchOnKeyPress.Value;
            checkStyleDisablePageOnKeyPress.Checked = Settings.StyleDisablePageOnKeyPress.Value;
            checkStyleShowTemplatesToggle.Checked = Settings.ShowWindowTemplatesToggle.Value;
            checkStyleShowMinimizeRestoreAll.Checked = Settings.ShowMinimizeRestoreAll.Value;
            checkStyleShowAccountBarToggle.Checked = Settings.ShowAccountBarToggle.Value;

            checkStyleShowLoginRewardIcon.Checked = Settings.StyleShowDailyLoginDay.Value != Settings.DailyLoginDayIconFlags.None;
            checkStyleMarkActive.Checked = Settings.StyleHighlightActive.Value;

            if (Settings.DxTimeout.HasValue)
            {
                checkDxTimeoutRelaunch.Checked = true;
                Util.NumericUpDown.SetValue(numericDxTimeoutRelaunch, Settings.DxTimeout.Value);
            }

            comboShowDailiesLang.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.Language>(Settings.Language.EN, "EN"),
                    new Util.ComboItem<Settings.Language>(Settings.Language.DE, "DE"),
                    new Util.ComboItem<Settings.Language>(Settings.Language.ES, "ES"),
                    new Util.ComboItem<Settings.Language>(Settings.Language.FR, "FR"),
                    new Util.ComboItem<Settings.Language>(Settings.Language.ZH, "ZH"),
                });

            Util.ComboItem<Settings.Language>.Select(comboShowDailiesLang, Settings.ShowDailiesLanguage.Value);

            comboTweakEmailMethod.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Clipboard, "Pasted via clipboard"),
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Post, "Characters via post"),
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Send, "Characters via send"),
                });
            
            comboTweakPasswordMethod.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Clipboard, "Pasted via clipboard"),
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Post, "Characters via post"),
                    new Util.ComboItem<Settings.LoginInputType>(Settings.LoginInputType.Send, "Characters via send"),
                });

            if (Settings.Tweaks.Login.HasValue)
            {
                checkTweakLogin.Checked = true;

                var v = Settings.Tweaks.Login.Value;

                Util.ComboItem<Settings.LoginInputType>.Select(comboTweakEmailMethod, v.Email);
                Util.ComboItem<Settings.LoginInputType>.Select(comboTweakPasswordMethod, v.Password);

                checkTweakLoginVerify.Checked = v.Verify;
                Util.NumericUpDown.SetValue(numericTweakLoginDelay, v.Delay);
            }
            else
            {
                Util.ComboItem<Settings.LoginInputType>.Select(comboTweakEmailMethod, Settings.LoginInputType.Clipboard);
                Util.ComboItem<Settings.LoginInputType>.Select(comboTweakPasswordMethod, Settings.LoginInputType.Post);
            }

            if (Settings.Tweaks.Launcher.HasValue)
            {
                checkTweakLauncher.Checked = true;

                var v = Settings.Tweaks.Launcher.Value;

                checkTweakLauncherCoherentLoad.Checked = v.WaitForCoherentLoaded;
                checkTweakLauncherCoherentMemory.Checked = v.WaitForCoherentMemory;
                Util.NumericUpDown.SetValue(numericTweakLauncherDelay, v.Delay);
            }

            if (Settings.Network.HasValue)
            {
                var v = Settings.Network.Value;

                checkNetworkEnable.Checked = (v & Settings.NetworkOptions.Enabled) != 0;
                checkNetworkExact.Checked = (v & Settings.NetworkOptions.Exact) != 0;
                checkNetworkWarnOnChange.Checked = (v & Settings.NetworkOptions.WarnOnChange) != 0;
            }

            if (Settings.Steam.Path.HasValue)
            {
                checkSteamPath.Checked = true;
                textSteamPath.Text = Settings.Steam.Path.Value;
            }

            if (Settings.Steam.Timeout.HasValue)
            {
                Util.NumericUpDown.SetValue(numericSteamTimeout, Settings.Steam.Timeout.Value);
            }

            if (Settings.Steam.Limitation.HasValue)
            {
                switch (Settings.Steam.Limitation.Value)
                {
                    case Settings.SteamLimitation.OnlyBlockSteam:

                        radioSteamLimitSteam.Checked = true;

                        break;
                    case Settings.SteamLimitation.BlockAll:

                        radioSteamLimitAll.Checked = true;

                        break;
                    case Settings.SteamLimitation.LaunchWithoutSteam:

                        radioSteamLimitLaunchWithout.Checked = true;

                        break;
                }
            }

            if (Settings.Tweaks.DisableMumbleLinkDailyLogin.Value)
            {
                checkTweakDailyLogin.Checked = false;
            }

            if (Settings.GuildWars2.LocalizeAccountExecutionSelection.HasValue)
            {
                var b = Settings.GuildWars2.LocalizeAccountExecutionSelection.Value == Settings.LocalizeAccountExecutionSelectionOptions.Include;
                radioLocalizedExecutionAccountsInclude.Checked = b;
                radioLocalizedExecutionAccountsExclude.Checked = !b;
            }

            checkStyleShowLaunchDaily.Checked = Settings.ShowLaunchDailyAccounts.Value;
            checkStyleHideExit.Checked = Settings.HideExit.Value;
            checkStyleHideMinimize.Checked = Settings.HideMinimize.Value;









            argsStateGw2 = ArgsState.Changed;
            argsStateGw1 = ArgsState.Changed;
            panelLaunchOptionsAdvancedGw2.PreVisiblePropertyChanged += panelLaunchOptionsAdvancedGw2_PreVisiblePropertyChanged;
            panelLaunchOptionsAdvancedGw1.PreVisiblePropertyChanged += panelLaunchOptionsAdvancedGw1_PreVisiblePropertyChanged;
            panelHotkeys.PreVisiblePropertyChanged += panelHotkeys_PreVisiblePropertyChanged;
            panelStyle.PreVisiblePropertyChanged += panelStyle_PreVisiblePropertyChanged;
            panelSecurity.PreVisiblePropertyChanged += panelSecurity_PreVisiblePropertyChanged;
            panelStyle.PreVisiblePropertyChanged += OnSamplePreVisiblePropertyChanged;
            panelColors.PreVisiblePropertyChanged += OnSamplePreVisiblePropertyChanged;
            buttonSample.SizeChanged += buttonSample_SizeChanged;
        }

        void panelSecurity_PreVisiblePropertyChanged(object sender, bool e)
        {
            panelSecurity.PreVisiblePropertyChanged -= panelSecurity_PreVisiblePropertyChanged;

            if (Settings.IPAddresses.HasValue)
            {
                var addresses = Settings.IPAddresses.Value;
                var sb = new StringBuilder(addresses.Length * 20);

                foreach (var a in addresses)
                {
                    a.ToString(sb);
                    sb.AppendLine();
                }

                textNetworkAddresses.Text = sb.ToString();
            }

            textNetworkAddresses.TextChanged += textNetworkAddresses_TextChanged;
        }

        void panelStyle_PreVisiblePropertyChanged(object sender, bool e)
        {
            panelStyle.PreVisiblePropertyChanged -= panelStyle_PreVisiblePropertyChanged;

            panelStyleLoginRewardIcons.SuspendLayout();

            var flags = new Settings.DailyLoginDayIconFlags[]
            {
                Settings.DailyLoginDayIconFlags.MysticCoins,
                Settings.DailyLoginDayIconFlags.Laurels,
                Settings.DailyLoginDayIconFlags.Luck,
                Settings.DailyLoginDayIconFlags.BlackLionGoods,
                Settings.DailyLoginDayIconFlags.CraftingMaterials,
                Settings.DailyLoginDayIconFlags.Experience,
                Settings.DailyLoginDayIconFlags.ExoticEquipment,
                Settings.DailyLoginDayIconFlags.CelebrationBooster,
                Settings.DailyLoginDayIconFlags.TransmutationCharge,
                Settings.DailyLoginDayIconFlags.ChestOfLoyalty,
            };

            var icons = new Image[]
            {
                Properties.Resources.loginreward0,
                Properties.Resources.loginreward1,
                Properties.Resources.loginreward2,
                Properties.Resources.loginreward3,
                Properties.Resources.loginreward4,
                Properties.Resources.loginreward5,
                Properties.Resources.loginreward6,
                Properties.Resources.loginreward7,
                Properties.Resources.loginreward8,
                Properties.Resources.loginreward9,
            };

            var v = Settings.StyleShowDailyLoginDay.Value;
            if (v == Settings.DailyLoginDayIconFlags.None)
                v = ~Settings.DailyLoginDayIconFlags.None;

            checkStyleLoginRewardIconsNumber.Checked = (v & Settings.DailyLoginDayIconFlags.ShowDay) != 0;

            var sz = Scale(icons[0].Width);

            for (var i = 0; i < flags.Length; i++)
            {
                var check = new CheckBox()
                {
                    Checked = (v & flags[i]) != 0,
                    AutoSize = true,
                    Margin = Padding.Empty,
                    Tag = flags[i],
                    Anchor = AnchorStyles.Left,
                };

                var icon = new PictureBox()
                {
                    Image = icons[i],
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Size = new Size(sz, sz),
                    Anchor = AnchorStyles.Left,
                };

                var panel = new StackPanel()
                {
                    Margin = checkStyleLoginRewardIconsNumber.Margin,
                    AutoSize = true,
                    AutoSizeFill = StackPanel.AutoSizeFillMode.NoWrap,
                    FlowDirection = FlowDirection.LeftToRight,
                    Tag = check
                };

                var onClick = new EventHandler(
                    delegate
                    {
                        check.Checked = !check.Checked;
                    });

                icon.Click += onClick;
                panel.Click += onClick;

                panel.Controls.AddRange(new Control[] { check, icon });
                panelStyleLoginRewardIcons.Controls.Add(panel);
            }

            panelStyleLoginRewardIcons.ResumeLayout();
        }

        void buttonSample_SizeChanged(object sender, EventArgs e)
        {
            if (!checkStyleHeight.Checked)
                Util.NumericUpDown.SetValue(numericStyleHeight, buttonSample.Height);

            panelColorsAccountSample.Size = buttonSample.Size;
            panelStyleSample.Size = buttonSample.Size;
        }

        void panelHotkeys_PreVisiblePropertyChanged(object sender, bool e)
        {
            panelHotkeys.PreVisiblePropertyChanged -= panelHotkeys_PreVisiblePropertyChanged;

            panelHotkeysHotkeys.SuspendLayout();
            if (Settings.Hotkeys.HasValue)
            {
                var hotkeys = Settings.Hotkeys.Value;
                foreach (var h in hotkeys)
                {
                    CreateHotkeyButton(h);
                }
            }
            foreach (var a in Util.Accounts.GetAccounts())
            {
                if (a.Hotkeys != null)
                {
                    foreach (var h in a.Hotkeys)
                    {
                        CreateHotkeyButton(h, a);
                    }
                }
            }
            panelHotkeysHotkeys.ResumeLayout();
            panelHotkeysHotkeys.ResetModified();
        }

        protected override void OnInitializeComponents()
        {
            base.OnInitializeComponents();

            InitializeComponent();

            Control p;

            p = labelScreenshotNameFormatSample.Parent;
            p.Margin = new System.Windows.Forms.Padding(comboScreenshotNameFormat.Left - 2, p.Margin.Top, p.Margin.Right, p.Margin.Bottom);

            p = panelScreenshotImageFormatJpg.Parent;
            p.Margin = new System.Windows.Forms.Padding(comboScreenshotImageFormat.Left - 2, p.Margin.Top, p.Margin.Right, p.Margin.Bottom);
        }

        private int GetScopeWeigth(Settings.EncryptionScope scope)
        {
            switch (scope)
            {
                case Settings.EncryptionScope.CurrentUser:
                    return 0;
                case Settings.EncryptionScope.LocalMachine:
                    return 1;
                case Settings.EncryptionScope.Portable:
                    return 2;
                case Settings.EncryptionScope.Unencrypted:
                    return 255;
                default:
                    throw new NotSupportedException();
            }
        }

        private void InitializeActions()
        {
            var none = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.None, "None");
            var focus = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Focus, "Focus");
            var focusCopyAuth = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.FocusAndCopyAuthenticator, "Focus + authenticator");
            var copyAuth = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.CopyAuthenticator, "Authenticator (copy)");
            var showAuth = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.ShowAuthenticator, "Authenticator (show only)");
            var close = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Close, "Terminate");
            var launch = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Launch, "Launch");
            var launchNormal = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.LaunchSingle, "Launch (normal)");
            
            comboActionActiveLClick.Items.AddRange(new object[]
                {
                    none,
                    focus,
                    focusCopyAuth,
                    copyAuth,
                    showAuth,
                    close
                });

            comboActionActiveMClick.Items.AddRange(new object[]
                {
                    none,
                    focus,
                    focusCopyAuth,
                    copyAuth,
                    showAuth,
                    close
                });

            comboActionActiveLPress.Items.AddRange(new object[]
                {
                    none,
                    close
                });

            comboActionInactiveLClick.Items.AddRange(new object[]
                {
                    none,
                    launch,
                    launchNormal,
                });

            comboActionInactiveMClick.Items.AddRange(new object[]
                {
                    none,
                    launch,
                    launchNormal,
                    copyAuth,
                    showAuth,
                });
        }

        public static CheckBox[] InitializeArguments(Settings.AccountType type, Panel container, Label templateHeader, CheckBox templateCheck, Label templateSwitch, Label templateDescription, EventHandler onCheckChanged)
        {
            string[] categories;
            string[][][] args;

            if (type == Settings.AccountType.GuildWars1)
            {
                categories = new string[]
                {
                    "Graphics",
                    "Interface",
                    "Compatibility",
                    "General"
                };

                args = new string[][][]
                {
                    new string[][] //graphics
                    {
                        new string[] { "-lodfull", "Enables 3D models on distant objects", "" },
                    },
                    new string[][] //ui
                    {
                        new string[] { "-noui", "Launches with the UI hidden", "" },
                        new string[] { "-perf", "Displays performance information", ""},
                    },
                    new string[][] //compatibility
                    {
                        new string[] { "-dsound", "Enables DirectSound audio", "" },
                        new string[] { "-dx8", "Enables DirectX 8", "" },
                        new string[] { "-mce", "Windows Media Center compatibility", "" },
                        new string[] { "-noshaders", "Disables shaders", "" },
                        new string[] { "-sndasio", "ASIO sound driver", "" },
                        new string[] { "-sndwinmm", "Windows Multimedia sound driver", "" },
                    },
                    new string[][] //other
                    {
                        new string[] { "-prefresetlocal", "Restores default settings", ""}
                    },
                };
            }
            else
            {
                categories = new string[]
                {
                    "Graphics",
                    "Interface",
                    "Compatibility",
                    "General"
                };

                args = new string[][][]
                {
                    new string[][] //graphics
                    {
                        new string[] { "-dx9", "Forces DX9 renderer", "" },
                        new string[] { "-dx11", "Forces DX11 renderer", "" },
                        new string[] { "-dx9single", "Limits DX9 renderer to a single thread", "" },
                        new string[] { "-forwardrenderer", "Enables forward rendering", "" }, //"Primarily affects how lighting is applied and limits the maximum number of light sources" },
                        new string[] { "-useoldfov", "Reverts to the original field-of-view", ""},
                        new string[] { "-umbra gpu", "Enables Umbra's GPU accelerated culling", "" },
                    },
                    new string[][] //ui
                    {
                        new string[] { "-noui", "Launches with the UI hidden", "" },
                        new string[] { "-uispanallmonitors", "Spans the UI across all monitors", "" }, //"For use with widescreen multi-panel displays"},
                    },
                    new string[][] //compatibility
                    {
                        new string[] { "-32", "Disables automatically switching to the 64-bit client", "" }, //"Used with the 32-bit client to prevent it from automatically switching to the 64-bit client on a 64-bit OS" },
                        new string[] { "-mce", "Windows Media Center compatibility", "" },
                        new string[] { "-nodelta", "Disables delta patching", "" }, //"Updated files will be downloaded in full rather than only downloading the part that has been changed" },
                    },
                    new string[][] //other
                    {
                        new string[] { "-maploadinfo", "Shows additional information while loading", "" }, //"Shows additional information such as the progress, elapsed time and server IP"},
                        new string[] { "-prefreset", "Restores default settings", "" }, //"Resets all in-game options to their default settings"}
                    },
                };
            }

            var c = InitializeArguments(categories, args, container, templateHeader, templateCheck, templateSwitch, templateDescription, onCheckChanged);

            EventHandler onDxCheckChanged = delegate(object o, EventArgs e)
            {
                var cb = (CheckBox)o;
                if (cb.Focused && cb.Checked)
                {
                    if (cb == c[0])
                        c[1].Checked = false;
                    else
                        c[0].Checked = false;
                }
            };
            c[0].CheckedChanged += onDxCheckChanged;
            c[1].CheckedChanged += onDxCheckChanged;

            return c;
        }

        public static CheckBox[] InitializeArguments(string[] categories, string[][][] args, Panel container, Label templateHeader, CheckBox templateCheck, Label templateSwitch, Label templateDescription, EventHandler onCheckChanged)
        {
            int count = categories.Length;
            int checks = 0;

            foreach (var items in args)
            {
                foreach (var item in items)
                {
                    count += item.Length;

                    if (string.IsNullOrEmpty(item[2]))
                        count--;
                }

                checks += items.Length;
            }

            var _checks = new CheckBox[checks];
            int c = 0, _c = 0;

            container.SuspendLayout();

            foreach (var items in args)
            {
                var l = new Label()
                {
                    Text = categories[c++],
                    Font = templateHeader.Font,
                    Margin = templateHeader.Margin,
                    AutoSize = true,
                };

                container.Controls.Add(l);

                foreach (var item in items)
                {
                    var check = new CheckBox()
                    {
                        Text = item[0],
                        Font = templateCheck.Font,
                        Margin = templateCheck.Margin,
                        AutoSize = true,
                        Tag = item[0]
                    };

                    check.CheckedChanged += onCheckChanged;

                    _checks[_c++] = check;

                    container.Controls.Add(check);

                    if (!string.IsNullOrEmpty(item[1]))
                    {
                        l = new Label()
                        {
                            Text = item[1],
                            Margin = templateDescription.Margin,
                            Font = templateDescription.Font,
                            ForeColor = templateDescription.ForeColor,
                            AutoSize = true,
                        };

                        container.Controls.Add(l);
                    }
                }
            }

            container.ResumeLayout();

            return _checks;
        }

        public AccountGridButton SampleButton
        {
            get
            {
                return buttonSample;
            }
        }

        private void OnLaunchOptionsAdvancedPreVisibleChanged(bool visible, ref ArgsState state, TextBox text, CheckBox[] checks)
        {
            if (visible)
            {
                if (state == ArgsState.Changed)
                {
                    state = ArgsState.Loading;

                    foreach (var check in checks)
                    {
                        var arg = check.Tag as string;
                        if (arg != null)
                            check.Checked = Util.Args.Contains(text.Text, arg.Substring(1));
                    }

                    state = ArgsState.Active;
                }
            }
            else if (state == ArgsState.Active)
            {
                state = ArgsState.Loaded;
                text.TextChanged += textArguments_TextChanged;
            }
        }

        void panelLaunchOptionsAdvancedGw1_PreVisiblePropertyChanged(object sender, bool e)
        {
            OnLaunchOptionsAdvancedPreVisibleChanged(e, ref argsStateGw1, textGw1Arguments, checkArgsGw1);
        }

        void panelLaunchOptionsAdvancedGw2_PreVisiblePropertyChanged(object sender, bool e)
        {
            OnLaunchOptionsAdvancedPreVisibleChanged(e, ref argsStateGw2, textGw2Arguments, checkArgsGw2);
        }

        void textArguments_TextChanged(object sender, EventArgs e)
        {
            var t = (TextBox)sender;
            t.TextChanged -= textArguments_TextChanged;
            if (t == textGw2Arguments)
                argsStateGw2 = ArgsState.Changed;
            else if (t == textGw1Arguments)
                argsStateGw1 = ArgsState.Changed;
        }

        private void OnArgsChanged(ArgsState state, CheckBox check, TextBox text)
        {
            if (state == ArgsState.Loading)
                return;

            var arg = check.Tag as string;

            if (arg != null)
            {
                string _arg;
                if (check.Checked)
                    _arg = arg;
                else
                    _arg = string.Empty;
                text.Text = Util.Args.AddOrReplace(text.Text, arg.Substring(1), _arg);
            }
        }

        void checkArgsGw2_CheckedChanged(object sender, EventArgs e)
        {
            OnArgsChanged(argsStateGw2, (CheckBox)sender, textGw2Arguments);
        }

        void checkArgsGw1_CheckedChanged(object sender, EventArgs e)
        {
            OnArgsChanged(argsStateGw1, (CheckBox)sender, textGw1Arguments);
        }

        private void buttonClearPasswords_Click(object sender, EventArgs e)
        {
            try
            {
                Security.Credentials.Clear();
                MessageBox.Show(this, "All stored passwords have been cleared", "Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
                MessageBox.Show(this, "Unable to clear all stored passwords\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void formSettings_Load(object sender, EventArgs e)
        {

        }

        private void buttonGw2Path_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
                if (Environment.Is64BitOperatingSystem)
                    f.Title = "Open Gw2-64.exe";
                else
                    f.Title = "Open Gw2.exe";

                if (textGw2Path.TextLength != 0)
                {
                    try
                    {
                        f.InitialDirectory = System.IO.Path.GetDirectoryName(textGw2Path.Text);
                    }
                    catch { }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    var bits = Util.FileUtil.GetExecutableBits(f.FileName);
                    if (Environment.Is64BitOperatingSystem && bits == 32)
                    {
                        if (!Util.Args.Contains(textGw2Arguments.Text, "32"))
                        {
                            if (MessageBox.Show(this, "You've selected to use the 32-bit version of Guild Wars 2 on a 64-bit system.\n\nAre you sure?", "32-bit?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                                return;
                            textGw2Arguments.Text = Util.Args.AddOrReplace(textGw2Arguments.Text, "32", "-32");
                        }
                    }
                    else if (bits == 64 && Util.Args.Contains(textGw2Arguments.Text, "32"))
                    {
                        textGw2Arguments.Text = Util.Args.AddOrReplace(textGw2Arguments.Text, "32", "");
                    }
                    textGw2Path.Text = f.FileName;
                    textGw2Path.Select(textGw2Path.TextLength, 0);

                    try
                    {
                        var path = Path.GetDirectoryName(f.FileName);
                        if (!Util.FileUtil.HasFolderPermissions(path, System.Security.AccessControl.FileSystemRights.Modify) && !Util.FileUtil.CanWriteToFolder(path))
                        {
                            if (MessageBox.Show(this, "The selected path will require elevated permissions to modify.\n\nAllow normal access to the folder?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                                Util.ProcessUtil.CreateFolder(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }

                    //try
                    //{
                    //    var path = Path.GetDirectoryName(f.FileName);
                    //    var programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    //    if (!string.IsNullOrEmpty(programfiles) && path.StartsWith(programfiles, StringComparison.OrdinalIgnoreCase))
                    //    {
                    //        if (!Util.FileUtil.HasFolderPermissions(path, System.Security.AccessControl.FileSystemRights.Modify))
                    //        {
                    //            if (MessageBox.Show(this, "Program Files is a protected directory and will require administrator permissions to modify.\n\nAllow access to the \"" + Path.GetFileName(path) + "\" folder?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                    //                Util.ProcessUtil.CreateFolder(path);
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    Util.Logging.Log(ex);
                    //}
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (activeWindow != null && !activeWindow.IsDisposed)
                activeWindow.Close();

            if (radioGw2ModeBasic.Checked != (Settings.GuildWars2.ProfileMode.Value == Settings.ProfileMode.Basic))
            {
                while (Client.Launcher.GetActiveProcessCount(Client.Launcher.AccountType.GuildWars2) != 0)
                {
                    if (MessageBox.Show(this, "Unable to change launch mode while Guild Wars 2 is active", "Close GW2 to continue", MessageBoxButtons.RetryCancel, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Retry)
                        return;
                }
            }

            if (checkLocalizedExecution.Checked && radioLocalizedExecutionBinaries.Checked && !checkLocalDatCache.Checked)
            {
                if (MessageBox.Show(this, "Using localized execution with only bin folders requires enabling a static cache location.\n\nDisable localized execution and continue?", "Disable localized execution?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != System.Windows.Forms.DialogResult.Yes)
                {
                    buttonGuildWars2.SelectPanel(panelLaunchConfigurationGw2);
                    return;
                }

                checkLocalizedExecution.Checked = false;
            }

            if (!string.IsNullOrEmpty(textGw2Path.Text))
                Settings.GuildWars2.Path.Value = textGw2Path.Text;
            else
                Settings.GuildWars2.Path.Clear();

            if (checkGw2PathSteam.Checked && !string.IsNullOrEmpty(textGw2PathSteam.Text))
                Settings.GuildWars2.PathSteam.Value = textGw2PathSteam.Text;
            else
                Settings.GuildWars2.PathSteam.Clear();

            if (!string.IsNullOrEmpty(textGw1Path.Text))
                Settings.GuildWars1.Path.Value = textGw1Path.Text;
            else
                Settings.GuildWars1.Path.Clear();

            Settings.GuildWars2.Arguments.Value = textGw2Arguments.Text;
            Settings.GuildWars1.Arguments.Value = textGw1Arguments.Text;

            Settings.MinimizeToTray.Value = checkMinimizeToTray.Checked;
            Settings.CloseToTray.Value = checkCloseToTray.Checked;
            Settings.ShowTray.Value = checkShowTrayIcon.Checked;
            Settings.BringToFrontOnExit.Value = checkBringToFrontOnExit.Checked;
            Settings.StoreCredentials.Value = checkStoreCredentials.Checked;
            Settings.CheckForNewBuilds.Value = checkCheckBuildOnLaunch.Checked;

            Settings.StyleShowAccount.Value = checkShowUser.Checked;
            Settings.StyleShowColor.Value = checkShowColor.Checked;
            Settings.StyleHighlightFocused.Value = checkStyleMarkFocused.Checked;
            Settings.StyleShowIcon.Value = checkStyleShowIcon.Checked;

            if (buttonSample.FontName.Equals(UI.Controls.AccountGridButton.FONT_NAME))
                Settings.FontName.Clear();
            else
                Settings.FontName.Value = buttonSample.FontName;

            if (buttonSample.FontStatus.Equals(UI.Controls.AccountGridButton.FONT_STATUS))
                Settings.FontStatus.Clear();
            else
                Settings.FontStatus.Value = buttonSample.FontStatus;

            if (buttonSample.FontUser.Equals(UI.Controls.AccountGridButton.FONT_USER))
                Settings.FontUser.Clear();
            else
                Settings.FontUser.Value = buttonSample.FontUser;

            if (checkGw2Volume.Checked)
                Settings.GuildWars2.Volume.Value = sliderGw2Volume.Value;
            else
                Settings.GuildWars2.Volume.Clear();

            if (checkGw1Volume.Checked)
                Settings.GuildWars1.Volume.Value = sliderGw1Volume.Value;
            else
                Settings.GuildWars1.Volume.Clear();

            Settings.AutoUpdateInterval.Value = (ushort)numericUpdateInterval.Value;
            Settings.AutoUpdate.Value = checkAutoUpdate.Checked;
            Settings.BackgroundPatchingEnabled.Value = checkAutoUpdateDownload.Checked;

            if (checkAutoUpdateDownloadNotifications.Checked)
                Settings.BackgroundPatchingNotifications.Value = (Settings.ScreenAttachment)checkAutoUpdateDownloadNotifications.Tag;
            else
                Settings.BackgroundPatchingNotifications.Clear();

            if (checkCheckVersionOnStart.Checked)
            {
                if (!Settings.LastProgramVersion.HasValue)
                    Settings.LastProgramVersion.Value = new Settings.LastCheckedVersion(DateTime.MinValue, 0);
            }
            else
                Settings.LastProgramVersion.Clear();

            Settings.PreventTaskbarGrouping.Value = checkPreventTaskbarGrouping.Checked;
            Settings.ForceTaskbarGrouping.Value = checkForceTaskbarGrouping.Checked;

            if (checkWindowCaption.Checked)
                Settings.WindowCaption.Value = textWindowCaption.Text;
            else
                Settings.WindowCaption.Clear();

            Settings.WindowIcon.Value = checkWindowIcon.Checked;

            Settings.TopMost.Value = checkTopMost.Checked;

            if (checkCustomUsername.Checked && !string.IsNullOrWhiteSpace(textCustomUsername.Text))
            {
                string displayUsername;
                if (Path.IsPathRooted(textCustomUsername.Text))
                    displayUsername = Util.FileUtil.GetTrimmedDirectoryPath(textCustomUsername.Text);
                else
                    displayUsername = Util.FileUtil.ReplaceInvalidFileNameChars(textCustomUsername.Text, '_');
                if (string.IsNullOrEmpty(displayUsername))
                    Settings.GuildWars2.VirtualUserPath.Clear();
                else
                    Settings.GuildWars2.VirtualUserPath.Value = displayUsername;
            }
            else
                Settings.GuildWars2.VirtualUserPath.Clear();

            Settings.ActionActiveLClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionActiveLClick, Settings.ButtonAction.None);
            Settings.ActionActiveLPress.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionActiveLPress, Settings.ButtonAction.None);
            Settings.ActionInactiveLClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionInactiveLClick, Settings.ButtonAction.Launch);
            Settings.ActionActiveMClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionActiveMClick, Settings.ButtonAction.None);
            Settings.ActionInactiveMClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionInactiveMClick, Settings.ButtonAction.None);

            Settings.GuildWars2.AutomaticRememberedLogin.Value = checkGw2AutomaticLauncherLogin.Checked;

            if (checkGw2MuteAll.Checked || checkGw2MuteMusic.Checked || checkGw2MuteVoices.Checked)
            {
                var mute = Settings.MuteOptions.None;
                if (checkGw2MuteAll.Checked)
                    mute |= Settings.MuteOptions.All;
                if (checkGw2MuteMusic.Checked)
                    mute |= Settings.MuteOptions.Music;
                if (checkGw2MuteVoices.Checked)
                    mute |= Settings.MuteOptions.Voices;
                Settings.GuildWars2.Mute.Value = mute;
            }
            else
                Settings.GuildWars2.Mute.Clear();

            if (checkGw1MuteAll.Checked)
            {
                Settings.GuildWars1.Mute.Value = Settings.MuteOptions.All;
            }
            else
                Settings.GuildWars1.Mute.Clear();

            if (checkGw2Port80.Checked)
                Settings.GuildWars2.ClientPort.Value = 80;
            else if (checkGw2Port443.Checked)
                Settings.GuildWars2.ClientPort.Value = 443;
            else
                Settings.GuildWars2.ClientPort.Clear();

            if (checkGw2ScreenshotsBmp.Checked)
                Settings.GuildWars2.ScreenshotsFormat.Value = Settings.ScreenshotFormat.Bitmap;
            else
                Settings.GuildWars2.ScreenshotsFormat.Clear();

            if (checkGw1ScreenshotsBmp.Checked)
                Settings.GuildWars1.ScreenshotsFormat.Value = Settings.ScreenshotFormat.Bitmap;
            else
                Settings.GuildWars1.ScreenshotsFormat.Clear();

            if (checkGw2ScreenshotsLocation.Checked && !string.IsNullOrEmpty(textGw2ScreenshotsLocation.Text))
            {
                var path = Util.FileUtil.GetTrimmedDirectoryPath(textGw2ScreenshotsLocation.Text);
                if (string.IsNullOrEmpty(path))
                    Settings.GuildWars2.ScreenshotsLocation.Clear();
                else
                    Settings.GuildWars2.ScreenshotsLocation.Value = path;
            }
            else
                Settings.GuildWars2.ScreenshotsLocation.Clear();

            if (checkGw1ScreenshotsLocation.Checked && !string.IsNullOrEmpty(textGw1ScreenshotsLocation.Text))
            {
                var path = Util.FileUtil.GetTrimmedDirectoryPath(textGw1ScreenshotsLocation.Text);
                if (string.IsNullOrEmpty(path))
                    Settings.GuildWars1.ScreenshotsLocation.Clear();
                else
                    Settings.GuildWars1.ScreenshotsLocation.Value = path;
            }
            else
                Settings.GuildWars1.ScreenshotsLocation.Clear();

            Settings.DeleteCacheOnLaunch.Value = checkDeleteCacheOnLaunch.Checked;
            if (checkShowDailies.Checked)
            {
                Settings.DailiesMode showDailies = Settings.ShowDailies.Value;

                showDailies |= Settings.DailiesMode.Show;
                if (checkShowDailiesAuto.Checked)
                    showDailies |= Settings.DailiesMode.AutoLoad;
                else
                    showDailies &= ~Settings.DailiesMode.AutoLoad;

                Settings.ShowDailies.Value = showDailies;
            }
            else
            {
                Settings.ShowDailies.Clear();
                if (Settings.WindowBounds.Contains(typeof(formDailies)))
                    Settings.WindowBounds[typeof(formDailies)].Clear();
            }

            if (checkAutoUpdateDownloadProgress.Checked)
                Settings.BackgroundPatchingProgress.Value = (Rectangle)checkAutoUpdateDownloadProgress.Tag;
            else
                Settings.BackgroundPatchingProgress.Clear();

            if (checkScreenshotNameFormat.Checked && !string.IsNullOrEmpty(comboScreenshotNameFormat.Text))
            {
                Tools.Screenshots.Formatter formatter = null;
                try
                {
                    formatter = Tools.Screenshots.Formatter.Convert(comboScreenshotNameFormat.Text);
                    if (formatter != null && formatter.ToString(0, DateTime.MinValue).Equals("gw000"))
                        formatter = null;
                }
                catch
                {
                    formatter = null;
                }

                if (formatter != null)
                    Settings.ScreenshotNaming.Value = comboScreenshotNameFormat.Text;
                else
                    Settings.ScreenshotNaming.Clear();
            }
            else
                Settings.ScreenshotNaming.Clear();

            if (checkScreenshotImageFormat.Checked && comboScreenshotImageFormat.SelectedIndex >= 0)
            {
                var conversion = new Settings.ScreenshotConversionOptions()
                {
                    DeleteOriginal = checkScreenshotDeleteOriginal.Checked,
                };

                switch (Util.ComboItem<Settings.ScreenshotConversionOptions.ImageFormat>.SelectedValue(comboScreenshotImageFormat))
                {
                    case Settings.ScreenshotConversionOptions.ImageFormat.Jpg:
                        conversion.Format = Settings.ScreenshotConversionOptions.ImageFormat.Jpg;
                        conversion.Options = (byte)numericScreenshotImageFormatJpgQuality.Value;
                        break;
                    case Settings.ScreenshotConversionOptions.ImageFormat.Png:
                        conversion.Format = Settings.ScreenshotConversionOptions.ImageFormat.Png;
                        if (radioScreenshotImageFormat16.Checked)
                            conversion.Options = 16;
                        else
                            conversion.Options = 24;
                        break;
                }

                Settings.ScreenshotConversion.Value = conversion;
            }
            else
                Settings.ScreenshotConversion.Clear();

            Settings.DeleteCrashLogsOnLaunch.Value = checkDeleteCrashLogsOnLaunch.Checked;

            if (checkGw2ProcessPriority.Checked && comboGw2ProcessPriority.SelectedIndex >= 0)
                Settings.GuildWars2.ProcessPriority.Value = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboGw2ProcessPriority);
            else
                Settings.GuildWars2.ProcessPriority.Clear();

            if (checkGw2ProcessPriorityDx.Checked && comboGw2ProcessPriorityDx.SelectedIndex >= 0)
                Settings.GuildWars2.DxLoadingPriority.Value = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboGw2ProcessPriorityDx);
            else
                Settings.GuildWars2.DxLoadingPriority.Clear();

            if (checkGw1ProcessPriority.Checked && comboGw1ProcessPriority.SelectedIndex >= 0)
                Settings.GuildWars1.ProcessPriority.Value = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboGw1ProcessPriority);
            else
                Settings.GuildWars1.ProcessPriority.Clear();

            if (checkProcessPriority.Checked && comboProcessPriority.SelectedIndex >= 0)
                Settings.ProcessPriority.Value = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboProcessPriority);
            else
                Settings.ProcessPriority.Clear();

            if (!checkGw2ProcessAffinityAll.Checked)
            {
                var bits = adGw2ProcessAffinity.Affinity;
                if (bits == 0)
                    Settings.GuildWars2.ProcessAffinity.Clear();
                else
                    Settings.GuildWars2.ProcessAffinity.Value = bits;
            }
            else
                Settings.GuildWars2.ProcessAffinity.Clear();

            if (!checkGw1ProcessAffinityAll.Checked)
            {
                var bits = adGw1ProcessAffinity.Affinity;
                if (bits == 0)
                    Settings.GuildWars1.ProcessAffinity.Clear();
                else
                    Settings.GuildWars1.ProcessAffinity.Value = bits;
            }
            else
                Settings.GuildWars1.ProcessAffinity.Clear();

            Settings.GuildWars2.PrioritizeCoherentUI.Value = checkGw2ProcessBoostBrowser.Checked;

            if (checkNoteNotifications.Checked)
            {
                var v = (Settings.NotificationScreenAttachment)checkNoteNotifications.Tag;
                if (v.OnlyWhileActive != checkNoteNotificationsOnlyWhileActive.Checked)
                    v = new Settings.NotificationScreenAttachment(v.Screen, v.Anchor, !v.OnlyWhileActive);
                Settings.NotesNotifications.Value = v;
            }
            else
                Settings.NotesNotifications.Clear();

            if (checkMaxPatchConnections.Checked)
                Settings.MaxPatchConnections.Value = (byte)numericMaxPatchConnections.Value;
            else
                Settings.MaxPatchConnections.Clear();

            if (checkAccountBarEnable.Checked)
            {
                var forceShow = !Settings.AccountBar.Options.HasValue;
                var parent = this.Owner as formMain;
                if (parent != null)
                    parent.ShowAccountBar(forceShow);
            }
            Settings.AccountBar.Enabled.Value = checkAccountBarEnable.Checked;

            if (checkLimitActiveAccount.Checked)
                Settings.LimitActiveAccounts.Value = (byte)numericLimitActiveAccount.Value;
            else
                Settings.LimitActiveAccounts.Clear();

            Settings.DelayLaunchUntilLoaded.Value = checkDelayUntilLoaded.Checked;

            if (checkDelaySeconds.Checked)
                Settings.DelayLaunchSeconds.Value = (byte)numericDelaySeconds.Value;
            else
                Settings.DelayLaunchSeconds.Clear();

            var localizedExe = Settings.LocalizeAccountExecutionOptions.None;
            if (checkLocalizedExecution.Checked)
                localizedExe |= Settings.LocalizeAccountExecutionOptions.Enabled;
            if (checkLocalizedExecutionExcludeUnknown.Checked)
                localizedExe |= Settings.LocalizeAccountExecutionOptions.ExcludeUnknownFiles;
            if (radioLocalizedExecutionBinaries.Checked)
                localizedExe |= Settings.LocalizeAccountExecutionOptions.OnlyIncludeBinFolders;
            if (radioLocalizedExecutionAutoSyncAll.Checked)
            {
                localizedExe |= Settings.LocalizeAccountExecutionOptions.AutoSync;
                if (checkLocalizedExecutionAutoSyncDelete.Checked)
                    localizedExe |= Settings.LocalizeAccountExecutionOptions.AutoSyncDeleteUnknowns;
            }
            Settings.GuildWars2.LocalizeAccountExecution.Value = localizedExe;

            Settings.UseDefaultIconForShortcuts.Value = checkUseDefaultShortcutIcon.Checked;

            Settings.GuildWars2.DatUpdaterEnabled.Value = checkLocalDatDirectUpdates.Checked;
            Settings.GuildWars2.UseCustomGw2Cache.Value = checkLocalDatCache.Checked;

            Settings.GuildWars2.PreventDefaultCoherentUI.Value = checkPreventDefaultCoherentUI.Checked;
            Settings.ShowKillAllAccounts.Value = checkStyleShowCloseAll.Checked;

            Settings.RepaintInitialWindow.Value = checkRepaintInitialWindow.Checked;
            Settings.HideInitialWindow.Value = checkHideInitialWindow.Checked;

            if (checkGw2MumbleName.Checked)
                Settings.GuildWars2.MumbleLinkName.Value = textGw2MumbleName.Text;
            else
                Settings.GuildWars2.MumbleLinkName.Clear();

            if (radioGw2ModeBasic.Checked)
            {
                Settings.GuildWars2.ProfileMode.Value = Settings.ProfileMode.Basic;

                var pmo = Settings.ProfileModeOptions.None;
                if (checkGw2ModeBasicClearTemporary.Checked)
                    pmo |= Settings.ProfileModeOptions.ClearTemporaryFiles;
                if (checkGw2ModeBasicRestorePath.Checked)
                    pmo |= Settings.ProfileModeOptions.RestoreOriginalPath;

                if (pmo != Settings.ProfileModeOptions.None)
                    Settings.GuildWars2.ProfileOptions.Value = pmo;
                else
                    Settings.GuildWars2.ProfileOptions.Clear();
            }
            else
            {
                Settings.GuildWars2.ProfileMode.Clear();
                Settings.GuildWars2.ProfileOptions.Clear();
            }

            if (checkScreenshotNotification.Checked)
                Settings.ScreenshotNotifications.Value = (Settings.ScreenAttachment)checkScreenshotNotification.Tag;
            else
                Settings.ScreenshotNotifications.Clear();

            if (comboEncryptionScope.Enabled)
            {
                var scope2 = Settings.Encryption.HasValue ? Settings.Encryption.Value.Scope : Settings.EncryptionScope.CurrentUser;
                var scope = Util.ComboItem<Settings.EncryptionScope>.SelectedValue(comboEncryptionScope);

                if (encryptionState == ArgsState.Loaded || GetScopeWeigth(scope) <= GetScopeWeigth(scope2))
                {
                    var o = Settings.Encryption.Value;

                    if (scope == Settings.EncryptionScope.CurrentUser)
                    {
                        Settings.Encryption.Clear();
                    }
                    else if (o == null || o.Scope != scope)
                    {
                        byte[] key = null;
                        if (scope == Settings.EncryptionScope.Portable)
                            key = Security.Cryptography.Crypto.GenerateCryptoKey();
                        Settings.Encryption.Value = new Settings.EncryptionOptions(scope, key);
                    }
                }
            }

            if (checkStyleColumns.Checked)
                Settings.StyleColumns.Value = (byte)numericStyleColumns.Value;
            else
                Settings.StyleColumns.Clear();

            if (checkStyleBackgroundImage.Checked && checkStyleBackgroundImage.Tag != null)
                Settings.StyleBackgroundImage.Value = (string)checkStyleBackgroundImage.Tag;
            else
                Settings.StyleBackgroundImage.Clear();

            Settings.LaunchBehindOtherAccounts.Value = checkLaunchBehindWindows.Checked;

            if (checkLaunchLimiter.Checked)
            {
                if (radioLimiterAutomatic.Checked)
                    Settings.LaunchLimiter.Value = new Settings.LaunchLimiterOptions();
                else
                    Settings.LaunchLimiter.Value = new Settings.LaunchLimiterOptions((byte)numericLaunchLimiterAmount.Value, (byte)numericLaunchLimiterRechargeAmount.Value, (byte)numericLaunchLimiterRechargeTime.Value);
            }
            else
                Settings.LaunchLimiter.Clear();

            Settings.ShowLaunchAllAccounts.Value = checkStyleShowLaunchAll.Checked;

            if (checkTimeoutRelaunch.Checked)
                Settings.LaunchTimeout.Value = (byte)numericTimeoutRelaunch.Value;
            else
                Settings.LaunchTimeout.Clear();

            if (checkJumpList.Enabled && checkJumpList.Checked)
            {
                var v = Settings.JumpListOptions.Enabled;
                if (checkJumpListOnlyShowDaily.Checked)
                    v |= Settings.JumpListOptions.OnlyShowDaily;
                else if (checkJumpListHideActive.Checked)
                    v |= Settings.JumpListOptions.OnlyShowInactive;
                Settings.JumpList.Value = v;
            }
            else
                Settings.JumpList.Clear();

            Settings.PreventTaskbarMinimize.Value = checkPreventMinimizing.Checked;

            if (checkPreventRelaunchingRelaunch.Checked || checkPreventRelaunchingExit.Checked)
            {
                var v = Settings.RelaunchOptions.None;
                if (checkPreventRelaunchingExit.Checked)
                    v |= Settings.RelaunchOptions.Exit;
                else if (checkPreventRelaunchingRelaunch.Checked)
                    v |= Settings.RelaunchOptions.Relaunch;
                Settings.GuildWars2.PreventRelaunching.Value = v;
            }
            else
                Settings.GuildWars2.PreventRelaunching.Clear();

            Settings.AuthenticatorPastingEnabled.Value = checkManualAuthenticator.Checked;

            if (panelGw2RunAfterPrograms.Controls.Count > 0)
            {
                var controls = panelGw2RunAfterPrograms.Controls;
                var ra = new Settings.RunAfter[controls.Count];
                var existing = Settings.GuildWars2.RunAfter.Value;
                var changed = existing == null || ra.Length != existing.Length;

                for (var i = 0; i < ra.Length; i++)
                {
                    ra[i] = (Settings.RunAfter)controls[i].Tag;
                    if (!changed)
                        changed = !existing[i].Equals(ra[i]);
                }

                if (changed)
                    Settings.GuildWars2.RunAfter.Value = ra;
            }
            else
                Settings.GuildWars2.RunAfter.Clear();

            if (panelGw1RunAfterPrograms.Controls.Count > 0)
            {
                var controls = panelGw1RunAfterPrograms.Controls;
                var ra = new Settings.RunAfter[controls.Count];
                var existing = Settings.GuildWars1.RunAfter.Value;
                var changed = existing == null || ra.Length != existing.Length;

                for (var i = 0; i < ra.Length; i++)
                {
                    ra[i] = (Settings.RunAfter)controls[i].Tag;
                    if (!changed)
                        changed = !existing[i].Equals(ra[i]);
                }

                if (changed)
                    Settings.GuildWars1.RunAfter.Value = ra;
            }
            else
                Settings.GuildWars1.RunAfter.Clear();

            if (panelHotkeysHotkeys.Modified)
            {
                foreach (var m in panelHotkeysHotkeys.GetModified())
                {
                    if (m.Account == null)
                    {
                        if (m.Hotkeys != null)
                            Settings.Hotkeys.Value = m.Hotkeys;
                        else
                            Settings.Hotkeys.Clear();
                    }
                    else
                    {
                        m.Account.Hotkeys = m.Hotkeys;
                    }
                }
            }

            Settings.HotkeysEnabled.Value = checkHotkeysEnable.Checked;

            if (checkStyleCustomizeOffsets.Checked)
            {
                Settings.StyleOffsets.Value = buttonSample.Offsets;
                if (checkStyleSpacing.Checked)
                    Settings.StyleGridSpacing.Value = (byte)numericStyleSpacing.Value;
                else
                    Settings.StyleGridSpacing.Clear();
            }
            else
            {
                Settings.StyleOffsets.Clear();
                Settings.StyleGridSpacing.Clear();
            }

            Settings.StyleDisableWindowShadows.Value = checkStyleDisableShadows.Checked;
            Settings.StyleDisableSearchOnKeyPress.Value = checkStyleDisableSearchOnKeyPress.Checked;
            Settings.StyleDisablePageOnKeyPress.Value = checkStyleDisablePageOnKeyPress.Checked;
            Settings.ShowWindowTemplatesToggle.Value = checkStyleShowTemplatesToggle.Checked;
            Settings.ShowMinimizeRestoreAll.Value = checkStyleShowMinimizeRestoreAll.Checked;
            Settings.ShowAccountBarToggle.Value = checkStyleShowAccountBarToggle.Checked;

            if (panelStyleLoginRewardIcons.Controls.Count > 0)
            {
                var v = Settings.DailyLoginDayIconFlags.None;

                if (checkStyleShowLoginRewardIcon.Checked)
                {
                    if (checkStyleLoginRewardIconsNumber.Checked)
                        v |= Settings.DailyLoginDayIconFlags.ShowDay;
                    foreach (Control c in panelStyleLoginRewardIcons.Controls)
                    {
                        var check = (CheckBox)c.Tag;
                        if (check.Checked)
                            v |= (Settings.DailyLoginDayIconFlags)check.Tag;
                    }
                }

                Settings.StyleShowDailyLoginDay.Value = v;
            }

            if (panelGw2ProcessAffinityAccounts.Tag != null)
            {
                ((AffinityAccountsPanel)panelGw2ProcessAffinityAccounts.Tag).Save();
            }

            if (panelGw1ProcessAffinityAccounts.Tag != null)
            {
                ((AffinityAccountsPanel)panelGw1ProcessAffinityAccounts.Tag).Save();
            }

            Settings.StyleHighlightActive.Value = checkStyleMarkActive.Checked;

            if (checkDxTimeoutRelaunch.Checked)
                Settings.DxTimeout.Value = (byte)numericDxTimeoutRelaunch.Value;
            else
                Settings.DxTimeout.Clear();

            Settings.ShowDailiesLanguage.Value = Util.ComboItem<Settings.Language>.SelectedValue(comboShowDailiesLang, Settings.Language.EN);

            if (labelLocalizedExecutionAccountsSelected.Tag != null)
            {
                foreach (var a in GetLocalizedExecutionAccountsSelected())
                {
                    ((Settings.IGw2Account)a.Key).LocalizedExecution = a.Value;
                }
            }

            if (radioLocalizedExecutionAccountsInclude.Checked)
                Settings.GuildWars2.LocalizeAccountExecutionSelection.Value = Settings.LocalizeAccountExecutionSelectionOptions.Include;
            else if (radioLocalizedExecutionAccountsExclude.Checked)
                Settings.GuildWars2.LocalizeAccountExecutionSelection.Value = Settings.LocalizeAccountExecutionSelectionOptions.Exclude;
            else
                Settings.GuildWars2.LocalizeAccountExecutionSelection.Clear();

            if (checkTweakLogin.Checked)
            {
                Settings.Tweaks.Login.Value = new Settings.LoginTweaks(
                    Util.ComboItem<Settings.LoginInputType>.SelectedValue(comboTweakEmailMethod, Settings.LoginInputType.Clipboard),
                    Util.ComboItem<Settings.LoginInputType>.SelectedValue(comboTweakPasswordMethod, Settings.LoginInputType.Post),
                    checkTweakLoginVerify.Checked,
                    (byte)numericTweakLoginDelay.Value);
            }
            else
                Settings.Tweaks.Login.Clear();

            if (checkTweakLauncher.Checked)
            {
                Settings.Tweaks.Launcher.Value = new Settings.LauncherTweaks(
                    (byte)numericTweakLauncherDelay.Value,
                    checkTweakLauncherCoherentLoad.Checked,
                    checkTweakLauncherCoherentMemory.Checked);
            }
            else
                Settings.Tweaks.Launcher.Clear();

            if (labelTweakLauncherCoords.Tag != null)
            {
                var v = (Settings.LauncherPoints)labelTweakLauncherCoords.Tag;

                if (v.EmptyArea.IsEmpty && v.PlayButton.IsEmpty)
                    Settings.GuildWars2.LauncherAutologinPoints.Clear();
                else
                    Settings.GuildWars2.LauncherAutologinPoints.Value = v;
            }

            if (checkNetworkEnable.Checked)
            {
                var v = Settings.NetworkOptions.Enabled;
                if (checkNetworkExact.Checked)
                    v |= Settings.NetworkOptions.Exact;
                if (checkNetworkWarnOnChange.Checked)
                    v |= Settings.NetworkOptions.WarnOnChange;
                Settings.Network.Value = v;
            }
            else
                Settings.Network.Clear();

            if (textNetworkAddresses.Tag != null)
            {
                var lines = textNetworkAddresses.Lines;
                var addresses = new Net.IP.WildcardAddress[lines.Length];
                var count = 0;

                foreach (var l in textNetworkAddresses.Lines)
                {
                    if (string.IsNullOrWhiteSpace(l))
                        continue;

                    try
                    {
                        var ip = Net.IP.WildcardAddress.Parse(l.Trim());
                        addresses[count++] = ip;
                    }
                    catch { }
                }

                if (count > 0)
                {
                    if (count != addresses.Length)
                        Array.Resize(ref addresses, count);

                    Settings.IPAddresses.Value = addresses;
                }
                else
                    Settings.IPAddresses.Clear();
            }

            if (checkSteamPath.Checked && !string.IsNullOrEmpty(textSteamPath.Text))
            {
                Settings.Steam.Path.Value = textSteamPath.Text;
            }

            if (numericSteamTimeout.Value != 5)
                Settings.Steam.Timeout.Value = (byte)numericSteamTimeout.Value;
            else
                Settings.Steam.Timeout.Clear();

            if (radioSteamLimitAll.Checked)
                Settings.Steam.Limitation.Value = Settings.SteamLimitation.BlockAll;
            else if (radioSteamLimitSteam.Checked)
                Settings.Steam.Limitation.Value = Settings.SteamLimitation.OnlyBlockSteam;
            else if (radioSteamLimitLaunchWithout.Checked)
                Settings.Steam.Limitation.Value = Settings.SteamLimitation.LaunchWithoutSteam;

            if (colorInfo != null && colorInfo.Modified) //only loaded if the color settings were viewed
            {
                colorInfo.Save();
            }

            if (checkTweakDailyLogin.Checked)
                Settings.Tweaks.DisableMumbleLinkDailyLogin.Clear();
            else
                Settings.Tweaks.DisableMumbleLinkDailyLogin.Value = true;

            Settings.ShowLaunchDailyAccounts.Value = checkStyleShowLaunchDaily.Checked;
            Settings.HideExit.Value = checkStyleHideExit.Checked;
            Settings.HideMinimize.Value = checkStyleHideMinimize.Checked;






            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkShowTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            checkMinimizeToTray.Enabled = checkShowTrayIcon.Checked;
            checkCloseToTray.Enabled = checkShowTrayIcon.Checked;
        }

        private void buttonWindowReset_Click(object sender, EventArgs e)
        {
            Settings.WindowBounds[typeof(formMain)].Clear();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private Font ShowFontDialog(Font font)
        {
            using (FontDialog f = new FontDialog())
            {
                f.FontMustExist = true;
                f.AllowSimulations = false;
                f.AllowVerticalFonts = false;
                f.Font = font;

                try
                {
                    if (f.ShowDialog(this) == DialogResult.OK)
                    {
                        return f.Font;
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                    MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            return null;
        }

        private void buttonFontTitle_Click(object sender, EventArgs e)
        {
            var f = ShowFontDialog(buttonSample.FontName);
            if (f != null)
            {
                buttonSample.FontName = f;

                buttonFontTitle.Parent.SuspendLayout();
                labelFontTitleReset.Visible = true;
                labelFontTitleResetSep.Visible = true;
                buttonFontTitle.Text = GetFontName(buttonSample.FontName);
                buttonFontTitle.Parent.ResumeLayout();
            }
        }

        private void buttonFontStatus_Click(object sender, EventArgs e)
        {
            Font f = ShowFontDialog(buttonSample.FontStatus);
            if (f != null)
            {
                buttonSample.FontStatus = f;

                buttonFontStatus.Parent.SuspendLayout();
                labelFontStatusReset.Visible = true;
                labelFontStatusResetSep.Visible = true;
                buttonFontStatus.Text = GetFontName(buttonSample.FontStatus);
                buttonFontStatus.Parent.ResumeLayout();
            }
        }

        private void buttonFontUser_Click(object sender, EventArgs e)
        {
            Font f = ShowFontDialog(buttonSample.FontUser);
            if (f != null)
            {
                buttonSample.FontUser = f;

                buttonFontUser.Parent.SuspendLayout();
                labelFontUserReset.Visible = true;
                labelFontUserResetSep.Visible = true;
                buttonFontUser.Text = GetFontName(buttonSample.FontUser);
                buttonFontUser.Parent.ResumeLayout();
            }
        }

        private void checkShowUser_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.ShowAccount = checkShowUser.Checked;
        }

        private void checkGw2Volume_CheckedChanged(object sender, EventArgs e)
        {
            sliderGw2Volume.Enabled = checkGw2Volume.Checked;
        }

        private void labelVersionUpdate_Click(object sender, EventArgs e)
        {
            using (formVersionUpdate f = new formVersionUpdate())
            {
                f.ShowDialog(this);
            }
        }

        private void sliderGw2Volume_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            labelGw2Volume.Text = (int)(v * 100 + 0.5f) + "%";
        }

        private void checkAutoUpdateDownload_CheckedChanged(object sender, EventArgs e)
        {
            checkAutoUpdateDownloadNotifications.Parent.Enabled = checkAutoUpdateDownload.Checked;
            checkAutoUpdateDownloadProgress.Parent.Enabled = checkAutoUpdateDownload.Checked;
            numericUpdateInterval.Parent.Enabled = checkAutoUpdateDownload.Checked || checkAutoUpdate.Checked;
        }

        private Form SetActive(Form form)
        {
            if (activeWindow != null && !activeWindow.IsDisposed)
                activeWindow.Close();
            activeWindow = form;
            return form;
        }

        private void labelAutoUpdateDownloadNotificationsConfig_Click(object sender, EventArgs e)
        {
            ShowNotificationSelector(formScreenPosition.NotificationType.Patch, checkAutoUpdateDownloadNotifications, labelAutoUpdateDownloadNotificationsConfig);
        }

        private void checkWindowCaption_CheckedChanged(object sender, EventArgs e)
        {
            textWindowCaption.Enabled = checkWindowCaption.Checked;
            labelWindowCaptionVariables.Enabled = checkWindowCaption.Checked;
        }

        private void checkCustomUsername_CheckedChanged(object sender, EventArgs e)
        {
            textCustomUsername.Enabled = buttonCustomUsername.Enabled =checkCustomUsername.Checked;
        }

        private void checkGw2ScreenshotsLocation_CheckedChanged(object sender, EventArgs e)
        {
            textGw2ScreenshotsLocation.Enabled = buttonGw2ScreenshotsLocation.Enabled = checkGw2ScreenshotsLocation.Checked;
        }

        private void checkGw2Port80_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                checkGw2Port443.Checked = false;
        }

        private void checkGw2Port443_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
                checkGw2Port80.Checked = false;
        }

        private void checkShowDailies_CheckedChanged(object sender, EventArgs e)
        {
            checkShowDailiesAuto.Enabled = checkShowDailies.Checked;
        }

        private void buttonCustomUsername_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.SaveFolderDialog();
            var filename = textCustomUsername.Text;
            string userprofile;

            try
            {
                userprofile = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                f.InitialDirectory = userprofile;
            }
            catch
            {
                userprofile = null;
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                f.FileName = "Gw2Launcher";
            }
            else
            {
                if (Path.IsPathRooted(filename))
                {
                    f.SetPath(filename, false);
                }
                else
                {
                    f.FileName = filename;
                }
            }

            try
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (userprofile != null && f.FileName.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase) && f.FileName.IndexOf(Path.DirectorySeparatorChar, userprofile.Length + 1) == -1)
                        textCustomUsername.Text = Path.GetFileName(f.FileName);
                    else
                        textCustomUsername.Text = f.FileName;
                    textCustomUsername.Select(textCustomUsername.TextLength, 0);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private void ShowOpenScreenshotLocationDialog(TextBox path)
        {
            var f = new Windows.Dialogs.OpenFolderDialog();

            if (path.Text.Length > 0)
            {
                f.SetPath(path.Text, false);
            }

            try
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    path.Text = f.FileName;
                    path.Select(path.TextLength, 0);
                }
            }
            catch (Exception ex)
            {
                Util.Logging.Log(ex);
            }
        }

        private void buttonGw2ScreenshotsLocation_Click(object sender, EventArgs e)
        {
            ShowOpenScreenshotLocationDialog(textGw2ScreenshotsLocation);
        }

        private void labelAutoUpdateDownloadProgressConfig_Click(object sender, EventArgs e)
        {
            var v = (Rectangle)checkAutoUpdateDownloadProgress.Tag;
            var f = (formProgressOverlay)SetActive(new formProgressOverlay());

            f.MinimumSize = new Size(Scale(50), 1);
            f.MaximumSize = new Size(ushort.MaxValue, Scale(50));
            f.Bounds = Util.ScreenUtil.Constrain(v);
            f.Disposed += progress_Disposed;

            EventHandler onActivate = null;
            onActivate = delegate
            {
                this.Activated -= onActivate;
                f.Dispose();
            };
            this.Activated += onActivate;

            var o = new formSizingBox(f);
            o.Show(f);
            f.Show();

            AnimateProgress(f);
        }

        async void AnimateProgress(formProgressOverlay progress)
        {
            var p = progress.Progress;
            p.Animated = false;
            p.Maximum = 100;

            //var start = DateTime.UtcNow.Ticks;

            while (!progress.IsDisposed)
            {
                if (p.Value == p.Maximum)
                    p.Value = 0;
                else
                    p.Value += 1;
                //p.Value = 50 + (int)(Math.Sin((DateTime.UtcNow.Ticks - start) / 10000 / 500d / 4) * 50);

                await Task.Delay(50);
            }
        }
        
        void progress_Disposed(object sender, EventArgs e)
        {
            var f = sender as formProgressOverlay;
            if (f != null)
            {
                checkAutoUpdateDownloadProgress.Tag = f.Bounds;
            }
        }

        private void checkEnableNetworkAuthorization_CheckedChanged(object sender, EventArgs e)
        {
            checkRemovePreviousNetworks.Enabled = checkEnableNetworkAuthorization.Checked;
            checkRemoveAllNetworks.Parent.Enabled = checkEnableNetworkAuthorization.Checked;
            checkNetworkAbortOnCancel.Enabled = checkEnableNetworkAuthorization.Checked;
        }

        private void checkScreenshotNameFormat_CheckedChanged(object sender, EventArgs e)
        {
            comboScreenshotNameFormat.Enabled = checkScreenshotNameFormat.Checked;
            buttonScreenshotsExistingApply.Enabled = checkScreenshotNameFormat.Checked || checkScreenshotImageFormat.Checked;
        }

        private void comboScreenshotNameFormat_TextChanged(object sender, EventArgs e)
        {
            if (comboScreenshotNameFormat.SelectedIndex == -1)
                Util.ScheduledEvents.Register(OnScreenshotNameFormatChangedCallback, 500);
            else
                OnScreenshotNameFormatChangedCallback();
        }

        private int OnScreenshotNameFormatChangedCallback()
        {
            try
            {
                var format = Tools.Screenshots.Formatter.Convert(comboScreenshotNameFormat.Text);
                if (format != null)
                {
                    labelScreenshotNameFormatSample.Text = format.ToString(1, DateTime.Now);
                    return -1;
                }
            }
            catch { }

            labelScreenshotNameFormatSample.Text = "invalid";

            return -1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (colorInfo != null)
                {
                    colorInfo.Reset();
                    colorInfo = null;
                }

                Util.ScheduledEvents.Unregister(OnScreenshotNameFormatChangedCallback);

                using (buttonSample.BackgroundImage) { }
                using (buttonSample.Image) { }
                using (ftooltip) { }
                using (tooltipColors) { }
                using (cancelPressed) { }

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);

            if (disposing)
            {
                using (markerIcons) { }

                foreach (object o in iconsRunAfter.Values)
                {
                    if (o is Icon)
                    {
                        ((Icon)o).Dispose();
                    }
                }
            }
        }

        private void checkScreenshotImageFormat_CheckedChanged(object sender, EventArgs e)
        {
            comboScreenshotImageFormat.Enabled = checkScreenshotImageFormat.Checked;
            buttonScreenshotsExistingApply.Enabled = checkScreenshotNameFormat.Checked || checkScreenshotImageFormat.Checked;
            panelScreenshotImageFormatJpg.Parent.Visible = checkScreenshotImageFormat.Checked;
        }

        private void comboScreenshotImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelScreenshotImageFormatJpg.Parent.SuspendLayout();
            panelScreenshotImageFormatJpg.Visible = comboScreenshotImageFormat.SelectedIndex == 0;
            panelScreenshotImageFormatPng.Visible = comboScreenshotImageFormat.SelectedIndex == 1;
            panelScreenshotImageFormatJpg.Parent.ResumeLayout();
        }

        private async void buttonScreenshotsExistingApply_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.OpenFolderDialog();
            try
            {
                if (buttonScreenshotsExistingApply.Tag != null)
                {
                    f.InitialDirectory = (string)buttonScreenshotsExistingApply.Tag;
                }
                else
                {
                    if (checkGw2ScreenshotsLocation.Checked && string.IsNullOrEmpty(textGw2ScreenshotsLocation.Text) && Directory.Exists(textGw2ScreenshotsLocation.Text))
                        f.InitialDirectory = textGw2ScreenshotsLocation.Text;
                    else
                        f.InitialDirectory = Client.FileManager.GetPath(Client.FileManager.SpecialPath.Screens);
                }
            }
            catch { }
            f.Title = "Open screenshots folder";
            if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                return;

            buttonScreenshotsExistingApply.Enabled = false;

            var path = f.FileName;
            buttonScreenshotsExistingApply.Tag = path;

            Tools.Screenshots.Formatter formatter = null;

            if (checkScreenshotNameFormat.Checked)
            {
                try
                {
                    formatter = Tools.Screenshots.Formatter.Convert(comboScreenshotNameFormat.Text);
                    if (formatter != null && checkScreenshotsExistingApplyOnlyToDefault.Checked && formatter.ToString(0, DateTime.MinValue).Equals("gw000"))
                        formatter = null;
                }
                catch
                {
                    formatter = null;
                }
            }

            var conversion = new Settings.ScreenshotConversionOptions();
            bool convert;

            if (convert = checkScreenshotImageFormat.Checked)
            {
                switch (comboScreenshotImageFormat.SelectedIndex)
                {
                    case 0:
                        conversion.Format = Settings.ScreenshotConversionOptions.ImageFormat.Jpg;
                        conversion.Options = (byte)numericScreenshotImageFormatJpgQuality.Value;
                        break;
                    case 1:
                        conversion.Format = Settings.ScreenshotConversionOptions.ImageFormat.Png;
                        if (radioScreenshotImageFormat16.Checked)
                            conversion.Options = 16;
                        else
                            conversion.Options = 24;
                        break;
                }

                conversion.DeleteOriginal = checkScreenshotDeleteOriginal.Checked;
            }

            await Task.Run(new Action(
                delegate
                {
                    string[] files;

                    if (checkScreenshotsExistingApplyOnlyToDefault.Checked)
                    {
                        files = Directory.GetFiles(path, "gw*.*");
                        var length = files.Length;
                        var files2 = new string[length];
                        var count = 0;

                        for (var i = 0; i < length; i++)
                        {
                            var file = files[i];

                            if (Path.GetFileName(file).Length == 9)
                            {
                                var ext = Path.GetExtension(file);
                                if (ext.Equals(".bmp", StringComparison.OrdinalIgnoreCase) || ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
                                    files2[count++] = file;
                            }
                        }

                        if (count != length)
                        {
                            files = new string[count];
                            Array.Copy(files2, files, count);
                        }
                    }
                    else
                    {
                        files = Directory.GetFiles(path);
                    }

                    if (files.Length > 0)
                        Tools.Screenshots.ConvertRename(path, files, formatter != null, convert, formatter, conversion);
                }));

            buttonScreenshotsExistingApply.Enabled = true;
        }

        private void checkGw2ProcessPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboGw2ProcessPriority.Enabled = checkGw2ProcessPriority.Checked;
            OnGw2ProcessPriorityChanged();
        }

        private void checkGw2ProcessAffinityAll_CheckedChanged(object sender, EventArgs e)
        {
            panelGw2ProcessAffinity.Visible = !checkGw2ProcessAffinityAll.Checked;
        }

        private void checkNoteNotifications_CheckedChanged(object sender, EventArgs e)
        {
            checkNoteNotificationsOnlyWhileActive.Parent.Enabled = checkNoteNotifications.Checked;
        }

        private void labelNoteNotifications_Click(object sender, EventArgs e)
        {
            ShowNotificationSelector(formScreenPosition.NotificationType.Note, checkNoteNotifications, labelNoteNotifications);
        }

        private void checkMaxPatchConnections_CheckedChanged(object sender, EventArgs e)
        {
            numericMaxPatchConnections.Enabled = checkMaxPatchConnections.Checked;
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

        private void checkShowColor_CheckedChanged(object sender, EventArgs e)
        {
            if (checkShowColor.Checked)
            {
                ushort index;
                if (checkShowColor.Tag != null)
                    index = (ushort)checkShowColor.Tag;
                else
                    index = 1;
                buttonSample.ColorKey = Util.Color.FromUID(index);
                buttonSample.ShowColorKey = true;
                checkShowColor.Tag = ++index;
            }
            else
            {
                buttonSample.ShowColorKey = false;
            }
        }

        private void checkStyleMarkFocused_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.IsFocused = checkStyleMarkFocused.Checked;
            buttonSample.IsActiveHighlight = checkStyleMarkActive.Checked && !buttonSample.IsFocused;
            buttonSample.IsActive = checkStyleMarkActive.Checked;
        }

        private void buttonAccountBarShow_Click(object sender, EventArgs e)
        {
            var parent = this.Owner as formMain;
            if (parent != null)
                parent.ShowAccountBar(true);
        }

        private void checkLimitActiveAccount_CheckedChanged(object sender, EventArgs e)
        {
            numericLimitActiveAccount.Enabled = checkLimitActiveAccount.Checked;
        }

        private void checkDelaySeconds_CheckedChanged(object sender, EventArgs e)
        {
            numericDelaySeconds.Enabled = labelDelaySeconds.Enabled = checkDelaySeconds.Checked;
        }

        private void SetShowLocalizedExecutionVisibility(bool visible)
        {
            var p =labelShowLocalizedExecution.Parent;
            p.SuspendLayout();

            labelShowLocalizedExecution.Visible = visible;
            labelShowLocalizedExecutionSeparator.Visible = visible;
            labelResyncLocalizedExecution.Visible = visible;

            p.ResumeLayout();
        }

        private void labelShowLocalizedExecution_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(Path.GetDirectoryName(textGw2Path.Text), Client.FileManager.LOCALIZED_EXE_FOLDER_NAME);
                if (Directory.Exists(path))
                    Util.Explorer.OpenFolder(path);
                else
                    throw new DirectoryNotFoundException();
            }
            catch
            {
                SetShowLocalizedExecutionVisibility(false);
            }
        }

        private void buttonAccountBarReset_Click(object sender, EventArgs e)
        {
            Settings.WindowBounds[typeof(formAccountBar)].Clear();
        }

        private void labelResyncLocalizedExecution_Click(object sender, EventArgs e)
        {
            var failed = 0;

            try
            {
                var path = Path.Combine(Path.GetDirectoryName(textGw2Path.Text), Client.FileManager.LOCALIZED_EXE_FOLDER_NAME);
                if (Directory.Exists(path))
                {
                    foreach (var d in Directory.GetDirectories(path))
                    {
                        var n = Path.GetFileName(d);
                        ushort uid;
                        if (ushort.TryParse(n, out uid))
                        {
                            try
                            {
                                foreach (var f in Directory.GetFiles(d, "*.exe", SearchOption.TopDirectoryOnly))
                                {
                                    File.Delete(f);
                                }
                                Util.FileUtil.DeleteDirectory(d);
                            }
                            catch
                            {
                                failed++;
                            }
                        }
                    }

                    if (failed == 0)
                    {
                        labelResyncLocalizedExecution.Parent.SuspendLayout();
                        labelResyncLocalizedExecution.Visible = false;
                        labelShowLocalizedExecutionSeparator.Visible = false;
                        labelResyncLocalizedExecution.Parent.ResumeLayout();
                    }
                }
            }
            catch
            {
            }

            if (failed > 0)
            {
                MessageBox.Show(this, "Unable to resync accounts; files are in use", "Failed to sync accounts", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    
        private void checkLocalizedExecution_CheckedChanged(object sender, EventArgs e)
        {
            labelPreventDefaultCoherentUILocalizedNote.Visible = checkLocalizedExecution.Checked;
            panelLocalizedExecution.Visible = checkLocalizedExecution.Checked;
        }

        private void checkAutoUpdate_CheckedChanged(object sender, EventArgs e)
        {
            numericUpdateInterval.Parent.Enabled = checkAutoUpdateDownload.Checked || checkAutoUpdate.Checked;
        }

        private void buttonGw1Path_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Guild Wars 1|Gw.exe|All executables|*.exe";
                f.Title = "Open Gw.exe";

                if (textGw1Path.TextLength != 0)
                {
                    try
                    {
                        f.InitialDirectory = System.IO.Path.GetDirectoryName(textGw1Path.Text);
                    }
                    catch { }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    textGw1Path.Text = f.FileName;
                    textGw1Path.Select(textGw1Path.TextLength, 0);

                    try
                    {
                        var path = Path.GetDirectoryName(f.FileName);
                        if (!Util.FileUtil.HasFolderPermissions(path, System.Security.AccessControl.FileSystemRights.Modify) && !Util.FileUtil.CanWriteToFolder(path))
                        {
                            if (MessageBox.Show(this, "The selected path will require elevated permissions to modify.\n\nAllow access to the \"" + Path.GetFileName(path) + "\" folder?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                                Util.ProcessUtil.CreateFolder(path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Logging.Log(ex);
                    }
                }
            }
        }

        private async void labelQuickLaunch_Click(object sender, EventArgs e)
        {
            var l = (Label)sender;

            l.Enabled = false;

            try
            {
                var args = l.Text; // l != labelQuickLaunch ? labelQuickLaunch.Text + " " + l.Text : l.Text;

                await Task.Run(new Action(
                    delegate
                    {
                        Util.ProcessUtil.Execute(args, false);
                    }));
            }
            catch { }

            l.Enabled = true;
        }

        private void numericDelaySeconds_ValueChanged(object sender, EventArgs e)
        {
            var v = (byte)numericDelaySeconds.Value;
            if (v != 1)
            {
                v = 0;
            }

            if (labelDelaySeconds.Tag == null || (byte)labelDelaySeconds.Tag != v)
            {
                labelDelaySeconds.Tag = v;
                labelDelaySeconds.Text = v == 1 ? "second" : "seconds";
            }
        }

        private void checkGw1Volume_CheckedChanged(object sender, EventArgs e)
        {
            sliderGw1Volume.Enabled = checkGw1Volume.Checked;
        }

        private void checkGw1ScreenshotsLocation_CheckedChanged(object sender, EventArgs e)
        {
            textGw1ScreenshotsLocation.Enabled = buttonGw1ScreenshotsLocation.Enabled = checkGw1ScreenshotsLocation.Checked;
        }

        private void buttonGw1ScreenshotsLocation_Click(object sender, EventArgs e)
        {
            ShowOpenScreenshotLocationDialog(textGw1ScreenshotsLocation);
        }

        private void checkGw1ProcessPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboGw1ProcessPriority.Enabled = checkGw1ProcessPriority.Checked;
        }

        private void checkGw1ProcessAffinityAll_CheckedChanged(object sender, EventArgs e)
        {
            panelGw1ProcessAffinity.Visible = !checkGw1ProcessAffinityAll.Checked;
        }

        private void OnGw2ProcessPriorityChanged()
        {
            var enabled = !checkGw2ProcessPriority.Checked;

            if (!enabled)
            {
                var selected = (Util.ComboItem<Settings.ProcessPriorityClass>)comboGw2ProcessPriority.SelectedItem;
                if (selected == null)
                    return;

                switch (selected.Value)
                {
                    case Settings.ProcessPriorityClass.AboveNormal:
                    case Settings.ProcessPriorityClass.Normal:

                        enabled = true;

                        break;
                }
            }

            checkGw2ProcessBoostBrowser.Enabled = enabled;
        }

        private void comboGw2ProcessPriority_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnGw2ProcessPriorityChanged();
        }

        private void labelGw1LaunchOptionsAdvanced_Click(object sender, EventArgs e)
        {
            buttonGuildWars1.SelectPanel(panelLaunchOptionsGw1, panelLaunchOptionsAdvancedGw1);
        }

        private void labelGw2LaunchOptionsAdvanced_Click(object sender, EventArgs e)
        {
            buttonGuildWars2.SelectPanel(panelLaunchOptionsGw2, panelLaunchOptionsAdvancedGw2);
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

        private void labelLaunchOptionsAdvancedGw2Back_Click(object sender, EventArgs e)
        {
            buttonGuildWars2.SelectPanel(panelLaunchOptionsGw2);
        }

        private void labelLaunchOptionsAdvancedGw1Back_Click(object sender, EventArgs e)
        {
            buttonGuildWars1.SelectPanel(panelLaunchOptionsGw1);
        }

        private void checkGw2MumbleName_CheckedChanged(object sender, EventArgs e)
        {
            textGw2MumbleName.Enabled = checkGw2MumbleName.Checked;
            labelGw2MumbleNameVariables.Enabled = checkGw2MumbleName.Checked;
        }

        private void labelGw2MumbleNameVariables_Click(object sender, EventArgs e)
        {
            ShowVariables(textGw2MumbleName, sender, AnchorStyles.Top, Client.Variables.VariableType.Account);
        }

        private void labelWindowCaptionVariables_Click(object sender, EventArgs e)
        {
            ShowVariables(textWindowCaption, sender, AnchorStyles.Top, Client.Variables.VariableType.Account, Client.Variables.VariableType.Process);
        }

        private void comboEncryptionScope_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selected = comboEncryptionScope.SelectedItem as Util.ComboItem<Settings.EncryptionScope>;
            if (selected == null)
                return;

            string s;

            switch (selected.Value)
            {
                case Settings.EncryptionScope.CurrentUser:

                    s = "Passwords will only be accessible to the currently logged in user";

                    break;
                case Settings.EncryptionScope.LocalMachine:

                    s = "Passwords will be accesible to anyone on this PC";

                    break;
                case Settings.EncryptionScope.Portable:

                    s = "Passwords can be transferred to other computers";

                    break;
                case Settings.EncryptionScope.Unencrypted:

                    s = "Passwords will be stored in clear text";

                    break;
                default:

                    s = null;

                    break;
            }

            if (s != null)
                labelEncryptionScope.Text = s;
            labelEncryptionScope.Visible = s != null;

            if (encryptionState == ArgsState.Loading)
            {
                var scope = Settings.Encryption.HasValue ? Settings.Encryption.Value.Scope : Settings.EncryptionScope.CurrentUser;

                if (GetScopeWeigth(selected.Value) > GetScopeWeigth(scope))
                {
                    Settings.IAccount account = null;
                    var date = DateTime.MaxValue;
                    var puid = ushort.MaxValue;
                    bool b;

                    foreach (var a in Util.Accounts.GetAccounts())
                    {
                        if (!a.HasCredentials)
                            continue;

                        if (a.Password.UID < puid || a.Password.UID == puid && a.CreatedUtc < date)
                        {
                            account = a;
                            puid = a.Password.UID;
                            date = a.CreatedUtc;
                        }
                    }

                    if (!(b = account == null))
                    {
                        using (var f = new formPassword(account.Email, account.Password.ToSecureString()))
                        {
                            b = f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK;
                        }
                    }

                    if (!b)
                    {
                        Util.ComboItem<Settings.EncryptionScope>.Select(comboEncryptionScope, scope);
                        return;
                    }
                    else
                    {
                        encryptionState = ArgsState.Loaded;
                    }
                }
            }
        }

        private void radioGw2Mode_CheckedChanged(object sender, EventArgs e)
        {
            var isBasic = radioGw2ModeBasic.Checked;
            panelGw2ModeAdvanced.Parent.SuspendLayout();

            labelCustomUsernameBasicWarning.Visible = isBasic;
            panelGw2ModeBasic.Visible = isBasic;
            panelGw2ModeAdvanced.Visible = !isBasic;

            panelGw2ModeAdvanced.Parent.ResumeLayout();
        }

        private void panelLaunchConfigurationGw2_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (!e)
                return;

            string root;

            try
            {
                root = Path.GetDirectoryName(textGw2Path.Text);
            }
            catch
            {
                return;
            }

            if (!root.Equals(labelShowLocalizedExecution.Tag))
            {
                labelShowLocalizedExecution.Tag = root;
                SetShowLocalizedExecutionVisibility(Directory.Exists(Path.Combine(root, Client.FileManager.LOCALIZED_EXE_FOLDER_NAME)));
            }

            if (!root.Equals(checkLocalizedExecution.Tag))
            {
                checkLocalizedExecution.Tag = root;

                var s = Client.FileManager.IsPathSupported(root, true);

                if (!(checkLocalizedExecution.Enabled = (s & Client.FileManager.PathSupportType.Files) != 0))
                    checkLocalizedExecution.Checked = false;

                radioLocalizedExecutionBinaries.Enabled = (s & Client.FileManager.PathSupportType.Folders) != 0;
            }
        }

        /// <summary>
        /// Shows notfication screen position window
        /// </summary>
        /// <param name="type">Type of notification</param>
        /// <param name="tagged">The control storing the value</param>
        /// <param name="source">The control where the window will be centered</param>
        private void ShowNotificationSelector(formScreenPosition.NotificationType type, Control tagged, Control source)
        {
            var v = (Settings.ScreenAttachment)tagged.Tag;
            var f = (formScreenPosition)SetActive(new formScreenPosition(type, v.Screen, v.Anchor));
            f.FormClosing += delegate
            {
                switch (type)
                {
                    case formScreenPosition.NotificationType.Note:

                        tagged.Tag = new Settings.NotificationScreenAttachment((byte)f.SelectedScreen, f.SelectedAnchor, checkNoteNotificationsOnlyWhileActive.Checked);

                        break;
                    default:

                        tagged.Tag = new Settings.ScreenAttachment((byte)f.SelectedScreen, f.SelectedAnchor);

                        break;
                }
                f.Dispose();
            };
            f.StartPosition = FormStartPosition.Manual;
            f.Location = Point.Add(source.Parent.PointToScreen(Point.Empty), new Size(source.Location.X - f.Width / 2, source.Location.Y - f.Height / 2));
            f.Show(this);
        }

        private void labelScreenshotNotification_Click(object sender, EventArgs e)
        {
            ShowNotificationSelector(formScreenPosition.NotificationType.Screenshot, checkScreenshotNotification, labelScreenshotNotification);
        }

        private SidebarButton GetSidebarButton(Panels p)
        {
            switch (p)
            {
                case Panels.GuildWars1:
                    return buttonGuildWars1;
                case Panels.GuildWars2:
                    return buttonGuildWars2;
                case Panels.Security:
                    return buttonSecurity;
                case Panels.Style:
                    return buttonStyle;
                case Panels.Tools:
                    return buttonTools;
                case Panels.Updates:
                    return buttonUpdates;
                case Panels.Backup:
                    return buttonBackup;
                case Panels.General:
                default:
                    return buttonGeneral;
            }
        }

        public void SelectPanel(Panels p)
        {
            GetSidebarButton(p).Selected = true;
        }

        private void labelLocalizedExecutionBinariesCache_Click(object sender, EventArgs e)
        {
            buttonTools.SelectPanel(panelLocalDat);
        }

        private void radioLocalizedExecutionBinaries_CheckedChanged(object sender, EventArgs e)
        {
            EventHandler onChanged;

            if (labelLocalizedExecutionBinariesCache.Tag == null)
            {
                onChanged = delegate
                {
                    var showRequirement = radioLocalizedExecutionBinaries.Checked && !checkLocalDatCache.Checked;

                    labelLocalizedExecutionBinariesCache.Visible = showRequirement;
                    labelLocalizedExecutionBinariesNote.Visible = !showRequirement;
                };

                checkLocalDatCache.CheckedChanged += onChanged;
                labelLocalizedExecutionBinariesCache.Tag = onChanged;
            }
            else
            {
                onChanged = (EventHandler)labelLocalizedExecutionBinariesCache.Tag;
            }

            onChanged(null, null);
        }

        private void checkStyleShowIcon_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.ShowImage = checkStyleShowIcon.Checked;
        }

        private void labelFontTitleReset_Click(object sender, EventArgs e)
        {
            buttonFontTitle.Parent.SuspendLayout();
            labelFontTitleReset.Visible = false;
            labelFontTitleResetSep.Visible = false;
            buttonSample.FontName = UI.Controls.AccountGridButton.FONT_NAME;
            buttonFontTitle.Text = GetFontName(buttonSample.FontName);
            buttonFontTitle.Parent.ResumeLayout();
        }

        private void labelFontStatusReset_Click(object sender, EventArgs e)
        {
            buttonFontStatus.Parent.SuspendLayout();
            labelFontStatusReset.Visible = false;
            labelFontStatusResetSep.Visible = false;
            buttonSample.FontStatus = UI.Controls.AccountGridButton.FONT_STATUS;
            buttonFontStatus.Text = GetFontName(buttonSample.FontStatus);
            buttonFontStatus.Parent.ResumeLayout();
        }

        private void labelFontUserReset_Click(object sender, EventArgs e)
        {
            buttonFontUser.Parent.SuspendLayout();
            labelFontUserReset.Visible = false;
            labelFontUserResetSep.Visible = false;
            buttonSample.FontUser = UI.Controls.AccountGridButton.FONT_USER;
            buttonFontUser.Text = GetFontName(buttonSample.FontUser);
            buttonFontUser.Parent.ResumeLayout();
        }

        private void checkStyleColumns_CheckedChanged(object sender, EventArgs e)
        {
            labelStyleColumns.Parent.SuspendLayout();
            numericStyleColumns.Enabled = checkStyleColumns.Checked;
            labelStyleColumns.Visible = !checkStyleColumns.Checked;
            labelStyleColumns.Parent.ResumeLayout();
        }

        private string GetFontName(Font f)
        {
            return string.Format("{0}, {1:0.##}pt", f.Name, f.SizeInPoints);
        }

        private void labelStyleColumns_Click(object sender, EventArgs e)
        {
            checkStyleColumns.Checked = true;
        }
        
        private void OnSelectStyleBackgroundImage(string path)
        {
            var i = Bitmap.FromFile(path);

            using (buttonSample.BackgroundImage)
            {
                buttonSample.BackgroundImage = i;
            }

            checkStyleBackgroundImage.Tag = path;
            checkStyleBackgroundImage.Checked = true;
            menuImageBackgroundDefaultToolStripMenuItem.Checked = false;
        }

        private void OnSelectStyleBackgroundImage(bool reset)
        {
            if (reset)
            {
                using (buttonSample.BackgroundImage)
                {
                    buttonSample.BackgroundImage = null;
                }

                checkStyleBackgroundImage.Tag = null;
                checkStyleBackgroundImage.Checked = false;
                menuImageBackgroundDefaultToolStripMenuItem.Checked = true;
            }
            else
            {
                using (var f = new OpenFileDialog())
                {
                    f.Filter = "Images|*.bmp;*.jpg;*.jpeg;*.png";

                    var path = (string)checkStyleBackgroundImage.Tag;
                    if (!string.IsNullOrEmpty(path))
                        f.FileName = path;

                    if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        try
                        {
                            OnSelectStyleBackgroundImage(f.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void checkStyleBackgroundImage_Click(object sender, EventArgs e)
        {
            OnSelectStyleBackgroundImage(checkStyleBackgroundImage.Checked);
        }

        private void buttonSample_Click(object sender, EventArgs e)
        {
            if (panelStyle.Visible)
                contextImage.Show(Cursor.Position);
        }

        private void menuImageBackgroundToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnSelectStyleBackgroundImage(false);
        }

        private void menuImageBackgroundDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!menuImageBackgroundDefaultToolStripMenuItem.Checked)
            {
                OnSelectStyleBackgroundImage(true);
            }
        }

        private void ShowColorDialog(ColorNames c, Color color, UiColorsPreviewPanel panel)
        {
            using (var f = new ColorPicker.formColorDialog())
            {
                if (locationColorDialog.X != int.MinValue)
                {
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = locationColorDialog;
                }

                f.DefaultColor = colorInfo.Defaults[c];

                f.AllowAlphaTransparency = UiColors.SupportsAlpha(c);
                f.Color = color;

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    if (panel != null)
                    {
                        panel.Color1 = f.Color;
                    }

                    if (checkColorsShared.Checked)
                    {
                        foreach (var cn in colorInfo.GetShared(c))
                        {
                            if (cn != c)
                            {
                                colorInfo.SetColor(cn, f.Color, true, true);
                            }
                        }
                    }

                    colorInfo.SetColor(c, f.Color, panel == null, true);

                    if (checkColorsPreview.Checked)
                    {
                        colorInfo.Preview();
                    }
                    else if (buttonSample.Visible)
                    {
                        buttonSample.Colors = colorInfo.Values;
                    }

                }

                locationColorDialog = f.Location;
            }
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            var panel = (UiColorsPreviewPanel)sender;

            panel.Focus();

            ShowColorDialog(panel.ColorName, panel.Color1, panel);
        }

        private void ShowStatusColorSample(UiColors.Colors c)
        {
            switch (c)
            {
                case UiColors.Colors.AccountStatusDefault:

                    buttonSample.SetStatus("a minute ago (sample)", AccountGridButton.StatusColors.Default);

                    break;
                case UiColors.Colors.AccountStatusError:

                    buttonSample.SetStatus("failed (sample)", AccountGridButton.StatusColors.Error);

                    break;
                case UiColors.Colors.AccountStatusOK:

                    buttonSample.SetStatus("active (sample)", AccountGridButton.StatusColors.Ok);

                    break;
                case UiColors.Colors.AccountStatusWaiting:

                    buttonSample.SetStatus("waiting (sample)", AccountGridButton.StatusColors.Waiting);

                    break;
                default:

                    buttonSample.SetStatus("sample", AccountGridButton.StatusColors.Default);

                    break;
            }
        }

        private void panelColor_MouseEnter(object sender, EventArgs e)
        {
            ShowStatusColorSample(((UiColorsPreviewPanel)sender).ColorName);
        }

        private void panelColor_MouseLeave(object sender, EventArgs e)
        {
            //ShowStatusColorSample(UiColors.Colors.AccountName);
        }

        private void sliderGw1Volume_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            labelGw1Volume.Text = (int)(v * 100 + 0.5f) + "%";
        }

        private void panelColorHighlight_MouseEnter(object sender, EventArgs e)
        {
            bool focused = false, active = false;

            switch (((UiColorsPreviewPanel)sender).ColorName)
            {
                case ColorNames.AccountFocusedBorder:
                case ColorNames.AccountFocusedHighlight:
                    focused = true;
                    break;
                case ColorNames.AccountActiveBorder:
                case ColorNames.AccountActiveHighlight:
                    active = true;
                    break;
            }

            buttonSample.IsFocused = focused;
            buttonSample.IsActive = active;
            buttonSample.IsActiveHighlight = active;
        }

        private void panelColorHighlight_MouseLeave(object sender, EventArgs e)
        {
            //buttonSample.IsFocused = false;
            //buttonSample.IsActiveHighlight = false;
            //buttonSample.IsActive = false;
        }

        private void ShowPressedSample(AccountGridButton.PressedState state, Color flash, Color fill)
        {
            if (cancelPressed != null)
            {
                cancelPressed.Cancel();
                cancelPressed.Dispose();
            }

            cancelPressed = new CancellationTokenSource();

            var pe = new AccountGridButtonContainer.MousePressedEventArgs(System.Windows.Forms.MouseButtons.Left, 0, 0, 0, 0)
            {
                Handled = true,
                FlashColor = flash,
                FillColor = fill,
            };

            buttonSample.BeginPressed(state, cancelPressed.Token, pe);
        }

        private void panelColorFocus_MouseEnter(object sender, EventArgs e)
        {
            ShowPressedSample(AccountGridButton.PressedState.Pressed, colorInfo.Values[ColorNames.AccountActionFocusFlash], Color.Empty);
        }

        private void panelColorExitFill_MouseEnter(object sender, EventArgs e)
        {
            ShowPressedSample(AccountGridButton.PressedState.Pressing, colorInfo.Values[ColorNames.AccountActionExitFlash], colorInfo.Values[ColorNames.AccountActionExitFill]);
        }

        private void panelColorExitFlash_MouseEnter(object sender, EventArgs e)
        {
            ShowPressedSample(AccountGridButton.PressedState.Pressed, colorInfo.Values[ColorNames.AccountActionExitFlash], Color.Empty);
        }

        private void panelColorBackground_MouseEnter(object sender, EventArgs e)
        {
            bool hovered = false, selected = false, active = false;

            switch (((UiColorsPreviewPanel)sender).ColorName)
            {
                case ColorNames.AccountBackColorHovered:
                case ColorNames.AccountBorderDarkHovered:
                case ColorNames.AccountBorderLightHovered:
                case ColorNames.AccountForeColorHovered:
                    hovered = true;
                    break;
                case ColorNames.AccountBackColorSelected:
                case ColorNames.AccountBorderDarkSelected:
                case ColorNames.AccountBorderLightSelected:
                case ColorNames.AccountForeColorSelected:
                    selected = true;
                    break;
                case ColorNames.AccountBackColorActive:
                case ColorNames.AccountBorderDarkActive:
                case ColorNames.AccountBorderLightActive:
                case ColorNames.AccountForeColorActive:
                    active = true;
                    break;
            }

            buttonSample.IsHovered = hovered;
            buttonSample.Selected = selected;
            buttonSample.IsFocused = false;
            buttonSample.IsActiveHighlight = false;
            buttonSample.IsActive = active;
        }
        
        private void panelColorBackground_MouseLeave(object sender, EventArgs e)
        {
            //buttonSample.IsHovered = false;
            //buttonSample.Selected = false;
            //buttonSample.IsFocused = false;
            //buttonSample.IsActiveHighlight = false;
            //buttonSample.IsActive = false;
        }

        private void numericLaunchLimiterRechargeTime_ValueChanged(object sender, EventArgs e)
        {
            var v = (byte)numericLaunchLimiterRechargeTime.Value;
            if (v != 1)
            {
                v = 0;
            }

            if (labelLaunchLimiterRechargeTime.Tag == null || (byte)labelLaunchLimiterRechargeTime.Tag != v)
            {
                labelLaunchLimiterRechargeTime.Tag = v;
                labelLaunchLimiterRechargeTime.Text = v == 1 ? "second" : "seconds";
            }
        }

        private void checkLaunchLimiter_CheckedChanged(object sender, EventArgs e)
        {
            panelLimiter.Parent.SuspendLayout();

            panelLimiter.Visible = checkLaunchLimiter.Checked;
            panelLimiterManual.Visible = radioLimiterManual.Checked && checkLaunchLimiter.Checked;

            panelLimiter.Parent.ResumeLayout();
        }

        private void checkTimeoutRelaunch_CheckedChanged(object sender, EventArgs e)
        {
            numericTimeoutRelaunch.Enabled = label171.Enabled = checkTimeoutRelaunch.Checked;
        }

        private void radioLocalizedExecutionAutoSyncAll_CheckedChanged(object sender, EventArgs e)
        {
            checkLocalizedExecutionAutoSyncDelete.Enabled = radioLocalizedExecutionAutoSyncAll.Checked;
        }

        private void checkJumpList_CheckedChanged(object sender, EventArgs e)
        {
            panelJumpListOptions.Visible = checkJumpList.Checked;
        }

        private void checkProcessPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboProcessPriority.Enabled = checkProcessPriority.Checked;
        }

        private void radioLimiterManual_CheckedChanged(object sender, EventArgs e)
        {
            panelLimiterManual.Visible = radioLimiterManual.Checked && checkLaunchLimiter.Checked;
        }

        private async void labelShowTotpCodeOption_Click(object sender, EventArgs e)
        {
            var l = (Label)sender;

            l.Enabled = false;

            try
            {
                var args = l.Text;

                await Task.Run(new Action(
                    delegate
                    {
                        Util.ProcessUtil.Execute(args, false);
                    }));
            }
            catch { }

            l.Enabled = true;
        }

        private void radioNetworkVerifyAutomatic_CheckedChanged(object sender, EventArgs e)
        {
            checkNetworkVerifyAutomaticIP.Parent.Enabled = radioNetworkVerifyAutomatic.Checked;
            checkNetworkVerifyAutomaticIP.Enabled = true;
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
                Margin = template.Margin,
                TextAlign = ContentAlignment.MiddleLeft,
            };
            l.Click += onClick;
            container.Controls.Add(l);

            if (container.Controls.Count == 1)
            {
                if (container == panelGw2RunAfterPrograms)
                    panelGw2RunAfterProgramsAddSeparator.Visible = true;
                else if (container == panelGw1RunAfterPrograms)
                    panelGw1RunAfterProgramsAddSeparator.Visible = true;
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

        private void labelGw2RunAfterProgramsAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formRunAfter(Settings.AccountType.GuildWars2))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    CreateRunAfterButton(f.Result, panelGw2RunAfterPrograms, labelGw2RunAfterProgramsAdd, labelRunAfterProgram_Click);
                    panelLaunchOptionsGw2.ScrollControlIntoView((Control)sender);
                }
            }
        }

        private void labelGw1RunAfterProgramsAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formRunAfter(Settings.AccountType.GuildWars1))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    CreateRunAfterButton(f.Result, panelGw1RunAfterPrograms, labelGw1RunAfterProgramsAdd, labelRunAfterProgram_Click);
                    panelLaunchOptionsGw1.ScrollControlIntoView((Control)sender);
                }
            }
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
            var type = Settings.AccountType.GuildWars2;

            if (l.Parent == panelGw1RunAfterPrograms)
                type = Settings.AccountType.GuildWars1;

            using (var f = new formRunAfter(r, type))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    UpdateRunAfterButton(l, f.Result);
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
                if (panel == panelGw2RunAfterPrograms)
                    panelGw2RunAfterProgramsAddSeparator.Visible = false;
                else if (panel == panelGw1RunAfterPrograms)
                    panelGw1RunAfterProgramsAddSeparator.Visible = false;
                panel.Visible = false;
                panel.Parent.ResumeLayout();
            }
            label.Dispose();
        }

        private void CreateHotkeyButton(Settings.Hotkey h, Settings.IAccount account = null)
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
            using (var f = new formHotkey())
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    CreateHotkeyButton(f.Result, f.SelectedAccount);
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

            using (var f = new formHotkey(h.Hotkey, h.Account, false))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panelHotkeysHotkeys.Update(f.Result, h.Hotkey, panelHotkeysHotkeys.GetText(f.Result), f.SelectedAccount);
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

        private void checkStyleHeight_CheckedChanged(object sender, EventArgs e)
        {
            labelStyleHeight.Parent.SuspendLayout();
            numericStyleHeight.Enabled = checkStyleHeight.Checked;
            labelStyleHeight.Visible = !checkStyleHeight.Checked;
            labelStyleHeight.Parent.ResumeLayout();
            OnStyleCustomizeOffsetsChanged();
        }

        private void checkStyleSpacing_CheckedChanged(object sender, EventArgs e)
        {
            numericStyleSpacing.Enabled = checkStyleSpacing.Checked;
        }

        private void checkStyleCustomizeOffsets_CheckedChanged(object sender, EventArgs e)
        {
            panelStyleCustomizeOffsets.Visible = checkStyleCustomizeOffsets.Checked;
            OnStyleCustomizeOffsetsChanged();
        }

        private void OnStyleCustomizeOffsetsChanged()
        {
            if (!checkStyleCustomizeOffsets.Checked)
            {
                buttonSample.Offsets = null;
                return;
            }

            var offsets = buttonSample.Offsets;
            if (offsets == null)
                offsets = new Settings.AccountGridButtonOffsets();

            offsets[Settings.AccountGridButtonOffsets.Offsets.Name] = new Settings.Point<sbyte>((sbyte)numericStyleOffsetNameX.Value, (sbyte)numericStyleOffsetNameY.Value);
            offsets[Settings.AccountGridButtonOffsets.Offsets.Status] = new Settings.Point<sbyte>((sbyte)numericStyleOffsetStatusX.Value, (sbyte)numericStyleOffsetStatusY.Value);
            offsets[Settings.AccountGridButtonOffsets.Offsets.User] = new Settings.Point<sbyte>((sbyte)numericStyleOffsetUserX.Value, (sbyte)numericStyleOffsetUserY.Value);
            offsets.Height = checkStyleHeight.Checked ? (ushort)numericStyleHeight.Value : (ushort)0;

            buttonSample.Offsets = offsets;
        }

        private void numericStyleOffset_ValueChanged(object sender, EventArgs e)
        {
            OnStyleCustomizeOffsetsChanged();
        }

        private void numericStyleHeight_ValueChanged(object sender, EventArgs e)
        {
            OnStyleCustomizeOffsetsChanged();
        }

        private void labelExportAccounts_Click(object sender, EventArgs e)
        {
            using (var f = new Backup.formAccountExport())
            {
                f.ShowDialog(this);
            }
        }

        private void labelImportAccounts_Click(object sender, EventArgs e)
        {
            using (var f = new Backup.formAccountImport())
            {
                f.ShowDialog(this);

                if (f.Accounts != null && f.Accounts.Count > 0)
                {
                    owner.AddAccounts(f.Accounts, true);
                    MessageBox.Show(this, f.Accounts.Count + (f.Accounts.Count == 1 ? " account has" : " accounts have") + " been imported", "Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void checkStyleShowLoginRewardIcon_CheckedChanged(object sender, EventArgs e)
        {
            panelStyleLoginRewardIconsContainer.Visible = checkStyleShowLoginRewardIcon.Checked;
        }

        private void InitializeAffinityAccountsPanel(StackPanel panel, IEnumerable<Settings.IAccount> accounts)
        {
            var p = new AffinityAccountsPanel(accounts)
            {
                AutoSizeFill = StackPanel.AutoSizeFillMode.Width,
                AutoSize = true,
                Margin = Padding.Empty,
            };
            Scale(p);
            panel.Controls.Add(p);
            panel.Tag = p;
        }

        private void checkGw2ProcessAffinityAccounts_CheckedChanged(object sender, EventArgs e)
        {
            if (checkGw2ProcessAffinityAccounts.Checked && panelGw2ProcessAffinityAccounts.Tag == null)
            {
                InitializeAffinityAccountsPanel(panelGw2ProcessAffinityAccounts, Util.Accounts.GetGw2Accounts());
            }
            panelGw2ProcessAffinityAccounts.Visible = checkGw2ProcessAffinityAccounts.Checked;
        }

        private void checkGw1ProcessAffinityAccounts_CheckedChanged(object sender, EventArgs e)
        {
            if (checkGw1ProcessAffinityAccounts.Checked && panelGw1ProcessAffinityAccounts.Tag == null)
            {
                InitializeAffinityAccountsPanel(panelGw1ProcessAffinityAccounts, Util.Accounts.GetGw1Accounts());
            }
            panelGw1ProcessAffinityAccounts.Visible = checkGw1ProcessAffinityAccounts.Checked;
        }

        private void panelBackup_PreVisiblePropertyChanged(object sender, bool e)
        {
            if (e)
            {
                var hasPasswords = false;

                if (labelBackupPasswordChange.Tag == null)
                {
                    foreach (var a in Util.Accounts.GetAccounts())
                    {
                        if (a.Password != null)
                        {
                            hasPasswords = true;
                            break;
                        }
                    }

                    labelBackupPasswordChange.Tag = hasPasswords;
                }
                else
                {
                    hasPasswords = (bool)labelBackupPasswordChange.Tag;
                }

                var scope = Util.ComboItem<Settings.EncryptionScope>.SelectedValue(comboEncryptionScope, Settings.EncryptionScope.CurrentUser);
                var b = hasPasswords && (scope == Settings.EncryptionScope.CurrentUser || scope == Settings.EncryptionScope.LocalMachine);

                panelBackup.SuspendLayout();
                labelBackupPasswordUser.Visible = b && scope == Settings.EncryptionScope.CurrentUser;
                labelBackupPasswordMachine.Visible = b && scope == Settings.EncryptionScope.LocalMachine;
                labelBackupPasswordChange.Visible = b;
                labelBackupPasswords.Visible = b;
                panelBackup.ResumeLayout();
            }
        }

        private void labelBackupPasswordChange_Click(object sender, EventArgs e)
        {
            buttonSecurity.SelectPanel(panelSecurity);
        }

        private async void buttonBackupBackup_Click(object sender, EventArgs e)
        {
            string path;

            using (var f = new SaveFileDialog())
            {
                f.Filter = "Backup|*.bkp";
                f.FileName = "Gw2Launcher-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

                if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                    return;

                path = f.FileName;
            }

            this.Enabled = false;

            using (var f = new formProgressBar()
            {
                Maximum = 1000,
            })
            {
                using (var cancel = new CancellationTokenSource())
                {
                    f.FormClosing += delegate
                    {
                        cancel.Cancel();
                    };

                    var o = new Tools.Backup.Backup.BackupOptions()
                    {
                        Format = checkBackupSingleFile.Checked ? Tools.Backup.Backup.BackupFormat.File : Tools.Backup.Backup.BackupFormat.Directory,
                        IncludeLocalDat = checkBackupIncludeLocalDat.Checked,
                        IncludeGfxSettings = checkBackupIncludeGfxSettings.Checked,
                    };

                    var backup = new Tools.Backup.Backup.Exporter(path, o);

                    backup.ProgressChanged += delegate
                    {
                        f.Value = (int)(backup.Progress * f.Maximum);
                    };

                    var t = new Task(new Action(
                        delegate
                        {
                            backup.Export(cancel.Token);
                        }), TaskCreationOptions.LongRunning);
                    t.Start();

                    f.CenterAt(this);
                    f.Show(this);

                    try
                    {
                        await t;
                        f.Hide();
                    }
                    catch (Exception ex)
                    {
                        f.Hide();
                        if (!cancel.IsCancellationRequested)
                            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

            this.Enabled = true;
        }

        private void labelBackupRestore_Click(object sender, EventArgs e)
        {
            string path;

            using (var f = new OpenFileDialog())
            {
                f.Filter = "Backup|*.bkp";

                if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                    return;

                path = f.FileName;
            }

            var backup = new Tools.Backup.Backup.Importer(path);
            Tools.Backup.Backup.RestoreInformation ri;

            try
            {
                ri = backup.ReadInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var hasFiles = false;

            foreach (var f in ri.Files)
            {
                if (f.Input.Exists)
                {
                    hasFiles = true;
                    break;
                }
            }

            if (!hasFiles)
            {
                MessageBox.Show(this, "Backup contains no recoverable data", "No files found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var f = new UI.Backup.formBackupRestore(ri))
            {
                f.ShowDialog(this);
            }
        }

        private Tools.Markers.MarkerIcons GetMarkerIcons()
        {
            if (markerIcons != null)
                return markerIcons;

            var icons = markerIcons = new Tools.Markers.MarkerIcons(false);

            for (int i = 0, count = Settings.Markers.Count; i < count; i++)
            {
                var m = Settings.Markers[i];
                if (m.IconType == Settings.MarkerIconType.Icon)
                {
                    icons.Add(m.IconPath);
                }
            }

            return icons;
        }

        private void labelMarkersAdd_Click(object sender, EventArgs e)
        {
            using (var f = new Markers.formMarker(GetMarkerIcons()))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panelMarkersContainer.Add(f.Result, markerIcons, true);
                }
            }
        }

        private void editMarkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = (MarkerListPanel.MarkerListItem)contextMarker.Tag;
            var m = (Settings.IMarker)l.Value;

            using (var f = new Markers.formMarker(GetMarkerIcons(), m))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK && !f.Result.Equals(m))
                {
                    l.Update(f.Result, markerIcons);
                }
            }
        }

        private void deleteMarkerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var l = (MarkerListPanel.MarkerListItem)contextMarker.Tag;
            var m = (Settings.IMarker)l.Value;
            var inUse = false;

            foreach (var a in Util.Accounts.GetAccounts())
            {
            }

            if (inUse && MessageBox.Show(this, "Deleting this marker will remove it from any account using it. Are you sure?", "Marker in use", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                return;

            l.Hidden = true;
        }

        private void panelMarkersContainer_MarkerMouseClick(object sender, ControlListPanel.ListItemMouseEventArgs e)
        {
            contextMarker.Tag = e.Item;
            contextMarker.Show(Cursor.Position);
        }

        private void panelMarkersContainer_LabelMouseClick(object sender, ControlListPanel.ListItemMouseEventArgs e)
        {
            contextMarker.Tag = e.Item;
            contextMarker.Show(Cursor.Position);
        }

        private void checkStyleMarkActive_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.IsActiveHighlight = checkStyleMarkActive.Checked;
            buttonSample.IsActive = checkStyleMarkActive.Checked;
            buttonSample.IsFocused = checkStyleMarkFocused.Checked && !buttonSample.IsActiveHighlight;
        }

        private void checkDxTimeoutRelaunch_CheckedChanged(object sender, EventArgs e)
        {
            numericDxTimeoutRelaunch.Enabled = label221.Enabled = checkDxTimeoutRelaunch.Checked;
        }

        private void checkGw2ProcessPriorityDx_CheckedChanged(object sender, EventArgs e)
        {
            comboGw2ProcessPriorityDx.Enabled = checkGw2ProcessPriorityDx.Checked;
        }

        private void checkPreventTaskbarGrouping_CheckedChanged(object sender, EventArgs e)
        {
            if (checkPreventTaskbarGrouping.Checked)
                checkForceTaskbarGrouping.Checked = false;
        }

        private void checkForceTaskbarGrouping_CheckedChanged(object sender, EventArgs e)
        {
            if (checkForceTaskbarGrouping.Checked)
                checkPreventTaskbarGrouping.Checked = false;
        }

        private async void labelVersionChangelog_Click(object sender, EventArgs e)
        {
            labelVersionChangelog.Enabled = false;

            Util.Explorer.OpenUrl("https://github.com/Healix/Gw2Launcher/wiki/Changes");

            await Task.Delay(500);

            labelVersionChangelog.Enabled = true;
        }

        private void ShowTooltip(Control c, string tooltip)
        {
            if (ftooltip == null)
                ftooltip = new Tooltip.FloatingTooltip();
            ftooltip.ShowTooltip(c, tooltip);
        }

        private void labelVersionChangelog_MouseEnter(object sender, EventArgs e)
        {
            ShowTooltip(labelVersionChangelog, "Open changelog on GitHub");
        }

        private Dictionary<Settings.IAccount, bool> GetLocalizedExecutionAccountsSelected()
        {
            var accounts = (Dictionary<Settings.IAccount, bool>)labelLocalizedExecutionAccountsSelected.Tag;

            if (accounts == null)
            {
                var selected = 0;

                accounts = new Dictionary<Settings.IAccount, bool>();

                foreach (var a in Util.Accounts.GetGw2Accounts())
                {
                    accounts[a] = a.LocalizedExecution;

                    if (a.LocalizedExecution)
                        selected++;
                }

                labelLocalizedExecutionAccountsSelected.Tag = accounts;

                labelLocalizedExecutionAccountsSelectedCount.Text = selected + " selected";
                labelLocalizedExecutionAccountsSelectedCount.Visible = selected > 0;
            }

            return accounts;
        }

        private void labelLocalizedExecutionAccountsSelected_Click(object sender, EventArgs e)
        {
            var accounts = GetLocalizedExecutionAccountsSelected();

            using (var f = new formAccountSelect("Select accounts", accounts, true))
            {
                if (f.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                    return;

                foreach (var a in accounts.Keys.ToArray())
                {
                    accounts[a] = false;
                }

                foreach (var a in f.Selected)
                {
                    accounts[a] = true;
                }

                labelLocalizedExecutionAccountsSelectedCount.Text = f.Selected.Count + " selected";
                labelLocalizedExecutionAccountsSelectedCount.Visible = f.Selected.Count > 0;
            }
        }

        private void panelLocalizedExecution_VisibleChanged(object sender, EventArgs e)
        {
            if (panelLocalizedExecution.Visible)
                GetLocalizedExecutionAccountsSelected();
        }

        private void numericTweakLoginDelay_ValueChanged(object sender, EventArgs e)
        {
            labelTweakLoginDelay.Text = numericTweakLoginDelay.Value == 1 ? "second" : "seconds";
        }

        private void numericTweakLauncherDelay_ValueChanged(object sender, EventArgs e)
        {
            labelTweakLauncherDelay.Text = numericTweakLauncherDelay.Value == 1 ? "second" : "seconds";
        }

        private void checkTweakLogin_CheckedChanged(object sender, EventArgs e)
        {
            panelTweakLogin.Visible = checkTweakLogin.Checked;
        }

        private void checkTweakLauncher_CheckedChanged(object sender, EventArgs e)
        {
            panelTweakLauncher.Visible = checkTweakLauncher.Checked;
        }

        private void labelTweakLauncherCoords_Click(object sender, EventArgs e)
        {
            Settings.Point<ushort> empty, play;

            if (labelTweakLauncherCoords.Tag != null)
            {
                var v = (Settings.LauncherPoints)labelTweakLauncherCoords.Tag;
                empty = v.EmptyArea;
                play = v.PlayButton;
            }
            else if (Settings.GuildWars2.LauncherAutologinPoints.HasValue)
            {
                var v = Settings.GuildWars2.LauncherAutologinPoints.Value;
                empty = v.EmptyArea;
                play = v.PlayButton;
            }
            else
            {
                empty = play = new Settings.Point<ushort>();
            }

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
                labelTweakLauncherCoords.Tag = new Settings.LauncherPoints()
                {
                    EmptyArea = f.EmptyLocation,
                    PlayButton = f.PlayLocation,
                };
            }

            this.Owner.Visible = true;
            this.Visible = true;

            this.Select();

            f.Dispose();
        }

        void textNetworkAddresses_TextChanged(object sender, EventArgs e)
        {
            textNetworkAddresses.TextChanged -= textNetworkAddresses_TextChanged;
            textNetworkAddresses.Tag = true;
        }

        private void checkNetworkEnable_CheckedChanged(object sender, EventArgs e)
        {
            checkNetworkExact.Parent.Enabled = checkNetworkEnable.Checked;
            checkNetworkWarnOnChange.Enabled = checkNetworkEnable.Checked;
        }

        private void checkGw2PathSteam_CheckedChanged(object sender, EventArgs e)
        {
            var b = checkGw2PathSteam.Checked;

            checkGw2PathSteam.Parent.SuspendLayout();

            if (b)
            {
                checkGw2PathSteam.Tag = checkGw2PathSteam.Text;
                checkGw2PathSteam.Text = "";
            }
            else
            {
                checkGw2PathSteam.Text = checkGw2PathSteam.Tag as string;
            }

            textGw2PathSteam.Visible = b;
            buttonGw2PathSteam.Visible = b;

            checkGw2PathSteam.Parent.ResumeLayout();
        }

        private void buttonGw2PathSteam_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.ValidateNames = false;
                f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
                f.Title = "Open Gw2-64.exe";

                if (textGw2PathSteam.TextLength != 0)
                {
                    try
                    {
                        f.InitialDirectory = System.IO.Path.GetDirectoryName(textGw2PathSteam.Text);
                    }
                    catch { }
                }

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    textGw2PathSteam.Text = f.FileName;
                    textGw2PathSteam.Select(textGw2PathSteam.TextLength, 0);
                }
            }
        }

        private void checkSteamPath_CheckedChanged(object sender, EventArgs e)
        {
            var b = checkSteamPath.Checked;
            textSteamPath.Enabled = b;
            buttonSteamPath.Enabled = b;
        }

        private void panelSteamGw2_PreVisiblePropertyChanged(object sender, bool e)
        {
            panelSteamGw2.PreVisiblePropertyChanged -= panelSteamGw2_PreVisiblePropertyChanged;

            if (!checkSteamPath.Checked)
            {
                try
                {
                    textSteamPath.Text = Path.GetFullPath(Client.Steam.Path);
                }
                catch { }
            }
        }

        private void InitColors()
        {
            tooltipColors = new ToolTip();
            tooltipColors.InitialDelay = 100;

            colorInfo = new ColorInfo()
            {
                Panels = new Dictionary<ColorNames, UiColorsPreviewPanel>(),
            };

            panelColorsContent.SuspendLayout();
            
            panelColorsAccount.Controls.AddRange(new Control[]
                {
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Name"),
                        CreateColor(ColorNames.AccountName, ""),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Status"),
                        CreateColor(ColorNames.AccountStatusDefault, "Default status color", panelColor_MouseEnter, panelColor_MouseLeave),
                        CreateColor(ColorNames.AccountStatusOK, "OK status color", panelColor_MouseEnter, panelColor_MouseLeave),
                        CreateColor(ColorNames.AccountStatusWaiting, "Pending status color", panelColor_MouseEnter, panelColor_MouseLeave),
                        CreateColor(ColorNames.AccountStatusError, "Error status color", panelColor_MouseEnter, panelColor_MouseLeave),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("User"),
                        CreateColor(ColorNames.AccountUser, ""),
                    }),

                    CreateColorsSeparator(),

                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Default"),
                        CreateColor(ColorNames.AccountBackColorDefault, "Background color", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderLightDefault, "Border color (top/left)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderDarkDefault, "Border color (bottom/right)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Hovered"),
                        CreateColor(ColorNames.AccountBackColorHovered, "Background color", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderLightHovered, "Border color (top/left)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderDarkHovered, "Border color (bottom/right)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountForeColorHovered, "Overlay color (when using a background image)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Selected"),
                        CreateColor(ColorNames.AccountBackColorSelected, "Background color", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderLightSelected, "Border color (top/left)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderDarkSelected, "Border color (bottom/right)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountForeColorSelected, "Overlay color (when using a background image)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Active"),
                        CreateColor(ColorNames.AccountBackColorActive, "Background color", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderLightActive, "Border color (top/left)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountBorderDarkActive, "Border color (bottom/right)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                        CreateColor(ColorNames.AccountForeColorActive, "Overlay color (when using a background image)", panelColorBackground_MouseEnter, panelColorBackground_MouseLeave),
                    }),

                    CreateColorsSeparator(),

                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Highlight"),
                        CreateColor(ColorNames.AccountFocusedHighlight, "Focused background color", panelColorHighlight_MouseEnter, panelColorHighlight_MouseLeave),
                        CreateColor(ColorNames.AccountFocusedBorder, "Focused border color", panelColorHighlight_MouseEnter, panelColorHighlight_MouseLeave),
                        CreateColor(ColorNames.AccountActiveHighlight, "Active background color", panelColorHighlight_MouseEnter, panelColorHighlight_MouseLeave),
                        CreateColor(ColorNames.AccountActiveBorder, "Active border color", panelColorHighlight_MouseEnter, panelColorHighlight_MouseLeave),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Actions"),
                        CreateColor(ColorNames.AccountActionFocusFlash, "Focus flash color", panelColorFocus_MouseEnter),
                        CreateColor(ColorNames.AccountActionExitFill, "Exit fill color", panelColorExitFill_MouseEnter),
                        CreateColor(ColorNames.AccountActionExitFlash, "Exit flash color", panelColorExitFlash_MouseEnter),
                    }),
                });

            panelColorsUi.Controls.AddRange(new Control[]
                {
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Background"),
                        CreateColor(ColorNames.MainBackColor, "Main background color"),
                        CreateColor(ColorNames.MainBorder, "Main border color"),
                        CreateColor(ColorNames.BackColor100, "100% background color"),
                        CreateColor(ColorNames.BackColor95, "95% background color"),
                        CreateColor(ColorNames.BackColor90, "90% background color"),
                        CreateColor(ColorNames.BackColor80, "80% background color"),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Text"),
                        CreateColor(ColorNames.Text, "Text color"),
                        CreateColor(ColorNames.TextGray, "Gray text"),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Title bar"),
                        CreateColor(ColorNames.BarTitle, "Title color"),
                        CreateColor(ColorNames.BarTitleInactive, "Title color (inactive)"),
                        CreateColor(ColorNames.BarBorder, "Border color"),
                        CreateColor(ColorNames.BarBackColor, "Background color"),
                        CreateColor(ColorNames.BarBackColorHovered, "Background color (hovered)"),
                        CreateColor(ColorNames.BarForeColor, "Foreground color"),
                        CreateColor(ColorNames.BarForeColorHovered, "Foreground color (hovered)"),
                        CreateColor(ColorNames.BarForeColorInactive, "Foreground color (inactive)"),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel(""),
                        CreateColor(ColorNames.BarMinimizeForeColor, "Minimize button foreground color"),
                        CreateColor(ColorNames.BarMinimizeForeColorInactive, "Minimize button foreground color (inactive)"),
                        CreateColor(ColorNames.BarCloseForeColor, "Close button foreground color"),
                        CreateColor(ColorNames.BarCloseForeColorInactive, "Close button foreground color (inactive)"),
                    }),
                    CreateColorsContainer(new Control[]
                    {
                        CreateColorLabel("Menu"),
                        CreateColor(ColorNames.MenuText, "Text color"),
                        CreateColor(ColorNames.MenuBackColor, "Background color"),
                        CreateColor(ColorNames.MenuBackColorHovered, "Background color (hovered)"),
                        CreateColor(ColorNames.MenuSeparator, "Separator color"),
                    }),
                });

            panelColorsContent.ResumeLayout();
        }

        private Control CreateColorsSeparator()
        {
            var panel = new Control()
            {
                Size = Scale(160, 1),
                BackColor = Color.FromArgb(224, 224, 224),
                Margin = Scale(10, 8, 10, 5),
            };

            return panel;
        }

        private StackPanel CreateColorsContainer(Control[] controls)
        {
            var panel = new StackPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                Margin = Scale(0, 3, 0, 0),
                AutoSize = true,
                AutoSizeFill = StackPanel.AutoSizeFillMode.NoWrap,
            };

            if (controls != null)
            {
                panel.Controls.AddRange(controls);
            }

            return panel;
        }

        private Label CreateColorLabel(string text, int indent = 0)
        {
            return new Label()
                {
                    Text = text,
                    Font = labelColorTemplate.Font,
                    MinimumSize = labelColorTemplate.MinimumSize,
                    Anchor = AnchorStyles.Left,
                    Margin = indent == 0 ? Padding.Empty : new Padding(Scale(indent), 0, 0, 0),
                    AutoSize = true,
                };
        }

        private UiColorsPreviewPanel CreateColor(UiColors.Colors color, string tooltip = null, EventHandler onMouseEnter = null, EventHandler onMouseLeave = null)
        {
            var panel = new UiColorsPreviewPanel()
            {
                Margin = panelColorTemplate.Margin,
                Anchor = AnchorStyles.Left,
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Windows.Cursors.Hand,
                ColorName = color,
                Tooltip = tooltip,
                Size = panelColorTemplate.Size,
                Color1 = colorInfo.Values[color],
            };

            if (onMouseEnter != null)
                panel.MouseEnter += onMouseEnter;
            if (onMouseLeave != null)
                panel.MouseLeave += onMouseLeave;

            if (!string.IsNullOrEmpty(tooltip))
                tooltipColors.SetToolTip(panel, panel.Tooltip);
            panel.Click += panelColor_Click;

            colorInfo.Panels[color] = panel;

            return panel;
        }

        private void panelColors_PreVisiblePropertyChanged(object sender, bool e)
        {
            panelColors.PreVisiblePropertyChanged -= panelColors_PreVisiblePropertyChanged;

            InitColors();

            comboColors.SelectedIndex = 0;
        }

        private void OnSamplePreVisiblePropertyChanged(object sender, bool e)
        {
            if (!e)
                return;

            StackPanel parent;

            if (sender == panelStyle)
            {
                parent = panelStyleSample;

                buttonSample.IsFocused = checkStyleMarkFocused.Checked;
                buttonSample.IsActiveHighlight = checkStyleMarkActive.Checked && !buttonSample.IsFocused;
                buttonSample.IsActive = checkStyleMarkActive.Checked;
            }
            else if (sender == panelColors)
            {
                parent = panelColorsAccountSample;

                buttonSample.IsHovered = false;
                buttonSample.Selected = false;
                buttonSample.IsFocused = false;
                buttonSample.IsActive = false;
                buttonSample.IsActiveHighlight = false;
            }
            else
            {
                return;
            }

            if (buttonSample.Parent != parent)
            {
                parent.Controls.Add(buttonSample);
            }
        }

        private IEnumerable<UiColorsPreviewPanel> EnumerateColorPanels(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is UiColorsPreviewPanel)
                {
                    yield return (UiColorsPreviewPanel)c;
                }
                else
                {
                    foreach (var p in EnumerateColorPanels(c))
                    {
                        yield return p;
                    }
                }
            }
        }

        private void presetExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new SaveFileDialog())
            {
                f.Filter = "Colors in #RRGGBB|*.txt|Colors in R,G,B|*.txt";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var format = UiColors.ColorFormat.Hex;

                        if (f.FilterIndex == 2)
                            format = UiColors.ColorFormat.RGB;

                        UiColors.Export(f.FileName, colorInfo.Values, format);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void presetImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var f = new OpenFileDialog())
            {
                f.Filter = "Colors|*.txt";

                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        var count = 0;

                        foreach (var v in UiColors.Import(f.FileName))
                        {
                            colorInfo.SetColor(v.Key, v.Value, true);
                            ++count;
                        }

                        if (count > 0)
                        {
                            if (checkColorsPreview.Checked)
                            {
                                colorInfo.Preview();
                            }
                            else
                            {
                                buttonSample.Colors = colorInfo.Values;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadTheme(UiColors.Theme theme)
        {
            var colors = UiColors.GetTheme(theme);
            var count = colors.Count;

            for (var i = 0; i < count; i++)
            {
                colorInfo.SetColor((ColorNames)i, colors[(ColorNames)i], false);
            }

            colorInfo.Values.BaseTheme = colors.BaseTheme;
            colorInfo.ToControls();

            if (checkColorsPreview.Checked)
            {
                colorInfo.Preview();
            }
            else
            {
                buttonSample.Colors = colorInfo.Values;
            }
        }

        private void presetLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Light);
        }

        private void presetDarkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Dark);
        }

        private void labelColorsPreset_Click(object sender, EventArgs e)
        {
            contextColorsPreset.Show(Cursor.Position);
        }

        private void checkColorsPreview_CheckedChanged(object sender, EventArgs e)
        {
            if (checkColorsPreview.Checked)
            {
                colorInfo.Preview();
            }
            else
            {
                colorInfo.Reset();
            }
        }

        private void currentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadTheme(UiColors.Theme.Settings);
        }

        private void InitColorsRows()
        {
            var rows = colorInfo.Rows = new DataGridViewRow[colorInfo.Values.Count];

            for (var i = 0; i < rows.Length; i++)
            {
                var c = (ColorNames)i;
                var r = rows[i] = (DataGridViewRow)gridColors.RowTemplate.Clone();

                r.CreateCells(gridColors);

                r.Cells[0].Value = colorInfo.Values[c];
                r.Cells[1].Value = c.ToString();
                r.Tag = c;
            }

            gridColors.SuspendLayout();

            gridColors.Rows.AddRange(rows);
            gridColors.Sort(columnName, ListSortDirection.Ascending);

            gridColors.ResumeLayout();
        }

        private void comboColors_SelectedIndexChanged(object sender, EventArgs e)
        {
            var i = comboColors.SelectedIndex;

            panelColorsContent.SuspendLayout();

            if (i == 0)
            {
                buttonSample.Colors = colorInfo.Values;
            }

            if (i == 2 && colorInfo.Rows == null)
            {
                InitColorsRows();
            }

            panelColorsAccount.Visible = i == 0;
            panelColorsUi.Visible = i == 1;
            gridColors.Visible = i == 2;
            textColorsFilter.Visible = i == 2;

            panelColorsContent.ResumeLayout();
        }

        private void gridColors_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                var r = gridColors.Rows[e.RowIndex];
                var c = (ColorNames)r.Tag;

                UiColorsPreviewPanel p;
                colorInfo.Panels.TryGetValue(c, out p);

                ShowColorDialog(c, (Color)r.Cells[0].Value, p);
            }
        }

        private void gridColors_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                e.Handled = true;
                e.PaintBackground(e.ClipBounds, true);
                var g = e.Graphics;

                var r = Rectangle.Inflate(e.CellBounds, -e.CellStyle.Padding.Left, -e.CellStyle.Padding.Top);

                using (var brush = new SolidBrush((Color)e.Value))
                {
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(Pens.Black, r);
                }
            }
        }

        private async void DoColorsFilter()
        {
            if (textColorsFilter.Tag != null)
                return;
            textColorsFilter.Tag = true;

            await Task.Delay(500);

            textColorsFilter.Tag = null;
            var t = textColorsFilter.Text;

            gridColors.SuspendLayout();

            foreach (DataGridViewRow row in gridColors.Rows)
            {
                row.Visible = ((string)row.Cells[1].Value).IndexOf(t, StringComparison.OrdinalIgnoreCase) != -1;
            }

            gridColors.ResumeLayout();
        }

        private void textColorsFilter_TextChanged(object sender, EventArgs e)
        {
            DoColorsFilter();
        }

        private void adProcessAffinity_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                contextAffinity.Tag = sender;
                contextAffinity.Show(Cursor.Position);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ad = (AffinityDisplay)contextAffinity.Tag;

            using (var f = new UI.Affinity.formAffinitySelectDialog(ad.Affinity))
            {
                f.ShowDialog(this);
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ad = (AffinityDisplay)contextAffinity.Tag;

            using (var f = new UI.Affinity.formAffinitySelectDialog(Affinity.formAffinitySelectDialog.DialogMode.Select))
            {
                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    ad.Affinity = f.SelectedAffinity.Affinity;
                }
            }
        }
    }
}
