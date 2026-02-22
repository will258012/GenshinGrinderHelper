using GenshinGrinderHelper.Managers;
using GenshinGrinderHelper.Properties;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using Timer = System.Windows.Forms.Timer;

namespace GenshinGrinderHelper.Forms
{
    public partial class BrowserForm : Form
    {
        public WebView2 WebView { get; private set; }
        private Panel toolbar;
        private TextBox addressBar;
        private Button backButton;
        private Button forwardButton;
        private Button refreshButton;
        private Button historyButton;
        private Timer hideTimer;
        private bool firstLoad = true;

        private LoadingForm _loadingForm;
        public LoadingForm LoadingForm
        {
            get
            {
                if (_loadingForm == null || _loadingForm.IsDisposed)
                {
                    _loadingForm = new LoadingForm(this);
                }
                return _loadingForm;
            }
        }

        private bool isToolbarVisible = true;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private const string INITIAL_URL = "https://search.bilibili.com/";

        #region 初始化
        public BrowserForm()
        {
            logger.Info("Initalizing");
            FormBorderStyle = FormBorderStyle.Sizable;
            Text = "Genshin Grinder Helper";
            Size = new Size(1600, 1200);
            StartPosition = FormStartPosition.CenterScreen;
            FormClosed += (s, e) => Environment.Exit(0);
            TopMost = Config.Instance.ShowOnTop;
            ShowIcon = true;
            Font = new Font("Microsoft YaHei UI", Font.Size);

            InitializeBrowser();

            logger.Info("Successfully initalized");
        }
        private async void InitializeBrowser()
        {
            try
            {
                CheckWebView2Environment();
                InitializeHideTimer();
                InitalizeToolbar();
                InitalizeWebView();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to initalize browser window");
                throw;
            }
        }

        private void CheckWebView2Environment()
        {
            string browserVersion = null;
            try
            {
                browserVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Error checking WebView2 runtime version");
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(browserVersion))
                    logger.Info($"Detected WebView2 runtime version: {browserVersion}");
                else
                {
                    TopMost = false;
                    try
                    {
                        var legecyBrowser = new WebBrowser()
                        {
                            Location = new Point(0, 0),
                            Size = new Size(Width, Height),
                            Dock = DockStyle.Fill,
                        };
                        Controls.Add(legecyBrowser);
                        legecyBrowser.AllowNavigation = false;
                        legecyBrowser.IsWebBrowserContextMenuEnabled = false;
                        legecyBrowser.WebBrowserShortcutsEnabled = false;
                        legecyBrowser.ScrollBarsEnabled = false;
                        legecyBrowser.DocumentText = Resources.WebView2NotInstalledError_html;
                    }
                    catch
                    {
                        Process.Start(new ProcessStartInfo() { UseShellExecute = true, FileName = "https://go.microsoft.com/fwlink/p/?LinkId=2124703" });
                    }
                }
            }
        }
        private void InitializeHideTimer()
        {
            hideTimer = new()
            {
                Interval = 2000
            };
            hideTimer.Tick += (s, e) => HideToolbar();
            hideTimer.Start();
        }

