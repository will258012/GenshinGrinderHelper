using GenshinGrinderHelper.Managers;
using GenshinGrinderHelper.Properties;
using static GenshinGrinderHelper.WindowUtils;
using System.ComponentModel;

namespace GenshinGrinderHelper.Forms
{
    public partial class DirectionForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal bool IsController
        {
            get;
            set
            {
                BackgroundImage = value ? Resources.Direction_Controller : Resources.Direction;
                field = value;
                UpdateMarkerPositions();
            }
        }

        private readonly Dictionary<Direction, Point> directionPoints = [];
        private readonly List<(Direction direction, string[] keywords)> directionKeywords =
[
                (Direction.WestNorth, ["西北"]),
                (Direction.WestSouth, ["西南"]),
                (Direction.EastNorth, ["东北"]),
                (Direction.EastSouth, ["东南"]),
                (Direction.North,    ["北"]),
                (Direction.East,     ["东"]),
                (Direction.South,    ["南"]),
                (Direction.West,     ["西"]),

                (Direction.WestNorth, ["10点", "十点", "11点", "十一点"]),
                (Direction.North,     ["12点", "十二点"]),
                (Direction.EastNorth, ["1点钟", "一点钟", "2点", "二点", "两点"]),
                (Direction.East,      ["3点", "三点"]),
                (Direction.EastSouth, ["4点", "四点", "5点", "五点"]),
                (Direction.South,     ["6点", "六点"]),
                (Direction.WestSouth, ["7点", "七点", "8点", "八点"]),
                (Direction.West,      ["9点", "九点"]),

                (Direction.WestNorth, ["地图左上"]),
                (Direction.WestSouth, ["地图左下"]),
                (Direction.EastNorth, ["地图右上"]),
                (Direction.EastSouth, ["地图右下"]),
                (Direction.North,    ["地图上"]),
                (Direction.East,     ["地图右"]),
                (Direction.South,    ["地图下"]),
                (Direction.West,     ["地图左"]),
            ];
        private static readonly Dictionary<Direction, double> compoundDirections = new()
        {
            { Direction.EastNorth,  Math.PI / 4 },
            { Direction.EastSouth, -Math.PI / 4 },
            { Direction.WestSouth, -3 * Math.PI / 4 },
            { Direction.WestNorth,  3 * Math.PI / 4 }
        };

        private PictureBox markerBox;
        private Direction currentDirection;
        private readonly System.Windows.Forms.Timer windowTimer = new();
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        protected override bool ShowWithoutActivation => true;
        public DirectionForm()
        {
            logger.Info("Initlizing");
            Initialize();
            InitializeDirectionMarkers();
            InitalizeWindowUpdate();
            IsController = Config.Instance.IsController;
            Enabled = false;
            logger.Info("Successfully initalized");
        }

        private void Initialize()
        {
            try
            {
                Height = Screen.PrimaryScreen.Bounds.Height;
                Width = Screen.PrimaryScreen.Bounds.Width;

                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.UserPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);

                SuspendLayout();

                AutoScaleDimensions = new SizeF(6f, 12f);
                AutoScaleMode = AutoScaleMode.Font;
                AutoSize = true;
                BackColor = Color.White;
                BackgroundImageLayout = ImageLayout.Stretch;
                CausesValidation = false;
                ClientSize = new Size(2560, 1440);
                ControlBox = false;
                DoubleBuffered = true;
                FormBorderStyle = FormBorderStyle.None;
                ShowInTaskbar = false;
                TabStop = false;
                StartPosition = FormStartPosition.CenterScreen;
                Text = "Genshin Grinder Helper - Direction Window";
                TopMost = true;
                TransparencyKey = Color.White;
                ResumeLayout(false);
#if DEBUG
                _ = TestMarker();
#endif
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to initialize DirectionForm");
                throw;
            }
        }
        private void InitalizeWindowUpdate()
        {
            windowTimer.Interval = 16;
            windowTimer.Tick += (s, e) => UpdateWindow();
            windowTimer.Start();
        }

        private void UpdateWindow()
        {
            try
            {
                LPRECT ct = default;

                if (!FindGameWindow(out var hWnd) || !GetClientRect(hWnd, ref ct))
                    return;

                int newWidth = ct.Right - ct.Left;
                int newHeight = ct.Bottom - ct.Top;
                float widthUnit = newWidth / 16f;
                float heightUnit = newHeight / 9f;

                if (!ClientToScreen(hWnd, ref ct)) return;

                if (newWidth != newHeight)
                {
                    if (heightUnit > widthUnit)
                        newHeight = (int)(widthUnit * 9);
                    else
                        newWidth = (int)(heightUnit * 16);
                }

                var newLocation = new Point(ct.Left, ct.Top);

                var needUpdate = !(Width == newWidth &&
                           Height == newHeight &&
                           Location == newLocation);

                if (!needUpdate) return;

                SuspendLayout();

                Size = new Size(newWidth, newHeight);
                Location = newLocation;

                TopMost = true;
                UpdateMarkerPositions();
                ResumeLayout();
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to update");
            }
        }

        private void InitializeDirectionMarkers()
        {
            try
            {
                markerBox = new PictureBox
                {
                    Image = Resources.marker,
                    Size = new Size(64, 64),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Visible = false,
                    BackColor = Color.Transparent
                };
                markerBox.Hide();
                Controls.Add(markerBox);
                markerBox.BringToFront();

                UpdateMarkerPositions();
                SubtitleManager.Instance.OnSubtitleReceived += HandleSubtitleText;
            }
            catch (Exception e)
            {
                logger.Fatal(e, "Failed to initialize direction markers");
                throw;
            }
        }
