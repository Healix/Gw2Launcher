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
using ColorNames = Gw2Launcher.Settings.AccountGridButtonColors.Colors;

namespace Gw2Launcher.UI
{
    public partial class formSettings : Base.BaseForm
    {
        private class AccountGridButtonColorPreviewPanel : ColorPicker.Controls.ColorPreviewPanel
        {
            public Settings.AccountGridButtonColors.Colors ColorName
            {
                get;
                set;
            }

            public string Tooltip
            {
                get;
                set;
            }

            public bool SupportsAlpha
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
        private CheckBox[] checkProcessorAffinityGw2, checkProcessorAffinityGw1;
        private ToolTip tooltipColors;
        private CancellationTokenSource cancelPressed;
        private Point locationColorDialog;
        private Dictionary<string, object> iconsRunAfter;
        private Windows.ShellIcons shellIcons;

        public formSettings()
        {
            InitializeComponents();

            locationColorDialog = new Point(int.MinValue, int.MinValue);
            iconsRunAfter = new Dictionary<string, object>();

            Util.CheckedButton.Group(radioNetworkVerifyAutomatic, radioNetworkVerifyManual);
            Util.CheckedButton.Group(checkRemoveAllNetworks, checkRemovePreviousNetworks);
            Util.CheckedButton.Group(radioGw2ModeAdvanced, radioGw2ModeBasic);
            Util.CheckedButton.Group(radioLocalizedExecutionFull, radioLocalizedExecutionBinaries);
            Util.CheckedButton.Group(radioLocalizedExecutionAutoSyncBasic, radioLocalizedExecutionAutoSyncAll);
            Util.CheckedButton.Group(checkJumpListOnlyShowDaily, checkJumpListHideActive);
            Util.CheckedButton.Group(checkPreventRelaunchingExit, checkPreventRelaunchingRelaunch);

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

            buttonGeneral.Panels = new Panel[] { panelGeneral, panelLaunchConfiguration, panelWindows };
            buttonGeneral.SubItems = new string[] { "Launching", "Windows" };

            buttonGuildWars2.Panels = new Panel[] { panelGw2, panelLaunchOptionsGw2, panelLaunchConfigurationGw2,
                                                    panelLaunchOptionsAdvancedGw2 };
            buttonGuildWars2.SubItems = new string[] { "Launch options", "Management" };

            buttonGuildWars1.Panels = new Panel[] { panelGw1, panelLaunchOptionsGw1,
                                                    panelLaunchOptionsAdvancedGw1 };
            buttonGuildWars1.SubItems = new string[] { "Launch options" };

            buttonSecurity.Panels = new Panel[] { panelSecurity, panelPasswords };
            buttonSecurity.SubItems = new string[] { "Windows" };

            buttonStyle.Panels = new Panel[] { panelStyle, panelActions };
            buttonStyle.SubItems = new string[] { "Actions" };

            buttonTools.Panels = new Panel[] { panelTools, panelAccountBar, panelLocalDat, panelScreenshots };
            buttonTools.SubItems = new string[] { "Account bar", "Local.dat", "Screenshots" };

            buttonUpdates.Panels = new Panel[] { panelUpdates };

            sidebarPanel1.Initialize(new SidebarButton[]
                {
                    buttonGeneral,
                    buttonGuildWars2,
                    buttonGuildWars1,
                    buttonSecurity,
                    buttonStyle,
                    buttonTools,
                    buttonUpdates
                });

            buttonGeneral.Selected = true;

            InitializeActions();

            if (Settings.GuildWars2.Path.HasValue)
            {
                textGw2Path.Text = Settings.GuildWars2.Path.Value;
                textGw2Path.Select(textGw2Path.TextLength, 0);
            }

            if (Settings.GuildWars1.Path.HasValue)
            {
                textGw1Path.Text = Settings.GuildWars1.Path.Value;
                textGw1Path.Select(textGw1Path.TextLength, 0);
            }

            if (Settings.GuildWars2.Arguments.HasValue)
                textGw2Arguments.Text = Settings.GuildWars2.Arguments.Value;

            if (Settings.GuildWars1.Arguments.HasValue)
                textGw1Arguments.Text = Settings.GuildWars1.Arguments.Value;

            checkMinimizeToTray.Checked = Settings.MinimizeToTray.Value;
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
            checkTopMost.Checked = Settings.TopMost.Value;

            if (Client.FileManager.IsFolderLinkingSupported)
            {
                if (Settings.GuildWars2.VirtualUserPath.HasValue)
                {
                    checkCustomUsername.Checked = true;
                    textCustomUsername.Text = Settings.GuildWars2.VirtualUserPath.Value;
                    textCustomUsername.Select(textCustomUsername.TextLength, 0);
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
                    textGw2ScreenshotsLocation.Select(textGw2ScreenshotsLocation.TextLength, 0);
                }
                if (checkGw1ScreenshotsLocation.Checked = Settings.GuildWars1.ScreenshotsLocation.HasValue)
                {
                    textGw1ScreenshotsLocation.Text = Settings.GuildWars1.ScreenshotsLocation.Value;
                    textGw1ScreenshotsLocation.Select(textGw1ScreenshotsLocation.TextLength, 0);
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

            if (Settings.NetworkAuthorization.HasValue)
            {
                checkEnableNetworkAuthorization.Checked = true;
                var v = Settings.NetworkAuthorization.Value;
                switch (v & Settings.NetworkAuthorizationFlags.VerificationModes)
                {
                    case Settings.NetworkAuthorizationFlags.Manual:
                        radioNetworkVerifyManual.Checked = true;
                        break;
                    case Settings.NetworkAuthorizationFlags.Automatic:
                    default:
                        radioNetworkVerifyAutomatic.Checked = true;
                        break;
                }
                checkRemovePreviousNetworks.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized);
                checkRemoveAllNetworks.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.RemoveAll);
                checkNetworkAbortOnCancel.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail);
                checkNetworkVerifyAutomaticIP.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.VerifyIP);
            }

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
            comboGw1ProcessPriority.Items.AddRange(priorityValues);
            comboProcessPriority.Items.AddRange(priorityValues);

