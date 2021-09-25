using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadoutRandomiser 
{
    public class LoadoutData {
        private List<Node> roots;
        private Node current;
        public int categoryCount {get; private set;}
        public int optionCount {get; private set;}
        private Node latestNode;

        public LoadoutData() {
            current = null;
            roots = new List<Node>();
        }

        public void AddCategory(string name) {
            
            Category newCategory;
            if (current != null && current is Option) {
                newCategory = new Category(current, name);
                current.AddChild(newCategory);
                latestNode = newCategory;
                ++categoryCount;
            }
            else if (current == null){
                newCategory = new Category(null, name);
                roots.Add(newCategory);
                latestNode = newCategory;
                ++categoryCount;
            }
            else {
                Console.WriteLine("> Failed to add category " + name + " as it's parent " + current.name + " is a category. Categories must be children of options.");
            }
        }

        public void AddOption(string name) {
            
            // Options can only be children of categories
            if (current != null && current is Category) {
                Option newOption = new Option(current, name);
                current.AddChild(newOption);
                latestNode = newOption;
                ++optionCount;
            }
            else if(current == null) {
                Console.WriteLine("> Warning the root element is an option whilst it should be a category. Loadout will not load correctly unless this is fixed.");
            }
            else {
                Console.WriteLine("> Failed to add option " + name + " as it's parent " + current.name + " is an option. Options must be children of categories.");
            }
        }

        public void MoveUpNode() {
            current = current.parent;
        }
        public void MoveDownNode() {
            if(current == null) {
                current = latestNode;
            }
            else {
                current = current.children[current.children.Count - 1];
            }
        }

        public void SetRandomCount(string randomCount) {
            if(Int32.TryParse(randomCount, out int rCount)) {
                if (latestNode is Category) {
                    ((Category)latestNode).SetRandomCount(rCount);
                }
                else {
                    Console.WriteLine("> Warning you attempted to define random count for option {0}. Random count can only be used on categories.", latestNode.name);
                }
            }
            else {
                Console.WriteLine("> Warning random count failed to set as {0} is not an integer.", randomCount);
            }
        }

        public void GenerateRandomLoadout() {

            List<Tuple<string, string>> generatedLoadout = new List<Tuple<string, string>>();
            
            foreach(Node root in roots) {
                generatedLoadout.AddRange(GenerateRandomLoadoutR(root, new List<Tuple<string, string>>(), new Random()));
            }   
            
            foreach (var t in generatedLoadout) {
                Console.WriteLine(t.Item1 + ":  " + t.Item2);
            }
        }

        private List<Tuple<string, string>> GenerateRandomLoadoutR(Node node, List<Tuple<string, string>> randomLoadout, Random random) {

            if (node is Category) {
                
                // Choose a random option
                List<Option> options = ((Category)node).options;

                // If it's not possible to get unique options then just select 1 as normal
                if (((Category)node).GetRandomCount() == 1 || (((Category)node).GetRandomCount() > options.Count)) {
                    
                    int randomIndex = random.Next(options.Count);
                    randomLoadout.Add(new Tuple<string, string>(node.name, options[randomIndex].name));
                    randomLoadout = GenerateRandomLoadoutR(options[randomIndex], randomLoadout, random);
                }
                else {
                    Option[] randomOptions = new Option[((Category)node).GetRandomCount()];

                    int randomIndex;

                    for (int i = 0; i < randomOptions.Length; i++) {
                        do{
                            randomIndex = random.Next(options.Count);
                            
                            if (!randomOptions.Contains(options[randomIndex])) {
                                randomOptions[i] = options[randomIndex];
                            }
                        }while(randomOptions[i] == null);
                    }

                    // Now concat arr into a string
                    string result = "";
                    foreach(Option o in randomOptions) {
                        result = result + o.name + ", ";
                    }
                    result = result.Remove(result.Length - 2, 2);
                    randomLoadout.Add(new Tuple<string, string>(node.name, result));

                    foreach(Option o in randomOptions) {
                        randomLoadout = GenerateRandomLoadoutR(o, randomLoadout, random);
                    }
                }
            }
            else {
                foreach (Node child in node.children) {
                    randomLoadout = GenerateRandomLoadoutR(child, randomLoadout, random);
                }
            }

            return randomLoadout;
        }
    }
}