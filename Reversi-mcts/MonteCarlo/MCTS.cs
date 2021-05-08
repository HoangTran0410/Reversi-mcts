using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts
{
    public class MCTS
    {
        Game Game;
        Dictionary<int, Node> Nodes;
        double UCB1ExploreParam;

        Random Random;

        public MCTS(Game game, double ucb1ExploreParam = 1.41)
        {
            UCB1ExploreParam = ucb1ExploreParam;
            Game = game;
            Nodes = new Dictionary<int, Node>();
            Random = new Random();
        }

        /// <summary>
        /// If state does not exist, create dangling node.
        /// </summary>
        /// <param name="state">The state to make a dangling node for; its parent is set to null.</param>
        public void MakeNode(State state)
        {
            int stateHash = state.Hash();

            if (!Nodes.ContainsKey(stateHash))
            {
                List<ulong> unexpandedPlays = Game.LegalMoves(state);
                Nodes[stateHash] = new Node(null, 0, state, unexpandedPlays);
            }
        }

        /// <summary>
        /// From given state, run as many simulations as possible until the time limit, building statistics.
        /// </summary>
        /// <param name="state">The state to run the search from.</param>
        /// <param name="timeout">The time to run the simulations for, in seconds.</param>
        public void RunSearch(State state, int timeout = 3)
        {
            MakeNode(state);

            int draws = 0;
            int totalSims = 0;

            TimeSpan timeLimit = TimeSpan.FromSeconds(timeout);
            DateTime start = DateTime.Now;

            while (DateTime.Now - start < timeLimit)
            {
                // Phase 1: Selection
                Node node = SelectChild(state);
                byte winner = Game.Winner(state);

                if (node.IsLeaf() == false && winner == Constant.GameNotCompleted)
                {
                    // Phase 2: Expansion
                    node = ExpandChild(node);

                    // Phase 3: Simulation
                    winner = Simulate(node);
                }

                // Phase 4: Backpropagation
                Backpropagate(node, winner);

                if (winner == 0) draws++;
                totalSims++;
            }

            Console.WriteLine("statistic: runtime:" + timeout + "s, draws:" + draws + ", totalSims:" + totalSims + " playouts.");
        }

        public ulong BestMove(State state, string policy = "robust")
        {
            MakeNode(state);

            // If not all children are expanded, not enough information
            if (Nodes[state.Hash()].IsFullyExpanded() == false)
                throw new Exception("Not enough information!");

            Node node = Nodes[state.Hash()];
            List<ulong> allMoves = node.AllMoves();
            ulong bestPlay = 0UL;

            // Most visits (robust child)
            if (policy.Equals("robust"))
            {
                double max = Double.MinValue;
                foreach (ulong move in allMoves)
                {
                    Node childNode = node.ChildNode(move);
                    if (childNode.Visits > max)
                    {
                        bestPlay = move;
                        max = childNode.Visits;
                    }
                }
            }

            // Highest winrate (max child)
            else if (policy.Equals("max"))
            {
                double max = Double.MinValue;
                foreach (ulong move in allMoves)
                {
                    Node childNode = node.ChildNode(move);
                    double ratio = childNode.Wins / childNode.Visits;
                    if (ratio > max)
                    {
                        bestPlay = move;
                        max = ratio;
                    }
                }
            }

            return bestPlay;
        }

        private Node SelectChild(State state)
        {
            Node node = Nodes.GetValueOrDefault(state.Hash());

            while (node.IsFullyExpanded() && !node.IsLeaf())
            {
                List<ulong> moves = node.AllMoves();
                ulong bestMove = 0;
                double bestUCB1 = Double.MinValue;
                foreach (ulong move in moves)
                {
                    double childUCB1 = node.ChildNode(move).GetUCB1(UCB1ExploreParam);
                    if (childUCB1 > bestUCB1)
                    {
                        bestMove = move;
                        bestUCB1 = childUCB1;
                    }
                }
                node = node.ChildNode(bestMove);
            }

            return node;
        }

        private Node ExpandChild(Node node)
        {
            List<ulong> moves = node.UnexpandedMoves();
            int index = Random.Next(0, moves.Count);
            ulong move = moves[index];

            State childState = Game.NextState(node.State, move);
            List<ulong> childUnexpandedPlays = Game.LegalMoves(childState);
            Node childNode = node.Expand(move, childState, childUnexpandedPlays);
            Nodes[childState.Hash()] = childNode;

            return childNode;
        }

        private byte Simulate(Node node)
        {
            State state = node.State;
            byte winner = Game.Winner(state);

            while (winner == Constant.GameNotCompleted)
            {
                List<ulong> moves = Game.LegalMoves(state);

                ulong move = 0;
                if(moves.Count > 0)
                {
                    int index = Random.Next(0, moves.Count);
                    move = moves[index];
                }
                
                state = Game.NextState(state, move);
                winner = Game.Winner(state);
            }

            return winner;
        }

        private void Backpropagate(Node node, int winner)
        {
            while (node != null)
            {
                node.Visits += 1;
                // Parent's choice
                if (node.State.Opponent == winner)
                {
                    node.Wins += 1;
                }
                node = node.Parent;
            }
        }
    }
}
