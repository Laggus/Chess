using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess.Classes.Pieces {
    class Queen : Piece, IPiece {
        int[,] PositionValues = {
            { -20, -10, -10,  0, -5, -10, -10, -20 },
            { -10,   0,   5,  0,  0,   0,   0, -10 },
            { -10,   5,   5,  5,  5,   5,   0, -10 },
            { -5,    0,   5,  5,  5,   5,   0,  -5 },
            { -5,    0,   5,  5,  5,   5,   0,  -5 },
            { -10,   0,   5,  5,  5,   5,   0, -10 },
            { -10,   0,   0,  0,  0,   0,   0, -10 },
            { -20, -10, -10, -5, -5, -10, -10, -10 }
        };

        public override int GetValue(int x, int y) => PositionValues[x, y] + 900;
        public string GetId() => $"queen_{Color}";
        public Image GetImage(Grid mainGrid, System.Windows.Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());
        public Queen(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.Queen;
        }

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();
            int i = 1;
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
            // Bishop moves
            i = 1;
            while ( _startX + i <= 7 && _startY + i <= 7 ) {
                if ( board.Squares[_startX + i, _startY + i].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + i, _startY + i));
                }
                else if ( PieceColor.White == (board.Squares[_startX + i, _startY + i].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + i, _startY + i));
                    break;
                }
                else {
                    break;
                }
                i++;
            }
            i = 1;
            while ( _startX + i <= 7 && _startY - i >= 0 ) {
                if ( board.Squares[_startX + i, _startY - i].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + i, _startY - i));
                }
                else if ( PieceColor.White == (board.Squares[_startX + i, _startY - i].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + i, _startY - i));
                    break;
                }
                else {
                    break;
                }
                i++;
            }
            i = 1;
            while ( _startX - i >= 0 && _startY - i >= 0 ) {
                if ( board.Squares[_startX - i, _startY - i].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - i, _startY - i));
                }
                else if ( PieceColor.White == (board.Squares[_startX - i, _startY - i].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - i, _startY - i));
                    break;
                }
                else {
                    break;
                }
                i++;
            }
            i = 1;
            while ( _startX - i >= 0 && _startY + i <= 7 ) {
                if ( board.Squares[_startX - i, _startY + i].Piece == null ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - i, _startY + i));
                }
                else if ( PieceColor.White == (board.Squares[_startX - i, _startY + i].Piece.GetColor()) != (GetColor() == PieceColor.White) ) {
                    possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - i, _startY + i));
                    break;
                }
                else {
                    break;
                }
                i++;
            }

            return possibleMoves;
        }

        public IPiece NewCopy() {
            return new Queen(Color);
        }
    }
}
