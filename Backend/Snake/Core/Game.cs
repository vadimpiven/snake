using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Snake.Models;

namespace Snake.Core
{
    public interface IGame
    {
        /// <summary>
        ///     Returns the game state
        /// </summary>
        GameBoard Board { get; }

        /// <summary>
        ///     Returns true if the game is over
        /// </summary>
        bool IsOver { get; }

        /// <summary>
        ///     Sets the new direction of movement
        /// </summary>
        void Turn(Direction d);

        /// <summary>
        ///     Restarts the game
        /// </summary>
        void Reset();
    }

    public interface ITestable
    {
        /// <summary>
        ///     Start the play
        /// </summary>
        void Start();

        /// <summary>
        ///     Stop the play
        /// </summary>
        void Stop();

        /// <summary>
        ///     Play the next round
        /// </summary>
        void Play();
    }

    public class Game : IGame, ITestable
    {
        private static readonly Random Random = new Random();

        private readonly object _lockObject;

        private readonly ILogger<Game> _logger;
        private volatile Direction _directionCurr, _directionNext;
        private volatile GameBoard _gameBoard;
        private volatile bool _gameOver;

        private volatile CancellationTokenSource _tokenSource;

        /// <summary>
        ///     Initialise the board
        /// </summary>
        public Game(ILogger<Game> logger)
        {
            _logger = logger;

            _lockObject = new object();

            _gameBoard = new GameBoard();
            _gameOver = true;

            Start();
        }

        /// <summary>
        ///     Restarts the game
        /// </summary>
        public void Reset()
        {
            lock (_lockObject)
            {
                if (_tokenSource != null)
                {
                    _tokenSource.Cancel();
                    _tokenSource = new CancellationTokenSource();
                }

                _gameBoard.TurnNumber = 0;
                _gameBoard.GameBoardSize = new Size(20, 20);
                _gameBoard.Snake = new LinkedList<Point>(
                    new[]
                    {
                        new Point(10, 10),
                        new Point(10, 11)
                    });
                _gameBoard.Food = new List<Point>
                {
                    new Point(10, 2)
                };
                _directionNext = _directionCurr = Direction.Top;
                _gameOver = false;

                SpeedUp();
                ScheduleNextRound();
            }
        }

        /// <summary>
        ///     Returns the copy of game board state
        /// </summary>
        public GameBoard Board
        {
            get
            {
                lock (_lockObject)
                {
                    return (GameBoard) _gameBoard.Clone();
                }
            }
        }

        /// <summary>
        ///     Returns true if the game is over
        /// </summary>
        public bool IsOver
        {
            get
            {
                lock (_lockObject)
                {
                    return _gameOver;
                }
            }
        }

        /// <summary>
        ///     Sets the new direction of movement
        /// </summary>
        /// <param name="d">One of Top, Right, Bottom, Left</param>
        /// <returns>True if direction is valid</returns>
        public void Turn(Direction d)
        {
            if (d < Direction.First ||
                d > Direction.Last) return;

            lock (_lockObject)
            {
                if (_gameOver ||
                    d == _directionNext ||
                    d == _directionCurr.Opposite()) return;

                _directionNext = d;
            }
        }

