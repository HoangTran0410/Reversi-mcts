using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS
{
    class Node
    {
        static readonly Random RandomGenerator = new Random();

        public State state { get; set; }
        public Node parent { get; set; }
        public List<Node> childArray { get; set; }

        public Node()
        {
            this.state = new State();
            this.childArray = new List<Node>();
        }

        public Node(State state)
        {
            this.state = state;
            this.childArray = new List<Node>();
        }

        public Node(State state, Node parent, List<Node> childArray)
        {
            this.state = state;
            this.parent = parent;
            this.childArray = childArray;
        }

        public Node(Node node)
        {
            this.childArray = new List<Node>();
            this.state = new State(node.state);

            if (node.parent != null)
                this.parent = node.parent;

            foreach (Node child in node.childArray)
            {
                this.childArray.Add(new Node(child));
            }
        }

        public Node GetRandomChildNode()
        {
            int noOfPossibleMoves = this.childArray.Count;
            int selectRandom = (int)(RandomGenerator.NextDouble() * noOfPossibleMoves);
            return this.childArray[selectRandom];
        }

        public Node GetChildWithMaxScore()
        {
            Node node = childArray[0];
            int maxVisit = node.state.visitCount;
            foreach (Node child in childArray)
            {
                if(child.state.visitCount > maxVisit)
                {
                    node = child;
                    maxVisit = child.state.visitCount;
                }
            }
            return node;
        }
    }
}
