using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChessApp1
{
    public partial class PawnPromotionWindow : Window
    {
        public string SelectedPiece { get; private set; }

        public PawnPromotionWindow(bool isWhite)
        {
            InitializeComponent();
            SetupButtons(isWhite);
        }

        private void SetupButtons(bool isWhite)
        {
            string prefix = isWhite ? "w" : "b";
            SetupButton(QueenButton, $"{prefix}q");
            SetupButton(RookButton, $"{prefix}r");
            SetupButton(BishopButton, $"{prefix}b");
            SetupButton(KnightButton, $"{prefix}n");
        }

        private void SetupButton(Button button, string imageName)
        {
            var imageSource = new BitmapImage(new Uri($"pack://application:,,,/Images/{imageName}.png"));
            var imageBrush = new ImageBrush(imageSource)
            {
                Stretch = Stretch.Uniform
            };
            button.Background = imageBrush;
        }

        private void QueenButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPiece = "Q";
            DialogResult = true;
            Close();
        }

        private void RookButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPiece = "R";
            DialogResult = true;
            Close();
        }

        private void BishopButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPiece = "B";
            DialogResult = true;
            Close();
        }

        private void KnightButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPiece = "N";
            DialogResult = true;
            Close();
        }
    }
}