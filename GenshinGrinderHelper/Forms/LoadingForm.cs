using System.ComponentModel;
using System.Drawing.Drawing2D;
using Timer = System.Windows.Forms.Timer;

namespace GenshinGrinderHelper.Forms
{
    public partial class LoadingForm : Form
    {
        private Timer fadeTimer;
        private bool fadeIn;

        private const double FadeStep = 0.06;

        private ProgressRing ring;
        private Label textLabel;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected override bool ShowWithoutActivation => true;
        public LoadingForm(Form owner = null)
        {
            logger.Info("Initalizing");
            try
            {
                InitForm(owner);
                InitControls();
                InitFade();
#if NET10_0_OR_GREATER
                ApplyTheme();
#endif
                logger.Info("Successfully initalized");
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to initalize LoadingForm");
                throw;
            }
        }

        #region 初始化

        private void InitForm(Form owner)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(245, 245, 245);
            Opacity = 1;
            Owner = owner;
            Width = 320;
            Height = 200;
            TopMost = true;
            TabStop = false;
            ShowInTaskbar = false;
            DoubleBuffered = true;
        }

        private void InitControls()
        {
            ring = new ProgressRing
            {
                Size = new Size(64, 64),
                Location = new Point((Width - 64) / 2, 50)
            };

            textLabel = new Label
            {
                Text = "加载中……",
                Dock = DockStyle.Bottom,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(120, 120, 120),
                Font = new Font("Microsoft YaHei UI", 10f),
            };

            Controls.Add(ring);
            Controls.Add(textLabel);
        }

        private void InitFade()
        {
            fadeTimer = new Timer();
            fadeTimer.Interval = 15;
            fadeTimer.Tick += FadeTick;
        }
        private void AlignToOwner(object s = null, EventArgs e = null)
        {
            if (Owner == null || Owner.WindowState == FormWindowState.Minimized)
                return;

            var rect = Owner.Bounds;

            Location = new Point(
                rect.Left + (rect.Width - Width) / 2,
                rect.Top + (rect.Height - Height) / 2
            );
        }
#if NET10_0_OR_GREATER
        private void ApplyTheme()
        {
            if (Application.SystemColorMode == SystemColorMode.Dark)
            {
                BackColor = Color.FromArgb(32, 32, 32);
                textLabel.ForeColor = Color.FromArgb(200, 200, 200);
            }
            else
            {
                BackColor = Color.FromArgb(245, 245, 245);
                textLabel.ForeColor = Color.FromArgb(120, 120, 120);
            }
        }
#endif
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;

                createParams.ExStyle |= 0x20;  // WS_EX_TRANSPARENT
                createParams.ExStyle &= ~0x40000;  // ~WS_EX_APPWINDOW
                createParams.ExStyle |= 0x80;  // WS_EX_TOOLWINDOW
                createParams.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return createParams;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                fadeTimer?.Dispose();
            base.Dispose(disposing);
        }
        #endregion

        #region 公共方法

        public void ShowLoading(string text = "加载中……")
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => ShowLoading(text));
                return;
            }
            textLabel.Text = text;

            if (Enabled)
                return;

            BringToFront();


            fadeIn = true;
            Opacity = 0;

            AlignToOwner();

            Owner?.Move -= AlignToOwner;
            Owner?.Resize -= AlignToOwner;
            Owner?.VisibleChanged -= OwnerVisibleChanged;
            
            Owner?.Move += AlignToOwner;
            Owner?.Resize += AlignToOwner;
            Owner?.VisibleChanged += OwnerVisibleChanged;

            Show();
            Enabled = true;
            fadeTimer.Start();
            ring.Resume();
        }

        private void OwnerVisibleChanged(object sender, EventArgs e)
        {
            if (!Owner.Visible) Hide(); else Show();
        }

        public void HideLoading()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(HideLoading));
                return;
            }

            if (!Enabled)
                return;

            fadeIn = false;
            fadeTimer.Start();
            ring.Suspend();

            Owner?.Move -= AlignToOwner;
            Owner?.Resize -= AlignToOwner;
        }

        #endregion

        #region Fade 动画

        private void FadeTick(object sender, EventArgs e)
        {
            if (fadeIn)
            {
                if (Opacity < 1)
                {
                    Opacity += FadeStep;
                }
                else
                {
                    Opacity = 1;
                    fadeTimer.Stop();
                }
            }
            else
            {
                if (Opacity > 0)
                {
                    Opacity -= FadeStep;
                }
                else
                {
                    Opacity = 0;
                    fadeTimer.Stop();
                    Hide();
                    Enabled = false;
                }
            }
        }

        #endregion
    }

    #region ProgressRing

    internal class ProgressRing : Control
    {
        private readonly Timer timer;

        private float startAngle = 0f;
        private float sweepAngle = 0f;

        private const float rotationSpeed = 3f;
        private const float breathDuration = 5000f;
        private const float minSweepAngle = 45f;
        private const float maxSweepAngle = 315f;

        private DateTime lastUpdateTime;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color RingColor { get; set; } = Color.DodgerBlue;

        public ProgressRing()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer,
                true);

            lastUpdateTime = DateTime.Now;
            timer = new Timer { Interval = 16 }; // 约60FPS
            timer.Tick += (s, e) => TickAnimation();
            timer.Start();
        }

        public void Resume()
        {
            lastUpdateTime = DateTime.Now;
            startAngle = sweepAngle = 0f;
            timer.Start();
        }
        public void Suspend() => timer.Stop();

        private void TickAnimation()
        {
            var now = DateTime.Now;
            float deltaTime = (float)(now - lastUpdateTime).TotalMilliseconds;
            lastUpdateTime = now;

            // 平滑旋转
            startAngle += rotationSpeed;
            if (startAngle >= 360f)
                startAngle -= 360f;

            // 计算缓动的呼吸效果
            float totalElapsed = (float)(Environment.TickCount % breathDuration);
            float normalizedTime = totalElapsed / breathDuration; // 0到1之间


            float sineWave = (float)Math.Sin(normalizedTime * Math.PI * 2);
            float easedFactor = (sineWave + 1) / 2;

            float smoothFactor = SmoothStep(sineWave);

            sweepAngle = Lerp(minSweepAngle, maxSweepAngle, easedFactor);

            Invalidate();
        }

        private float Lerp(float start, float end, float t)
        {
            return start + (end - start) * t;
        }
        private float SmoothStep(float t)
        {
            // 三次平滑：t²(3-2t)
            return t * t * (3f - 2f * t);
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 计算合适的内边距
            int diameter = Math.Min(Width, Height) - 8;
            int x = (Width - diameter) / 2;
            int y = (Height - diameter) / 2;

            using (Pen pen = new Pen(RingColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                e.Graphics.DrawArc(
                    pen,
                    x, y,
                    diameter,
                    diameter,
                    startAngle,
                    sweepAngle
                );
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                timer?.Dispose();
            base.Dispose(disposing);
        }
    }


    #endregion
}
