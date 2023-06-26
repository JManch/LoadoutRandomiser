using System;
using System.IO;
using System.Diagnostics;

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
                    "LoadoutRandom");
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
