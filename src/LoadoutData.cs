using System;
using System.Collections.Generic;
using System.Linq;

namespace LoadoutRandomiser 
{
    public class LoadoutData {
        private Node root;
        private Node current;
        public int categoryCount {get; private set;}
        public int optionCount {get; private set;}
        public List<Node> nodes {get; private set;}

        public LoadoutData() {
            nodes = new List<Node>();
            current = null;
        }

        public void AddCategory(string name) {
            
            Category newCategory;
            if (current == null) {
                newCategory = new Category(null, name);
                root = newCategory;
            }
            else {
                newCategory = new Category(current, name);
                current.AddChild(newCategory);
            }
            nodes.Add(newCategory);
            ++categoryCount;
        }

        public void AddOption(string name) {
            
            // Options can only be children of categories
            if (current != null && current is Category) {
                Option newOption = new Option(current, name);
                current.AddChild(newOption);
                nodes.Add(newOption);
                ++optionCount;
            }
            else {
                Console.WriteLine("Failed to add option " + name + " as it's parent " + current.name + " is an option. Options must be children of categories.");
            }
        }

        public void MoveUpNode() {
            current = current.parent;
        }
        public void MoveDownNode() {
            if(current == null) {
                current = nodes[0];
            }
            else {
                current = current.children[current.children.Count - 1];
            }
        }

        public void GenerateRandomLoadout() {
            List<Tuple<string, string>> randomLoadout = GenerateRandomLoadoutR(root, new List<Tuple<string, string>>(), new Random());
            foreach (var t in randomLoadout) {
                Console.WriteLine(t.Item1 + ":  " + t.Item2);
            }
        }

        private List<Tuple<string, string>> GenerateRandomLoadoutR(Node node, List<Tuple<string, string>> randomLoadout, Random random) {

            if (node is Category) {
                // Choose a random option
                List<Option> options = ((Category)node).options;
                int randomIndex = random.Next(options.Count);
                randomLoadout.Add(new Tuple<string, string>(node.name, options[randomIndex].name));
                randomLoadout = GenerateRandomLoadoutR(options[randomIndex], randomLoadout, random);
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