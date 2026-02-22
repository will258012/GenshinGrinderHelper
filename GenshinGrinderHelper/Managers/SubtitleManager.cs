using Microsoft.Web.WebView2.Core;

namespace GenshinGrinderHelper.Managers
{
    public class SubtitleManager : IDisposable
    {
        public static SubtitleManager Instance { get; } = new();
        public event Action<string> OnSubtitleReceived;
        private const string getSubtitleScript = @"
            (function() {
                // 查找包含指定class的span元素
                var subtitleSpans = document.querySelectorAll('span.bili-subtitle-x-subtitle-panel-text.bili-subtitle-x-subtitle-panel-text-mouse-move-cursor');
                
                if (subtitleSpans.length > 0) {
                    // 返回第一个匹配元素的文本内容
                    return subtitleSpans[0].textContent || subtitleSpans[0].innerText;
                }
                
                // 如果没有找到精确匹配，尝试查找包含这些class的元素
                var fallbackSpans = document.querySelectorAll('span[class*=""bili-subtitle-x-subtitle-panel-text""]');
                if (fallbackSpans.length > 0) {
                    return fallbackSpans[0].textContent || fallbackSpans[0].innerText;
                }
                
                return '';
            })();
        ";
        private const string enableSubtitleScript = @"
            (function() {
                // 查找字幕开关按钮
                var subtitleSwitch = document.querySelector('.bpx-player-ctrl-subtitle-close-switch');
                if (!subtitleSwitch) return 'false';
                
                // 检查字幕是否已开启
                if (!subtitleSwitch.classList.contains('bpx-state-active')) {
                    return 'true';
                }
                
                // 字幕未开启，执行开启操作
                subtitleSwitch.click();
                
                // 短暂延迟后选择第一个字幕选项
                setTimeout(function() {
                    var firstSubtitleItem = document.querySelector('.bpx-player-ctrl-subtitle-major .bpx-player-ctrl-subtitle-language-item');
                    if (firstSubtitleItem) {
                        firstSubtitleItem.click();
                    }
                }, 300);
                
                return 'true';
            })();
        ";
        private readonly System.Windows.Forms.Timer subtitleTimer = new();
        private CoreWebView2 coreWebView2;
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private SubtitleManager()
        {
            logger.Info("Initalizing");
            subtitleTimer.Interval = 1000;
            subtitleTimer.Tick += (s, e) => Update();
            subtitleTimer.Start();
            logger.Info("Successfully initalized");
        }
        ~SubtitleManager() => Dispose();
        private async void Update()
        {
            if (coreWebView2 == null || coreWebView2.Source == null)
            {
                Suspend();
                return;
            }

            var subtitleText = await coreWebView2.ExecuteScriptAsync(getSubtitleScript);

            if (!string.IsNullOrEmpty(subtitleText) && subtitleText != "null")
            {
                subtitleText = subtitleText.Trim('"');

                if (!string.IsNullOrEmpty(subtitleText))
                {
                    OnSubtitleReceived?.Invoke(subtitleText);
                    logger.Trace("Received subtitle: " + subtitleText);
                }
            }
        }
        public async void Resume(CoreWebView2 coreWebView2)
        {
            logger.Info("Resuming SubtitleManager");
            this.coreWebView2 ??= coreWebView2;

            string enableResult = "";

            if (Config.Instance.LoadingIndicator)
                Program.BrowserForm.LoadingForm.ShowLoading("尝试启用字幕中……");

            for (int i = 1; i <= 5; i++)
            {
                await Task.Delay(1000);
                enableResult = await coreWebView2.ExecuteScriptAsync(enableSubtitleScript);
                if (enableResult.Contains("true"))
                {
                    subtitleTimer.Start();
                    break;
                }
            }
            if (Config.Instance.LoadingIndicator)
                Program.BrowserForm.LoadingForm.HideLoading();
        }
        public void Suspend()
        {
            subtitleTimer.Stop();
            logger.Info("SubtitleManager was suspended");
        }

        public void Dispose()
        {
            subtitleTimer?.Dispose();
        }
    }
}
