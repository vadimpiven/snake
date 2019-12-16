using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Threading.Tasks;

namespace Snake.Models
{
    /// <summary>
    /// Data format of GET gameboard request
    /// </summary>
    public class GameBoard
    {
        public int TurnNumber { get; set; }
        public int TimeUntilNextTurnMilliseconds { get; set; }
        public Size GameBoardSize { get; set; }
        public List<Point> Snake { get; set; }
        public List<Point> Food { get; set; }
    }

    public static class MainModel
    {
        private static RestClient client = new RestClient(ConfigurationManager.AppSettings["baseUrl"]);

        private static Action<GameBoard> Notify;

        /// <summary>
        /// Subscribe for gameboard updates
        /// </summary>
        /// <param name="callback">function that should be called when new data recieved</param>
        public static void Subscribe(Action<GameBoard> callback)
        {
            Notify += callback;
            if (Notify.GetInvocationList().Length == 1) { GetBoard(); }
        }

        /// <summary>
        /// Performs GET gameboard request
        /// </summary>
        private static void GetBoard()
        {
            var request = new RestRequest("gameboard", Method.GET);
            client.ExecuteAsync<GameBoard>(request, response => {
                if (null == response.ErrorException)
                {
                    SheduleUpdate(response.Data.TimeUntilNextTurnMilliseconds);
                    Notify(response.Data);
                }
                else
                {
                    SheduleUpdate(1000);
                }
            });
        }

        /// <summary>
        /// Wait given delay, then call GetBoard
        /// </summary>
        /// <param name="delay">time in milliseconds</param>
        private static void SheduleUpdate(int delay)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(delay);
                GetBoard();
            });
        }

        /// <summary>
        /// Performs POST direction request
        /// </summary>
        /// <param name="direction"></param>
        public static void ChangeDirection(string direction)
        {
            var request = new RestRequest("direction", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { Direction = direction });
            client.ExecuteAsync(request, response => { });
        }
    }
}
