using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Snake.Core;
using Snake.Models;
using Xunit;
using Xunit.Abstractions;

namespace SnakeTests
{
    public sealed class GameTests
    {
        public GameTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        [Theory]
        [InlineData(Direction.None)]
        [InlineData((Direction) 10)]
        [InlineData(Direction.Bottom)]
        public void Game_TurnDoesNothing_WhenInputIsInvalidOrOpposite(Direction d)
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            game.Turn(d);
            var before = game.Board.Snake.First.Value;
            game.Play();
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_Autoplay_ByDefault()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_ContinuesPlay_AfterCallingStartWhileGameIsNotOver()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Start();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_DoesNothing_AfterCallingStartWhileGameIsOver()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Start();
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_DoesNothing_WhenAutoplayIsOff()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(before, after);
        }

        [Fact]
        public void Game_DoNotChange_WhenTheGameIsOver()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            while (!game.IsOver) game.Play();
            var before = game.Board.Snake.First.Value;
            game.Play();
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(before, after);
        }

        [Fact]
        public async void Game_DoNotPlay_AfterCallingStop()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Stop();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(before, after);
        }

        [Fact]
        public void Game_Finishes_WhenSnakeCrossesTheBoardEdge()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            for (var snake = game.Board.Snake.First.Value;
                snake.Y > 0;
                snake = game.Board.Snake.First.Value) game.Play();
            game.Play();

            // ASSERT
            Assert.True(game.IsOver);
        }

        [Fact]
        public void Game_IncreasesTurnNumber_AfterMoveWhenGameIsNotOver()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            var turnNumberBeforeMove = game.Board.TurnNumber;
            game.Play();
            var turnNumberAfterMove = game.Board.TurnNumber;

            // ASSERT
            Assert.Equal(turnNumberBeforeMove + 1, turnNumberAfterMove);
        }

        [Fact]
        public void Game_IsOver_WhenCreated()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            var gameOver = game.IsOver;

            // ASSERT
            Assert.True(gameOver);
        }

        [Fact]
        public void Game_MakesOneMove_DuringMove()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;

            game.Play();
            board = game.Board;
            var after = board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_MakesOneMove_DuringTurn()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public void Game_MovesSnakeToTop_AfterReset()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;

            game.Play();
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.True(after.X == before.X);
            Assert.True(after.Y < before.Y);
        }

        [Fact]
        public void Game_NotIncreasesTurnNumber_AfterMoveWhenGameIsOver()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            var turnNumberBeforeMove = game.Board.TurnNumber;
            game.Play();
            var turnNumberAfterMove = game.Board.TurnNumber;

            // ASSERT
            Assert.Equal(turnNumberBeforeMove, turnNumberAfterMove);
        }

        [Fact]
        public void Game_OnePeaceOfFoodOnSnakeTrajectory_AfterReset()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            var board = game.Board;
            var snake = board.Snake.First.Value;
            var foodOnTrajectory = board
                .Food
                .Where(f => f.X == snake.X && f.Y < snake.Y)
                .ToList();

            // ASSERT
            Assert.NotEmpty(foodOnTrajectory);
        }

        [Fact]
        public async void Game_SchedulesNewMovement_WhenStartingStoppedGame()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Stop();
            game.Start();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_SchedulesOneStep_WhenResetTwice()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Reset();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public void Game_SnakeGrows_WhenEatsFood()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();

            var board = game.Board;
            var snake = board.Snake.First.Value;
            var nearestFoodOnTrajectory = board
                .Food
                .Where(f => f.X == snake.X && f.Y < snake.Y)
                .Aggregate((max, p) => null == max || p.Y > max.Y ? p : max);

            for (var head = game.Board.Snake.First;
                1 + nearestFoodOnTrajectory.Y != head.Value.Y;
                head = game.Board.Snake.First) game.Play();

            var boardBeforeMove = game.Board;
            game.Play();
            var boardAfterMove = game.Board;

            // ASSERT
            Assert.Equal(boardBeforeMove.Snake.Count + 1,
                boardAfterMove.Snake.Count);
            Assert.Equal(nearestFoodOnTrajectory,
                boardAfterMove.Snake.First.Value);
            Assert.Equal(boardBeforeMove.Snake.Last.Value,
                boardAfterMove.Snake.Last.Value);
        }

        [Fact]
        public void Game_SnakeTurnsLeft_WhenPossible()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);
            game.Stop();

            // ACT
            game.Reset();
            game.Turn(Direction.Left);
            var before = game.Board.Snake.First.Value;
            game.Play();
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(before.Y, after.Y);
            Assert.Equal(before.X - 1, after.X);
        }

        [Fact]
        public async void Game_StartKeepsIdempotent_WhenCalledTwice()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Start();
            game.Start();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(1, before.Y - after.Y);
        }

        [Fact]
        public async void Game_StopKeepsIdempotent_WhenCalledTwice()
        {
            // ARRANGE
            var logger = new NullLogger<Game>();
            var game = new Game(logger);

            // ACT
            game.Reset();
            game.Stop();
            game.Stop();
            var board = game.Board;
            var before = board.Snake.First.Value;
            await Task.Delay(board.TimeUntilNextTurnMilliseconds);
            var after = game.Board.Snake.First.Value;

            // ASSERT
            Assert.Equal(before, after);
        }
    }
}