            if (Settings.GuildWars2.ProcessPriority.HasValue && Settings.GuildWars2.ProcessPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriority, Settings.GuildWars2.ProcessPriority.Value);
                checkGw2ProcessPriority.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboGw2ProcessPriority, Settings.ProcessPriorityClass.Normal);

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

            panelGw2ProcessAffinity.SuspendLayout();
            panelGw1ProcessAffinity.SuspendLayout();
            checkProcessorAffinityGw2 = InitializeProcessorAffinity(panelGw2ProcessAffinity, checkGw2ProcessAffinityAll, label29);
            checkProcessorAffinityGw1 = InitializeProcessorAffinity(panelGw1ProcessAffinity, checkGw1ProcessAffinityAll, label29);
            panelGw2ProcessAffinity.AutoSize = true;
            panelGw1ProcessAffinity.AutoSize = true;
            panelGw2ProcessAffinity.ResumeLayout();
            panelGw1ProcessAffinity.ResumeLayout();

            SetProcessorAffinity(Settings.GuildWars2.ProcessAffinity, checkGw2ProcessAffinityAll, checkProcessorAffinityGw2);
            SetProcessorAffinity(Settings.GuildWars1.ProcessAffinity, checkGw1ProcessAffinityAll, checkProcessorAffinityGw1);
            
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

            if (Settings.GuildWars2.MumbleLinkName.HasValue)
            {
                checkGw2MumbleName.Checked = true;
                textGw2MumbleName.Text = Settings.GuildWars2.MumbleLinkName.Value;
            }

