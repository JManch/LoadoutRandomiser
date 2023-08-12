using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using LoadoutRandomiser.Parsing;
using OptionKeyAction = System.Func<
    Spectre.Console.ListPromptState<LoadoutRandomiser.Database.SelectionOption>,
    Spectre.Console.ListPromptInputResult>;
using CategoryKeyAction =
    System.Func<Spectre.Console.ListPromptState<
                    LoadoutRandomiser.Database.SelectionCategory>,
                Spectre.Console.ListPromptInputResult>;

using Spectre.Console;

namespace LoadoutRandomiser {
public static class Database {
    private static Dictionary<int, Option> EnabledOptions = new();
    private static Dictionary<int, Option> DisabledOptions = new();

    public struct SelectionCategory {
        public string Name { get; set; }
        public Category Category { get; init; }

        public static string ToString(SelectionCategory category) {
            return category.Name;
        }
    }

    public struct SelectionOption {
        public Option Option { get; init; }
        public string Name { get; set; }

        public static string ToString(SelectionOption option) {
            return option.Name;
        }
    }

    private static void DisableOption(Loadout loadout, Option option) {
        if (loadout.DisabledNodes.Contains(option.Id)) {
            if (EnabledOptions.ContainsKey(option.Id))
                EnabledOptions.Remove(option.Id);
        } else {
            if (!DisabledOptions.ContainsKey(option.Id))
                DisabledOptions.Add(option.Id, option);
        }
    }

    private static void DisableOptionRecursive(Loadout loadout, Option option) {
        DisableOption(loadout, option);
        foreach (var category in option.Children)
            foreach (var child in category.Children)
                DisableOptionRecursive(loadout, child as Option);
    }

    private static void EnableOption(Loadout loadout, Option option) {
        if (!loadout.DisabledNodes.Contains(option.Id)) {
            if (DisabledOptions.ContainsKey(option.Id))
                DisabledOptions.Remove(option.Id);
        } else {
            if (!EnabledOptions.ContainsKey(option.Id))
                EnabledOptions.Add(option.Id, option);
        }
    }

    private static void EnableOptionRecursive(Loadout loadout, Option option) {
        EnableOptionRecursive(loadout, option);
        foreach (var category in option.Children)
            foreach (var child in category.Children)
                EnableOptionRecursive(loadout, child as Option);
    }

    private static SelectionCategory
    BrowseCategories(Loadout loadout, SelectionCategory[] categories,
                     out bool quit) {
        bool stopBrowsing = false;

        CategoryKeyAction quitCategoriesAction = (state) => {
            stopBrowsing = true;
            return ListPromptInputResult.Submit;
        };

        CategoryKeyAction enableAllCategories = (state) => {
            foreach (var category in categories)
                foreach (Option option in category.Category.Children)
                    EnableOptionRecursive(loadout, option);
            return ListPromptInputResult.Refresh;
        };

        CategoryKeyAction disableAllCategories = (state) => {
            foreach (var category in categories)
                foreach (Option option in category.Category.Children)
                    DisableOptionRecursive(loadout, option);
            return ListPromptInputResult.Refresh;
        };

        // Browse categories
        var category = AnsiConsole.Prompt(
            new SelectionPrompt<SelectionCategory>()
                .UseConverter(SelectionCategory.ToString)
                .Title(string.Format(
                    "{0}\n\n{1}\n{2}",
                    "  Select a [white bold]category[/] to modify",
                    "[dim]  Use [white bold]ARROW KEYS[/] to scroll and press [white bold]ENTER[/] to select[/]",
                    "[dim]  Use [green bold]E[/] to enable [white bold]ALL[/] options or [red bold]D[/] to disable [white bold]ALL[/] options[/]"))
                .PageSize(15)
                .AddKeyActions(new[] {
                    Tuple.Create(ConsoleKey.Escape, quitCategoriesAction),
                    Tuple.Create(ConsoleKey.E, enableAllCategories),
                    Tuple.Create(ConsoleKey.D, disableAllCategories),
                })
                .AddChoices(categories)
                .HighlightStyle(null)
                .MoreChoicesText(""));

        quit = stopBrowsing;
        return category;
    }

