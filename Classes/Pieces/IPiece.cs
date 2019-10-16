using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Chess.Classes.Pieces {
    enum PieceColor { Black = 0x00, White = 0x10 }
    enum PieceType { None=0x00, Bishop = 0x01, King = 0x02, Knight = 0x03, Pawn = 0x04, Queen = 0x05, Rook = 0x06 }
    interface IPiece {
        byte GetByteData();
        byte PositionData { get; set; }


        void SetSquare(int x, int y);

        int GetValue(int x, int y);
        int GetValue();
        int GetValueAlt();

        PieceColor GetColor();
        PieceType GetPieceType();
        bool GetHasMoved();
        void SetHasMoved(bool _hasMoved);
        Image GetImage(Grid mainGrid, Thickness _thickness);

        //string GetId();
        string GetId();

        List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY);

        /*
        void SetX(int x);
        int GetX();
        void SetY(int y);
        int GetY();
        */

        bool Active { get; set; }
        Square Square { get; set; }
        IPiece NewCopy();

        Board Board { get; set; }
    }

    class Piece {
        public static Image GetImage(Grid mainGrid, Thickness _thickness, string id) {

            Image tmpImg = new Image {
                Source = new BitmapImage(new Uri(Path.GetFullPath($"images/Chess_{id}.png"), UriKind.Absolute)),
                Width = mainGrid.ActualWidth / 8,
                Height = mainGrid.ActualHeight / 8,
            };
            mainGrid.Children.Add(tmpImg);
            tmpImg.Margin = _thickness;
            return tmpImg;
        }

        public byte GetByteData() {
            return (byte)((byte)GetPieceType() | (byte)GetColor());
        }
        public byte PositionData {
            get {
                if ( this.Square == null ) return 0xFF;
                else return (byte)(Square.YPos * 8 + Square.XPos);
            }
            set {
                if ( value == 0xFF ) Square = null;
                SetSquare(value % 8, value / 8);
            }
        }

        public Board Board { get; set; }
        public bool Active { get; set; } // Change get to [return Square != null], remove set


        protected PieceColor Color;
        public PieceColor GetColor() => Color;

        protected PieceType Type;
        public PieceType GetPieceType() => Type;

        protected bool hasMoved = false;
        public bool GetHasMoved() => hasMoved;
        public void SetHasMoved(bool _newValue) { hasMoved = _newValue; }
        public Square Square { get; set; }


        public void SetSquare(int x, int y) {
            Square = Board.GetSquare(x, y);
            Board.Squares[x, y].SetPiece((IPiece)this);
            Active = true;
        }


        public virtual int GetValue(int x, int y) => 0;
        public int GetValue(Square square) => GetValue(square.XPos, square.YPos);
        public int GetValue() => GetValue(Square);
        public int GetValueAlt() => GetValue(7 - Square.XPos, 7 - Square.YPos);


    }

}
