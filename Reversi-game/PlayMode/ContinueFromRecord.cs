using Reversi_mcts.Board;

namespace Reversi_game.PlayMode
{
    public static class ContinueFromRecord
    {
        public static void NewGame(string recordText)
        {
            // example record: "d3c3b3c5f6f5g6f3b6b5b4a5f4f7g4e3c4a3f8b7c6d6a8g7d2f2f1h4c2c1a6g3d7e1d1g1h3h2e6e7h8d8g8e2c8h5b2a4e8b8a2g5g2b1a1h1c7a7h6"
            var state = State.FromRecordText(recordText);
            state.Board.Display();

            // while (!state.IsTerminal())
            // {
            //     var move = Mcts.RunSearch(state, 500);
            //
            //     Console.WriteLine(move.ToNotation());
            // }
        }
    }
}