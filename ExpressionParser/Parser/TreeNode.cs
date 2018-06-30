using System;
using System.Collections.Generic;

namespace ExpressionParser.Parser
{
    /// <summary>
    /// Simplistic tree node for parsed tree
    /// </summary>
    public class TreeNode
    {
        public string Name { get; private set; }
        public int Position { get; private set; }
        public IEnumerable<TreeNode> Children { get; private set; }

        public TreeNode(string name, int position, IEnumerable<TreeNode> children)
        {
            Name = name;
            Position = position;
            Children = children;
        }

        public void Dump(int i = 0)
        {
            Console.Write(new string(' ', i));
            Console.WriteLine($"{Name} (@ Position {Position})");
            foreach(var c in Children ?? new List<TreeNode>())
            {
                c.Dump(i + 1);
            }
        }

    }
}
