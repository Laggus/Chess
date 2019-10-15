using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Chess.MainWindow;

namespace Chess.Classes {
    class Board {
        public object ThreadLock { get; set; } = new object();

        public Square[,] Squares { get; set; } = new Square[8, 8];

        public Square GetSquare(int x, int y) {
            return Squares[x, y];
        }
        public Square GetSquare(Move.PieceMove move) => GetSquare(move.StartX, move.StartY);
        public Square GetSquare(Square square) => GetSquare(square.XPos, square.YPos);

        public IPiece GetPiece(int x, int y) => GetSquare(x, y).Piece;
        public IPiece GetPiece(Move.PieceMove move) => GetSquare(move).Piece;


        public List<IPiece> ActivePieces { get; set; } = new List<IPiece>();
        public int TurnNumber { get; set; } = 1;
        public PieceColor CurrentTurn = PieceColor.White;
        public void SwitchTurn() {
            if ( CurrentTurn == PieceColor.White )
                CurrentTurn = PieceColor.Black;
            else
                CurrentTurn = PieceColor.White;

            TurnNumber++;
        }

        Thickness GetThicknessOfPos(Grid mainGrid, int _row, int _column) {
            return new Thickness(-mainGrid.ActualWidth - mainGrid.ActualWidth / 8 + mainGrid.ActualWidth / 4 * (_row + 1), +mainGrid.ActualHeight + mainGrid.ActualHeight / 8 - mainGrid.ActualHeight / 4 * (_column + 1), 0, 0);
        }

