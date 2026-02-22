using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;
using Timer = System.Windows.Forms.Timer;

namespace GenshinGrinderHelper.Managers
{
    public class HotkeyManager : IDisposable
    {
        public class HotKeyListener : IDisposable
        {
            private readonly CancellationTokenSource cancellationTokenSource = new();
            private bool isMonitoring = false;
            private Process listenerProcess;
            private NamedPipeServerStream server;
            private readonly Logger logger = LogManager.GetCurrentClassLogger();

            public bool IsMonitoring { get => isMonitoring; }

            public event Action<Keys> OnKeyPressed;

            public HotKeyListener() => Start();
            ~HotKeyListener() => Dispose();
            public void Start()
            {
                if (isMonitoring) return;
                logger.Info("Starting listener");
                Task.Run(ListenPipeServer);
                StartListenerProcess();

                isMonitoring = true;
                logger.Info("Successfully started listener");
            }

            private void StartListenerProcess()
            {
                try
                {
                    logger.Info("Starting listener process");
                    if (listenerProcess != null) return;

                    string monitorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GenshinGrinderHelper.HotKeyListener.exe");

                    if (Process.GetProcessesByName("GenshinGrinderHelper.HotKeyListener") is var processes && processes.Length > 0)
                    {
                        foreach (var proc in processes)
                        {
                            try { proc.Kill(); } catch { }
                        }
                    }

                    listenerProcess = new Process();
                    listenerProcess.StartInfo.FileName = monitorPath;
                    listenerProcess.StartInfo.UseShellExecute = false;
                    listenerProcess.StartInfo.CreateNoWindow = true;
                    listenerProcess.StartInfo.Verb = "runas";
                    listenerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    listenerProcess.EnableRaisingEvents = true;

                    listenerProcess.Exited += (s, e) =>
                    {
                        isMonitoring = false;
                    };

                    listenerProcess.Start();
                    Program.BrowserForm.LoadingForm.HideLoading();
                    logger.Info($"Sucessfully started listener process, pid:{listenerProcess.Id}");
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to start listener process");
                }
            }

