using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json.Linq;
using Snake.Controllers;
using Snake.Core;
using Snake.Models;
using Xunit;
using Xunit.Abstractions;

namespace SnakeTests
{
    public sealed class GameControllerTests
    {
        public GameControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private readonly ITestOutputHelper _output;

        public static IEnumerable<object[]> Directions()
        {
            yield return new object[] {"Top", true};
            yield return new object[] {"Right", true};
            yield return new object[] {"Bottom", true};
            yield return new object[] {"Left", true};
            yield return new object[] {"top", false};
            yield return new object[] {"1", false};
            yield return new object[] {"", false};
            yield return new object[] {"mx9gkZfHdm6\"%*SL", false};
        }

        [Theory]
        [MemberData(nameof(Directions))]
        public void Direction_ReturnsOk_WhenDirectionIsValid(string direction, bool isValid)
        {
            // ARRANGE
            var logger = new NullLogger<GameController>();
            var mock = new Mock<IGame>();
            var game = mock.Object;
            var controller = new GameController(logger, game);
            var obj = new JObject
            {
                ["direction"] = direction
            };
            var json = obj.ToString();

            // ACT
            // set the default behaviour - capitalise first letter of property name
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var dw = JsonSerializer.Deserialize<DirectionWrapper>(json, options);
            var response = controller.Direction(dw);

            // ASSERT
            if (isValid)
                Assert.IsType<OkResult>(response);
            else
                Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public void GameBoard_ResetsGame_WhenGameIsOver()
        {
            // ARRANGE
            var logger = new NullLogger<GameController>();
            var mock = new Mock<IGame>();
            mock.SetupGet(d => d.IsOver).Returns(true);
            var game = mock.Object;
            var controller = new GameController(logger, game);

            // ACT
            var response = controller.GameBoard() as ObjectResult;

            // ASSERT
            Assert.IsType<OkObjectResult>(response);
            var ex = Record.Exception(() => { mock.Verify(m => m.Reset(), Times.Once()); });
            Assert.Null(ex);
        }

        [Fact]
        public void GameBoard_ReturnsOkObject_Always()
        {
            // ARRANGE
            var logger = new NullLogger<GameController>();
            var mock = new Mock<IGame>();
            var game = mock.Object;
            var controller = new GameController(logger, game);

            // ACT
            var response = controller.GameBoard() as ObjectResult;

            // ASSERT
            Assert.IsType<OkObjectResult>(response);
        }
    }
}