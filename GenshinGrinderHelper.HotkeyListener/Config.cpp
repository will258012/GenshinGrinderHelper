#include <fstream>
#include <set>
#include "json.hpp" 
#include <windows.h>

using namespace std;
using json = nlohmann::json;
    class ConfigManager {
    private:
        bool configLoaded = false;
        set<int> monitoredKeys;
    public:
        bool LoadConfig() 
        {
            try {
                std::ifstream file("GenshinGrinderHelper.Config.json");
                if (!file.is_open()) {
                    throw runtime_error("配置文件不存在或无法打开");
                }

                json config;
                file >> config;
                file.close();

                // 检查必要的配置项是否存在
                if (!config.contains("HotKeys")) {
                    throw runtime_error("配置文件中缺少 HotKeys 部分");
                }

                auto& hotkeys = config["HotKeys"];

                if (!hotkeys.contains("KeyBindings")) {
                    throw runtime_error("配置文件中缺少 KeyBindings 部分");
                }

                auto& keybindings = hotkeys["KeyBindings"];

                if (!keybindings.is_object() && !keybindings.is_array()) {
                    throw runtime_error("KeyBindings 格式错误");
                }


                monitoredKeys.clear();

                for (auto& [_, vkCode] : keybindings.items()) {
                    if (vkCode.is_number()) {
                        int key = vkCode.get<int>();
                        monitoredKeys.insert(key);
					}
					else throw runtime_error("KeyBindings 中的热键值格式错误");
                }

				if (monitoredKeys.empty()) {
                    throw runtime_error("配置文件中缺少 KeyBindings 部分");
                }

                configLoaded = true;
                return true;
            }
            catch (exception &e) {
                string errorMsg = format("加载热键配置失败，热键无法正常工作！\n错误信息: {}", e.what());
                MessageBoxA(NULL, errorMsg.c_str(), "错误", MB_OK | MB_ICONERROR | MB_TOPMOST);
                return false;
			}
            catch (...) {
                MessageBoxA(NULL,
                    "加载热键配置失败，热键无法正常工作！\n\n未知异常",
                    "错误",
                    MB_OK | MB_ICONERROR | MB_TOPMOST);
                return false;
            }
        }

        bool ShouldMonitorKey(int vkCode) const {
            return configLoaded && monitoredKeys.contains(vkCode);
        }

        const set<int>& GetMonitoredKeys() const {
            return monitoredKeys;
        }
    };