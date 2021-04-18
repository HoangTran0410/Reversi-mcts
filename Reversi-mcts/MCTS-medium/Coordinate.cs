using System;
using System.Collections.Generic;
using System.Text;

namespace Reversi_mcts.MCTS_medium
{
    // https://github.com/EivindEE/Reversi/blob/master/src/com/eivind/reversi/game/Coordinate.java
    class Coordinate
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            int alfa = 97;
            alfa += X;
            return "[" + (char)alfa + "," + (char)(Y + 1) + "]";
        }

        /// <summary>
        /// Equal if the object is of the type coordinate, and occupy the same space
        /// </summary>
        /// <param name="c">other object</param>
        /// <returns>true if equal</returns>
        public override bool Equals(object c)
        {
            if (c.GetType() == typeof(Coordinate))
            {
                Coordinate other = (Coordinate)c;
                return other.X == this.Y && other.Y == this.Y;
            }

            return false;
        }

        /// <summary>
        /// Assumes that all coordinates between this and c falls on valid coordinates.
        /// </summary>
        /// <param name="c">other coordinate object</param>
        /// <returns>List of coordinate between two coordinates</returns>
        public static List<Coordinate> Between(Coordinate a, Coordinate b)
        {
            // Use static method: https://stackoverflow.com/a/1496656/11898496

            List<Coordinate> between = new List<Coordinate>();

            // Horizontally
            if (a.X == b.X)
            {
                int MinY = Math.Min(a.Y, b.Y) + 1;
                int MaxY = Math.Max(a.Y, b.Y);
                while (MinY < MaxY)
                {
                    between.Add(new Coordinate(a.X, MinY));
                    MinY++;
                }
            }
            // Vertically
            else if (a.Y == b.Y)
            {
                int MinX = Math.Min(a.X, b.X) + 1;
                int MaxX = Math.Max(a.X, b.X);
                while (MinX < MaxX)
                {
                    between.Add(new Coordinate(MinX, a.Y));
                    MinX++;
                }
            }
            // Diagonal up right
            else if (a.Y < b.Y && a.X < b.X)
            {
                int xB = a.X + 1;
                int yB = a.Y + 1;
                while (yB < b.Y)
                {
                    between.Add(new Coordinate(xB, yB));
                    yB++;
                    xB++;
                }
            }
            // Diagonal down right
            else if (a.Y > b.Y && a.X < b.X)
            {
                int xB = a.X + 1;
                int yB = a.Y - 1;
                while (yB > b.Y)
                {
                    between.Add(new Coordinate(xB, yB));
                    yB--;
                    xB++;
                }
            }
            // Diagonal up left
            else if (a.Y < b.Y && a.X > b.X)
            {
                int xB = a.X - 1;
                int yB = a.Y + 1;
                while (yB < b.Y)
                {
                    between.Add(new Coordinate(xB, yB));
                    yB++;
                    xB--;
                }
            }
            // Diagonal down left
            else if (a.Y > b.Y && a.X > b.X)
            {
                int xB = a.X - 1;
                int yB = a.Y - 1;
                while (yB > b.Y)
                {
                    between.Add(new Coordinate(xB, yB));
                    yB--;
                    xB--;
                }
            }

            return between;
        }

        // generate from auto complete visual studio
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }
}
