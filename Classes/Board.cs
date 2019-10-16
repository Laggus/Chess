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

        public Dictionary<string, int> BoardHistory = new Dictionary<string, int>();

        public bool ThreefoldRep { get; set; } = false;

        public Square GetSquare(int x, int y) {
            return Squares[x, y];
        }
        public Square GetSquare(Move.PieceMove move) => GetSquare(move.StartX, move.StartY);
        public Square GetSquare(Square square) => GetSquare(square.XPos, square.YPos);

        public IPiece GetPiece(int x, int y) => GetSquare(x, y).Piece;
        public IPiece GetPiece(Move.PieceMove move) => GetSquare(move).Piece;


        public IPiece[] AllPieces { get; set; } = new IPiece[0];
        public List<IPiece> ActivePieces { get; set; } = new List<IPiece>();
        public bool[] ActiveKings { get; set; } = new bool[2] { true, true };
        public int TurnNumber { get; set; } = 1;
        public PieceColor CurrentTurn = PieceColor.White;
        public void SwitchTurn() {
            if ( CurrentTurn == PieceColor.White )
                CurrentTurn = PieceColor.Black;
            else
                CurrentTurn = PieceColor.White;
            //bool[][][,] boolArray = GetAsBoolArray();
            //string boolString = GetAsBoolString(boolArray);
            string boolString = PositionalDataString; //GetDataString();
            if ( !BoardHistory.ContainsKey(boolString) ) {
                BoardHistory.Add(boolString, 1);
            }
            else {
                BoardHistory[boolString]++;
                if ( BoardHistory[boolString] == 3 ) ThreefoldRep = true;
            }
            TurnNumber++;
        }

        Thickness GetThicknessOfPos(Grid mainGrid, int _row, int _column) {
            return new Thickness(-mainGrid.ActualWidth - mainGrid.ActualWidth / 8 + mainGrid.ActualWidth / 4 * (_row + 1), +mainGrid.ActualHeight + mainGrid.ActualHeight / 8 - mainGrid.ActualHeight / 4 * (_column + 1), 0, 0);
        }

        public void Draw(Grid mainGrid, List<Image> dots) {
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
            AllPieces = new IPiece[AllPieces.Length + 1];
            AllPieces[AllPieces.Length - 1] = piece;
            ActivePieces.Add(piece);
            piece.SetSquare(x, y);
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
            AddPiece(1, 7, new Knight(PieceColor.Black) { Board = this });
            AddPiece(6, 7, new Knight(PieceColor.Black) { Board = this });
            AddPiece(2, 0, new Bishop(PieceColor.White) { Board = this });
            AddPiece(5, 0, new Bishop(PieceColor.White) { Board = this });
            AddPiece(2, 7, new Bishop(PieceColor.Black) { Board = this });
            AddPiece(5, 7, new Bishop(PieceColor.Black) { Board = this });
            AddPiece(3, 0, new Queen(PieceColor.White) { Board = this });
            AddPiece(3, 7, new Queen(PieceColor.Black) { Board = this });
            AddPiece(4, 0, new King(PieceColor.White) { Board = this });
            AddPiece(4, 7, new King(PieceColor.Black) { Board = this });

            Draw(mainGrid, dots);
            foreach(Piece piece in ActivePieces) piece.GeneratePositionValues();

        }
        public Board() { }

        public void ClearBoard() {
            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                }
            }
        }

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

            if ( endSquare.Piece != null ) {
                endSquare.Piece.Square = null;
                endSquare.Piece.Active = false;
            }

            startSquare.SetPiece(null);
            endSquare.SetPiece(piece);
        }

        private void MovePiece(Move.PieceMove _move, IPiece _startChar) {
            _startChar.SetHasMoved(true);
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

        public void Castle(Move.PieceMove _move) {
            if ( _move.EndX == 6 ) {
                MovePiece(new Move.PieceMove(7, _move.StartY, 5, _move.StartY));
                MovePiece(_move);
            }
            if ( _move.EndX == 2 ) {
                MovePiece(new Move.PieceMove(0, _move.StartY, 3, _move.StartY));
                MovePiece(_move);
            }
        }
        public void UndoCastle(Move.PieceMove _move) {
            if ( _move.EndX == 6 ) {
                MovePieceReverse(new Move.PieceMove(7, _move.StartY, 5, _move.StartY));
                MovePieceReverse(_move);
            }
            if ( _move.EndX == 2 ) {
                MovePieceReverse(new Move.PieceMove(0, _move.StartY, 3, _move.StartY));
                MovePieceReverse(_move);
            }
            GetPiece(_move.StartX, _move.StartY).SetHasMoved(false);
            GetPiece((_move.EndX == 6) ? 7 : 0, _move.StartY).SetHasMoved(false);
        }

        public void DoMove(Move.PieceMove _move, bool _reverseing) {
            if ( !_move.IsCastling ) {
                if ( !_reverseing ) MovePiece(_move);
                else MovePieceReverse(_move);
            }
            else Castle(_move);
        }

        public List<Move.PieceMove> GetPossibleMovesForPiece(int _startX, int _startY) {
            return GetSquare(_startX, _startY).Piece.GetPossibleMoves(this, _startX, _startY);
        }
        public List<Move.PieceMove> GetAllPossibleMoves(PieceColor currentTurn) {
            List<Move.PieceMove> possibleMoves = new List<Move.PieceMove>();
            // If one of the kings dead -> No possible moves
            if ( CheckIfDone() ) return possibleMoves;
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
                    //Console.WriteLine(this.GetSquare(x, y).Piece.GetColor() == PieceColor.White ? 1 : 0);
                    //value += this.GetSquare(x, y).Piece.GetDefinedValue(x, y);
                    
                    if ( this.Squares[x, y].Piece.GetColor() == PieceColor.White)
                        value += this.GetSquare(x, y).Piece.GetDefinedValue(x, y);
                        //
                    else
                        value -= this.GetSquare(x, y).Piece.GetDefinedValue(7 - x, 7 - y);
                      
                }
            }


            if ( !_color ) value *= -1;
            return value;
        }
        public double EvaluateBoard(PieceColor color) => EvaluateBoard(color == PieceColor.White);
        public double EvaluateBoard() => EvaluateBoard(CurrentTurn);

        public bool CheckIfDone() {
            if ( ActiveKings[0] == false || ActiveKings[1] == false ) return true;
            return false;
        }

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
                copy.DefinedValues = piece.DefinedValues;
                copy.Active = piece.Active;
                copy.Square = boardClone.GetSquare(piece.Square);
                if ( copy.Active ) boardClone.GetSquare(copy.Square).Piece = copy;
                boardClone.ActivePieces.Add(copy);
                copy.Board = boardClone;
            });


            return boardClone;
        }

        public bool[][][,] GetAsBoolArray() {
            bool[][][,] BoolArray = new bool[2][][,];
            for ( int i = 0; i < 2; i++ ) {
                BoolArray[i] = new bool[6][,];
                for ( int i2 = 0; i2 < 6; i2++ ) {
                    BoolArray[i][i2] = new bool[8, 8];

                }
            }
            for ( int x = 0; x < 8; x++ ) {
                for ( int y = 0; y < 8; y++ ) {
                    IPiece piece = GetPiece(x, y);
                    if ( piece != null )
                        BoolArray[piece.GetColor() == PieceColor.White ? 1 : 0][(int)piece.GetPieceType()][x, y] = true;
                }
            }

            return BoolArray;
        }

        public string GetAsBoolString(bool[][][,] _BoolArray) {
            string boolString = "";
            for ( int color = 0; color < 2; color++ ) {
                for ( int pieceType = 0; pieceType < 6; pieceType++ ) {
                    for ( int x = 0; x < 8; x++ ) {
                        for ( int y = 0; y < 8; y++ ) {
                            boolString += _BoolArray[color][pieceType][x, y];
                        }
                    }
                }

            }
            return boolString;
        }

        /*
        public byte[] GetByteArray() {
            var output = new byte[8 * 8];
            for ( int i = 0; i < 8; i++ ) {
                for ( int j = 0; j < 8; j++ ) {
                    output[i * 8 + j] = Squares[i, j].GetByteData();
                }
            }
            return output;
        }
        public string GetDataString() => Convert.ToBase64String(GetByteArray());
        */
        public byte[] PositionalByteArray {
            get {
                return (byte[])AllPieces.Select(i => i.PositionData);
            }
            set {
                for (int i=0; i<value.Length; i++) {
                    AllPieces[i].PositionData = value[i];
                }
            }
        }

        public string PositionalDataString {
            get {
                return Convert.ToBase64String(PositionalByteArray);
            }
            set {
                PositionalByteArray = Convert.FromBase64String(value);
            }
        }


        public override string ToString() {
            string output = "";
            for ( int i = 7; i >= 0; i-- ) {
                for ( var j = 0; j < 8; j++ )
                    output += $"{Squares[j, i].ToString()},\t";
                output += "\n";
            }
            return output;
        }
    }
}
