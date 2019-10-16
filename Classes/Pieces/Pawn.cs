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
    class Pawn : Piece, IPiece {

        int[,] PositionValues = {
            { 0,   5,   5,  0,  5, 10, 50, 0 },
            { 0,  10,  -5,  0,  5, 10, 50, 0 },
            { 0,  10, -10,  0, 10, 20, 50, 0 },
            { 0, -20,   0, 20, 25, 30, 50, 0 },
            { 0, -20,   0, 20, 25, 30, 50, 0 },
            { 0,  10, -10,  0, 10, 20, 50, 0 },
            { 0,  10,  -5,  0,  5, 10, 50, 0 },
            { 0,  10,   5,  0,  5, 10, 50, 0 }
        };
        public override int GetValue(int x, int y) => PositionValues[x, y] + 100;
        public string GetId() => $"pawn_{Color}";

        public Image GetImage(Grid mainGrid, System.Windows.Thickness _thickness) => Piece.GetImage(mainGrid, _thickness, GetId());

 
        public Pawn(PieceColor color) {
            this.Color = color;
            this.Type = PieceType.Pawn;
            GeneratePositionValues();
        }

        public List<Move.PieceMove> GetPossibleMoves(Board board, int _startX, int _startY) {
            var possibleMoves = new List<Move.PieceMove>();
            // Moving normally
            if ( (GetColor() == PieceColor.White) ) {
                if ( _startY == 1 )
                    if ( board.Squares[_startX, _startY + 2].Piece == null && board.Squares[_startX, _startY + 1].Piece == null ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, _startY + 2));
                if (_startY + 1 <= 7) if (board.Squares[_startX, _startY + 1].Piece == null)
                    {
                        Move.PieceMove move = new Move.PieceMove(_startX, _startY, _startX, _startY + 1);
                        if (_startY == 6) move.IsPromoting = true;
                        possibleMoves.Add(move); 
                    }
            }
            else {
                if ( _startY == 6 ) {
                    if ( board.Squares[_startX, _startY - 2].Piece == null && board.Squares[_startX, _startY - 1].Piece == null ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX, _startY - 2));
                }
                if (_startY - 1 >= 0) if (board.Squares[_startX, _startY - 1].Piece == null)
                    {
                        Move.PieceMove move = new Move.PieceMove(_startX, _startY, _startX, _startY - 1);
                        if (_startY == 1) move.IsPromoting = true;
                        possibleMoves.Add(move);
                    }
            }

            // Check taking piece
            if ( (GetColor() == PieceColor.White) ) {
                if ( _startX + 1 <= 7 && _startY + 1 <= 7 ) if ( board.Squares[_startX + 1, _startY + 1].Piece != null ) if ( PieceColor.White == (board.Squares[_startX + 1, _startY + 1].Piece.GetColor()) != (GetColor() == PieceColor.White) ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + 1, _startY + 1));
                if ( _startX - 1 >= 0 && _startY + 1 <= 7 ) if ( board.Squares[_startX - 1, _startY + 1].Piece != null ) if ( PieceColor.White == (board.Squares[_startX - 1, _startY + 1].Piece.GetColor()) != (GetColor() == PieceColor.White) ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - 1, _startY + 1));
            }
            else {
                if ( _startX + 1 <= 7 && _startY - 1 >= 0 ) if ( board.Squares[_startX + 1, _startY - 1].Piece != null ) if ( PieceColor.White == (board.Squares[_startX + 1, _startY - 1].Piece.GetColor()) != (GetColor() == PieceColor.White) ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX + 1, _startY - 1));
                if ( _startX - 1 >= 0 && _startY - 1 >= 0 ) if ( board.Squares[_startX - 1, _startY - 1].Piece != null ) if ( PieceColor.White == (board.Squares[_startX - 1, _startY - 1].Piece.GetColor()) != (GetColor() == PieceColor.White) ) possibleMoves.Add(Chess.Move.PieceMove.ConvertToMove(_startX, _startY, _startX - 1, _startY - 1));
            }

            return possibleMoves;
        }

        public IPiece NewCopy() {
            return new Pawn(Color);
        }

    }
}