        private void InitalizeToolbar()
        {
            toolbar = new Panel
            {
                Height = 40,
                Dock = DockStyle.Top
            };
            toolbar.MouseEnter += (s, e) =>
            {
                hideTimer.Stop();
                if (!isToolbarVisible)
                {
                    ShowToolbar();
                }
            };
            toolbar.MouseLeave += (s, e) => hideTimer.Start();
#if NET48
            toolbar.ContextMenu = new ContextMenu();
            toolbar.ContextMenu.MenuItems.Add("键位绑定工具 (&K)", (s, e) =>
            {
                using KeyBindingForm form = new();
                form.TopMost = true;
                form.ShowDialog();
                form.BringToFront();
            });
            toolbar.ContextMenu.MenuItems.Add("调试工具 (F12)", (s, e) =>
            {
                WebView.CoreWebView2.OpenDevToolsWindow();
            });
            toolbar.ContextMenu.MenuItems.Add("关于 (&A)", (s, e) =>
            {
                using About aboutBox = new();
                aboutBox.TopMost = true;
                aboutBox.ShowDialog();
                aboutBox.BringToFront();
            });

#endif
#if NET10_0_OR_GREATER
            toolbar.ContextMenuStrip = new ContextMenuStrip();
            toolbar.ContextMenuStrip.Items.Add("键位绑定工具 (&K)", null, (s, e) =>
            {
                using KeyBindingForm hotkeyForm = new();
                hotkeyForm.TopMost = true;
                hotkeyForm.ShowDialog();
                hotkeyForm.BringToFront();
            });
            toolbar.ContextMenuStrip.Items.Add("调试工具 (F12)", null, (s, e) =>
            {
                WebView.CoreWebView2.OpenDevToolsWindow();
            });
            toolbar.ContextMenuStrip.Items.Add("关于 (&A)", null, (s, e) =>
            {
                using About aboutBox = new();
                aboutBox.TopMost = true;
                aboutBox.ShowDialog();
                aboutBox.BringToFront();
            });

#endif
            InitalizeAddressbar();
            InitalizeButtons();

            toolbar.Controls.AddRange([backButton, forwardButton, refreshButton, historyButton, addressBar]);
            Controls.Add(toolbar);
        }
        private void InitalizeButtons()
        {
            backButton = new Button
            {
                Text = "←",
                Location = new Point(5, 8),
                Size = new Size(30, 25),
                FlatStyle = FlatStyle.System,
                Font = Font
            };
            forwardButton = new Button
            {
                Text = "→",
                Location = new Point(40, 8),
                Size = new Size(30, 25),
                FlatStyle = FlatStyle.System,
                Font = Font
            };
            refreshButton = new Button
            {
                Text = "刷新",
                Location = new Point(75, 8),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.System,
                Font = Font
            };
            historyButton = new Button
            {
                Text = "历史",
                Location = new Point(130, 8),
                Size = new Size(50, 25),
                FlatStyle = FlatStyle.System,
                Font = Font
            };
            backButton.Click += (s, e) =>
            {
                WebView?.GoBack();
            };
            forwardButton.Click += (s, e) =>
            {
                WebView?.GoForward();
            };
            refreshButton.Click += (s, e) =>
            {
                WebView?.Reload();
            };
            historyButton.Click += (s, e) =>
            {
                NavigateToUrl("edge://history");
            };
        }

