using System;
using System.IO;
using System.Threading;
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
            Console.WriteLine("\n==== LoadoutRandomiser initializing ====\n");
            Thread.Sleep(1000);

            //dataLoader = new DataLoader();
            DataLoader.LoadData();

            while(true){
                Console.Write("> ");
                var input = Console.ReadLine();
                string[] inputArgs = input.Split(' ');
                switch(inputArgs[0].ToLower()) {
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
                        
                        // string requestedLoadout = inputArgs[1].ToLower();

                        // if(inputArgs.Length > 1 && DataLoader.loadoutData.TryGetValue(requestedLoadout, out LoadoutData loadoutData)) {
                        //     Console.Write("\n");
                        //     loadoutData.GenerateRandomLoadout();
                        //     Console.Write("\n");
                        // }
                        // else {
                        //     Console.WriteLine("Could not find a loadout with a matching name. Making a guess...");
                        //     int lowestLev = int.MaxValue;
                        //     string bestMatch = "";
                        //     foreach (var loadout in DataLoader.loadoutData.Keys) {
                                
                        //         //int levDistance = DataLoader.levenshteinDistance(loadout.Substring(0, Math.Min(requestedLoadout.Length, loadout.Length)), requestedLoadout.Substring(0, Math.Min(loadout.Length, requestedLoadout.Length)));
                        //         int levDistance = DataLoader.levenshteinDistance(loadout, requestedLoadout);
                        //         if (levDistance < lowestLev){
                        //             lowestLev = levDistance;
                        //             bestMatch = loadout;
                        //         }
                        //     }

                        //     Console.WriteLine("Lowest lev was {0}", lowestLev);
                        //     if (lowestLev / requestedLoadout.Length > 0.8) {
                        //         Console.WriteLine("Failed to make a guess.");
                        //     }
                        //     Console.WriteLine("Guessed {0}", DataLoader.loadoutNames[bestMatch]);
                        //     Console.Write("\n");
                        //     DataLoader.loadoutData[bestMatch].GenerateRandomLoadout();
                        //     Console.Write("\n");
                        // }

                        if(inputArgs.Length > 1 && DataLoader.loadoutData.TryGetValue(inputArgs[1].ToLower(), out LoadoutData loadout)) {
                            Console.Write("\n");
                            loadout.GenerateRandomLoadout();
                            Console.Write("\n");
                        }
                        else {
                            Console.WriteLine("Error could not find loadout with name {0}", inputArgs.Length > 1 ? inputArgs[1] : "");
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
                        Process.Start("explorer.exe", Path.Combine(Directory.GetCurrentDirectory(), "LoadoutData"));
                        break;
                    case "exit":
                        System.Environment.Exit(1);
                        break;
                    case "clear":
                        Console.Clear();
                        break;
                    default :
                        Console.WriteLine("> Unknown command. Type \"help\" for list of commands");
                        break;
                }
            }
        }
    }
}
