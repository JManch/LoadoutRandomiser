using System;

using Spectre.Console;

using LoadoutRandomiser.Parsing;

namespace LoadoutRandomiser {
public static class Console {

    private static Loadout LastRandomisedLoadout;

    public static void Start() {
        AnsiConsole.Console.Clear();
        AnsiConsole.Write(
            new FigletText("Loadout Randomiser")
                .Centered()
                .Color(Color.FromConsoleColor(ConsoleColor.Blue)));
        AnsiConsole.Write(new Rule("[green]Welcome[/]").Centered());

        LoadoutParser.LoadLoadouts();

        while (true) {
            var input = AnsiConsole.Ask<string>("[white bold] >[/]");
            var inputArgs = input.Split(' ');
            if (inputArgs.Length > 2) {
                AnsiConsole.MarkupLine("[red]Too many args supplied[/]");
                continue;
            }
            var command = inputArgs[0].ToLower();

            switch (command) {
            case "help":
                break;
            case "generate":
                GenerateLoadout(inputArgs[1]);
                break;
            case "reload":
                LoadoutParser.LoadLoadouts();
                break;
            case "exit":
            case "quit":
                System.Environment.Exit(0);
                break;
            case "loadoutfolder":
                Utilities.OpenPath(Utilities.ConfigDir);
                break;
            case "datafolder":
                Utilities.OpenPath(Utilities.DataDir);
                break;
            case "loadouts":
                ListLoadouts();
                break;
            case "modify":
                AnsiConsole.WriteLine();
                AnsiConsole.Write(new Rule("[green]Entering modification mode[/]").Centered());
                Database.ModifyLoadout(inputArgs[1]);
                break;
            }
        }
    }

    public static void GenerateLoadout(string loadoutName, bool last = false) {
        Loadout loadout;
        if (LoadoutParser.TryGetLoadout(loadoutName, out loadout)) {
            loadout.Randomise();
        }
    }

    public static void PrintHelp() { return; }

    public static void ListLoadouts() {
        var table = new Table().Centered().RoundedBorder();
        table.AddColumn(new TableColumn("[bold]Loadout[/]"));
        table.AddColumn(new TableColumn("[bold]Parsed Successfully[/]"));
        table.AddColumn(new TableColumn("[bold]Categories[/]"));
        table.AddColumn(new TableColumn("[bold]Options[/]"));

        foreach (var loadout in LoadoutParser.Loadouts) {
            table.AddRow(loadout.Name, loadout.Parsed.ToString(),
                         loadout.CategoryCount.ToString(),
                         loadout.OptionCount.ToString());
        }

        AnsiConsole.Write(table);
    }
}
}
