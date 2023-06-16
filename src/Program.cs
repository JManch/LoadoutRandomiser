using System;
using System.IO;
using System.Diagnostics;

namespace LoadoutRandomiser
{
    public static class Program
    {
        public static Random random;

        static void Main(string[] args)
        {
            random = new Random();
            Console.Clear();
            Console.WriteLine("Loading randomiser initializing...");
            DataLoader.LoadData();
            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                string[] inputArgs = input.Split(' ');
                switch (inputArgs[0].ToLower())
                {
                    case "help":
                        Console.Write("\n");
                        Console.WriteLine("generate [loadout]");
                        Console.WriteLine("loadouts");
                        Console.WriteLine("reload");
                        Console.WriteLine("folder");
                        Console.WriteLine("clear");
                        Console.WriteLine("exit");
                        Console.Write("\n");
                        break;
                    case "generate":
                        if (
                            inputArgs.Length > 1
                            && DataLoader.loadoutData.TryGetValue(
                                inputArgs[1].ToLower(),
                                out LoadoutData loadout
                            )
                        )
                        {
                            Console.Write("\n");
                            loadout.GenerateRandomLoadout();
                            Console.Write("\n");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Error could not find loadout with name {0}",
                                inputArgs.Length > 1 ? inputArgs[1] : ""
                            );
                        }
                        break;
                    case "loadouts":
                        foreach (var l in DataLoader.loadoutNames.Values)
                            Console.WriteLine(l);
                        break;
                    case "reload":
                        DataLoader.LoadData();
                        break;
                    case "folder":
                        Process.Start(
                            "explorer.exe",
                            Path.Combine(Directory.GetCurrentDirectory(), "LoadoutData")
                        );
                        break;
                    case "exit":
                        System.Environment.Exit(1);
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    default:
                        Console.WriteLine("> Unknown command. Type \"help\" for list of commands");
                        break;
                }
            }
        }
    }
}
