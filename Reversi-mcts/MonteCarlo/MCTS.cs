using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class MCTS
    {
        Game game;
        Dictionary<int, Node> nodes;
        double UCB1ExploreParam;
        Random random;

        public MCTS(Game game, double UCB1ExploreParam = 1.41)
        {
            this.UCB1ExploreParam = UCB1ExploreParam;
            this.game = game;
            nodes = new Dictionary<int, Node>();
            random = new Random();
        }

        public void MakeNode(State state)
        {
            int stateHash = state.Hash();

            if (!nodes.ContainsKey(stateHash))
            {
                List<Move> unexpandedPlays = state.GetAllPossibleMoves();
                nodes[stateHash] = new Node(null, null, state, unexpandedPlays);
            }
        }

        public void RunSearch(State state, byte player)
        {
            MakeNode(state);

            int draws = 0;
            int totalSims = 0;

            var timeLimit = TimeSpan.FromSeconds(1);
            var start = DateTime.Now;

            while (DateTime.Now - start < timeLimit)
            {
                var node = SelectChild(state);
                var winner = node.State.Board.GetBoardStatus();

                if (node.IsLeaf() == false && winner == Constant.GameNotCompleted)
                {
                    node = ExpandChild(node);
                    winner = Simulate(node);
                }
                Backpropagate(node, winner);

                if (winner == 0) draws++;
                totalSims++;
            }
        }

        private Node SelectChild(State state) {
            var node = nodes.GetValueOrDefault(state.Hash());
            while (node.IsFullyExpanded() && !node.IsLeaf())
            {
                var plays = node.AllMoves();
                Move bestMove = null;
                var bestUCB1 = Double.MinValue;
                foreach (var play in plays)
                {
                    var childUCB1 = node.ChildNode(play).GetUCB1(this.UCB1ExploreParam);
                    if (childUCB1 > bestUCB1)
                    {
                        bestMove = play;
                        bestUCB1 = childUCB1;
                    }
                }
                node = node.ChildNode(bestMove);
            }
            return node;
        }

        private void ExpandChild(Node node) {
            var plays = node.UnexpandedMoves();
            var index = random.Next(0, plays.Count);
            var play = plays[index];

            var childState = this.game.NextState(node.state, play);
            var childUnexpandedPlays = this.game.LegalPlays(childState);
            var childNode = node.Expand(play, childState, childUnexpandedPlays);
            this.nodes[childState.hash()] = childNode;

            return childNode;
        }

        private byte Simulate(Node node) {
            
        }

        private void Backpropagate(Node nodeToExplore, int playoutResult) {
            
        }
    }
}