    private static void BrowseOptions(Loadout loadout, Category category) {
        var options = loadout.GetOptionsAtCategory(category);

        // Check if all the options are disabled from a parent option
        string disabledParent = string.Empty;
        var parent = category.Parent;
        while (parent != null) {
            if ((loadout.DisabledNodes.Contains(parent.Id) ||
                 DisabledOptions.ContainsKey(parent.Id)) &&
                !EnabledOptions.ContainsKey(parent.Id)) {
                disabledParent = string.Format(
                    "[white]{0} [blue bold]>[/] [italic bold]{1}[/][/]",
                    parent.Parent.Name, parent.Name);
                break;
            }
            parent = parent.Parent;
        }

        // Set option colours
        for (int i = 0; i < options.Length; i++) {
            var nodeID = options[i].Option.Id;
            if (disabledParent != string.Empty ||
                DisabledOptions.ContainsKey(nodeID))
                options[i].Name = string.Format("[red]{0}[/]", options[i].Name);
            else if (EnabledOptions.ContainsKey(nodeID))
                options[i].Name =
                    string.Format("[green]{0}[/]", options[i].Name);
            else if (loadout.DisabledNodes.Contains(nodeID))
                options[i].Name = string.Format("[red]{0}[/]", options[i].Name);
            else
                options[i].Name =
                    string.Format("[green]{0}[/]", options[i].Name);
        }

        // Set prompt title
        var title = string.Format("  Modifying {0} options", category.Name);
        if (disabledParent != string.Empty)
            title = string.Format(
                "{0}\n\n  [red bold]WARNING[/] parent option {1} is disabled so options cannot be enabled",
                title, disabledParent);
        else
            title = string.Format(
                "{0}\n\n  {1}\n  {2}", title,
                "[dim]Use [white bold]RIGHT ARROW[/] to [green bold]enable[/] and [white bold]LEFT ARROW[/] to [red bold]disable[/][/]",
                "[dim]Use [green bold]E[/] to enable [white bold]ALL[/] options or [red bold]D[/] to disable [white bold]ALL[/] options[/]");

        OptionKeyAction quitOptionsAction =
            (state) => { return ListPromptInputResult.Submit; };

        OptionKeyAction enableOptionAction = (state) => {
            if (disabledParent != string.Empty)
                return ListPromptInputResult.None;

            // Set text color to green
            var option = state.Current.Data;
            option.Name = "[green]" + Markup.Remove(option.Name) + "[/]";
            state.Current.Data = option;

            // Update database
            EnableOption(loadout, option.Option);
            return ListPromptInputResult.Refresh;
        };

        OptionKeyAction disableOptionAction = (state) => {
            if (disabledParent != string.Empty)
                return ListPromptInputResult.None;

            // Set text color to red
            var option = state.Current.Data;
            option.Name = "[red]" + Markup.Remove(option.Name) + "[/]";
            state.Current.Data = option;

            // Update database
            DisableOption(loadout, option.Option);
            return ListPromptInputResult.Refresh;
        };

        OptionKeyAction disableAllOptionsAction = (state) => {
            if (disabledParent != string.Empty)
                return ListPromptInputResult.None;

            for (int i = 0; i < state.Items.Count; i++) {
                var option = state.Items[i].Data;
                option.Name = "[red]" + Markup.Remove(option.Name) + "[/]";
                state.Items[i].Data = option;

                // Update database
                DisableOption(loadout, option.Option);
            }
            return ListPromptInputResult.Refresh;
        };

        OptionKeyAction enableAllOptionsAction = (state) => {
            if (disabledParent != string.Empty)
                return ListPromptInputResult.None;

            for (int i = 0; i < state.Items.Count; i++) {
                var option = state.Items[i].Data;
                option.Name = "[green]" + Markup.Remove(option.Name) + "[/]";
                state.Items[i].Data = option;

                // Update database
                EnableOption(loadout, option.Option);
            }
            return ListPromptInputResult.Refresh;
        };

        AnsiConsole.Prompt(
            new SelectionPrompt<SelectionOption>()
                .UseConverter(SelectionOption.ToString)
                .Title(title)
                .PageSize(15)
                .HighlightStyle(Style.Plain)
                .AddKeyActions(new[] {
                    Tuple.Create(ConsoleKey.RightArrow, enableOptionAction),
                    Tuple.Create(ConsoleKey.LeftArrow, disableOptionAction),
                    Tuple.Create(ConsoleKey.Escape, quitOptionsAction),
                    Tuple.Create(ConsoleKey.E, enableAllOptionsAction),
                    Tuple.Create(ConsoleKey.D, disableAllOptionsAction),
                })
                .AddChoices(options)
                .MoreChoicesText(""));
    }

