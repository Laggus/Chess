using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    class SelectedPiece
    {
        public Image image;
        public int startX { get; set; }
        public int startY { get; set; }

        public SelectedPiece(Image _image, int _startX, int _startY)
        {
            image = _image;
            startX = _startX;
            startY = _startY;
        }
    }
}
