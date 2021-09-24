using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace LoadoutRandomiser 
{
    public class DataLoader
    {
        private string dataPath;
        public static Dictionary<string, LoadoutData> loadoutData = new Dictionary<string, LoadoutData>();

        public DataLoader() {
            this.dataPath = Directory.GetCurrentDirectory() + @"\LoadoutData";
        }

        public void LoadData() {
            
            loadoutData.Clear();

            Console.WriteLine("> Searching " + dataPath + " for loadouts");

            string[] dataPaths = Directory.GetFiles(dataPath);
            List<string> loadoutPaths = new List<string>();

            foreach (string dataPath in dataPaths) {
                if (Path.GetExtension(dataPath) == ".loadout") {
                    loadoutPaths.Add(dataPath);
                }
            }

            Thread.Sleep(1000);
            Console.WriteLine("> Found {0} loadouts", loadoutPaths.Count);

            Stopwatch sw = new Stopwatch();

            foreach (string loadoutPath in loadoutPaths) {
                string loadoutName = Path.GetFileNameWithoutExtension(loadoutPath);
                Thread.Sleep(500);
                sw.Start();
                loadoutData.Add(loadoutName.ToLower(), Deserialize(loadoutPath));
                sw.Stop();
                Console.WriteLine("> " + loadoutName + " loaded. Found {0} categories and {1} options in {2} seconds.", loadoutData[loadoutName.ToLower()].categoryCount, loadoutData[loadoutName.ToLower()].optionCount, sw.Elapsed);
                sw.Reset();
                Thread.Sleep(500);
            }
            Console.WriteLine("> Loading finished. Type \"help\" to get a list of commands");
        }

        public void PrintData() {
            foreach (var pair in loadoutData) {
                foreach (Node n in pair.Value.nodes) {
                    if(n is Category) {
                        Console.WriteLine("Found category " + n.name + " with options:");
                        foreach (Option o in ((Category)n).options) {
                            Console.WriteLine("    " + o.name);
                        }
                    }
                }
            } 
        }

        private LoadoutData Deserialize(string path) {

            StreamReader sr = new StreamReader(path);

            LoadoutData loadoutData = new LoadoutData();
            
            //Regex alphabet = new Regex(@"[a-zA-Z 0-9]");
            Regex syntax = new Regex(@"[:{},]");
            char c;
            string buffer = "";

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
                }

                if(c == '\r' || c == '\n' || (buffer == "" && c == ' ')) {
                    continue;
                }
                else {
                    buffer = buffer + c;
                }
            }
            while(!sr.EndOfStream);

            return loadoutData;
        }

        private void OnDeserializeError(string error) {
            Console.WriteLine("Error deserializing: " + error);
        }
    }
}