            private async Task ListenPipeServer()
            {
                logger.Info("Starting listening pipe server");
                var token = cancellationTokenSource.Token;

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using (server = new NamedPipeServerStream(
                            "GenshinGrinderHelper.HotKeyListener",
                            PipeDirection.InOut,
                            1,
                            PipeTransmissionMode.Message,
                            PipeOptions.Asynchronous))
                        {
                            await server.WaitForConnectionAsync(token);

                            byte[] buffer = new byte[16];

                            while (server.IsConnected && !token.IsCancellationRequested)
                            {
                                if (isMonitoring)
                                {
                                    int bytesRead = await server.ReadAsync(buffer, 0, buffer.Length, token);
                                    if (bytesRead > 0)
                                    {
                                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                                        logger.Trace($"Rechived: {bytesRead},{data}");

                                        if (Enum.TryParse(data, out Keys vkCode))
                                        {
                                            OnKeyPressed?.Invoke(vkCode);
                                        }
                                    }
                                }
                                else
                                {
                                    await Task.Delay(100, token);
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        logger.Warn("Stopped listening pipe server");
                        break;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Pipe server error");
                        if (!token.IsCancellationRequested)
                        {
                            await Task.Delay(2000, token);
                        }
                    }
                }
            }
            internal async Task SendControlAsync(string msg)
            {
                if (server == null || !server.IsConnected) return;

                byte[] data = Encoding.UTF8.GetBytes(msg);

                await server.WriteAsync(data, 0, data.Length);
                await server.FlushAsync();
            }
            public async void Suspend()
            {
                isMonitoring = false;

                await SendControlAsync("PAUSE");
                logger.Info("Listener was suspended");
            }

            public async void Resume()
            {
                logger.Info("Resuming listener");
                if (!isMonitoring)
                {
                    isMonitoring = true;

                    if (listenerProcess?.HasExited == true)
                    {
                        logger.Warn("Listener process has exited early, restarting...");
                        listenerProcess?.Dispose();
                        StartListenerProcess();
                    }
                    else await SendControlAsync("RESUME");
                }
            }

            public void Stop()
            {
                logger.Info("Stopping listener");
                isMonitoring = false;
                cancellationTokenSource?.Cancel();
                StopListenerProcess();
                logger.Info("Successfully stopped listener");
            }

            private void StopListenerProcess()
            {
                try
                {
                    if (listenerProcess != null && !listenerProcess.HasExited)
                    {
                        listenerProcess.Kill();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Stop listener process error: {e.Message}");
                }
            }

            public void Dispose()
            {
                Stop();
                server?.Dispose();
                listenerProcess?.Dispose();
                cancellationTokenSource?.Dispose();
            }
        }

        public static HotkeyManager Instance { get; } = new();

        public event Action<HotKeyActions, Keys> OnHotkeyTriggered;

        private readonly HotKeyListener hotKeyListener;
        private readonly Timer GenshinTimer = new();
        private readonly Dictionary<HotKeyActions, Keys> hotkeyActions = new()
        {
            { HotKeyActions.PlayPause, Keys.Space },
            { HotKeyActions.Forward, Keys.Right },
            { HotKeyActions.Rewind, Keys.Left },
            { HotKeyActions.NextPart, Keys.OemCloseBrackets },
            { HotKeyActions.PreviousPart, Keys.OemOpenBrackets }
        };
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        public enum HotKeyActions
        {
            None,
            PlayPause,
            Rewind,
            Forward,
            NextPart,
            PreviousPart
        }

        public HotkeyManager()
        {
            if (!Config.Instance.HotKeys.Enabled) return;
            logger.Info("Initalizing");

            hotKeyListener = new();
            hotKeyListener.OnKeyPressed += SendInput;

            if (Config.Instance.HotKeys.HotKeyEnabledInGenshinOnly)
            {
                GenshinTimer.Interval = 100;
                GenshinTimer.Tick += OnGenshinTimer;
                GenshinTimer.Enabled = true;
            }

            logger.Info("Sucessfully initalized");
        }

        public void Resume()
        {
            if (hotKeyListener?.IsMonitoring == false)
                hotKeyListener?.Resume();
        }
        public void Suspend()
        {
            if (hotKeyListener?.IsMonitoring == true)
                hotKeyListener?.Suspend();
        }
        public void ReloadConfig()
        {
            logger.Info("Reloading config");
            hotKeyListener?.SendControlAsync("RELOADCONFIG");
        }
        private async void SendInput(Keys key)
        {
            if (Program.BrowserForm.InvokeRequired)
            {
                Program.BrowserForm.Invoke(new Action(() => SendInput(key)));
                return;
            }
/*
            if (!hotKeyListener.IsMonitoring) return;*/

            var coreWebView2 = Program.BrowserForm?.WebView?.CoreWebView2;
            if (coreWebView2 == null || coreWebView2.Source == null)
            {
                Suspend();
                return;
            }

/*            if (!Config.Instance.HotKeys.ReversedHotkeys.TryGetValue(key, out var action) || !hotkeyActions.TryGetValue(action, out var actionKey))
                return;*/

            Keys actionKey = Keys.None;
            HotKeyActions action = HotKeyActions.None;
            foreach (var action2 in Config.Instance.HotKeys.ReversedHotkeys
                .Where(x => x.Item1 == key)
                .Select(x => x.Item2))
            {
                if (!hotkeyActions.TryGetValue(action2, out actionKey))
                    continue;
                action = action2;
            }
            if(action == HotKeyActions.None || actionKey == Keys.None)
                return;

            string script = $@"
                (function() {{
                    var event = new KeyboardEvent('keydown', {{
                        keyCode: {(int)actionKey},
                        which: {(int)actionKey},
                        bubbles: true,
                        cancelable: true
                    }});
                    
                    document.activeElement.dispatchEvent(event);
                    document.dispatchEvent(event);
                    
                    // 短暂延迟后触发keyup
                    setTimeout(function() {{
                        var eventUp = new KeyboardEvent('keyup', {{
                            keyCode: {(int)actionKey},
                            which: {(int)actionKey},
                            bubbles: true,
                            cancelable: true
                        }});
                        
                        document.activeElement.dispatchEvent(eventUp);
                        document.dispatchEvent(eventUp);
                    }}, 50);
                }})();
            ";

            await coreWebView2.ExecuteScriptAsync(script);
            logger.Trace($"Hotkey triggered: {action} ({actionKey})");
            OnHotkeyTriggered?.Invoke(action, actionKey);
        }
        private void OnGenshinTimer(object sender, EventArgs e)
        {
            if (WindowUtils.IsGenshinActive() && Program.BrowserForm.IsVideoPage())
            {
                Resume();
            }
            else
            {
                Suspend();
            }
        }
        public void Dispose()
        {
            hotKeyListener?.Dispose();
        }
    }
}