        private void InitalizeAddressbar()
        {
            addressBar = new TextBox
            {
                Width = ClientSize.Width - 250,
                Height = 25,
                Location = new Point(180, 8),
                Margin = new Padding(2),
                Font = Font
            };
            addressBar.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    NavigateToUrl(addressBar.Text);
                }
            };
            addressBar.MouseEnter += (s, e) => hideTimer.Stop();
        }

        private async void InitalizeWebView()
        {
            WebView = new()
            {
                Location = new Point(0, 40),
                Size = new Size(Width, Height - 40),
            };

            await WebView.EnsureCoreWebView2Async();

            WebView.NavigationStarting += CoreWebView2_NavigationStarting;
            WebView.NavigationCompleted += CoreWebView2_NavigationCompleted;

            WebView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            WebView.CoreWebView2.DocumentTitleChanged += (_, _) => Text = $"Genshin Grinder Helper - {WebView.CoreWebView2.DocumentTitle}";

            WebView.CoreWebView2.HistoryChanged += CoreWebView2_HistoryChanged;

            WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = false;
            WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
            WebView.CoreWebView2.Settings.IsPinchZoomEnabled = false;
            WebView.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            WebView.CoreWebView2.Profile.PreferredColorScheme = CoreWebView2PreferredColorScheme.Auto;
#if NET10_0_OR_GREATER
            WebView.CoreWebView2.Settings.IsReputationCheckingRequired = false;
#endif

            Controls.Add(WebView);
            await LoadIntro();


            async Task LoadIntro()
            {
                try
                {
                    var cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://bilibili.com");

                    bool hasBilibiliCookie = cookies != null && cookies.Any(c =>
                        c.Name == "SESSDATA" &&
                        c.Expires > DateTime.Now);

                    if (!hasBilibiliCookie)
                    {
                        WebView.CoreWebView2.NavigateToString(Resources.Intro_html);
                    }
                    else
                    {
                        NavigateToUrl(INITIAL_URL);
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, "Error on checking cookies");
                    NavigateToUrl(INITIAL_URL);
                }
            }
        }
        #endregion
        #region WinForms 重写
        protected override void OnSizeChanged(EventArgs e)
        {
            WebView?.Size = new Size(ClientSize.Width,
                isToolbarVisible ? ClientSize.Height - 40 : ClientSize.Height);

            addressBar?.Width = ClientSize.Width - 250;
            base.OnSizeChanged(e);
        }
        protected override void OnActivated(EventArgs e)
        {
            if (!Config.Instance.HotKeys.HotKeyEnabledInGenshinOnly)
            {
                HotkeyManager.Instance.Suspend();
            }

            base.OnActivated(e);
        }
        protected override void OnDeactivate(EventArgs e)
        {
            if (!Config.Instance.HotKeys.HotKeyEnabledInGenshinOnly)
                if (IsVideoPage())
                {
                    HotkeyManager.Instance.Resume();
                }
                else
                {
                    HotkeyManager.Instance.Suspend();
                }
            base.OnDeactivate(e);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                hideTimer?.Dispose();

            base.Dispose(disposing);
        }
        #endregion
        #region CoreWebView2 事件
        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            Text = $"加载中…… - {e.Uri}";

            SubtitleManager.Instance.Suspend();
            HotkeyManager.Instance.Suspend();

            if (Config.Instance.LoadingIndicator)
                if (!firstLoad)
                    LoadingForm.ShowLoading();
                else
                {
                    LoadingForm.ShowLoading("正在启动……");
                    firstLoad = false;
                }

            UseWaitCursor = true;
        }
        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Text = $"Genshin Grinder Helper - {WebView.CoreWebView2.DocumentTitle}";
            addressBar.Text = WebView.Source.ToString();

            if (!Config.Instance.HotKeys.HotKeyEnabledInGenshinOnly)
                if (IsVideoPage() && WindowUtils.GetForegroundWindow() == Handle)
                {
                    HotkeyManager.Instance.Resume();
                }
                else
                {
                    HotkeyManager.Instance.Suspend();
                }

            if (Config.Instance.LoadingIndicator)
            {
                LoadingForm.HideLoading();
            }

            UseWaitCursor = false;
        }

        private void CoreWebView2_HistoryChanged(object sender, object e)
        {
            if (IsVideoPage())
            {
                SubtitleManager.Instance.Resume(WebView.CoreWebView2);
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            e.Handled = true;

            if (!string.IsNullOrEmpty(e.Uri))
            {
                WebView.CoreWebView2.Navigate(e.Uri);
            }
        }
        #endregion
        #region 辅助方法
        private void ShowToolbar()
        {
            if (!isToolbarVisible)
            {
                SuspendLayout();
                isToolbarVisible = true;
                toolbar.Height = 40;
                addressBar.Show();
                WebView.Location = new Point(0, 40);
                WebView.Height = ClientSize.Height - 40;
                FormBorderStyle = FormBorderStyle.Sizable;
                ResumeLayout();
            }
        }

        private void HideToolbar()
        {
            if (isToolbarVisible)
            {
                SuspendLayout();
                isToolbarVisible = false;
                toolbar.Height = 15;
                addressBar.Hide();
                WebView.Location = new Point(0, 0);
                WebView.Height = ClientSize.Height;
                FormBorderStyle = FormBorderStyle.None;
                ResumeLayout();
            }
        }

        private void NavigateToUrl(string url)
        {
            url = url.ToLower().Trim();
            if (!string.IsNullOrEmpty(url))
            {
                if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("edge://"))
                {
                    url = "https://bing.com/search?q=" + url;
                }
                WebView.CoreWebView2?.Navigate(url);
            }
        }
        internal bool IsVideoPage()
        {
            var source = WebView?.Source;
            if (source == null) return false;

            string url = source.OriginalString.ToLower();
            string host = source.Host.ToLower();

            return host.Contains("bilibili.com") &&
                   (url.Contains("video/") ||
                    url.Contains("bvid"));

        }
        #endregion

    }
}