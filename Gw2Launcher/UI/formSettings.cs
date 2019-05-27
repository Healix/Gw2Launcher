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
    public partial class formSettings : Form
    {
        private enum ArgsState : byte
        {
            Changed,
            Loading,
            Loaded,
            Active
        }

        private Form activeWindow;
        private CheckBox[] checkArgs;
        private ArgsState argsState;
        private CheckBox[] checkProcessorAffinity;
        private AutoScrollContainerPanel containerGeneral, containerTools, containerWindows, containerLaunchOptions, containerLaunchOptionsAdvanced,
            containerPasswords, containerStyle, containerActions, containerUpdates, containerSecurity, containerScreenshots,containerLaunchOptionsProcess,containerAccountBar,
            containerLaunchConfiguration, containerLocalDat;

        public formSettings()
        {
            InitializeComponent();

            this.MaximumSize = new Size(this.MinimumSize.Width, int.MaxValue);

            var sbounds = Settings.WindowBounds[typeof(formSettings)];
            if (sbounds.HasValue)
                this.Size = sbounds.Value.Size;

            checkArgs = InitializeArguments(panelLaunchOptionsAdvancedContainer, labelArgsTemplateHeader, checkArgsTemplate, labelArgsTemplateSwitch, labelArgsTemplateDesc, checkArgs_CheckedChanged);

            containerGeneral = CreateContainer(panelGeneral);
            containerTools = CreateContainer(panelTools);
            containerWindows = CreateContainer(panelWindows);
            containerLaunchOptions = CreateContainer(panelLaunchOptions);
            containerLaunchOptionsAdvanced = CreateContainer(panelLaunchOptionsAdvanced);
            containerPasswords = CreateContainer(panelPasswords);
            containerStyle = CreateContainer(panelStyle);
            containerActions = CreateContainer(panelActions);
            containerUpdates = CreateContainer(panelUpdates);
            containerSecurity = CreateContainer(panelSecurity);
            containerScreenshots = CreateContainer(panelScreenshots);
            containerAccountBar = CreateContainer(panelAccountBar);
            containerLaunchOptionsProcess = CreateContainer(panelLaunchOptionsProcess);
            containerLaunchConfiguration = CreateContainer(panelLaunchConfiguration);
            containerLocalDat = CreateContainer(panelLocalDat);

            buttonGeneral.Panels = new Panel[] { containerGeneral, containerLaunchConfiguration, containerWindows };
            buttonGeneral.SubItems = new string[] { "Launching", "Windows" };

            buttonLaunchOptions.Panels = new Panel[] { containerLaunchOptions, containerLaunchOptionsAdvanced, containerLaunchOptionsProcess };
            buttonLaunchOptions.SubItems = new string[] { "Advanced", "Processor" };

            buttonSecurity.Panels = new Panel[] { containerSecurity, containerPasswords };
            buttonSecurity.SubItems = new string[] { "Windows" };

            buttonStyle.Panels = new Panel[] { containerStyle, containerActions };
            buttonStyle.SubItems = new string[] { "Actions" };

            buttonTools.Panels = new Panel[] { containerTools, containerAccountBar, containerLocalDat, containerScreenshots };
            buttonTools.SubItems = new string[] { "Account bar", "Local.dat", "Screenshots" };

            buttonUpdates.Panels = new Panel[] { containerUpdates };

            sidebarPanel1.Initialize(new SidebarButton[]
                {
                    buttonGeneral,
                    buttonLaunchOptions,
                    buttonSecurity,
                    buttonStyle,
                    buttonTools,
                    buttonUpdates
                });

            buttonGeneral.Selected = true;

            InitializeActions();

            var path = Settings.GW2Path;
            if (path.HasValue)
            {
                textGW2Path.Text = path.Value;
                textGW2Path.Select(textGW2Path.TextLength, 0);
            }
            else
                textGW2Path.Text = "";

            var args = Settings.GW2Arguments;
            if (args.HasValue)
                textArguments.Text = args.Value;
            else
                textArguments.Text = "";

            checkMinimizeToTray.Checked = Settings.MinimizeToTray.Value;
            checkShowTrayIcon.Checked = !Settings.ShowTray.HasValue || Settings.ShowTray.Value;
            checkBringToFrontOnExit.Checked = Settings.BringToFrontOnExit.Value;
            checkStoreCredentials.Checked = Settings.StoreCredentials.Value;
            checkShowUser.Checked = !Settings.StyleShowAccount.HasValue || Settings.StyleShowAccount.Value;
            checkShowColor.Checked = Settings.StyleShowColor.Value;
            checkStyleMarkFocused.Checked = Settings.StyleHighlightFocused.Value;

            //buttonSample.ShowAccount = checkShowUser.Checked;
            //buttonSample.ShowColorKey = checkShowColor.Checked;
            if (Settings.FontLarge.HasValue)
                buttonSample.FontLarge = Settings.FontLarge.Value;
            if (Settings.FontSmall.HasValue)
                buttonSample.FontSmall = Settings.FontSmall.Value;

            labelFontRestoreDefaults.Visible = Settings.FontLarge.HasValue || Settings.FontSmall.HasValue;

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
            checkVolume.Checked = Settings.Volume.HasValue;
            if (Settings.Volume.HasValue)
                sliderVolume.Value = Settings.Volume.Value;
            else
                sliderVolume.Value = 1f;
            if (Settings.RunAfterLaunching.HasValue)
                textRunAfterLaunch.Text = Settings.RunAfterLaunching.Value;
            else
                textRunAfterLaunch.Text = "";

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

            if (Client.FileManager.IsDataLinkingSupported)
            {
                if (Settings.VirtualUserPath.HasValue)
                {
                    checkCustomUsername.Checked = true;
                    textCustomUsername.Text = Settings.VirtualUserPath.Value;
                    textCustomUsername.Select(textCustomUsername.TextLength, 0);
                }
                else
                {
                    var n = Settings.VirtualUserPath.ValueCommit;
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

            checkAutomaticLauncherLogin.Checked = Settings.AutomaticRememberedLogin.Value;
            if (Settings.Mute.HasValue)
            {
                var v = Settings.Mute.Value;
                checkMuteAll.Checked = v.HasFlag(Settings.MuteOptions.All);
                checkMuteMusic.Checked = v.HasFlag(Settings.MuteOptions.Music);
                checkMuteVoices.Checked = v.HasFlag(Settings.MuteOptions.Voices);
            }

            checkPort80.Checked = Settings.ClientPort.Value == 80;
            checkPort443.Checked = Settings.ClientPort.Value == 443;
            checkScreenshotsBmp.Checked = Settings.ScreenshotsFormat.Value == Settings.ScreenshotFormat.Bitmap;

            if (Client.FileManager.IsDataLinkingSupported)
            {
                if (checkScreenshotsLocation.Checked = Settings.ScreenshotsLocation.HasValue)
                {
                    textScreenshotsLocation.Text = Settings.ScreenshotsLocation.Value;
                    textScreenshotsLocation.Select(textScreenshotsLocation.TextLength, 0);
                }
            }
            else
            {
                checkScreenshotsLocation.Enabled = false;
                buttonScreenshotsLocation.Enabled = false;
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
                checkNetworkAbortOnCancel.Checked = v.HasFlag(Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail);
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

            comboProcessPriority.Items.AddRange(new object[]
                {
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.High, "High"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.AboveNormal, "Above normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Normal, "Normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.BelowNormal, "Below normal"),
                    new Util.ComboItem<Settings.ProcessPriorityClass>(Settings.ProcessPriorityClass.Low, "Low"),
                });

            if (Settings.ProcessPriority.HasValue && Settings.ProcessPriority.Value != Settings.ProcessPriorityClass.None)
            {
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriority.Value);
                checkProcessPriority.Checked = true;
            }
            else
                Util.ComboItem<Settings.ProcessPriorityClass>.Select(comboProcessPriority, Settings.ProcessPriorityClass.Normal);

            checkProcessorAffinity = InitializeProcessorAffinity(panelProcessAffinity, checkProcessAffinityAll, label76);

            if (Settings.ProcessAffinity.HasValue)
            {
                var bits = Settings.ProcessAffinity.Value;
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
            else
                checkProcessAffinityAll.Checked = true;

            checkProcessBoostBrowser.Checked = Settings.PrioritizeCoherentUI.Value;

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

            checkLocalizedExecution.Checked = Settings.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.Enabled);
            checkLocalizedExecutionExcludeUnknown.Checked = Settings.LocalizeAccountExecution.Value.HasFlag(Settings.LocalizeAccountExecutionOptions.ExcludeUnkownFiles);

            checkUseGw2ShortcutIcon.Checked = Settings.UseGw2IconForShortcuts.Value;

            checkLocalDatDirectUpdates.Enabled = checkLocalDatCache.Enabled = BitConverter.IsLittleEndian;
            checkLocalDatDirectUpdates.Checked = Settings.DatUpdaterEnabled.Value;
            checkLocalDatCache.Checked = Settings.UseCustomGw2Cache.Value;

            checkPreventDefaultCoherentUI.Checked = Settings.PreventDefaultCoherentUI.Value;
            checkStyleShowCloseAll.Checked = Settings.ShowKillAllAccounts.Value;

            argsState = ArgsState.Changed;
            containerLaunchOptionsAdvanced.PreVisiblePropertyChanged += containerLaunchOptionsAdvanced_PreVisiblePropertyChanged;

            labelAccountBarInfo_SizeChanged(null, null);
        }

        private void InitializeActions()
        {
            var none = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.None, "None");
            var focus = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Focus, "Focus");
            var close = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Close, "Terminate");
            var launch = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.Launch, "Launch");
            var launchNormal = new Util.ComboItem<Settings.ButtonAction>(Settings.ButtonAction.LaunchSingle, "Launch (normal)");
            
            comboActionActiveLClick.Items.AddRange(new object[]
                {
                    none,
                    focus,
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
        }

        private AutoScrollContainerPanel CreateContainer(Panel panel)
        {
            var container = new AutoScrollContainerPanel(panel);

            this.Controls.Add(container);

            return container;
        }

        public static CheckBox[] InitializeProcessorAffinity(Panel container, CheckBox templateCheck, Label templateLabel)
        {
            var count = Environment.ProcessorCount;
            var controls = new CheckBox[count];
            int x = 0,
                y = 0,
                columnW = container.Width / 6,
                columnH = templateCheck.Height;
            var format = new string('0', (count - 1).ToString().Length);
            container.Controls.Add(new Label()
                {
                    Font = templateLabel.Font,
                    AutoSize = true,
                    Location = new Point(0, 0),
                    Text = "Select which processors to use"
                });

            y += templateLabel.Height + 8;
            x = 7;

            for (var i = 0; i < count; i++)
            {
                if (i % 6 == 0 && i > 0)
                {
                    x = 7;
                    y += columnH + 6;
                }

                CheckBox check;
                controls[i] = check = new CheckBox()
                {
                    Text = i.ToString(format),
                    Font = templateCheck.Font,
                    Location = new Point(x, y),
                    AutoSize = true,
                    Enabled = i < 64,
                };

                x += columnW;
            }

            container.Height = y + columnH;

            container.Controls.AddRange(controls);

            return controls;
        }

        public static CheckBox[] InitializeArguments(Panel container, Label templateHeader, CheckBox templateCheck, Label templateSwitch, Label templateDescription, EventHandler onCheckChanged)
        {
            var categories =  new string[]
            {
                "Graphics",
                "User Interface",
                "Compatibility",
                "General"
            };

            var args = new string[][][]
            {
                new string[][] //graphics
                {
                    new string[] { "-dx9single", "Single renderer mode", "" },
                    new string[] { "-forwardrenderer", "Use forward rendering", "Primarily affects how lighting is applied and limits the maximum number of light sources" },
                    new string[] { "-useoldfov", "Revert to the original field-of-view", ""},
                    new string[] { "-umbra gpu", "Enable Umbra's GPU accelerated culling", "" },
                },
                new string[][] //ui
                {
                    new string[] { "-noui", "Launch with the UI hidden", "" },
                    new string[] { "-uispanallmonitors", "Spread the UI across all monitors", "For use with widescreen multi-panel displays"},
                },
                new string[][] //compatibility
                {
                    new string[] { "-32", "Disable switching to 64-bit", "Used with the 32-bit client to prevent it from automatically switching to the 64-bit client on a 64-bit OS" },
                    new string[] { "-mce", "Enable Windows Media Center compatibility", "" },
                    new string[] { "-nodelta", "Disable delta patching", "Updated files will be downloaded in full rather than only downloading the part that has been changed" },
                },
                new string[][] //other
                {
                    new string[] { "-maploadinfo", "Show extra details while loading maps", "Shows additional information such as the progress, elapsed time and server IP"},
                    new string[] { "-prefreset", "Restore default settings", "Resets all in-game options to their default settings"}
                },
            };

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

            var controls = new Control[count];
            var _checks = new CheckBox[checks];
            int i = 0, c = 0, _c = 0;
            var headerY = templateHeader.Top;
            var maxSize = new Size(container.Width - templateDescription.Left * 2, int.MaxValue);
            var headerYspacing = templateCheck.Top - templateHeader.Bottom;
            var switchXspacing = templateSwitch.Left - templateCheck.Right;
            var switchYspacing = templateSwitch.Top - templateCheck.Top;
            var descYspacing = templateDescription.Top - templateCheck.Bottom;

            foreach (var items in args)
            {
                controls[i++] = new Label()
                {
                    Text = categories[c],
                    Font = templateHeader.Font,
                    Location = new Point(templateHeader.Left, headerY),
                    AutoSize = true,
                };

                var itemY = headerY + templateHeader.Height + headerYspacing;

                foreach (var item in items)
                {
                    CheckBox check;
                    controls[i++] = check = new CheckBox()
                    {
                        Text = item[1],
                        Font = templateCheck.Font,
                        Location = new Point(templateCheck.Left, itemY),
                        AutoSize = true,
                        Tag = item[0]
                    };

                    check.CheckedChanged += onCheckChanged;

                    _checks[_c++] = check;

                    var size = check.GetPreferredSize(Size.Empty);

                    controls[i++] = new Label()
                    {
                        Text = item[0],
                        Font = templateSwitch.Font,
                        ForeColor = templateSwitch.ForeColor,
                        Location = new Point(check.Left + size.Width + switchXspacing, check.Top + switchYspacing),
                        AutoSize = true,
                    };

                    if (!string.IsNullOrEmpty(item[2]))
                    {
                        var label = controls[i++] = new Label()
                        {
                            Text = item[2],
                            Font = templateDescription.Font,
                            Location = new Point(templateDescription.Left, check.Top + size.Height + descYspacing),
                            AutoSize = true,
                            MaximumSize = maxSize,
                        };

                        size = label.GetPreferredSize(Size.Empty);
                        itemY = label.Top + size.Height + 10;
                    }
                    else
                    {
                        itemY = check.Top + size.Height + 10;
                    }
                }

                headerY = itemY + 5;

                c++;
            }

            container.Controls.Clear();
            container.Controls.AddRange(controls);

            return _checks;
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

        private void buttonGW2Path_Click(object sender, EventArgs e)
        {
            OpenFileDialog f = new OpenFileDialog();

            f.Filter = "Guild Wars 2|Gw2*.exe|All executables|*.exe";
            if (Environment.Is64BitOperatingSystem)
                f.Title = "Open Gw2-64.exe";
            else
                f.Title = "Open Gw2.exe";

            if (textGW2Path.TextLength != 0)
            {
                f.InitialDirectory = System.IO.Path.GetDirectoryName(textGW2Path.Text);
            }

            if (f.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                var bits = Util.FileUtil.GetExecutableBits(f.FileName);
                if (Environment.Is64BitOperatingSystem && bits == 32)
                {
                    if (!Util.Args.Contains(textArguments.Text, "32"))
                    {
                        if (MessageBox.Show(this, "You've selected to use the 32-bit version of Guild Wars 2 on a 64-bit system.\n\nAre you sure?", "32-bit?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) != System.Windows.Forms.DialogResult.Yes)
                            return;
                        textArguments.Text = Util.Args.AddOrReplace(textArguments.Text, "32", "-32");
                    }
                }
                else if (bits == 64 && Util.Args.Contains(textArguments.Text, "32"))
                {
                    textArguments.Text = Util.Args.AddOrReplace(textArguments.Text, "32", "");
                }
                textGW2Path.Text = f.FileName;
                textGW2Path.Select(textGW2Path.TextLength, 0);

                try
                {
                    var path = Path.GetDirectoryName(f.FileName);
                    var programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    if (!string.IsNullOrEmpty(programfiles) && path.StartsWith(programfiles, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!Util.FileUtil.HasFolderPermissions(path, System.Security.AccessControl.FileSystemRights.Modify))
                        {
                            if (MessageBox.Show(this, "Program Files is a protected directory and will require administrator permissions to modify.\n\nAllow access to the \"" + Path.GetFileName(path) + "\" folder?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.Yes)
                                Util.ProcessUtil.CreateFolder(path);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.Logging.Log(ex);
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (activeWindow != null && !activeWindow.IsDisposed)
                activeWindow.Close();

            Settings.GW2Path.Value = textGW2Path.Text;
            Settings.GW2Arguments.Value = textArguments.Text;
            Settings.MinimizeToTray.Value = checkMinimizeToTray.Checked;
            Settings.ShowTray.Value = checkShowTrayIcon.Checked;
            Settings.BringToFrontOnExit.Value = checkBringToFrontOnExit.Checked;
            Settings.StoreCredentials.Value = checkStoreCredentials.Checked;
            Settings.CheckForNewBuilds.Value = checkCheckBuildOnLaunch.Checked;

            Settings.StyleShowAccount.Value = checkShowUser.Checked;
            Settings.StyleShowColor.Value = checkShowColor.Checked;
            Settings.StyleHighlightFocused.Value = checkStyleMarkFocused.Checked;

            if (buttonSample.FontLarge.Equals(UI.Controls.AccountGridButton.FONT_LARGE))
                Settings.FontLarge.Clear();
            else
                Settings.FontLarge.Value = buttonSample.FontLarge;

            if (buttonSample.FontSmall.Equals(UI.Controls.AccountGridButton.FONT_SMALL))
                Settings.FontSmall.Clear();
            else
                Settings.FontSmall.Value = buttonSample.FontSmall;

            if (checkVolume.Checked)
                Settings.Volume.Value = sliderVolume.Value;
            else
                Settings.Volume.Clear();

            Settings.AutoUpdateInterval.Value = (ushort)numericUpdateInterval.Value;
            Settings.AutoUpdate.Value = checkAutoUpdate.Checked;
            Settings.BackgroundPatchingEnabled.Value = checkAutoUpdateDownload.Checked;
            if (checkAutoUpdateDownloadNotifications.Checked)
                Settings.BackgroundPatchingNotifications.Value = (Settings.ScreenAttachment)checkAutoUpdateDownloadNotifications.Tag;
            else
                Settings.BackgroundPatchingNotifications.Clear();
            if (!string.IsNullOrEmpty(textRunAfterLaunch.Text))
                Settings.RunAfterLaunching.Value = textRunAfterLaunch.Text;
            else
                Settings.RunAfterLaunching.Clear();

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
                    Settings.VirtualUserPath.Clear();
                else
                    Settings.VirtualUserPath.Value = displayUsername;
            }
            else
                Settings.VirtualUserPath.Clear();

            Settings.ActionActiveLClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionActiveLClick, Settings.ButtonAction.None);
            Settings.ActionActiveLPress.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionActiveLPress, Settings.ButtonAction.None);
            Settings.ActionInactiveLClick.Value = Util.ComboItem<Settings.ButtonAction>.SelectedValue(comboActionInactiveLClick, Settings.ButtonAction.Launch);

            Settings.AutomaticRememberedLogin.Value = checkAutomaticLauncherLogin.Checked;

            if (checkMuteAll.Checked || checkMuteMusic.Checked || checkMuteVoices.Checked)
            {
                var mute = Settings.MuteOptions.None;
                if (checkMuteAll.Checked)
                    mute |= Settings.MuteOptions.All;
                if (checkMuteMusic.Checked)
                    mute |= Settings.MuteOptions.Music;
                if (checkMuteVoices.Checked)
                    mute |= Settings.MuteOptions.Voices;
                Settings.Mute.Value = mute;
            }
            else
                Settings.Mute.Clear();

            if (checkPort80.Checked)
                Settings.ClientPort.Value = 80;
            else if (checkPort443.Checked)
                Settings.ClientPort.Value = 443;
            else
                Settings.ClientPort.Clear();
            if (checkScreenshotsBmp.Checked)
                Settings.ScreenshotsFormat.Value = Settings.ScreenshotFormat.Bitmap;
            else
                Settings.ScreenshotsFormat.Clear();
            if (checkScreenshotsLocation.Checked && !string.IsNullOrEmpty(textScreenshotsLocation.Text))
            {
                var path = Util.FileUtil.GetTrimmedDirectoryPath(textScreenshotsLocation.Text);
                if (string.IsNullOrEmpty(path))
                    Settings.ScreenshotsLocation.Clear();
                else
                    Settings.ScreenshotsLocation.Value = path;
            }
            else
                Settings.ScreenshotsLocation.Clear();

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
                    v = Settings.NetworkAuthorizationFlags.Manual;
                else
                    v = Settings.NetworkAuthorizationFlags.Automatic;

                if (checkRemovePreviousNetworks.Checked)
                    v |= Settings.NetworkAuthorizationFlags.RemovePreviouslyAuthorized;

                if (checkNetworkAbortOnCancel.Checked)
                    v |= Settings.NetworkAuthorizationFlags.AbortLaunchingOnFail;

                Settings.NetworkAuthorization.Value = v;
            }
            else
                Settings.NetworkAuthorization.Clear();

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

            if (checkProcessPriority.Checked && comboProcessPriority.SelectedIndex >= 0)
                Settings.ProcessPriority.Value = Util.ComboItem<Settings.ProcessPriorityClass>.SelectedValue(comboProcessPriority);
            else
                Settings.ProcessPriority.Clear();

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

                if (bits == 0)
                    Settings.ProcessAffinity.Clear();
                else
                    Settings.ProcessAffinity.Value = bits;
            }
            else
                Settings.ProcessAffinity.Clear();

            Settings.PrioritizeCoherentUI.Value = checkProcessBoostBrowser.Checked;

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
                localizedExe |= Settings.LocalizeAccountExecutionOptions.ExcludeUnkownFiles;
            Settings.LocalizeAccountExecution.Value = localizedExe;

            Settings.UseGw2IconForShortcuts.Value = checkUseGw2ShortcutIcon.Checked;

            Settings.DatUpdaterEnabled.Value = checkLocalDatDirectUpdates.Checked;
            Settings.UseCustomGw2Cache.Value = checkLocalDatCache.Checked;

            Settings.PreventDefaultCoherentUI.Value = checkPreventDefaultCoherentUI.Checked;
            Settings.ShowKillAllAccounts.Value = checkStyleShowCloseAll.Checked;

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
            Font f = ShowFontDialog(buttonSample.FontLarge);
            if (f != null)
            {
                buttonSample.FontLarge = f;
                labelFontRestoreDefaults.Visible = true;
            }
        }

        private void buttonFontDescriptors_Click(object sender, EventArgs e)
        {
            Font f = ShowFontDialog(buttonSample.FontSmall);
            if (f != null)
            {
                buttonSample.FontSmall = f;
                labelFontRestoreDefaults.Visible = true;
            }
        }

        private void checkShowUser_CheckedChanged(object sender, EventArgs e)
        {
            buttonSample.ShowAccount = checkShowUser.Checked;
        }

        private void labelFontRestoreDefaults_Click(object sender, EventArgs e)
        {
            labelFontRestoreDefaults.Visible = false;
            buttonSample.FontLarge = UI.Controls.AccountGridButton.FONT_LARGE;
            buttonSample.FontSmall = UI.Controls.AccountGridButton.FONT_SMALL;
        }

        private void checkVolume_CheckedChanged(object sender, EventArgs e)
        {
            sliderVolume.Enabled = checkVolume.Checked;
        }

        private void label30_Click(object sender, EventArgs e)
        {
            textRunAfterLaunch.SelectedText = ((Label)sender).Text;
        }

        private void labelVersionUpdate_Click(object sender, EventArgs e)
        {
            using (formVersionUpdate f = new formVersionUpdate())
            {
                f.ShowDialog(this);
            }
        }

        private void sliderVolume_ValueChanged(object sender, float e)
        {
            labelVolume.Text = (int)(e * 100 + 0.5f) + "%";
        }

        private void checkAutoUpdateDownload_CheckedChanged(object sender, EventArgs e)
        {
            checkAutoUpdateDownloadNotifications.Enabled = checkAutoUpdateDownload.Checked;
            checkAutoUpdateDownloadProgress.Enabled = checkAutoUpdateDownload.Checked;
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
            var v = (Settings.ScreenAttachment)checkAutoUpdateDownloadNotifications.Tag;
            var f = (formScreenPosition)SetActive(new formScreenPosition(formScreenPosition.NotificationType.Patch, v.Screen, v.Anchor));
            f.FormClosing += delegate
            {
                checkAutoUpdateDownloadNotifications.Tag = new Settings.ScreenAttachment((byte)f.SelectedScreen, f.SelectedAnchor);
                f.Dispose();
            };
            f.StartPosition = FormStartPosition.Manual;
            var source = labelAutoUpdateDownloadNotificationsConfig;
            f.Location = Point.Add(source.Parent.PointToScreen(Point.Empty), new Size(source.Location.X - f.Width / 2, source.Location.Y - f.Height / 2));
            f.Show(this);
        }

        private void checkWindowCaption_CheckedChanged(object sender, EventArgs e)
        {
            textWindowCaption.Enabled = checkWindowCaption.Checked;
        }

        private void label32_Click(object sender, EventArgs e)
        {
            textWindowCaption.SelectedText = ((Label)sender).Text;
        }

        private void checkCustomUsername_CheckedChanged(object sender, EventArgs e)
        {
            textCustomUsername.Enabled = buttonCustomUsername.Enabled =checkCustomUsername.Checked;
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

        private void checkShowDailies_CheckedChanged(object sender, EventArgs e)
        {
            checkShowDailiesAuto.Enabled = checkShowDailies.Checked;
        }

        private void buttonCustomUsername_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.SaveFolderDialog();
            var filename = textCustomUsername.Text;
            var userprofile = Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

            f.InitialDirectory = userprofile;

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
                    if (f.FileName.StartsWith(userprofile, StringComparison.OrdinalIgnoreCase) && f.FileName.IndexOf(Path.DirectorySeparatorChar, userprofile.Length + 1) == -1)
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

        private void labelAutoUpdateDownloadProgressConfig_Click(object sender, EventArgs e)
        {
            var v = (Rectangle)checkAutoUpdateDownloadProgress.Tag;
            var f = (formProgressOverlay)SetActive(new formProgressOverlay());
            f.MinimumSize = new Size(50, 1);
            f.MaximumSize = new Size(ushort.MaxValue, 50);
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

                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void checkScreenshotImageFormat_CheckedChanged(object sender, EventArgs e)
        {
            comboScreenshotImageFormat.Enabled = checkScreenshotImageFormat.Checked;
            buttonScreenshotsExistingApply.Enabled = checkScreenshotNameFormat.Checked || checkScreenshotImageFormat.Checked;
        }

        private void comboScreenshotImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelScreenshotImageFormatJpg.Visible = comboScreenshotImageFormat.SelectedIndex == 0;
            panelScreenshotImageFormatPng.Visible = comboScreenshotImageFormat.SelectedIndex == 1;
        }

        private async void buttonScreenshotsExistingApply_Click(object sender, EventArgs e)
        {
            var f = new Windows.Dialogs.OpenFolderDialog();
            if (buttonScreenshotsExistingApply.Tag != null)
            {
                f.InitialDirectory = (string)buttonScreenshotsExistingApply.Tag;
            }
            else
            {
                if (checkScreenshotsLocation.Checked && string.IsNullOrEmpty(textScreenshotsLocation.Text) && Directory.Exists(textScreenshotsLocation.Text))
                    f.InitialDirectory = textScreenshotsLocation.Text;
                else
                    f.InitialDirectory = Client.FileManager.GetPath(Client.FileManager.SpecialPath.Screens);
            }
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

        private void checkProcessPriority_CheckedChanged(object sender, EventArgs e)
        {
            comboProcessPriority.Enabled = checkProcessPriority.Checked;
        }

        private void panelProcessAffinity_SizeChanged(object sender, EventArgs e)
        {
            int b;
            if (panelProcessAffinity.Visible)
                b = panelProcessAffinity.Bottom;
            else
                b = checkProcessAffinityAll.Bottom;
            panelLaunchOptionsProcessContent.Top = b + 13;
        }

        private void panelProcessAffinity_VisibleChanged(object sender, EventArgs e)
        {
            panelProcessAffinity_SizeChanged(sender, e);
        }

        private void checkProcessAffinityAll_CheckedChanged(object sender, EventArgs e)
        {
            panelProcessAffinity.Visible = !checkProcessAffinityAll.Checked;
        }

        private void checkNoteNotifications_CheckedChanged(object sender, EventArgs e)
        {
            checkNoteNotificationsOnlyWhileActive.Enabled = checkNoteNotifications.Checked;
        }

        private void labelNoteNotifications_Click(object sender, EventArgs e)
        {
            var v = (Settings.NotificationScreenAttachment)checkNoteNotifications.Tag;
            var f = (formScreenPosition)SetActive(new formScreenPosition(formScreenPosition.NotificationType.Note, v.Screen, v.Anchor));
            f.FormClosing += delegate
            {
                checkNoteNotifications.Tag = new Settings.NotificationScreenAttachment((byte)f.SelectedScreen, f.SelectedAnchor, checkNoteNotificationsOnlyWhileActive.Checked);
                f.Dispose();
            };
            f.StartPosition = FormStartPosition.Manual;
            var source = labelNoteNotifications;
            f.Location = Point.Add(source.Parent.PointToScreen(Point.Empty), new Size(source.Location.X - f.Width / 2, source.Location.Y - f.Height / 2));
            f.Show(this);
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

                    switch ((HitTest)m.Result.GetValue())
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
            numericDelaySeconds.Enabled = checkDelaySeconds.Checked;
        }

        private void labelShowLocalizedExecution_Click(object sender, EventArgs e)
        {
            try
            {
                var path = Path.Combine(Path.GetDirectoryName(textGW2Path.Text), Client.FileManager.LOCALIZED_EXE_FOLDER_NAME);
                if (Directory.Exists(path))
                    Util.Explorer.OpenFolder(path);
                else
                    throw new DirectoryNotFoundException();
            }
            catch
            {
                labelShowLocalizedExecution.Visible = false;
            }
        }

        private void panelLaunchConfiguration_VisibleChanged(object sender, EventArgs e)
        {
            if (!panelLaunchConfiguration.Visible)
                return;

            string root;

            try
            {
                root = Path.GetDirectoryName(textGW2Path.Text);
            }
            catch
            {
                return;
            }

            if (!root.Equals(labelShowLocalizedExecution.Tag))
            {
                labelShowLocalizedExecution.Tag = root;
                labelShowLocalizedExecution.Visible = Directory.Exists(Path.Combine(root, Client.FileManager.LOCALIZED_EXE_FOLDER_NAME));
            }

            if (!root.Equals(checkLocalizedExecution.Tag))
            {
                checkLocalizedExecution.Tag = root;

                if (!(checkLocalizedExecution.Enabled = Client.FileManager.IsPathSupported(root, false)))
                    checkLocalizedExecution.Checked = false;
            }
        }

        private void buttonAccountBarReset_Click(object sender, EventArgs e)
        {
            Settings.WindowBounds[typeof(formAccountBar)].Clear();
        }

        private void labelAccountBarInfo_SizeChanged(object sender, EventArgs e)
        {
            panelAccountBarControls.Top = labelAccountBarInfo.Bottom;
        }

        private void labelResyncLocalizedExecution_Click(object sender, EventArgs e)
        {
            var failed = 0;

            try
            {
                var path = Path.Combine(Path.GetDirectoryName(textGW2Path.Text), Client.FileManager.LOCALIZED_EXE_FOLDER_NAME);
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
                        labelResyncLocalizedExecution.Visible = false;
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

        private void labelShowLocalizedExecution_VisibleChanged(object sender, EventArgs e)
        {
            labelResyncLocalizedExecution.Visible = labelShowLocalizedExecution.Visible;
        }

        private void checkLocalizedExecution_CheckedChanged(object sender, EventArgs e)
        {
            checkPreventDefaultCoherentUI.Enabled = !checkLocalizedExecution.Checked;
            checkLocalizedExecutionExcludeUnknown.Enabled = checkLocalizedExecution.Checked;
        }

        private void buttonSample_SizeChanged(object sender, EventArgs e)
        {
            panelStyleGeneral.Top = buttonSample.Bottom;
        }
    }
}
