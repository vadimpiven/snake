using System.Windows;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // important! MVVM pattern followed
            DataContext = new MainViewModel();
        }
    }
}
