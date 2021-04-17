using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS
{
    class Tree
    {
        public Node root { get; set; }
        public Tree()
        {
            root = new Node();
        }

        public Tree(Node root)
        {
            this.root = root;
        }

        public void addChild(Node parent, Node child)
        {
            parent.childArray.Add(child);
        }
    }
}
