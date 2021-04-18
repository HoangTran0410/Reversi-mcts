using System;

namespace Reversi_mcts.MCTS_medium
{
	// https://github.com/EivindEE/Reversi/blob/master/src/com/eivind/reversi/game/Move.java
	class Move
    {
		public byte Player { get; }
		public Coordinate Coordinate { get; }

		public Move(byte player, Coordinate coordinate)
		{
			Player = player;
			Coordinate = coordinate;
		}

		public override string ToString()
		{
			String color = "";
			if (Player == 0)
				color = "BLACK";
			if (Player == 1)
				color = "WHITE";
			return "[" + color + ", " + Coordinate.ToString() + "]";
		}
	}
}