#if DEBUG
        private async Task TestMarker()
        {
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction == Direction.None)
                    continue;

                await UpdateDirectionMarker(direction);
                await Task.Delay(500);
            }
        }
#endif

        private void UpdateMarkerPositions()
        {
            // 基本方向的坐标
            int margin = IsController ? 95 : 0;

            // 计算罗盘中心点
            var centerX = (int)((margin + 195) / 2560f * Width);
            var centerY = (int)(165 / 1440f * Height);

            // 计算圆的半径
            var radius = (int)(140 / 2560f * Width);

            // 基本方向保持原有位置
            directionPoints[Direction.East] = new Point(
                (int)((margin + 350) / 2560f * Width),
                (int)(160 / 1440f * Height)
            );
            directionPoints[Direction.West] = new Point(
                (int)((margin + 35) / 2560f * Width),
                (int)(160 / 1440f * Height)
            );
            directionPoints[Direction.South] = new Point(
                (int)((margin + 195) / 2560f * Width),
                (int)(320 / 1440f * Height)
            );
            directionPoints[Direction.North] = new Point(
                (int)((margin + 195) / 2560f * Width),
                (int)(10 / 1440f * Height)
            );

            foreach (var dir in compoundDirections)
            {
                directionPoints[dir.Key] = new Point(
                    centerX + (int)(radius * Math.Cos(dir.Value)),
                    centerY - (int)(radius * Math.Sin(dir.Value))
                );
            }

            //logger.Trace("Updated direction positions: {@directions}", directionPoints);
        }

        public async void HandleSubtitleText(string subtitleText)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => HandleSubtitleText(subtitleText));
                return;
            }

            if (subtitleText == "字幕样式测试" || string.IsNullOrEmpty(subtitleText))
            {
                markerBox.Hide();
                currentDirection = Direction.None;
            }
            else
            {
                bool matched = false;

                foreach (var (direction, keywords) in directionKeywords)
                {
                    foreach (var keyword in keywords)
                    {
                        if (subtitleText.Contains(keyword))
                        {
                            matched = true;
                            await UpdateDirectionMarker(direction);
                            break;
                        }
                    }

                    if (matched)
                        break;
                }
            }
        }

        private async Task UpdateDirectionMarker(Direction direction)
        {
            try
            {
                logger.Trace("Updating direction marker to: " + direction);
                if (currentDirection == direction)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        markerBox.Hide();
                        await Task.Delay(100);
                        markerBox.Show();
                        await Task.Delay(100);
                    }
                }
                else
                {
                    markerBox.Show();
                    markerBox.Location = directionPoints[direction];
                    currentDirection = direction;
                }

            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to update direction marker");
            }
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.Exit();
            base.OnFormClosed(e);
        }
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
        internal enum Direction
        {
            None,
            East,
            West,
            South,
            North,
            WestNorth,
            WestSouth,
            EastNorth,
            EastSouth
        }
    }
}