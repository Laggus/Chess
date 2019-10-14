using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess.Classes.Pieces {
    class Rook : Piece, IPiece {
        private readonly int[,] PositionValues = {
            { 0, -5, -5, -5, -5, -5,  5, 0 },
            { 0,  0,  0,  0,  0,  0, 10, 0 },
            { 0,  0,  0,  0,  0,  0, 10, 0 },
            { 5,  0,  0,  0,  0,  0, 10, 0 },
            { 5,  0,  0,  0,  0,  0, 10, 0 },
            { 0,  0,  0,  0,  0,  0, 10, 0 },
            { 0,  0,  0,  0,  0,  0, 10, 0 },
            { 0, -5, -5, -5, -5, -5,  5, 0 }
        };

        public override int GetValue(int x, int y) => PositionValues[x, y] + 500;

        public string GetId() => $"rook_{Color}";

        public Image GetImage(Grid mainGrid, System.Windows.Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();

            for ( int x = _startX + 1; x <= 7; x++ ) {
                if ( board.Squares[x, _startY].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, x, _startY));
                }
                else if ( PieceColor.White == (board.Squares[x, _startY].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, x, _startY));
                    break;
                }
                else {
                    break;
                }
            }
            for ( int x = _startX - 1; x >= 0; x-- ) {
                if ( board.Squares[x, _startY].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, x, _startY));
                }
                else if ( PieceColor.White == (board.Squares[x, _startY].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, x, _startY));
                    break;
                }
                else {
                    break;
                }
            }
            // Checking vertically
            for ( int y = _startY + 1; y <= 7; y++ ) {
                if ( board.Squares[_startX, y].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, y));
                }
                else if ( PieceColor.White == (board.Squares[_startX, y].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, y));
                    break;
                }
                else {
                    break;
                }
            }
            for ( int y = _startY - 1; y >= 0; y-- ) {
                if ( board.Squares[_startX, y].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, y));
                }
                else if ( PieceColor.White == (board.Squares[_startX, y].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, y));
                    break;
                }
                else {
                    break;
                }
            }

            return possibleMoves;
        }

        public Rook(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.Rook;
        }
        public IPiece NewCopy() {
            return new Rook(Color);
        }
    }
}
