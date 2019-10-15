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
    enum PieceColor { Black, White }
    enum PieceType { Bishop, King, Knight, Pawn, Queen, Rook}
    interface IPiece {
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


        /*
        private int x;
        public void SetX(int x) { this.x = x; }
        public int GetX() { return x; }

        private int y;
        public void SetY(int y) { this.y = y; }
        public int GetY() { return y; }
        */
        public Board Board { get; set; }
        public bool Active { get; set; }


        protected PieceColor Color;
        public PieceColor GetColor() => Color;

        protected PieceType Type;
        public PieceType GetPieceType() => Type;

        protected bool hasMoved = false;
        public bool GetHasMoved() => hasMoved;
        public void SetHasMoved(bool _hasMoved)
        {
            hasMoved = _hasMoved;
        }
        public Square Square { get; set; }


        public void SetSquare(int x, int y) {
            Square = Board.GetSquare(x,y);
            Board.Squares[x, y].SetPiece((IPiece)this);
            Active = true;
            //SetX(x);
            //SetY(y);
        }


        public virtual int GetValue(int x, int y) => 0;
        public int GetValue(Square square) => GetValue(square.XPos, square.YPos);
        public int GetValue() => GetValue(Square);
        public int GetValueAlt() => GetValue(7-Square.XPos, 7-Square.YPos);

        
        /*
        protected void CopyToBoardBase(Board board, IPiece copy) {
            copy.Square = board.GetSquare(this.Square.XPos, this.Square.YPos);
            copy.Active = Active;
            if( copy.Active )
                copy.Square.Piece = copy;

            board.ActivePieces.Add(copy);
        }
        */

    }

}
