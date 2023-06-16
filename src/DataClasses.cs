using System;
using System.Collections.Generic;

namespace LoadoutRandomiser
{
    public enum NodeType
    {
        Root,
        Category,
        RandomCategory,
        Option,
    }

    public class NodeException : Exception
    {
        private Node _newNode;
        private Node _currentNode;

        public NodeException(Node currentNode, Node newNode, string message) : base(message) {
            _currentNode = currentNode;
            _newNode = newNode; 
        }
    }

    public class Tree
    {
        private Node _currentNode;
        private int _nodeCount;
        private Dictionary<int, Node> _nodes;

        public int GetUniqueNodeID => _nodeCount++;

        public Tree()
        {
            _nodes = new();
            AddNode(new Root(this));
        }

        public void AddNode(Node node) {
            // Set the node's ID
            node.Id = GetUniqueNodeID;

            // Exception checks
            if (node is Category && _currentNode is not Option && _currentNode is not Root)
                throw new NodeException(_currentNode, node, "A category can only be a child of an option");
            else if (node is Option && _currentNode is not Category)
                throw new NodeException(_currentNode, node, "An option can only be a child of a category");

            // Handle root node
            if (node is Root && _nodeCount != 0)
                throw new NodeException(_currentNode, node, "A tree can only contain one root node");
            else if (node is Root)
                node.ParentNode = null;
            else
                node.ParentNode = _currentNode;
        }

        public void MoveUpNode()
        {
            _currentNode = _currentNode.ParentNode;
        }

        public void MoveDownNode()
        {
            _currentNode = _currentNode.LastChildId;
        }

        public bool TryGetNode(int id, out Node node) {
            return _nodes.TryGetValue(id, out node);
        }
    }

    public abstract class Node : IComparable<Node>
    {
        public Node ParentNode { get; set; }
        public int Id { get; set; }

        protected Node()
        {
            _children = new();
        }

        public int CompareTo(Node other) => Id.CompareTo(other.Id);

        public Node[] Children
        {
            get { return _children.ToArray(); }
        }

        public Node LastChildId
        {
            get { return _children[_children.Count - 1]; }
        }

        private List<Node> _children;

        public virtual void AddChild(Node child)
        {
            _children.Add(child);
        }
    }

    public class Root : Node
    {
        private Tree _tree;
        public Root(Tree tree) : base()
        {
            _tree = tree;
        }
    }

    public class Category : Node
    {
        private string _categoryName;

        public Category(string categoryName) : base()
        {
            _categoryName = categoryName;
        }
    }

    public class Option : Node
    {
        private string _optionName;

        public Option(string optionName) : base()
        {
            _optionName = optionName;
        }
    }

    public class RandomCategory : Category
    {
        private int _randomCount;

        public RandomCategory(string categoryName, int randomCount) : base(categoryName)
        {
            _randomCount = randomCount;
        }
    }




    // public class Category : Node
    // {
    //     public Option[] Options
    //     {
    //         get { return _options.ToArray(); }
    //     }
    //
    //     private List<Option> _options;
    //
    //     public Category(Node parent, string name) : base(parent, name)
    //     {
    //         _options = new();
    //     }
    //
    //     public override void AddChild(Node child)
    //     {
    //         base.AddChild(child);
    //         if (child is Option)
    //         {
    //             _options.Add((Option)child);
    //         }
    //     }
    // }

    // public class RandomCategory : Category
    // {
    //     public int RandomCount { get; set; }
    //
    //     public RandomCategory(Node parent, string name) : base(parent, name) { }
    // }
    //
    // public class Option : Node
    // {
    //     public Option(Node Parent, string Name) : base(Parent, Name) { }
    // }
}
