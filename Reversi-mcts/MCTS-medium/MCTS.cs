using System;
using System.Collections.Generic;

namespace Reversi_mcts.MCTS_medium
{
    class SearchStatistic
    {
        double runtime;
        int simulations;
        int draws;

        public SearchStatistic(double runtime, int simulations, int draws)
        {
            this.runtime = runtime;
            this.simulations = simulations;
            this.draws = draws;
        }
    }

    class MCTS
    {

        Game game;
        double UCB1ExploreParam;
        Dictionary<string, Node> nodes;
        
        Random rand;

        /// <summary>
        /// Create a Monte Carlo search tree.
        /// </summary>
        /// <param name="game">The game to query regarding legal moves and state advancement.</param>
        /// <param name="UCB1ExploreParam">The square of the bias parameter in the UCB1 algorithm; defaults to 2.</param>
        public MCTS(Game game, double UCB1ExploreParam = 2)
        {
            this.game = game;
            this.UCB1ExploreParam = UCB1ExploreParam;
            this.nodes = new Dictionary<string, Node>(); // map: State.hash() => Node

            rand = new Random();
        }

        /// <summary>
        /// If state does not exist, create dangling node.
        /// </summary>
        /// <param name="state">The state to make a dangling node for; its parent is set to null.</param>
        public void MakeNode(State state)
        {
            if (!this.nodes.ContainsKey(state.hash()))
            {
                List<Play> unexpandedPlays = this.game.LegalPlays(state);
                this.nodes[state.hash()] = new Node(null, null, state, unexpandedPlays);
            }
        }

        /// <summary>
        /// From given state, run as many simulations as possible until the time limit, building statistics.
        /// </summary>
        /// <param name="state">The state to run the search from.</param>
        /// <param name="timeout">The time to run the simulations for, in seconds.</param>
        /// <returns>Search statistics.</returns>
        public SearchStatistic RunSearch(State state, double timeout = 3)
        {
            MakeNode(state);

            int draws = 0;
            int totalSims = 0;

            var timeLimit = TimeSpan.FromSeconds(timeout);
            var start = DateTime.Now;

            while (DateTime.Now - start < timeLimit)
            {
                var node = Select(state);
                var winner = this.game.Winner(node.state);

                if (node.isLeaf() == false && winner == -2)
                {
                    node = Expand(node);
                    winner = Simulate(node);
                }
                Backpropagate(node, winner);

                if (winner == 0) draws++;
                totalSims++;
            }

            return new SearchStatistic(timeout, totalSims, draws);
        }

        /// <summary>
        /// From the available statistics, calculate the best move from the given state.
        /// </summary>
        /// <param name="state">The state to get the best play from.</param>
        /// <param name="policy">The selection policy for the "best" play.</param>
        /// <returns>The best play, according to the given policy.</returns>
        public Play BestPlay(State state, string policy = "robust")
        {

            MakeNode(state);

            Node node = this.nodes.GetValueOrDefault(state.hash());

            // If not all children are expanded, not enough information
            if (node.isFullyExpanded() == false)
                throw null; //new Error("Not enough information!")

            var allPlays = node.allPlays();
            Play bestPlay = null;

            // Most visits (robust child)
            if (policy == "robust")
            {
                var max = Int32.MinValue;
                foreach (var play in allPlays)
                {
                    var childNode = node.ChildNode(play);
                    if (childNode.n_plays > max)
                    {
                        bestPlay = play;
                        max = childNode.n_plays;
                    }
                }
            }

            // Highest winrate (max child)
            else if (policy == "max")
            {
                var max = Int32.MinValue;
                foreach (var play in allPlays)
                {
                    var childNode = node.ChildNode(play);
                    var ratio = childNode.n_wins / childNode.n_plays;
                    if (ratio > max)
                    {
                        bestPlay = play;
                        max = ratio;
                    }
                }
            }

            return bestPlay;
        }

        /// <summary>
        /// Phase 1: Selection
        /// Select until EITHER not fully expanded OR leaf node
        /// </summary>
        /// <param name="state">The root state to start selection from.</param>
        /// <returns>The selected node.</returns>
        public Node Select(State state)
        {
            var node = this.nodes.GetValueOrDefault(state.hash());
            while (node.isFullyExpanded() && !node.isLeaf())
            {
                var plays = node.allPlays();
                Play bestPlay = null;
                var bestUCB1 = Double.MinValue;
                foreach (var play in plays)
                {
                    var childUCB1 = node.ChildNode(play).getUCB1(this.UCB1ExploreParam);
                    if (childUCB1 > bestUCB1)
                    {
                        bestPlay = play;
                        bestUCB1 = childUCB1;
                    }
                }
                node = node.ChildNode(bestPlay);
            }
            return node;
        }

        /// <summary>
        /// Phase 2: Expansion
        /// Of the given node, expand a random unexpanded child node
        /// </summary>
        /// <param name="node">The node to expand from. Assume not leaf.</param>
        /// <returns>The new expanded child node.</returns>
        public Node Expand(Node node)
        {
            var plays = node.unexpandedPlays();
            var index = rand.Next(0, plays.Count);
            var play = plays[index];

            var childState = this.game.NextState(node.state, play);
            var childUnexpandedPlays = this.game.LegalPlays(childState);
            var childNode = node.Expand(play, childState, childUnexpandedPlays);
            this.nodes[childState.hash()] = childNode;

            return childNode;
        }

        /// <summary>
        /// Phase 3: Simulation
        /// From given node, play the game until a terminal state, then return winner
        /// </summary>
        /// <param name="node">The node to simulate from.</param>
        /// <returns>The winner of the terminal game state.</returns>
        public int Simulate(Node node)
        {
            var state = node.state;
            var winner = this.game.Winner(state);

            while (winner == -2)
            {
                var plays = this.game.LegalPlays(state);
                var index = rand.Next(0, plays.Count);
                var play = plays[index];
                state = this.game.NextState(state, play);
                winner = this.game.Winner(state);
            }

            return winner;
        }

        /// <summary>
        /// Phase 4: Backpropagation
        /// From given node, propagate plays and winner to ancestors' statistics
        /// </summary>
        /// <param name="node">The node to backpropagate from. Typically leaf.</param>
        /// <param name="winner">The winner to propagate.</param>
        public void Backpropagate(Node node, int winner)
        {

            while (node != null)
            {
                node.n_plays += 1;
                // Parent's choice
                if (node.state.isPlayer(-winner))
                {
                    node.n_wins += 1;
                }
                node = node.parent;
            }
        }
    }
}
