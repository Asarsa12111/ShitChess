using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessApp1
{

    /// Логика взаимодействия для GameOverWindow.xaml
 
    public partial class GameOverWindow : Window
    {
        public bool PlayAgain { get; private set; }

        public GameOverWindow(string resultMessage)
        {
            InitializeComponent();
            ResultText.Text = resultMessage;
        }

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            PlayAgain = true;
            DialogResult = true;
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

