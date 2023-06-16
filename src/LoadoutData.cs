using System;
using System.Linq;
using System.Collections.Generic;

namespace LoadoutRandomiser
{
    public class LoadoutData
    {
        private Tree _tree;

        public LoadoutData()
        {
            _tree = new();
        }

        public void AddCategory(string categoryName) => _tree.AddNode(new Category(categoryName));

        public void AddOption(string optionName) => _tree.AddNode(new Option(optionName));

        public void AddRandomCategory(string randomCount)
        {
            // Remove the last category to replace it

        }

        public void SetRandomCount(string randomCount)
        {
            if (Int32.TryParse(randomCount, out int rCount))
            {
                if (_latestNode is Category)
                {
                    ((Category)_latestNode).SetRandomCount(rCount);
                }
                else
                {
                    Console.WriteLine(
                        "> Warning you attempted to define random count for option {0}. Random count can only be used on categories.",
                        _latestNode.name
                    );
                }
            }
            else
            {
                Console.WriteLine(
                    "> Warning random count failed to set as {0} is not an integer.",
                    randomCount
                );
            }
        }

        public void GenerateRandomLoadout()
        {
            List<Tuple<string, string>> generatedLoadout = new List<Tuple<string, string>>();

            foreach (Node root in _roots)
            {
                generatedLoadout.AddRange(
                    GenerateRandomLoadoutR(root, new List<Tuple<string, string>>())
                );
            }

            foreach (var t in generatedLoadout)
            {
                Console.WriteLine(t.Item1 + ":  " + t.Item2);
            }
        }

        private List<Tuple<string, string>> GenerateRandomLoadoutR(
            Node node,
            List<Tuple<string, string>> randomLoadout
        )
        {
            if (node is Category)
            {
                // Choose a random option
                List<Option> options = ((Category)node).options;

                // If it's not possible to get unique options then just select 1 as normal
                if (
                    ((Category)node).GetRandomCount() == 1
                    || (((Category)node).GetRandomCount() > options.Count)
                )
                {
                    int randomIndex = Program.random.Next(options.Count);
                    randomLoadout.Add(
                        new Tuple<string, string>(node.name, options[randomIndex].name)
                    );
                    randomLoadout = GenerateRandomLoadoutR(options[randomIndex], randomLoadout);
                }
                else
                {
                    Option[] randomOptions = new Option[((Category)node).GetRandomCount()];

                    int randomIndex;

                    for (int i = 0; i < randomOptions.Length; i++)
                    {
                        do
                        {
                            randomIndex = Program.random.Next(options.Count);

                            if (!randomOptions.Contains(options[randomIndex]))
                            {
                                randomOptions[i] = options[randomIndex];
                            }
                        } while (randomOptions[i] == null);
                    }

                    // Now concat arr into a string
                    string result = "";
                    foreach (Option o in randomOptions)
                    {
                        result = result + o.name + ", ";
                    }
                    result = result.Remove(result.Length - 2, 2);
                    randomLoadout.Add(new Tuple<string, string>(node.name, result));

                    foreach (Option o in randomOptions)
                    {
                        randomLoadout = GenerateRandomLoadoutR(o, randomLoadout);
                    }
                }
            }
            else
            {
                foreach (Node child in node.children)
                {
                    randomLoadout = GenerateRandomLoadoutR(child, randomLoadout);
                }
            }

            return randomLoadout;
        }
    }
}
