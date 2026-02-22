using GenshinGrinderHelper.Forms;
using GenshinGrinderHelper.Managers;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GenshinGrinderHelper
{
    public static class Program
    {
        public static DirectionForm DirectionForm { get; private set; }
        public static BrowserForm BrowserForm { get; private set; }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        [STAThread]
        private static void Main(string[] args)
        {
            _ = new Mutex(true, "Global\\GenshinGrinderHelper", out bool isNewInstance);
            if (!isNewInstance)
            {
                WindowUtils.ActivateExistingWindow();
                return;
            }
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            Application.ThreadException += OnThreadException;

            try
            {
                logger.Info($"Genshin Grinder Helper started. v{Assembly.GetExecutingAssembly().GetName().Version}");
                logger.Info($".NET Version: {RuntimeInformation.FrameworkDescription}");
                logger.Info("OS Version: {@OS}", Environment.OSVersion);
                var screen = Screen.PrimaryScreen.Bounds;
                logger.Info("Screen resolution: {Res}", $"{screen.Width}x{screen.Height}");
            }
            catch { }


            
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
#if NET10_0_OR_GREATER
                Application.SetDefaultFont(new Font("Microsoft YaHei UI", 9f));
                Application.SetColorMode(SystemColorMode.System);
                logger.Info($"Current system color: {Application.SystemColorMode}");
#endif

                Application.ApplicationExit += (s, e) =>
                {
                    HotkeyManager.Instance.Dispose();
                    SubtitleManager.Instance.Dispose();
                    logger.Info("Application exited");
                };

                BrowserForm = new BrowserForm();

                Thread browserThread = new Thread(() =>
                {
                    DirectionForm = new DirectionForm();
                    Application.Run(DirectionForm);
                });
                browserThread.SetApartmentState(ApartmentState.STA);
                browserThread.Start();

                Application.Run(BrowserForm);
            }
            catch (Exception e)
            {
                logger.Fatal(e);
                throw;
            }
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            logger.Fatal(e.Exception);
            using var dialog = new UnhandledExceptionDialog(e.Exception);
            dialog.TopMost = true;
            dialog.ShowDialog();
            dialog.BringToFront();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            logger.Fatal((Exception)e.ExceptionObject);
            using var dialog = new UnhandledExceptionDialog((Exception)e.ExceptionObject);
            dialog.TopMost = true;
            dialog.ShowDialog();
            dialog.BringToFront();
        }
    }
}