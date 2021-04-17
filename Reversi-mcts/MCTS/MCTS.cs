using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Reversi_mcts.MCTS
{
    class MCTS
    {
        public static readonly int WinScore = 10;
        public int level { get; set; }
        public int opponent { get; set; }

        public MCTS()
        {
            this.level = 3;
        }
  
        private int GetMillisForCurrentLevel()
        {
            return 2 * (this.level - 1) + 1;
        }

        // https://jeremylindsayni.wordpress.com/2016/05/28/how-to-set-a-maximum-time-to-allow-a-c-function-to-run-for/
        public Board findNextMove(Board board, int playerNo)
        {
            opponent = 3 - playerNo;
            Tree tree = new Tree();
            Node rootNode = tree.root;
            rootNode.state.board = board;
            rootNode.state.playerNo = opponent;

            var timeLimit = TimeSpan.FromMilliseconds(60 * GetMillisForCurrentLevel());
            var startTime = DateTime.Now;

            while (true)
            {
                // Phase 1 - Selection
                Node promisingNode = SelectChild(rootNode);

                // Phase 2 - Expansion
                if (promisingNode.state.board.CheckStatus() == Board.InProgress)
                    ExpandChild(promisingNode);

                // Phase 3 - Simulation
                Node nodeToExplore = promisingNode;
                if (promisingNode.childArray.Count > 0)
                {
                    nodeToExplore = promisingNode.GetRandomChildNode();
                }
                int playoutResult = Simulate(nodeToExplore);

                // Phase 4 - Update
                BackPropogation(nodeToExplore, playoutResult);

                if(DateTime.Now - startTime > timeLimit)
                {
                    break;
                }
            }

            Node winnerNode = rootNode.GetChildWithMaxScore();
            tree.root = winnerNode;
            return winnerNode.state.board;
        }

        // SelectPromisingNode
        private Node SelectChild(Node rootNode)
        {
            Node node = rootNode;
            while (node.childArray.Count != 0)
            {
                node = UCT.FindBestNodeWithUCT(node);
            }
            return node;
        }

        // ExpandNode
        private void ExpandChild(Node node)
        {
            List<State> possibleStates = node.state.GetAllPossibleStates();
            foreach(State state in possibleStates)
            {
                Node newNode = new Node(state);
                newNode.parent = node;
                newNode.state.playerNo = node.state.opponentNo;
                node.childArray.Add(newNode);
            }
        }

        // SimulateRandomPlayout
        private int Simulate(Node node)
        {
            Node tempNode = new Node(node);
            State tempState = tempNode.state;
            int boardStatus = tempState.board.CheckStatus();

            if (boardStatus == opponent)
            {
                tempNode.parent.state.winScore = Int32.MinValue;
                return boardStatus;
            }
            while (boardStatus == Board.InProgress)
            {
                tempState.TogglePlayer();
                tempState.RandomPlay();
                boardStatus = tempState.board.CheckStatus();
            }

            return boardStatus;
        }

        private void BackPropogation(Node nodeToExplore, int playerNo)
        {
            Node tempNode = nodeToExplore;
            while (tempNode != null)
            {
                tempNode.state.IncrementVisit();
                if (tempNode.state.playerNo == playerNo)
                    tempNode.state.AddScore(WinScore);
                tempNode = tempNode.parent;
            }
        }
    }
}
