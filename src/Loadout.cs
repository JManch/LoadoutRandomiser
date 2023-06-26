using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Spectre.Console;

using Generation = System.Collections.Generic.List<(string, string)>;
using Tree = LoadoutRandomiser.Parsing.Tree;
using LoadoutRandomiser.Parsing;

namespace LoadoutRandomiser {
public class Loadout {
    public string Name { get; private set; }
    public bool Parsed { get; set; }
    public int CategoryCount { get; private set; }
    public int OptionCount { get; private set; }
    public Tree Tree { get; private set; }
    public HashSet<int> DisabledNodes = new();

    public Loadout(string name) {
        Name = name;
        Tree = new();
    }

    public void MoveDown() => Tree.MoveDownNode();

    public void MoveUp() => Tree.MoveUpNode();

    public void AddCategory(string categoryName, string modifier) {
        Tree.AddNode(new Category(categoryName, modifier));
        CategoryCount++;
    }

    public void AddOption(string optionName) {
        Tree.AddNode(new Option(optionName));
        OptionCount++;
    }

    public void Randomise() {
        var generation = new Generation();
        Randomise(Tree.RootNode, generation);
        Tree.Revive();

        var table = new Table().Centered().RoundedBorder();
        table.AddColumn(new TableColumn("Category"));
        table.AddColumn(new TableColumn("Option"));
        table.HideHeaders();

        foreach (var gen in generation) {
            table.AddRow(gen.Item1, "[blue]" + gen.Item2 + "[/]");
        }

        AnsiConsole.Write(table);
    }

    public Database.SelectionOption[] GetOptionsAtCategory(Category category) {
        List<Database.SelectionOption> options = new();
        foreach (Option option in category.Children) {
            options.Add(new Database.SelectionOption {
                Option = option,
                Name = option.Name,
            });
        }
        return options.ToArray();
    }

    public Database.SelectionCategory[] GetCategories() {
        // Get categories in depth first traversal
        List<Database.SelectionCategory> categories = new();
        Stack<string> parentNodeNames = new();
        int lastDepth = 0;
        foreach (var node in Tree.DepthFirstTraversal()) {
            if (node is Root)
                continue;

            // Pop lower depth names off the stack
            if (node.Depth < lastDepth) {
                for (int i = 0; i < (lastDepth - node.Depth) + 1; i++)
                    parentNodeNames.Pop();
            } else if (node.Depth == lastDepth) {
                parentNodeNames.Pop();
            }

            if (node is Option)
                parentNodeNames.Push(
                    string.Format("[dim italic]{0}[/]", node.Name));
            else
                parentNodeNames.Push(
                    string.Format("[dim italic]{0}[/]", node.Name));

            if (node is Category) {
                var array = parentNodeNames.ToArray();
                array[0] = string.Format("[bold italic white]{0}[/]", array[0]);
                Array.Reverse(array);
                categories.Add(new Database.SelectionCategory {
                    Name = string.Join("[bold blue] > [/]", array),
                    Category = node as Category,
                });
            }
            lastDepth = node.Depth;
        }

        return categories.ToArray();
    }

    private void Randomise(Node node, Generation generation) {
        if (node is Category category) {
            string randomOptions = RandomiseCategory(category);
            generation.Add((category.Name, randomOptions));
        }
        foreach (var child in node.AliveChildren)
            Randomise(child, generation);
    }

    private string RandomiseCategory(Category category) {
        string result = "";
        var children = category.Children.Where(child => !DisabledNodes.Contains(child.Id)).ToArray();
        var childCount = children.Length;

        if (childCount == 0)
            return "";

        if (category.RandomCount > 0) {
            if (category.RandomCount > childCount)
                return "[red]Error: random selection too large[/]";
            StringBuilder sb = new();

            Node[] nodes = (Node[])children.Clone();
            List<Node> chosen = new();
            int pool = category.ChildCount;
            for (int i = 0; i < category.RandomCount; i++) {
                int rand = Program.random.Next(0, pool);
                chosen.Add(nodes[rand]);
                sb.Append(nodes[rand].Name + ", ");
                pool--;
                nodes[rand] = nodes[pool];
            }
            // Remove trailing comma and space
            sb.Remove(sb.Length - 2, 2);
            result = sb.ToString();

            // Kill all nodes that didn't get chosen
            foreach (var child in category.Children) {
                if (!chosen.Contains(child))
                    child.Kill();
            }
        } else {
            Node node = children[Program.random.Next(childCount)];
            result = node.Name;

            // Kill all nodes that didn't get chosen
            foreach (var child in category.Children.Where(n => n != node))
                child.Kill();
        }
        return result;

    }
}
}
