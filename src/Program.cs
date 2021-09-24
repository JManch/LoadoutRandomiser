using System;
using System.Threading;

namespace LoadoutRandomiser
{
    class Program
    {
        private static DataLoader dataLoader;

        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("\n==== LoadoutRandomiser initializing ====\n");
            Thread.Sleep(1000);

            dataLoader = new DataLoader();
            dataLoader.LoadData();

            while(true){
                Console.Write("> ");
                var input = Console.ReadLine();
                string[] inputArgs = input.Split(' ');
                switch(inputArgs[0].ToLower()) {
                    case "help":
                        Console.Write("\n");
                        Console.WriteLine("generate [game]");
                        Console.WriteLine("reload");
                        Console.WriteLine("clear");
                        Console.WriteLine("exit");
                        Console.Write("\n");
                        break;
                    case "generate":
                        if(1 < inputArgs.Length && DataLoader.loadoutData.TryGetValue(inputArgs[1].ToLower(), out LoadoutData loadout)) {
                            Console.Write("\n");
                            loadout.GenerateRandomLoadout();
                            Console.Write("\n");
                        }
                        else {
                            Console.WriteLine("Error could not find game. Game must be loaded and provide as an argument e.g. generate BattlefieldV");
                        }
                        break;
                    case "reload":
                        dataLoader.LoadData();
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
