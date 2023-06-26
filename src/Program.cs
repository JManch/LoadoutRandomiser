using System;


namespace LoadoutRandomiser {
public static class Program {
    public static Random random = new Random();

    static void Main(string[] args) {
        Console.Start();
        //
        //
        //
        // Console.Clear();
        // Console.WriteLine("Loading randomiser initializing...");
        // LoadoutParser.LoadLoadouts();
        // while (true)
        // {
        //     Console.Write("> ");
        //     var input = Console.ReadLine();
        //     string[] inputArgs = input.Split(' ');
        //     switch (inputArgs[0].ToLower())
        //     {
        //         case "help":
        //             Console.Write("\n");
        //             Console.WriteLine("generate [loadout]");
        //             Console.WriteLine("loadouts");
        //             Console.WriteLine("reload");
        //             Console.WriteLine("folder");
        //             Console.WriteLine("clear");
        //             Console.WriteLine("exit");
        //             Console.Write("\n");
        //             break;
        //         case "generate":
        //             if (
        //                 inputArgs.Length > 1
        //                 && LoadoutParser.loadouts.TryGetValue(
        //                     inputArgs[1].ToLower(),
        //                     out Loadout loadout
        //                 )
        //             )
        //             {
        //                 Console.Write("\n");
        //                 loadout.Generate();
        //                 Console.Write("\n");
        //             }
        //             else
        //             {
        //                 Console.WriteLine(
        //                     "Error could not find loadout with name {0}",
        //                     inputArgs.Length > 1 ? inputArgs[1] : ""
        //                 );
        //             }
        //             break;
        //         case "loadouts":
        //             // foreach (var l in LoadoutParser.loadoutNames.Values)
        //             //     Console.WriteLine(l);
        //             Console.WriteLine("Not implemented");
        //             break;
        //         case "reload":
        //             LoadoutParser.LoadLoadouts();
        //             // DataLoader.LoadData();
        //             break;
        //         case "folder":
        //             Process.Start(
        //                 "explorer.exe",
        //                 Path.Combine(Directory.GetCurrentDirectory(),
        //                 "LoadoutData")
        //             );
        //             break;
        //         case "exit":
        //             System.Environment.Exit(1);
        //             break;
        //         case "clear":
        //             Console.Clear();
        //             break;
        //         default:
        //             Console.WriteLine("> Unknown command. Type \"help\" for
        //             list of commands"); break;
        //     }
        // }
    }
}
}
