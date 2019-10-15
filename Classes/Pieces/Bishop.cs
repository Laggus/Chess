using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Chess.Classes.Pieces {
    class Bishop : Piece, IPiece {
        int[,] PositionValues =  {
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -10,   5,  10,   0,   5,   0,   0, -10 },
            { -10,   0,  10,  10,   5,   5,   0, -10 },
            { -10,   0,  10,  10,  10,  10,   0, -10 },
            { -10,   0,  10,  10,  10,  10,   0, -10 },
            { -10,   0,  10,  10,   5,   5,   0, -10 },
            { -10,   5,  10,   0,   5,   0,   0, -10 },
            { -10, -10, -10, -10, -10, -10, -10, -20 }
        };

        public override int GetValue(int x, int y) => PositionValues[x, y] + 300;

        public string GetId() => $"bishop_{Color}";
        public Image GetImage(Grid mainGrid, Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());

        public Bishop(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.Bishop;
        }

        public IPiece NewCopy() {
            return new Bishop(Color);
        }

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();

            int i = 1;
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

    }
}