        /// <summary>
        ///     Start making moves automatically
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                if (null != _tokenSource) return;
                _tokenSource = new CancellationTokenSource();
                if (!_gameOver) ScheduleNextRound();
            }
        }

        /// <summary>
        ///     Stop making moves automatically
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                if (null == _tokenSource) return;
                _tokenSource.Cancel();
                _tokenSource = null;
            }
        }

        /// <summary>
        ///     Play the next round manually
        /// </summary>
        public void Play()
        {
            lock (_lockObject)
            {
                // Manual usage when autoplay active is strictly prohibited
                if (_tokenSource != null) return;
                PlayInternals();
            }
        }

        /// <summary>
        ///     Speed up the snake when it grows.
        /// </summary>
        private void SpeedUp()
        {
            const uint maxSpeed = 600, minSpeed = 200;
            var t = (double) _gameBoard.Snake.Count /
                    (_gameBoard.GameBoardSize.Height *
                     _gameBoard.GameBoardSize.Width);
            _gameBoard.TimeUntilNextTurnMilliseconds =
                (int) (maxSpeed * (1 - t) + minSpeed * t);
        }

        /// <summary>
        ///     Create async task to perform one movement
        /// </summary>
        private void ScheduleNextRound()
        {
            if (null == _tokenSource) return;
            var token = _tokenSource.Token;
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(_gameBoard.TimeUntilNextTurnMilliseconds, token);
                if (!token.IsCancellationRequested) PlayScheduled();
            }, token);
        }

        /// <summary>
        ///     Places new piece of food in the empty cell if possible
        /// </summary>
        private void PlaceNewFood()
        {
            var gameBoardSize = _gameBoard.GameBoardSize.Height *
                                _gameBoard.GameBoardSize.Width;

            // if no empty cells
            if (_gameBoard.Snake.Count +
                _gameBoard.Food.Count == gameBoardSize) return;

            var newFood = new Point(
                Random.Next(_gameBoard.GameBoardSize.Width),
                Random.Next(_gameBoard.GameBoardSize.Height));

            // if selected cell is not empty - choose different one
            while (_gameBoard.Snake.Contains(newFood) ||
                   _gameBoard.Food.Contains(newFood))
            {
                newFood.X = Random.Next(_gameBoard.GameBoardSize.Width);
                newFood.Y = Random.Next(_gameBoard.GameBoardSize.Height);
            }

            _gameBoard.Food.Add(newFood);
        }

        /// <summary>
        ///     Calculates the cell where the snake would step
        /// </summary>
        private Point NewHead()
        {
            return _directionCurr switch
            {
                Direction.Top => new Point(
                    _gameBoard.Snake.First.Value.X,
                    _gameBoard.Snake.First.Value.Y - 1),
                Direction.Right => new Point(
                    _gameBoard.Snake.First.Value.X + 1,
                    _gameBoard.Snake.First.Value.Y),
                Direction.Bottom => new Point(
                    _gameBoard.Snake.First.Value.X,
                    _gameBoard.Snake.First.Value.Y + 1),
                Direction.Left => new Point(
                    _gameBoard.Snake.First.Value.X - 1,
                    _gameBoard.Snake.First.Value.Y),
                _ => throw new ArgumentException("Unsupported direction"),
            };
        }

        /// <summary>
        ///     One step of the game
        /// </summary>
        private void PlayInternals()
        {
            _logger.LogInformation("Snake moves");
            if (_gameOver) return;

            // calculate new shake head position
            _directionCurr = _directionNext;
            var head = NewHead();

            // if snake crosses the edge of the board or bites itself - game over
            if (head.X < 0 || head.X >= _gameBoard.GameBoardSize.Width ||
                head.Y < 0 || head.Y >= _gameBoard.GameBoardSize.Height ||
                _gameBoard.Snake.SkipLast(1).Contains(head))
            {
                _gameOver = true;
                return;
            }

            // place new head
            _gameBoard.Snake.AddFirst(head);

            // if ate some food
            if (_gameBoard.Food.Remove(head))
                // if there are no free cells - you win!
                // no code required in this situation because
                // during the next turn snake will bite itself
                // or cross the edge of the board, so the game
                // will over anyway (no winner notification required)

                // if there is some free space - place new food
                PlaceNewFood();
            // if new head is in empty cell - cut off the tip of the tail
            else
                _gameBoard.Snake.RemoveLast();

            // complete the move
            ++_gameBoard.TurnNumber;
            SpeedUp();
        }

        /// <summary>
        ///     Play the next round automatically
        /// </summary>
        private void PlayScheduled()
        {
            lock (_lockObject)
            {
                PlayInternals();
                ScheduleNextRound();
            }
        }
    }
}