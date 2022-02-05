using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace LoadoutRandomiser 
{
    public static class DataLoader
    {
        private static string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "LoadoutData");
        public static Dictionary<string, LoadoutData> loadoutData = new Dictionary<string, LoadoutData>();
        public static Dictionary<string, string> loadoutNames = new Dictionary<string, string>();

        public static void LoadData() {
            loadoutData.Clear();

            Console.WriteLine("> Searching " + dataPath + " for loadouts");
            Thread.Sleep(1000);

            string[] dataPaths;
            try{
                dataPaths = Directory.GetFiles(dataPath);
            }
            catch {
                Console.WriteLine("> Error could not find LoadoutData folder. Ensure folder exists in the same path as the executable then type \"reload\"");
                return;
            }
            
            List<string> loadoutPaths = new List<string>();

            foreach (string dataPath in dataPaths) {
                if (Path.GetExtension(dataPath) == ".loadout") {
                    loadoutPaths.Add(dataPath);
                }
            }

            Console.WriteLine("> Found {0} loadouts", loadoutPaths.Count);

            Stopwatch sw = new Stopwatch();

            foreach (string loadoutPath in loadoutPaths) {
                string loadoutName = Path.GetFileNameWithoutExtension(loadoutPath);
                loadoutNames.Add(loadoutName.ToLower(), loadoutName);
                // Thread.Sleep(500);
                sw.Start();
                loadoutData.Add(loadoutName.ToLower(), Deserialize(loadoutPath));
                sw.Stop();
                Console.WriteLine("> " + loadoutName + " loaded. Found {0} categories and {1} options in {2} seconds.", loadoutData[loadoutName.ToLower()].categoryCount, loadoutData[loadoutName.ToLower()].optionCount, sw.Elapsed);
                sw.Reset();
                // Thread.Sleep(500);
            }
            Console.WriteLine("> Loading finished. Type \"help\" to get a list of commands");
        }

        private static LoadoutData Deserialize(string path) {

            LoadoutData loadoutData = new LoadoutData();
            
            Regex syntax = new Regex(@"[:{},]");
            char c;
            string buffer = "";

            using (StreamReader sr = new StreamReader(path)) {
                do{
                    c = (char)sr.Read();
                    
                    switch (c){
                        case ':':
                            loadoutData.AddCategory(buffer);
                            buffer = "";
                            continue;
                        case '}':
                            loadoutData.MoveUpNode();
                            buffer = "";
                            continue;
                        case '{':
                            loadoutData.MoveDownNode();
                            buffer = "";
                            continue;
                        case ',':
                            loadoutData.AddOption(buffer);
                            buffer = "";
                            continue;
                        case '*':
                            loadoutData.SetRandomCount(buffer);
                            buffer = "";
                            continue;
                    }

                    if(c == '\r' || c == '\n' || (buffer == "" && (c == ' ' || c == '\t'))) {
                        continue;
                    }
                    else {
                        buffer = buffer + c;
                    }
                }
                while(!sr.EndOfStream);
            }

            return loadoutData;
        }
        
        // This implementation is horrible
        private static int lev(string a, string b) {

            if (a.Length == 0) {
                return b.Length;
            }
            else if (b.Length == 0) {
                return a.Length;
            }
            else if (a[0] == b[0]){
                return lev(a.Substring(1), b.Substring(1));
            }

            return 1 + Math.Min(lev(a.Substring(1), b), Math.Min(lev(a, b.Substring(1)), lev(a.Substring(1), b.Substring(1))));
        }

        public static int levenshteinDistance(string a, string b){
            
            return lev(a, b);
        }
    }
}