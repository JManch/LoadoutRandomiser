using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using LogLevel = LoadoutRandomiser.Logging.LogLevel;

namespace LoadoutRandomiser.Parsing {
public class ParseException : Exception {
    public Node NewNode;
    public Node CurrentNode;
    public int LineNumber;

    public ParseException(Node currentNode, Node newNode, string message)
        : base(message) {
        CurrentNode = currentNode;
        NewNode = newNode;
    }

    public ParseException(string message, int lineNumber) : base(message) {
        LineNumber = lineNumber;
    }
}

public class Tree {
    public Node RootNode { get; private set; }
    private Node _currentNode;
    private int _nodeCount;
    private Dictionary<int, Node> _nodes;
    private Dictionary<int, List<Node>> _nodesAtDepth;

    public int GetUniqueNodeID => _nodeCount++;

    public Tree() {
        _nodes = new();
        _nodesAtDepth = new();
        RootNode = new Root(this);
        AddNode(RootNode);
        _currentNode = RootNode;
    }

    public void AddNode(Node node) {
        // Exception checks
        if (node is Category && _currentNode is not Option &&
            _currentNode is not Root)
            throw new ParseException(
                _currentNode, node,
                "A category can only be a child of an option");
        else if (node is Option && _currentNode is not Category)
            throw new ParseException(
                _currentNode, node,
                "An option can only be a child of a category");
        else if (node is Root && _nodeCount != 0)
            throw new ParseException(_currentNode, node,
                                    "A tree can only contain one root node");

        // Handle root node
        if (node is Root)
            node.Parent = null;
        else {
            node.Parent = _currentNode;
            node.Parent.AddChild(node);
            node.Depth = node.Parent.Depth + 1;
        }

        if (!_nodesAtDepth.ContainsKey(node.Depth))
            _nodesAtDepth.Add(node.Depth, new());
        _nodesAtDepth[node.Depth].Add(node);

        // Set the node's ID
        node.Id = GetUniqueNodeID;
        _nodes.Add(node.Id, node);
    }

    public void MoveUpNode() {
        Logging.Log(LogLevel.Trace, "Moving up to node {0}",
                    _currentNode.Parent.ToString());
        _currentNode = _currentNode.Parent;
    }

    public void MoveDownNode() {
        Logging.Log(LogLevel.Trace, "Moving down to node {0}",
                    _currentNode.LastChild.ToString());
        _currentNode = _currentNode.LastChild;
    }

    public bool TryGetNode(int id, out Node node) {
        return _nodes.TryGetValue(id, out node);
    }

    public void Revive() {
        foreach (var node in _nodes.Values)
            node.Revive();
    }

    public Node[] GetNodesAtDepth(int depth) {
        if (_nodesAtDepth.TryGetValue(depth, out var nodes))
            return nodes.ToArray();
        return new Node[0];
    }

    public IEnumerable<Node> DepthFirstTraversal() {
        if (RootNode != null)
            foreach (var node in DepthFirstTraversal(RootNode))
                yield return node;
    }

    private IEnumerable<Node> DepthFirstTraversal(Node node) {
        yield return node;

        if (node.ChildCount != 0)
            foreach (var child in node.Children)
                foreach (var childNode in DepthFirstTraversal(child))
                    yield return childNode;
    }
}

public abstract class Node : IComparable<Node> {
    public string Name { get; private set; }
    public bool IsDead { get; private set; }
    public Node Parent { get; set; }
    public int Id { get; set; }
    public int Depth { get; set; }
    public int ChildCount {
        get { return _children.Count; }
    }
    public List<Node> Children {
        get { return _children; }
    }
    public Node LastChild {
        get {
            return _children.Count > 0? _children[_children.Count - 1] : null;
        }
    }
    public Node FirstChild {
        get { return _children.Count > 0? _children[0] : this; }
    }
    public bool IsLeaf {
        get { return ChildCount == 0; }
    }
    public List<Node> AliveChildren {
        get { return Children.Where(n => !n.IsDead).ToList(); }
    }
    public string PathName {
        get {
            StringBuilder sb = new();
            Node node = this;
            do {
                sb.Insert(0, node.Name + "/");
                node = node.Parent;
            } while(node.Parent != null);
            if (sb.Length > 0)
                sb.Length--;
            return sb.ToString();
        }
    }

    public IEnumerable<Node> DepthFirstTraversal() {
        yield return this;

        if (ChildCount != 0)
            foreach (var child in Children)
                foreach (var node in child.DepthFirstTraversal())
                    yield return node;
    }

    protected Node(string name) {
        Name = name;
        _children = new();
    }

    public int CompareTo(Node other) => Id.CompareTo(other.Id);

    private List<Node> _children;

    public void Kill() { IsDead = true; }

    public void Revive() { IsDead = false; }

    public override string ToString() {
        StringBuilder s = new();
        s.Append(String.Format("Name: {0}, Parent: {1}, Children: ", Name,
                               Parent?.Name));
        foreach (var child in Children) {
            s.Append(child.Name + ", ");
        }
        return s.ToString();
    }

    public virtual void AddChild(Node child) { _children.Add(child); }
}

public class Root : Node {
    private Tree _tree;

    public Root(Tree tree) : base("Root") { _tree = tree; }
}

public class Category : Node {
    private string _modifier;
    public int RandomCount {get; private set;}

    public Category(string categoryName, string modifier) : base(categoryName) {
        Logging.Log(LogLevel.Trace,
                    "Creating category with name {0} and modifier {1}",
                    categoryName, modifier);
        _modifier = modifier;
        ParseModifier();
    }

    private void ParseModifier() {
        if (!String.IsNullOrEmpty(_modifier))
            if (Int32.TryParse(_modifier, out var randomCount))
                RandomCount = randomCount;
    }
}

public class Option : Node {
    public Option(string optionName) : base(optionName) {
        Logging.Log(LogLevel.Trace, "Creating option with name {0}",
                    optionName);
    }
}
}
