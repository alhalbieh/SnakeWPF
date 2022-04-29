using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SnakeWPF
{
    public class SnakePart
    {
        public SolidColorBrush snakeBodyBrush = Brushes.Green;

        public SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;

        public UIElement UiElement { get; set; }

        public Point Position { get; set; }

        public bool IsHead { get; set; }
    }
}