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

            return possibleMoves;
        }
        public IPiece NewCopy() {
           return new King(Color);
        }
    }
}
