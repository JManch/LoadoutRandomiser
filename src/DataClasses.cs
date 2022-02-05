using System.Collections.Generic;

namespace LoadoutRandomiser
{
    public abstract class Node 
    {
        public string name {get; private set;}
        public Node parent {get; protected set;}
        public List<Node> children;

        public Node(Node parent, string name) {
            this.parent = parent;
            this.name = name;
            children = new List<Node>();
        }
        
        public virtual void AddChild(Node child) {
            children.Add(child);
        }
    }

    public class Category : Node
    {
        public List<Option> options {get; private set;}
        private int randomCount = 1;
        public Category(Node parent, string name) : base(parent, name)
        {
            options = new List<Option>();
        }

        public override void AddChild(Node child)
        {
            base.AddChild(child);
            if (child is Option) {
                options.Add((Option)child);
            }
        }

        public void SetRandomCount(int randomCount) {
            this.randomCount = randomCount;
        }

        public int GetRandomCount() {
            return randomCount;
        }
    }

    public class Option : Node
    {
        public Option(Node parent, string name) : base(parent, name){}
    }
}