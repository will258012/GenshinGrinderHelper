using System.Text.Json;
using System.Text.Json.Serialization;
using static GenshinGrinderHelper.Managers.HotkeyManager;

namespace GenshinGrinderHelper
{
    public class Config
    {
        #region 静态
        private const string ConfigFilePath = "GenshinGrinderHelper.Config.json";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        private static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
        };
        public static Config Instance { get; } = LoadConfig();
        public static Config LoadConfig()
        {
            logger.Info("Loading config");

            if (!File.Exists(ConfigFilePath))
            {
                var defaultConfig = new Config();
                defaultConfig.SaveConfig();
                return defaultConfig;
            }

            try
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<Config>(json, jsonSerializerOptions);
                return config ?? throw new Exception("配置文件内容为空");
            }
            catch (JsonException e)
            {
                // 如果配置文件损坏，创建新的默认配置
                MessageBox.Show($"配置文件格式错误，将使用默认配置。错误信息: {e}", "配置文件错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                var defaultConfig = new Config();
                defaultConfig.SaveConfig();
                return defaultConfig;
            }
            catch (Exception e)
            {
                MessageBox.Show($"读取配置文件时发生错误: {e}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new Config();
            }
        }

        public void SaveConfig()
        {
            logger.Info("Saving config");
            try
            {
                var json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception e)
            {
                MessageBox.Show($"保存配置文件时发生错误: {e}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region 设置实例

        public bool ShowOnTop { get; set; } = true;
        public bool LoadingIndicator { get; set; } = true;
        public bool IsController { get; set; } = false;
        public HotKeySettings HotKeys { get; set; } = new();
        public class HotKeySettings
        {
            public bool Enabled { get; set; } = true;
            public bool HotKeyEnabledInGenshinOnly { get; set; } = true;
            public Dictionary<HotKeyActions, Keys> KeyBindings { get; set; } = new()
            {
                { HotKeyActions.PlayPause, Keys.Oemtilde },
                { HotKeyActions.Rewind, Keys.Left },
                { HotKeyActions.Forward, Keys.Right },
                { HotKeyActions.NextPart, Keys.Oemcomma },
                { HotKeyActions.PreviousPart, Keys.OemQuestion }
            };
            [JsonIgnore]
            internal List<(Keys, HotKeyActions)> ReversedHotkeys => KeyBindings.Select(kvp => (kvp.Value, kvp.Key)).ToList();

        }
        #endregion

    }

}