using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess.Classes.Pieces {
    class King : Piece, IPiece {
        int[,] PositionValues = {
            { 20, 20, -10, -20, -30, -30, -30, -30 },
            { 30, 20, -20, -30, -40, -40, -40, -40 },
            { 10,  0, -20, -30, -40, -40, -40, -40 },
            {  0,  0, -20, -40, -50, -50, -50, -50 },
            {  0,  0, -20, -40, -50, -50, -50, -50 },
            { 10,  0, -20, -30, -40, -40, -40, -40 },
            { 30, 20, -20, -30, -40, -40, -40, -40 },
            { 20, 20, -10, -20, -30, -30, -30, -30 }
        };

        public override int GetValue(int x, int y) => PositionValues[x, y] + 10000;

        public string GetId() => $"king_{Color}";

        public Image GetImage(Grid mainGrid, System.Windows.Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());

        public King(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.King;
        }

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();

            for ( int x = -1; x < 2; x++ ) {
                for ( int y = -1; y < 2; y++ ) {
                    if ( x != 0 || y != 0 )
                        if ( _startX + x <= 7 && _startX + x >= 0 && _startY + y <= 7 && _startY + y >= 0 )
                            if ( board.CheckSquareLegality((GetColor() == PieceColor.White), _startX + x, _startY + y) ) {
                                possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + x, _startY + y));
                            }
                }
            }

            //// Check castling
            //int KingHeight = (GetColor() == PieceColor.White) ? 0 : 7;
            //if (!GetHasMoved())
            //{
            //    if (board.GetSquare(0, KingHeight).Piece != null)
            //        if (!board.GetSquare(0, KingHeight).Piece.GetHasMoved() && board.GetSquare(0, KingHeight).Piece.GetPieceType() == PieceType.Rook)
            //            if (board.GetSquare(1, KingHeight).Piece == null && board.GetSquare(2, KingHeight).Piece == null && board.GetSquare(3, KingHeight).Piece == null)
            //            {
            //                Move.PieceMove Move = Chess.Move.PieceMove.ConvertToMove(_startX, _startY, 2, KingHeight);
            //                Move.IsCastling = true;
            //                possibleMoves.Add(Move);
            //            }
            //    if (board.GetSquare(7, KingHeight).Piece != null)
            //        if (!board.GetSquare(7, KingHeight).Piece.GetHasMoved() && board.GetSquare(7, KingHeight).Piece.GetPieceType() == PieceType.Rook)
            //            if (board.GetSquare(5, KingHeight).Piece == null && board.GetSquare(6, KingHeight).Piece == null)
            //            {
            //                Move.PieceMove Move = Chess.Move.PieceMove.ConvertToMove(_startX, _startY, 6, KingHeight);
            //                Move.IsCastling = true;
            //                possibleMoves.Add(Move);
            //            }
            //}

            return possibleMoves;
        }
        public IPiece NewCopy() {
           return new King(Color);
        }
    }
}