            if (Client.FileManager.IsFolderLinkingSupported)
            {
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
            }
            else
            {
                radioGw2ModeBasic.Enabled = false;
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

            Settings.AccountGridButtonColors colors;

            if (Settings.StyleColors.HasValue)
            {
                colors = Settings.StyleColors.Value;
                labelColorsReset.Visible = true;
            }
            else
            {
                colors = AccountGridButton.DefaultColors;
            }

            tooltipColors = new ToolTip();
            tooltipColors.InitialDelay = 100;

            var colors2 = new Settings.AccountGridButtonColors();

            foreach (var panel in GetColorPanels())
            {
                panel.Color1 = colors[panel.ColorName];
                if (panel.Tooltip != null)
                    tooltipColors.SetToolTip(panel, panel.Tooltip);
                panel.Click += panelColor_Click;
                colors2[panel.ColorName] = colors[panel.ColorName];
            }

            buttonSample.Colors = colors2;
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

            argsStateGw2 = ArgsState.Changed;
            argsStateGw1 = ArgsState.Changed;
            panelLaunchOptionsAdvancedGw2.PreVisiblePropertyChanged += panelLaunchOptionsAdvancedGw2_PreVisiblePropertyChanged;
            panelLaunchOptionsAdvancedGw1.PreVisiblePropertyChanged += panelLaunchOptionsAdvancedGw1_PreVisiblePropertyChanged;
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

        private AccountGridButtonColorPreviewPanel[] GetColorPanels()
        {
            return new AccountGridButtonColorPreviewPanel[]
            {
                panelColorName,
                panelColorUser,
                panelColorStatusError,
                panelColorStatusNone,
                panelColorStatusOk,
                panelColorStatusWaiting,
                panelColorBackgroundDefault,
                panelColorBackgroundHovered,
                panelColorBackgroundSelected,
                panelColorForegroundHovered,
                panelColorForegroundSelected,
                panelColorBorderDarkDefault,
                panelColorBorderDarkHovered,
                panelColorBorderDarkSelected,
                panelColorBorderLightDefault,
                panelColorBorderLightHovered,
                panelColorBorderLightSelected,
                panelColorExitFill,
                panelColorExitFlash,
                panelColorFocus,
                panelColorHighlight,
                panelColorHighlightBorder,
            };
        }

        private void SetProcessorAffinity(Settings.ISettingValue<long> s, CheckBox checkAll, CheckBox[] checks)
        {
            if (s.HasValue)
            {
                var bits = s.Value;
                var isSet = false;
                var count = checks.Length;
                if (count > 64)
                    count = 64;

                for (var i = 0; i < count; i++)
                {
                    if (checks[i].Checked = (bits & 1) == 1)
                        isSet = true;
                    bits >>= 1;
                }
                checkAll.Checked = !isSet;
            }
            else
            {
                checkGw2ProcessAffinityAll.Checked = true;

            }
        }

        private long GetProcessorAffinity(CheckBox[] checks)
        {
            long bits = 0;
            var count = checks.Length;
            if (count > 64)
                count = 64;
            for (int i = 0; i < count; i++)
            {
                if (checks[i].Checked)
                    bits |= ((long)1 << i);
            }
            return bits;
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

        public static CheckBox[] InitializeProcessorAffinity(Panel container, CheckBox templateCheck, Label templateLabel)
        {
            var count = Environment.ProcessorCount;
            var controls = new CheckBox[count];
            var format = new string('0', (count - 1).ToString().Length);

            for (var i = 0; i < count; i++)
            {
                CheckBox check;
                controls[i] = check = new CheckBox()
                {
                    Text = i.ToString(format),
                    Font = templateCheck.Font,
                    AutoSize = true,
                    Enabled = i < 64,
                    Margin = new Padding(0, 0, 5, 5),
                };
            }

            container.Controls.AddRange(controls);

            return controls;
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

            if (!string.IsNullOrEmpty(textGw1Path.Text))
                Settings.GuildWars1.Path.Value = textGw1Path.Text;
            else
                Settings.GuildWars1.Path.Clear();

            Settings.GuildWars2.Arguments.Value = textGw2Arguments.Text;
            Settings.GuildWars1.Arguments.Value = textGw1Arguments.Text;

            Settings.MinimizeToTray.Value = checkMinimizeToTray.Checked;
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

            if (checkEnableNetworkAuthorization.Checked)
            {
                Settings.NetworkAuthorizationFlags v;

                if (radioNetworkVerifyManual.Checked)
                {
                    v = Settings.NetworkAuthorizationFlags.Manual;
                }
                else
                {
                    v = Settings.NetworkAuthorizationFlags.Automatic;
                    if (checkNetworkVerifyAutomaticIP.Checked)
                        v |= Settings.NetworkAuthorizationFlags.VerifyIP;
                }

                if (checkRemoveAllNetworks.Checked)
                    v |= Settings.NetworkAuthorizationFlags.RemoveAll;
                else if (checkRemovePreviousNetworks.Checked)
                    v |= Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized;

                if (checkNetworkAbortOnCancel.Checked)
                    v |= Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail;

                if (!v.HasFlag(Settings.NetworkAuthorizationFlags.VerifyIP) || (v & (Settings.NetworkAuthorizationFlags.Always | Settings.NetworkAuthorizationFlags.RemoveAll)) != 0)
                    Settings.PublicIPAddress.Clear();

                Settings.NetworkAuthorization.Value = v;
            }
            else
            {
                Settings.NetworkAuthorization.Clear();
                Settings.PublicIPAddress.Clear();
            }

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
                var bits = GetProcessorAffinity(checkProcessorAffinityGw2);
                if (bits == 0)
                    Settings.GuildWars2.ProcessAffinity.Clear();
                else
                    Settings.GuildWars2.ProcessAffinity.Value = bits;
            }
            else
                Settings.GuildWars2.ProcessAffinity.Clear();

            if (!checkGw1ProcessAffinityAll.Checked)
            {
                var bits = GetProcessorAffinity(checkProcessorAffinityGw1);
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

            if (!AccountGridButton.DefaultColors.Equals(buttonSample.Colors))
            {
                var colors = new Settings.AccountGridButtonColors();
                foreach (var panel in GetColorPanels())
                {
                    colors[panel.ColorName] = panel.Color1;
                }
                Settings.StyleColors.Value = colors;
            }
            else
            {
                Settings.StyleColors.Clear();
            }

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

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void checkShowTrayIcon_CheckedChanged(object sender, EventArgs e)
        {
            checkMinimizeToTray.Enabled = checkShowTrayIcon.Checked;
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
                Util.ScheduledEvents.Unregister(OnScreenshotNameFormatChangedCallback);

                using (buttonSample.BackgroundImage) { }
                using (buttonSample.Image) { }

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
            tableGw2ProcessAffinity.Visible = !checkGw2ProcessAffinityAll.Checked;
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
            tableGw1ProcessAffinity.Visible = !checkGw1ProcessAffinityAll.Checked;
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

        private void panelColor_Click(object sender, EventArgs e)
        {
            var panel = (AccountGridButtonColorPreviewPanel)sender;

            using (var f = new ColorPicker.formColorDialog())
            {
                var c = panel.Color1;

                if (locationColorDialog.X != int.MinValue)
                {
                    f.StartPosition = FormStartPosition.Manual;
                    f.Location = locationColorDialog;
                }

                f.AllowAlphaTransparency = panel.SupportsAlpha;
                f.Color = c;
                f.DefaultColor = AccountGridButton.DefaultColors[panel.ColorName];

                if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    panel.Color1 = buttonSample.Colors[panel.ColorName] = f.Color;

                    if (f.DefaultColor.ToArgb() != f.Color.ToArgb())
                    {
                        labelColorsReset.Visible = true;
                    }

                    buttonSample.Redraw();
                }

                locationColorDialog = f.Location;
            }
        }

        private void ShowStatusColorSample(Settings.AccountGridButtonColors.Colors c)
        {
            switch (c)
            {
                case Settings.AccountGridButtonColors.Colors.StatusDefault:

                    buttonSample.SetStatus("a minute ago (sample)", AccountGridButton.StatusColors.Default);

                    break;
                case Settings.AccountGridButtonColors.Colors.StatusError:

                    buttonSample.SetStatus("failed (sample)", AccountGridButton.StatusColors.Error);

                    break;
                case Settings.AccountGridButtonColors.Colors.StatusOK:

                    buttonSample.SetStatus("active (sample)", AccountGridButton.StatusColors.Ok);

                    break;
                case Settings.AccountGridButtonColors.Colors.StatusWaiting:

                    buttonSample.SetStatus("waiting (sample)", AccountGridButton.StatusColors.Waiting);

                    break;
                default:

                    buttonSample.SetStatus("sample", AccountGridButton.StatusColors.Default);

                    break;
            }
        }

        private void panelColor_MouseEnter(object sender, EventArgs e)
        {
            ShowStatusColorSample(((AccountGridButtonColorPreviewPanel)sender).ColorName);
        }

        private void panelColor_MouseLeave(object sender, EventArgs e)
        {
            ShowStatusColorSample(Settings.AccountGridButtonColors.Colors.Name);
        }

        private void sliderGw1Volume_ValueChanged(object sender, EventArgs e)
        {
            var v = ((UI.Controls.FlatSlider)sender).Value;
            labelGw1Volume.Text = (int)(v * 100 + 0.5f) + "%";
        }

        private void panelColorHighlight_MouseEnter(object sender, EventArgs e)
        {
            buttonSample.IsFocused = true;
        }

        private void panelColorHighlight_MouseLeave(object sender, EventArgs e)
        {
            buttonSample.IsFocused = checkStyleMarkFocused.Checked;
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
            ShowPressedSample(AccountGridButton.PressedState.Pressed, panelColorFocus.Color1, Color.Empty);
        }

        private void panelColorExitFill_MouseEnter(object sender, EventArgs e)
        {
            ShowPressedSample(AccountGridButton.PressedState.Pressing, panelColorExitFlash.Color1, panelColorExitFill.Color1);
        }

        private void panelColorExitFlash_MouseEnter(object sender, EventArgs e)
        {
            ShowPressedSample(AccountGridButton.PressedState.Pressed, panelColorExitFlash.Color1, Color.Empty);
        }

        private void panelColorBackground_MouseEnter(object sender, EventArgs e)
        {
            bool hovered = false, selected = false;

            switch (((AccountGridButtonColorPreviewPanel)sender).ColorName)
            {
                case ColorNames.BackColorHovered:
                case ColorNames.BorderDarkHovered:
                case ColorNames.BorderLightHovered:
                case ColorNames.ForeColorHovered:
                    hovered = true;
                    break;
                case ColorNames.BackColorSelected:
                case ColorNames.BorderDarkSelected:
                case ColorNames.BorderLightSelected:
                case ColorNames.ForeColorSelected:
                    selected = true;
                    break;
            }

            buttonSample.IsHovered = hovered;
            buttonSample.Selected = selected;
            buttonSample.IsFocused = false;
        }
        
        private void panelColorBackground_MouseLeave(object sender, EventArgs e)
        {
            buttonSample.IsHovered = false;
            buttonSample.Selected = false;
            buttonSample.IsFocused = checkStyleMarkFocused.Checked;
        }

        private void labelColorsReset_Click(object sender, EventArgs e)
        {
            var colors = buttonSample.Colors;

            foreach (var p in GetColorPanels())
            {
                p.Color1 = colors[p.ColorName] = AccountGridButton.DefaultColors[p.ColorName];
            }

            buttonSample.Redraw();
            labelColorsReset.Visible = false;
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

                if (shellIcons == null)
                    shellIcons = new Windows.ShellIcons();
                sz = shellIcons.GetSize(Windows.ShellIcons.IconSize.Small);

                l.Padding = new Padding(sz.Width + Scale(3), 0, 0, 0);
                l.MinimumSize = new Size(0, sz.Height + 2);

                if (!iconsRunAfter.TryGetValue(path, out o))
                {
                    var t = Task.Run<Icon>(
                        delegate
                        {
                            try
                            {
                                return shellIcons.GetIcon(path, Windows.ShellIcons.IconSize.Small);
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
                    panelGw2RunAfterProgramsAddSeperator.Visible = true;
                else if (container == panelGw1RunAfterPrograms)
                    panelGw1RunAfterProgramsAddSeperator.Visible = true;
                container.Visible = true;
            }

            UpdateRunAfterButton(l, r);

            return l;
        }

        private void UpdateRunAfterButton(UI.Controls.LinkLabel l, Settings.RunAfter r)
        {
            l.Text = r.GetName();
            l.Tag = r;

            if ((r.Options & Settings.RunAfter.RunAfterOptions.Enabled) == 0)
                l.ForeColor = SystemColors.GrayText;
            else
                l.ResetForeColor();

            LoadIconAsync(l, r);
        }

        private void labelGw2RunAfterProgramsAdd_Click(object sender, EventArgs e)
        {
            using (var f = new formRunAfter())
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
            using (var f = new formRunAfter())
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

            using (var f = new formRunAfter(r))
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
                    panelGw2RunAfterProgramsAddSeperator.Visible = false;
                else if (panel == panelGw1RunAfterPrograms)
                    panelGw1RunAfterProgramsAddSeperator.Visible = false;
                panel.Visible = false;
                panel.Parent.ResumeLayout();
            }
            label.Dispose();
        }
    }
}
