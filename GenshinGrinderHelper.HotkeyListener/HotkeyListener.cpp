#include <iostream>
#include <windows.h>
#include <tlhelp32.h>
#include <thread>
#include "Config.cpp"
#define MAIN_PROCESS_NAME L"GenshinGrinderHelper.exe"

atomic_bool paused = false;
atomic_bool running = true;
HANDLE hPipe = INVALID_HANDLE_VALUE;
ConfigManager configManager;

static bool ConnectPipe()
{
    if (hPipe != INVALID_HANDLE_VALUE)
        return true;

    hPipe = CreateFile(
        L"\\\\.\\pipe\\GenshinGrinderHelper.HotKeyListener",
        GENERIC_READ | GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        FILE_FLAG_OVERLAPPED, // 可选
        NULL);

    return hPipe != INVALID_HANDLE_VALUE;
}

static bool SendToPipe(int vkCode)
{
    if (hPipe == INVALID_HANDLE_VALUE)
        return false;
    if (paused.load(memory_order_relaxed))
        return true;

    char buffer[16];
    DWORD written;

    sprintf_s(buffer, "%d", vkCode);
    return WriteFile(hPipe, buffer, strlen(buffer), &written, NULL);
}

static bool IsProcessExist(const wchar_t* processName) {
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) return false;

    PROCESSENTRY32 pe32{};
    pe32.dwSize = sizeof(PROCESSENTRY32);

    bool found = false;
    if (Process32First(hSnapshot, &pe32)) {
        do {
            if (_wcsicmp(pe32.szExeFile, processName) == 0) {
                found = true;
                break;
            }
        } while (Process32Next(hSnapshot, &pe32));
    }

    CloseHandle(hSnapshot);
    return found;
}

void CommandThread()
{
    char buffer[64];
    DWORD bytesRead;
    short processCheckCounter = 0;

    while (running)
    {
        if (hPipe == INVALID_HANDLE_VALUE)
        {
            running = false;
            break;
        }

        if (processCheckCounter++ >= 20)
        {
            processCheckCounter = 0;
            if (!IsProcessExist(MAIN_PROCESS_NAME))
            {
                running = false;
                break;
            }
        }

        if (!ReadFile(hPipe, buffer, sizeof(buffer) - 1, &bytesRead, NULL))
        {
            Sleep(50);
            continue;
        }

        buffer[bytesRead] = '\0';

        if (strcmp(buffer, "PAUSE") == 0)
        {
            paused.store(true, std::memory_order_relaxed);
        }
        else if (strcmp(buffer, "RESUME") == 0)
        {
            paused.store(false, std::memory_order_relaxed);
        }
        else if (strcmp(buffer, "RELOADCONFIG") == 0)
        {
            configManager.LoadConfig();
        }

        Sleep(50);
    }
}

int main() {
    bool keyStates[256] = { false };

    HANDLE hMutex = CreateMutex(NULL, TRUE, L"GenshinGrinderHelper.HotKeyListener");

    if (GetLastError() == ERROR_ALREADY_EXISTS) {

        if (hMutex) {
            CloseHandle(hMutex);
        }
        return 0;
    }

    if (hMutex == NULL)
        return 0;

    if(!configManager.LoadConfig()) {
        if (hMutex) {
            ReleaseMutex(hMutex);
            CloseHandle(hMutex);
        }
        return 1;
    }
    
    if (!ConnectPipe()) {
        return 1;
    }

    thread cmdThread(CommandThread);

    while (running)
    {
        if (paused.load(memory_order_relaxed))
        {
            Sleep(50);
            continue;
        }

        for (int vk : configManager.GetMonitoredKeys())
        {
            if (vk <= 0) continue;

            if (GetAsyncKeyState(vk) & 0x8000)
            {
                if (!keyStates[vk])
                {
                    if (!SendToPipe(vk)) {
                        running = false;
						break;
                    }
                    keyStates[vk] = true;
                }
            }
            else
            {
                keyStates[vk] = false;
            }
        }

        Sleep(25);
    }

    running = false;

    if (cmdThread.joinable())
        cmdThread.join();

    if (hMutex) {
        ReleaseMutex(hMutex);
        CloseHandle(hMutex);
    }

    return 0;
}