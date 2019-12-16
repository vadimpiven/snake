using System.ComponentModel.DataAnnotations;

namespace Snake.Models
{
    /// <summary>
    ///     Wrapper for direction - for ease of json-deserealisation
    /// </summary>
    public class DirectionWrapper
    {
        [Required] public string Direction { get; set; }
    }
}