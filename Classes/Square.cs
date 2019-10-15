using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess.Classes {
    class Square {
        public Image Image { get; set; }
        //public MainWindow.Piece type { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }

        public IPiece Piece { get; set; }

        public void SetPiece(IPiece piece) {
            Console.WriteLine();
            Console.Write(piece.Board);
            if(this.Piece != null) { this.Piece.Active = false; }
            piece.Square.Piece = null;
            piece.Square = this;
            this.Piece = piece;
        }

        //public Square(Image _image, MainWindow.Piece _type, int _xPos, int _yPos)
        public Square(Image _image, int _xPos, int _yPos) {
            Image = _image;
            //type = _type;
            XPos = _xPos;
            YPos = _yPos;
        }

        public new string ToString() {
            if ( Piece == null )
                return $"[{XPos},{YPos}] Empty Space";
            else
                return $"[{XPos},{YPos}] {Piece.GetId()}";
        }
    }
}
