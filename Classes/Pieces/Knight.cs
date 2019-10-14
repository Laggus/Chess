using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Chess.Classes.Pieces {
    class Knight : Piece, IPiece {
        int[,] PositionValues = {
            { -50, -40, -30, -30, -30, -30, -40, -50 },
            { -40, -20,   5,   0,   5,   0, -20, -40 },
            { -30,   0,  10,  15,  15,  10,   0, -30 },
            { -30,   5,  15,  20,  20,  15,   0, -30 },
            { -30,   5,  15,  20,  20,  15,   0, -30 },
            { -30,   0,  10,  15,  15,  10,   0, -30 },
            { -40, -20,   5,   0,   5,   0, -20, -40 },
            { -50, -40, -30, -30, -30, -30, -40, -50 }
        };


        public override int GetValue(int x, int y) => PositionValues[x, y] + 300;
        public string GetId() => $"knight_{Color}";

        public Image GetImage(Grid mainGrid, System.Windows.Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());

        public Knight(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.Knight;
        }

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();

            for ( int y = -1; y < 2; y += 2 ) {

                if ( _startX + 2 <= 7 && _startY + y >= 0 && _startY + y <= 7 ) if ( board.CheckSquareLegality((GetColor() == PieceColor.White), _startX + 2, _startY + y) ) {
                        possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + 2, _startY + y));
                    }
                if ( _startX - 2 >= 0 && _startY + y >= 0 && _startY + y <= 7 ) if ( board.CheckSquareLegality((GetColor() == PieceColor.White), _startX - 2, _startY + y) ) {
                        possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - 2, _startY + y));
                    }
            }
            for ( int x = -1; x < 2; x += 2 ) {
                if ( _startY + 2 <= 7 && _startX + x >= 0 && _startX + x <= 7 ) if ( board.CheckSquareLegality((GetColor() == PieceColor.White), _startX + x, _startY + 2) ) {
                        possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + x, _startY + 2));
                    }
                if ( _startY - 2 >= 0 && _startX + x >= 0 && _startX + x <= 7 ) if ( board.CheckSquareLegality((GetColor() == PieceColor.White), _startX + x, _startY - 2) ) {
                        possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + x, _startY - 2));
                    }
            }

            return possibleMoves;
        }

        public IPiece NewCopy() {
            return new Knight(Color);
        }

    }
}
