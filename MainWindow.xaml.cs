﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

using Chess.Classes;
using Chess.Classes.Pieces;

namespace Chess {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        Board board;
        readonly List<Image> dots = new List<Image>();
        AI AI;

        SelectedPiece selectedPiece;
        //PieceColor currentTurn = PieceColor.White;
        //int turnNumber = 1;

        bool waitingForBackgroundWorker = false;

        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        public MainWindow() {

            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
            EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp));
            UpdateLayout();
            //CreateBoard();


            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e) {
            if ( mainGrid.ActualWidth != 0 ) {

                board = new Board(background, mainGrid, dots); ;
                AI = new AI(board, 5, -1000000, 1000000);
                UpdateVisualBoard();

                dispatcherTimer.Stop();
            }
        }

        /*
        void CreateBoard() {
            board = new Board(background, mainGrid, dots);
            AI = new AI(board, 5, -1000000, 1000000);
        }
        */

        void UpdateVisualBoard() {
            board.Draw(mainGrid, dots);
        }


        Thickness GetThicknessOfPos(int _row, int _column) {
            return new Thickness(-mainGrid.ActualWidth - mainGrid.ActualWidth / 8 + mainGrid.ActualWidth / 4 * (_row + 1), +mainGrid.ActualHeight + mainGrid.ActualHeight / 8 - mainGrid.ActualHeight / 4 * (_column + 1), 0, 0);
        }

        Point GetMousePos() {
            return Mouse.GetPosition(Application.Current.MainWindow);
        }

        Point PixelToBoardPos(Point _pixelPos) {
            double xPos = Map((long)_pixelPos.X, 0, (long)mainGrid.ActualWidth, 0, 8);
            double yPos = Map((long)mainGrid.ActualHeight - (long)_pixelPos.Y, 0, (long)mainGrid.ActualHeight, 0, 8);
            return new Point(xPos, yPos);
        }

        long Map(long x, long in_min, long in_max, long out_min, long out_max) {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

        void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if ( !waitingForBackgroundWorker ) {
                for ( int x = 0; x < board.Squares.GetLength(0); x++ ) {
                    for ( int y = 0; y < board.Squares.GetLength(1); y++ ) {
                        if ( board.GetSquare(x, y).Piece?.GetColor() == board.CurrentTurn )
                        //if (board.Squares[x, y].type != Piece.Blank && IsUpper(board.Squares[x, y].type) == currentTurn)
                        {
                            if ( board.Squares[x, y].Image.IsMouseOver ) {
                                //if (IsUpper(board.Squares[x, y].type) == currentTurn)
                                selectedPiece = new SelectedPiece(board.Squares[x, y].Image, x, y);
                                // Create dots
                                List<Move.PieceMove> possibleMoves = board.GetPossibleMovesForPiece(x, y);
                                foreach ( Move.PieceMove move in possibleMoves ) {
                                    Image newDot = new Image {
                                        Source =
                                    new BitmapImage(new Uri(System.IO.Path.GetFullPath("Images/green_dot.png"), UriKind.Absolute)),
                                        Width = mainGrid.ActualWidth / 16,
                                        Height = mainGrid.ActualHeight / 16,
                                        Name = "dot" + x + y,
                                        Tag = "dot"
                                    };
                                    dots.Add(newDot);
                                    mainGrid.Children.Add(newDot);
                                    switch ( 4 ) {
                                        /*
                                        case 3:
                                        newDot.Margin = GetThicknessOfPos(6, (currentTurn ? 0 : 7));
                                        break;
                                        */

                                        case 4:
                                        newDot.Margin = GetThicknessOfPos(move.EndX, move.EndY);
                                        break;
                                        /*
                                        case 5:
                                        newDot.Margin = GetThicknessOfPos(2, (currentTurn ? 0 : 7));
                                        break;
                                        */
                                    }

                                }
                                break;
                            }
                        }
                    }
                }
                if ( selectedPiece != null ) {
                    mainGrid.Children.Remove(selectedPiece.image);
                    mainGrid.Children.Add(selectedPiece.image);
                }
            }
        }

        private void HandleMouseMove(object sender, MouseEventArgs e) {
            if ( selectedPiece != null ) {
                Thickness tempPos = GetThicknessOfPos((int)PixelToBoardPos(GetMousePos()).X, (int)PixelToBoardPos(GetMousePos()).Y);
                selectedPiece.image.Margin = tempPos;
            }

            e.Handled = true;
        }

        void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            int targetX = (int)PixelToBoardPos(GetMousePos()).X;
            int targetY = (int)PixelToBoardPos(GetMousePos()).Y;


            if ( selectedPiece != null ) {
                if ( (targetX != selectedPiece.startX || targetY != selectedPiece.startY) && board.Squares[selectedPiece.startX, selectedPiece.startY].Piece.GetColor() == board.CurrentTurn ) {
                    Move.PieceMove move = new Move.PieceMove(selectedPiece.startX, selectedPiece.startY, targetX, targetY);
                    // Check if trying to castle
                    /*
                    if (ToLower(board.Squares[selectedPiece.startX, selectedPiece.startY].type) == Piece.BKing)
                    {
                        if (!kingMoved[currentTurn ? 1 : 0])
                        {
                            if (targetX == 2) move = "O-O-O";
                            else if (targetX == 6) move = "O-O";
                        }
                    }
                    */
                    List<Move.PieceMove> possibleMoves = board.GetPossibleMovesForPiece(selectedPiece.startX, selectedPiece.startY);
                    bool isIn = false;
                    foreach ( Move.PieceMove moveString in possibleMoves ) {
                        isIn = true;
                        if ( moveString.StartX != move.StartX ) isIn = false;
                        else if ( moveString.StartY != move.StartY ) isIn = false;
                        else if ( moveString.EndX != move.EndX ) isIn = false;
                        else if ( moveString.EndY != move.EndY ) isIn = false;
                        if (isIn) break;
                    }
                    if ( isIn ) {
                        board.MovePiece(move, board.Squares[selectedPiece.startX, selectedPiece.startY].Piece);
                        selectedPiece = null;
                        UpdateVisualBoard();
                        board.SwitchTurn();
                        StartBackgroundWorker();
                    }
                    else {
                        selectedPiece = null;
                        UpdateVisualBoard();
                    }
                }
            }

        }

        void StartBackgroundWorker() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(BackgroundWork);
            waitingForBackgroundWorker = true;
            worker.RunWorkerAsync();
        }

        void BackgroundWork(object sender, DoWorkEventArgs e) {
            //foreach ( Move.PieceMove move in board.GetAllPossibleMoves(board.CurrentTurn) ) Console.WriteLine(move);
            //Console.WriteLine();
            Move aiMove = AI.GetBestMove();
            IPiece startChar = board.Squares[aiMove.move.StartX, aiMove.move.StartY].Piece;
            board.MovePiece(aiMove.move, startChar);
            Application.Current.Dispatcher.Invoke(new Action(() => {
                UpdateVisualBoard();
            }));
            board.SwitchTurn();
            waitingForBackgroundWorker = false;
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e) {
            if ( e.PreviousSize != new Size() ) {
                selectedPiece = null;
                background.Width = mainGrid.ActualWidth;
                background.Height = mainGrid.ActualHeight;
                UpdateVisualBoard();
            }
        }


    }
}