    public static void ModifyLoadout(string loadoutName) {
        Loadout loadout;
        if (!LoadoutParser.TryGetLoadout(loadoutName, out loadout))
            return;

        DisabledOptions.Clear();
        EnabledOptions.Clear();

        AnsiConsole.WriteLine();
        var categories = loadout.GetCategories();

        while (true) {
            var category = BrowseCategories(loadout, categories, out bool quit);
            if (quit)
                break;

            BrowseOptions(loadout, category.Category);
        }

        if (EnabledOptions.Count + DisabledOptions.Count > 0) {
            var table = new Table().Centered().RoundedBorder();
            table.AddColumn(new TableColumn("[white bold]Modified Option[/]"));
            table.AddColumn(new TableColumn("[white bold]New Value[/]"));
            foreach (var option in EnabledOptions.Values)
                table.AddRow(option.PathName, "[green]enabled[/]");
            foreach (var option in DisabledOptions.Values)
                table.AddRow(option.PathName, "[red]disabled[/]");
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();

            var confirmation = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Do you want to save these modifications?")
                    .AddChoices(new[] { "[green]yes[/]", "[red]no[/]" }));

            if (Markup.Remove(confirmation) == "yes") {
                UpdateDatabase(loadout, DisabledOptions.Values.ToArray(),
                               EnabledOptions.Values.ToArray());
                AnsiConsole.Write(
                    new Rule("[green]Saved modifications[/]").Centered());
            } else
                AnsiConsole.Write(
                    new Rule("[red]Discarded modifications[/]").Centered());
        } else {
            AnsiConsole.MarkupLine(
                "  [dim]No changes were made to loadout [white bold]{0}[/][/]",
                loadout.Name);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Rule("[red]Exiting modification mode[/]").Centered());
        }
    }

    public static void LoadDatabase(Loadout loadout) {
        Logging.Log(Logging.LogLevel.Debug, "Loading database for loadout {0}",
                    loadout.Name);
        loadout.DisabledNodes.Clear();
        string path =
            Path.Combine(Utilities.DataDir, loadout.Name + ".database");

        if (!File.Exists(path)) {
            return;
        }

        using (StreamReader sr = new(path)) {
            while (!sr.EndOfStream) {
                string line = sr.ReadLine();

                // Parse the line
                Logging.Log(Logging.LogLevel.Debug, "Parsing line {0}", line);

                // Get the depth using the number of un-escape '/' characters
                int depth = 1;
                for (int i = 0; i < line.Length; i++)
                    if (line[i] == '/' && (i == 0 || line[i - 1] != '\\'))
                        depth++;

                Logging.Log(Logging.LogLevel.Debug, "Depth is {0}", depth);

                // Find the corresponsing node in the loadout
                foreach (var node in loadout.Tree.GetNodesAtDepth(depth)) {
                    Logging.Log(Logging.LogLevel.Debug,
                                "Node {0} is at deptch {1}", node.PathName,
                                depth);
                    if (node.PathName == line) {
                        loadout.DisabledNodes.Add(node.Id);
                        break;
                    }
                    // TODO: Don't break here, need to continue the search
                    // to see if there are multiple nodes with the same
                    // name and somehow resolve the naming conflict.
                }
            }
        }
    }

    private static void UpdateDatabase(Loadout loadout,
                                       Option[] disabledOptions,
                                       Option[] enabledOptions) {
        string path =
            Path.Combine(Utilities.DataDir, loadout.Name + ".database");

        if (disabledOptions.Length > 0) {
            using (StreamWriter sw = new(path)) {
                foreach (var option in disabledOptions) {
                    if (!loadout.DisabledNodes.Contains(option.Id)) {
                        sw.WriteLine(option.PathName);
                        loadout.DisabledNodes.Add(option.Id);
                    }
                }
            }
        }

        if (enabledOptions.Length > 0) {
            string tmpPath =
                Path.Combine(Utilities.DataDir, loadout.Name + ".tmp");
            List<string> pathNames = new();
            foreach (var option in enabledOptions) {
                if (loadout.DisabledNodes.Contains(option.Id)) {
                    pathNames.Add(option.PathName);
                    loadout.DisabledNodes.Remove(option.Id);
                }
            }
            using (StreamReader sr = new(path)) using (StreamWriter sw =
                                                           new(tmpPath)) {
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();
                    if (pathNames.Contains(line)) {
                        pathNames.Remove(line);
                        continue;
                    }
                    sw.WriteLine(line);
                }
            }
            File.Replace(tmpPath, path, null);
        }
    }
}
}
