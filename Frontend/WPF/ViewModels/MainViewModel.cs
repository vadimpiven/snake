using Prism.Commands;
using Prism.Mvvm;
using Snake.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Snake
{
    class MainViewModel : BindableBase
    {
        private int _width, _height;

        // static assets
        private static readonly ImageBrush headBrush = Application.Current.Resources["headBrush"] as ImageBrush;
        private static readonly ImageBrush tailBrush = Application.Current.Resources["tailBrush"] as ImageBrush;
        private static readonly ImageBrush foodBrush = Application.Current.Resources["foodBrush"] as ImageBrush;
        private static readonly ImageBrush noneBrush = Application.Current.Resources["noneBrush"] as ImageBrush;

        public MainViewModel()
        {
            Brushes = new ObservableCollection<ImageBrush>();

            ChangeDirectionCommand = new DelegateCommand<string>(
                direction => MainModel.ChangeDirection(direction),
                direction => true);

            MainModel.Subscribe(Update);
        }

        /// <summary>
        /// Command that is called from KeyBinding to change the snake movement direction
        /// </summary>
        public ICommand ChangeDirectionCommand { get; }

        /// <summary>
        /// Gameboard width
        /// </summary>
        public int Width
        {
            get => _width;
            private set => SetProperty(ref _width, value);
        }

        /// <summary>
        /// Gameboard height
        /// </summary>
        public int Height
        {
            get => _height;
            private set => SetProperty(ref _height, value);
        }

        /// <summary>
        /// Brushes which are used to draw each rectangle of gameboard line by line
        /// </summary>
        public ObservableCollection<ImageBrush> Brushes { get; }

        /// <summary>
        /// Update displayed gameboard
        /// </summary>
        /// <param name="board">fresh gameboard state</param>
        private void Update(GameBoard board)
        {
            Width = board.GameBoardSize.Width;
            Height = board.GameBoardSize.Height;

            Application.Current.Dispatcher.BeginInvoke(new Action<GameBoard>(board =>
            {
                Brushes.Clear();
                for (int i = 0; i < Width * Height; ++i) { Brushes.Add(noneBrush); }
                Brushes[board.Snake[0].X + board.Snake[0].Y * Width] = headBrush;
                for (int i = 1; i < board.Snake.Count; ++i)
                {
                    Brushes[board.Snake[i].X + board.Snake[i].Y * Width] = tailBrush;
                }
                for (int i = 0; i < board.Food.Count; ++i)
                {
                    Brushes[board.Food[i].X + board.Food[i].Y * Width] = foodBrush;
                }
            }), board);
        }
    }
}
