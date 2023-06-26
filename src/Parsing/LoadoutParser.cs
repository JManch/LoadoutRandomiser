using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using Spectre.Console;

namespace LoadoutRandomiser.Parsing {
public static class LoadoutParser {
    private static Dictionary<string, Loadout> _loadouts = new();
    public static Loadout[] Loadouts {
        get {
            return _loadouts.Values.ToArray();
        }
    }
    private static int sleepTime = 100;

    public static void LoadLoadouts() {
        AnsiConsole.Status().Start("[#FFA500]Search for loadouts...[/]", ctx => {
            AnsiConsole.WriteLine();
            ctx.Spinner(Spinner.Known.Dots);
            ctx.SpinnerStyle(Style.Parse("#FFA500"));

            _loadouts.Clear();
            var dir = Utilities.ConfigDir;

            AnsiConsole.MarkupLine(
                "[dim]  Searching [italic white bold]{0}[/] for loadouts[/]",
                dir);
            Thread.Sleep(sleepTime);

            List<string> loadoutPaths = new();
            foreach (var filePath in Directory.GetFiles(dir)) {
                if (Path.GetExtension(filePath) == ".loadout")
                    loadoutPaths.Add(filePath);
            }

            AnsiConsole.MarkupLine(
                "[dim]  Found [white bold italic]{0}[/] loadout(s)[/]",
                loadoutPaths.Count);
            Thread.Sleep(sleepTime);

            Stopwatch sw = new();

            foreach (var loadoutPath in loadoutPaths) {
                string loadoutName =
                    Path.GetFileNameWithoutExtension(loadoutPath);
                ctx.Status(
                    String.Format("[#FFA500]Parsing {0}...[/]", loadoutName));

                Loadout loadout = null;

                try {
                    Thread.Sleep(sleepTime);
                    sw.Restart();
                    loadout = ParseLoadout(loadoutPath);
                    Database.LoadDatabase(loadout);
                    sw.Stop();
                    AnsiConsole.MarkupLine(
                        "[green]  Parsed [white bold italic]{0}[/] containing [white bold italic]{1}[/] categories and [white bold italic]{2}[/] options in [white bold italic]{3}[/] seconds[/]",
                        loadoutName, loadout.CategoryCount, loadout.OptionCount,
                        sw.Elapsed.TotalSeconds);
                } catch (ParseException e) {
                    // AnsiConsole.WriteException(e);
                    // if (e.CurrentNode != null && e.NewNode != null) {
                    //     AnsiConsole.WriteLine(
                    //         "Current node: {0}",
                    //         e.CurrentNode?.ToString());
                    //     AnsiConsole.WriteLine("New node: {0}",
                    //                           e.NewNode?.ToString());
                    // }
                    // AnsiConsole.MarkupLine(e.Message);
                    AnsiConsole.MarkupLine("[red]  Failed to parse [white bold italic]{0}[/] - line:{1} {2}[/]", loadoutName, e.LineNumber, e.Message);
                    loadout = new Loadout(loadoutName);
                    continue;
                } catch (Exception e) {
                    AnsiConsole.WriteException(e);
                    System.Environment.Exit(1);
                } finally {
                    _loadouts.Add(loadoutName.ToLower(), loadout);
                }
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Rule(String.Format(
                             "[green]Successfully parsed {0}/{1} loadouts![/]",
                             _loadouts.Values.Count, loadoutPaths.Count,
                             _loadouts.Values.Count))
                    .Centered());
        });
        AnsiConsole.WriteLine();
    }

    public static bool TryGetLoadout(string loadoutName, out Loadout loadout) {
        if (_loadouts.TryGetValue(loadoutName, out loadout))
            return true;
        else {
            AnsiConsole.MarkupLine("[red]Loadout {0} does not exist[/]", loadoutName);
            return false;
        }
    }

    private enum WriteMode {
        Buffer,
        Modifier,
    }

    public static Loadout ParseLoadout(string loadoutPath) {
        Loadout loadout =
            new Loadout(Path.GetFileNameWithoutExtension(loadoutPath));

        var writeMode = WriteMode.Buffer;
        var buffer = new StringBuilder();
        var modifier = new StringBuilder();
        bool escape = false;

        int lineNumber = 1;

        using (StreamReader sr = new StreamReader(loadoutPath)) {
            do {

                char c = (char)sr.Read();

                switch (c) {
                case '\\':
                    escape = true;
                    continue;
                case '{':
                    loadout.MoveDown();
                    continue;
                case '}':
                    loadout.MoveUp();
                    continue;
                case '(':
                    if (escape)
                        break;
                    writeMode = WriteMode.Modifier;
                    continue;
                case ')':
                    if (escape)
                        break;
                    writeMode = WriteMode.Buffer;
                    continue;
                case '/':
                    if (!escape)
                        throw new ParseException("unescaped '/' character detected", lineNumber);
                    break;
                case ':':
                    loadout.AddCategory(buffer.ToString().Trim(),
                                        modifier.ToString().Trim());
                    buffer.Clear();
                    modifier.Clear();
                    continue;
                case ',':
                    loadout.AddOption(buffer.ToString().Trim());
                    buffer.Clear();
                    modifier.Clear();
                    continue;
                case '\n':
                    lineNumber++;
                    break;
                }

                escape = false;

                if (c == '\n' || c == '\r' || c == '\t')
                    continue;

                if (writeMode == WriteMode.Buffer)
                    buffer.Append(c);
                else if (writeMode == WriteMode.Modifier)
                    modifier.Append(c);

            } while (!sr.EndOfStream);
        }
        loadout.Parsed = true;
        return loadout;
    }
}
}
