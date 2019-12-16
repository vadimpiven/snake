namespace Snake.Models
{
    /// <summary>
    ///     All possible directions of movement
    /// </summary>
    public enum Direction
    {
        None,

        First,

        Top = First,
        Right,
        Bottom,
        Left,

        Last = Left
    }

    internal static class DirectionMethods
    {
        /// <summary>
        ///     Direction, opposite to given
        /// </summary>
        /// <param name="d">Current direction</param>
        /// <returns>Opposite direction</returns>
        public static Direction Opposite(this Direction d)
        {
            return d switch
            {
                Direction.Top => Direction.Bottom,
                Direction.Right => Direction.Left,
                Direction.Bottom => Direction.Top,
                Direction.Left => Direction.Right,
                _ => Direction.None,
            };
        }
    }
}