        public void Draw(Grid mainGrid, List<Image> dots) {
            Console.WriteLine("UpdateVisualBoard");
            foreach ( Image dot in dots ) mainGrid.Children.Remove(dot);

            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    if ( Squares[x, y].Image != null ) {
                        mainGrid.Children.Remove(Squares[x, y].Image);
                        Squares[x, y].Image = null;
                    }

                    Squares[x, y].Image = this.GetSquare(x, y).Piece?.GetImage(mainGrid, GetThicknessOfPos(mainGrid, x, y));

                }
            }
        }


        public void AddPiece(int x, int y, IPiece piece) {// IPiece piece) {
            ActivePieces.Add(piece);
            piece.SetSquare(x, y);
            //piece.SetX(x);
            //piece.SetY(y);
            //Squares[x, y].Piece = piece;
        }

        public Board(Canvas background, Grid mainGrid, List<Image> dots) {
            BitmapImage source = new BitmapImage(new Uri(Path.GetFullPath("Images/Blank_Chess_Board.png"), UriKind.Absolute));
            background.Background = new ImageBrush(source);
            background.Width = mainGrid.ActualWidth;
            background.Height = mainGrid.ActualHeight;


            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    Squares[x, y] = new Square(null, x, y);
                }
            }

            for ( int i = 0; i < 8; i++ ) {
                AddPiece(i, 1, new Pawn(PieceColor.White) { Board = this });
                AddPiece(i, 6, new Pawn(PieceColor.Black) { Board = this });
            }

            AddPiece(0, 0, new Rook(PieceColor.White) { Board = this });
            AddPiece(7, 0, new Rook(PieceColor.White) { Board = this });
            AddPiece(0, 7, new Rook(PieceColor.Black) { Board = this });
            AddPiece(7, 7, new Rook(PieceColor.Black) { Board = this });
            AddPiece(1, 0, new Knight(PieceColor.White) { Board = this });
            AddPiece(6, 0, new Knight(PieceColor.White) { Board = this });
            //AddPiece(1, 7, new Knight(PieceColor.Black) { Board = this });
            //AddPiece(6, 7, new Knight(PieceColor.Black) { Board = this });
            AddPiece(2, 0, new Bishop(PieceColor.White) { Board = this });
            AddPiece(5, 0, new Bishop(PieceColor.White) { Board = this });
            //AddPiece(2, 7, new Bishop(PieceColor.Black) { Board = this });
            //AddPiece(5, 7, new Bishop(PieceColor.Black) { Board = this });
            //AddPiece(3, 0, new Queen(PieceColor.White) { Board = this });
            AddPiece(3, 7, new Queen(PieceColor.Black) { Board = this });
            AddPiece(4, 0, new King(PieceColor.White) { Board = this });
            AddPiece(4, 7, new King(PieceColor.Black) { Board = this });

            Draw(mainGrid, dots);
        }
        public Board() { }

        public bool CheckSquareLegality(bool _isUpper, int _targetX, int _targetY) {
            if ( Squares[_targetX, _targetY].Piece == null ) return true;

            else if ( Squares[_targetX, _targetY].Piece.GetColor() != (_isUpper ? PieceColor.White : PieceColor.Black) ) return true;
            return false;
        }

        public void MovePiece(int xStart, int yStart, int xEnd, int yEnd) {
            var piece = GetPiece(xStart, yStart);
            piece.SetHasMoved(true);

            var startSquare = GetSquare(xStart, yStart);
            var endSquare = GetSquare(xEnd, yEnd);

            if(endSquare.Piece != null) {
                endSquare.Piece.Square = null;
                endSquare.Piece.Active = false;
            }

            startSquare.SetPiece(null);
            endSquare.SetPiece(piece);
        }

        private void MovePiece(Move.PieceMove _move, IPiece _startChar) {
            _startChar.SetHasMoved(true);
            PieceType pieceType = _startChar.GetPieceType();
            if ( pieceType == PieceType.King ) {
                // If castling
                if ( Math.Abs(_move.StartX - _move.EndX) > 1 ) {
                    // If Doing it aka not undoing it
                    if ( _move.StartX == 4 ) {
                        if ( _move.EndX == 6 ) MovePiece(new Move.PieceMove(7, _move.StartY, 5, _move.StartY));
                        //GetSquare(5, _move.StartY).SetPiece(GetPiece(7, _move.StartY));
                        if ( _move.EndX == 2 ) MovePiece(new Move.PieceMove(0, _move.StartY, 3, _move.StartY));
                        //GetSquare(3, _move.StartY).SetPiece(GetPiece(0, _move.StartY));
                    }
                    else {
                        if ( GetPiece(5, _move.StartY) == null )
                            Console.WriteLine("Potatoes");
                        if ( _move.StartX == 6 ) GetSquare(7, _move.StartY).SetPiece(GetPiece(5, _move.StartY));
                        if ( _move.StartX == 2 ) GetSquare(0, _move.StartY).SetPiece(GetPiece(3, _move.StartY));
                    }
                }
            }
            //    GetSquare(_move.EndX, _move.EndY).SetPiece(GetPiece(_move));
            //this.Squares[_move.EndX, _move.EndY].SetPiece(this.Squares[_move.StartX, _move.StartY].Piece);
            GetSquare(_move.EndX, _move.EndY).SetPiece(GetPiece(_move));

        }
        public void MovePiece(Move.PieceMove move) {
            if ( GetPiece(move) == null ) {
                Console.WriteLine("Potatoes");
            }
            MovePiece(move, GetPiece(move.StartX, move.StartY));
        }
        public void MovePieceReverse(Move.PieceMove move) {
            MovePiece(new Move.PieceMove(move.EndX, move.EndY, move.StartX, move.StartY));
        }


        public List<Move.PieceMove> GetPossibleMovesForPiece(int _startX, int _startY) {
            return GetSquare(_startX, _startY).Piece.GetPossibleMoves(this, _startX, _startY);
        }
        public List<Move.PieceMove> GetAllPossibleMoves(PieceColor currentTurn) {
            List<Move.PieceMove> possibleMoves = new List<Move.PieceMove>();
            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    if ( this.Squares[x, y].Piece?.GetColor() == currentTurn ) {
                        List<Move.PieceMove> possibleMovesForPiece = GetPossibleMovesForPiece(x, y);
                        foreach ( Move.PieceMove move in possibleMovesForPiece ) possibleMoves.Add(move);
                    }
                }
            }
            return possibleMoves;
        }
        public List<Move.PieceMove> GetAllPossibleMoves() => GetAllPossibleMoves(CurrentTurn);

        public double EvaluateBoard(bool _color) {
            double value = 0;

            /*
            board.ActivePieces.ForEach(item => {
                if( item.GetColor() == PieceColor.White)
                    value += item.GetValue();
                else
                    value -= item.GetValueAlt();
            });
            */


            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    if ( this.Squares[x, y].Piece == null ) continue;

                    //if ( board.Squares[x, y].Piece.GetColor() == PieceColor.White )
                    //    value += board.GetSquare(x, y).Piece.GetValue();
                    //else
                    //    value -= board.GetSquare(x, y).Piece.GetValueAlt();

                    if ( this.Squares[x, y].Piece.GetColor() == PieceColor.White )
                        value += this.GetSquare(x, y).Piece.GetValue(x, y);
                    else
                        value -= this.GetSquare(x, y).Piece.GetValue(7 - x, 7 - y);

                }
            }


            if ( !_color ) value *= -1;
            return value;
        }
        public double EvaluateBoard(PieceColor color) => EvaluateBoard(color == PieceColor.White);
        public double EvaluateBoard() => EvaluateBoard(CurrentTurn);

        public Board Clone() {
            var boardClone = new Board();
            boardClone.CurrentTurn = CurrentTurn;

            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    boardClone.Squares[x, y] = new Square(null, x, y);
                }
            }

            this.ActivePieces.ForEach(piece => {
                var copy = piece.NewCopy();
                copy.Active = piece.Active;
                copy.Square = boardClone.GetSquare(piece.Square);
                if ( copy.Active ) boardClone.GetSquare(copy.Square).Piece = copy;
                boardClone.ActivePieces.Add(copy);
                copy.Board = boardClone;
            });


            return boardClone;
        }
        public override string ToString() {
            string output = "";
            for(int i=7;i>=0;i--) {
                for(var j=0; j<8; j++)
                    output += $"{Squares[j,i].ToString()},\t";
                output += "\n";
            }
            return output;
        }
    }
}
