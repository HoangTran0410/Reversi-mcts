namespace Reversi_mcts.MCTS
{
    class Position
    {
        int x { get; set; }
        int y { get; set; }

        public Position()
        {
            x = 0;
            y = 0;
        }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
