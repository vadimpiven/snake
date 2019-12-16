using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Snake.Models
{
    /// <summary>
    ///     Analogue of System.Drawing.Point but not having IsEmpty,
    ///     as such field shouldn't be returned
    /// </summary>
    public class Point : IEquatable<Point>
    {
        public Point(in Point other)
        {
            X = other.X;
            Y = other.Y;
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public bool Equals(Point other)
        {
            return null != other &&
                   X == other.X &&
                   Y == other.Y;
        }
    }

    /// <summary>
    ///     Analogue of System.Drawing.Size but not having IsEmpty,
    ///     as such field shouldn't be returned
    /// </summary>
    public class Size : IEquatable<Size>
    {
        public Size(in Size other)
        {
            Width = other.Width;
            Height = other.Height;
        }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public bool Equals([AllowNull] Size other)
        {
            return null != other &&
                   Width == other.Width &&
                   Height == other.Height;
        }
    }

    /// <summary>
    ///     All information about game that should be returned
    /// </summary>
    public class GameBoard : ICloneable, IEquatable<GameBoard>
    {
        public int TurnNumber { get; set; }
        public int TimeUntilNextTurnMilliseconds { get; set; }
        public Size GameBoardSize { get; set; }
        public LinkedList<Point> Snake { get; set; }
        public List<Point> Food { get; set; }

        public object Clone()
        {
            var gameBoard = new GameBoard
            {
                TurnNumber = TurnNumber,
                TimeUntilNextTurnMilliseconds = TimeUntilNextTurnMilliseconds
            };
            if (null != GameBoardSize) gameBoard.GameBoardSize = new Size(GameBoardSize);
            if (null != Snake)
            {
                gameBoard.Snake = new LinkedList<Point>();
                foreach (var s in Snake) gameBoard.Snake.AddLast(new Point(s));
            }

            if (null != Food)
            {
                gameBoard.Food = new List<Point>();
                foreach (var f in Food) gameBoard.Food.Add(new Point(f));
            }

            return gameBoard;
        }

        public bool Equals([AllowNull] GameBoard other)
        {
            return null != other &&
                   TurnNumber == other.TurnNumber &&
                   TimeUntilNextTurnMilliseconds == other.TimeUntilNextTurnMilliseconds &&
                   !GameBoardSize.Equals(other.GameBoardSize) &&
                   Snake.SequenceEqual(other.Snake) &&
                   Food.SequenceEqual(other.Food);
        }
    }
}