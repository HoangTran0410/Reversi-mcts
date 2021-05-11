using Reversi_mcts.PlayMode;
using Reversi_mcts.SocketIo;

namespace Reversi_mcts
{
    internal static class Program
    {
        private static void Main()
        {
            //SelfPlay.OneRound(300, 15);
            //SelfPlay.MultiRounds(100, 100, 15);
            //HumanVsAi.NewGame(Constant.White);

            var socketClient = new SocketClient(500);
        }
    }
}