using System;
using System.IO;
using System.Diagnostics;

using System.Runtime.InteropServices;

using Spectre.Console;

namespace LoadoutRandomiser {
public static class Utilities {

    public static string ConfigDir {
        get {
            string path = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                path =
                    Path.Combine(Environment.GetFolderPath(
                                     Environment.SpecialFolder.ApplicationData),
                                 "LoadoutRandomiser");
            else if (Environment.OSVersion.Platform == PlatformID.Unix) {
                string xdgConfigPath =
                    Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (string.IsNullOrEmpty(xdgConfigPath))
                    xdgConfigPath =
                        Path.Combine(Environment.GetFolderPath(
                                         Environment.SpecialFolder.Personal),
                                     ".config");
                path = Path.Combine(xdgConfigPath, "LoadoutRandomiser");
            } else {
                throw new Exception("Unsupported platform");
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string DataDir {
        get {

            string path = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                path = Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "LoadoutRandomiser");
            else if (Environment.OSVersion.Platform == PlatformID.Unix) {
                string xdgDataPath =
                    Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(xdgDataPath))
                    xdgDataPath =
                        Path.Combine(Environment.GetFolderPath(
                                         Environment.SpecialFolder.Personal),
                                     ".local", "share");
                path = Path.Combine(xdgDataPath, "LoadoutRandomiser");
            } else {
                throw new Exception("Unsupported platform");
            }
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    public static void OpenPath(string path) {
        try {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                Process.Start("explorer.exe", path);
            } else if (Environment.OSVersion.Platform == PlatformID.Unix) {
                Process.Start("xdg-open", path);
            }
        } catch (Exception) {
            AnsiConsole.MarkupLine("[red]Failed to open file browser[/]");
        }
    }

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private static IntPtr hookId = IntPtr.Zero;

    public static void HookKeyboard()
    {
        AnsiConsole.WriteLine("Hooking keyboard");
        using (var process = Process.GetCurrentProcess())
        using (var module = process.MainModule)
        {
            var moduleHandle = GetModuleHandle(module.ModuleName);
            hookId = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProcCallback, moduleHandle, 0);
        }
        AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
        {
            Utilities.UnhookKeyboard();
        };
        System.Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
        {
            Utilities.UnhookKeyboard();
        };
    }

    private static void UnhookKeyboard() 
    {
        AnsiConsole.WriteLine("Unhooking keyboard");
        UnhookWindowsHookEx(hookId);
    }

    private static IntPtr LowLevelKeyboardProcCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
        {
            int vkCode = Marshal.ReadInt32(lParam); // Virtual Key Code
            char keyChar = Convert.ToChar(vkCode); // Character representation of the key

            // AnsiConsole.MarkupLine($"Key pressed: {keyChar} - Key code: {vkCode}");
        }

        return CallNextHookEx(hookId, nCode, wParam, lParam);
    }
}

public static class Logging {
    public static LogLevel LoggingLevel = LogLevel.None;

    public enum LogLevel {
        None = 0,
        Info = 1,
        Debug = 2,
        Trace = 3,
    }

    public static void Log(LogLevel level, string message,
                           params object[] args) {
        if (LoggingLevel >= level)
            AnsiConsole.MarkupLine(message, args);
    }
}
}
