using Reversi_mcts.PlayMode;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            //SelfPlay.OneRound(100, 1000);
            //SelfPlay.MultiRounds(100, 30, 50);
            HumanVsAi.NewGame();
        }
    }
}