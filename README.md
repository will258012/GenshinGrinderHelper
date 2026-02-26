# Genshin Grinder Helper（原神肝帝助手）

一个用于辅助《原神》地图探索与视频跟随操作的工具，旨在提升肝地图等任务执行时的操作效率。

## 功能特性

- 🎨 **方向提示** - 基于B站AI字幕的游戏内方向实时提示（需登录）
- 🎮 **视频控制快捷键** - 可自定义的快捷键绑定，支持全局热键监听
- 🌐 **WebView2 集成** - 内置浏览器环境，无需额外打开网页
- 🌓 **浅色/深色模式适配** - 启动时自动匹配系统主题（更改颜色模式需重启程序生效）

## 系统要求

- **操作系统**: Windows 7 SP1 x64 或更新版本（即原神所支持的系统版本）
- **运行环境**:
  - [.NET 10](https://dotnet.microsoft.com/zh-cn/download/dotnet/10.0) （Win10及以上系统）/ [.NET Framework 4.8](https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/thank-you/net48-web-installer) （Win7-Win10）
  - [Microsoft Edge WebView2](https://go.microsoft.com/fwlink/p/?LinkId=2124703)

## 注意事项
### 程序不会做出任何修改游戏文件、读写游戏内存等任何危害游戏本体的行为。
### 程序不会配备自动拾取、自动剧情等鼠标宏功能。
- 需将原神调整为**无边框模式**以保证方向提示、视频小窗等的置顶显示。
- 右击地址栏可见更多设置（键位绑定、方向显示偏移等）。

## 配置文件

应用配置保存在 `GenshinGrinderHelper.Config.json`：

```json5
{
  "ShowOnTop": true,//控制应用置顶
  "LoadingIndicator": true,//控制引用加载网页等场景的加载条
   "IsController": false,//启用则为手柄模式，禁用则为键盘模式。影响方向提示
  "HotKeys": {
    "Enabled": true,//热键总开关
    "HotKeyEnabledInGenshinOnly": true,//启用时，热键仅在焦点位于原神窗口时抓取。否则将全局抓取
    "KeyBindings": {
    //以下设置需填入
    //https://learn.microsoft.com/dotnet/api/system.windows.forms.keys
    //所指定的阿拉伯数字。仅支持单个键绑定
    //也可通过程序内的“键位绑定工具”设置
      "PlayPause": 192,//播放/暂停，默认为波浪键
      "Rewind": 37,//快退，默认为方向左键
      "Forward": 39,//快进，默认为方向右键
      "NextPart": 190,//下一P，默认为句号键
      "PreviousPart": 188//上一P，默认为逗号键
    }
  }
}
```

配置文件会在首次运行时自动生成。


## 错误报告
本程序纯属为自己编写，功能等方面会有些许粗糙。
欢迎通过Issue反馈问题。记得同时附上当天的日志文件（位于 `logs`）。


