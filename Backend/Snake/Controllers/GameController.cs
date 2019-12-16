using System.Net.Mime;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Snake.Core;
using Snake.Models;

namespace Snake.Controllers
{
    [ApiController]
    [Route("/api/[action]")]
    public class GameController : ControllerBase
    {
        private readonly IGame _game;
        private readonly ILogger<GameController> _logger;

        public GameController(ILogger<GameController> logger, IGame game)
        {
            _logger = logger;
            _game = game;
        }

        /// <summary>
        ///     Return the game state, starts the game if non was started,
        ///     restarts the game if the previous is over
        /// </summary>
        /// <returns>Game state</returns>
        [HttpGet]
        [EnableCors]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(GameBoard), StatusCodes.Status200OK)]
        public IActionResult GameBoard()
        {
            _logger.LogInformation("Board requested");
            if (_game.IsOver) _game.Reset();
            return Ok(_game.Board);
        }

        /// <summary>Changes the derection of snake movement</summary>
        /// <param name="dw">
        ///     String representation of direction,
        ///     should be one of: Top, Right, Bottom or Left
        /// </param>
        [HttpPost]
        [EnableCors]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Direction([FromBody] DirectionWrapper dw)
        {
            var direction = dw.Direction switch
            {
                "Top" => Models.Direction.Top,
                "Right" => Models.Direction.Right,
                "Bottom" => Models.Direction.Bottom,
                "Left" => Models.Direction.Left,
                _ => Models.Direction.None,
            };

            if (direction == Models.Direction.None)
            {
                _logger.LogInformation("Incorrect direction: {direction}", dw.Direction);
                return BadRequest();
            }

            _logger.LogInformation("Got new direction: {direction}", dw.Direction);
            _game.Turn(direction);
            return Ok();
        }
    }
}