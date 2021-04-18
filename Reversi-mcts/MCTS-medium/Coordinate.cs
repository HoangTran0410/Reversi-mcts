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
        public List<Coordinate> Between(Coordinate c)
        {
            List<Coordinate> between = new List<Coordinate>();

            // Horizontally
            if (this.X == c.Y)
            {
                int MinY = Math.Min(Y, c.Y) + 1;
                int MaxY = Math.Max(Y, c.Y);
                while (MinY < MaxY)
                {
                    between.Add(new Coordinate(X, MinY));
                    MinY++;
                }
            }
            // Vertically
            else if (this.Y == c.Y)
            {
                int MinX = Math.Min(X, c.X) + 1;
                int MaxX = Math.Max(X, c.X);
                while (MinX < MaxX)
                {
                    between.Add(new Coordinate(MinX, Y));
                    MinX++;
                }
            }
            // Diagonal up right
            else if (this.Y < c.Y && this.X < c.X)
            {
                int xC = this.X + 1;
                int yC = this.Y + 1;
                while (yC < c.Y)
                {
                    between.Add(new Coordinate(xC, yC));
                    yC++;
                    xC++;
                }
            }
            // Diagonal down right
            else if (this.Y > c.Y && this.X < c.X)
            {
                int xC = this.X + 1;
                int yC = this.Y - 1;
                while (yC > c.Y)
                {
                    between.Add(new Coordinate(xC, yC));
                    yC--;
                    xC++;
                }
            }
            // Diagonal up left
            else if (this.Y < c.Y && this.X > c.X)
            {
                int xC = this.X - 1;
                int yC = this.Y + 1;
                while (yC < c.Y)
                {
                    between.Add(new Coordinate(xC, yC));
                    yC++;
                    xC--;
                }
            }
            // Diagonal down left
            else if (this.Y > c.Y && this.X > c.X)
            {
                int xC = this.X - 1;
                int yC = this.Y - 1;
                while (yC > c.Y)
                {
                    between.Add(new Coordinate(xC, yC));
                    yC--;
                    xC--;
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
