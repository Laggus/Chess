using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Classes {

    class AIParallel {
        private class ThreadManager {

            private int GetNumberOfLogicalCores() {
                return Environment.ProcessorCount;
            }

            private Board[] BoardBuffer { get; set; }
            private int NumberOfManagedProcesses { get; set; }
            public AIParallel AIParallel { get; set; }


            public List<Thread> ActiveThreads { get; set; } = new List<Thread>();
            public ThreadManager(AIParallel aIParallel) {
                NumberOfManagedProcesses = GetNumberOfLogicalCores();
                BoardBuffer = new Board[NumberOfManagedProcesses];
                for ( int i = 0; i < NumberOfManagedProcesses; i++ ) {
                    BoardBuffer[i] = new Board();
                }
                this.AIParallel = aIParallel;

            }

            public class GetScoreCallback {
                public double Score { get; set; }
            }

            public void GetScore(Board board, Move.PieceMove move, int depth, bool isMax, double _a, double _b, GetScoreCallback callback) {
                var thread = new Thread(() => {
                    lock ( board ) {
                        callback.Score = AIParallel.GetScore(board, move, depth, isMax, _a, _b);
                    }

                });
                lock ( ActiveThreads ) ActiveThreads.Add(thread);
                thread.Start();
                //System.Threading.ThreadPool.QueueUserWorkItem(() => { })

            }
            private void AfterGetScore(ref double output, double value) {
                output = value;
            }

            public void AbortAll() {
                ActiveThreads.ForEach(t => t.Abort());
            }

            public void WaitAll() {
                foreach ( var thread in ActiveThreads ) {
                    if ( thread.IsAlive )
                        thread.Join();
                }
                WaitAll();
            }


        }

        private double GetScore(Board board, Move.PieceMove move, int depth, bool isMax, double _a, double _b) {
            double score;
            if ( move.IsCastling ) {
                board.Castle(move);
                board.SwitchTurn();
                score = MiniMax(board, depth - 1, !isMax, _a, _b).Value;
                board.SwitchTurn();
                board.UndoCastle(move);
            }
            else {
                bool[] OldKingStates = (bool[])board.ActiveKings.Clone();
                if ( board.GetPiece(move.EndX, move.EndY) != null && board.GetPiece(move.EndX, move.EndY).GetPieceType() == PieceType.King ) {
                    board.ActiveKings[(board.GetPiece(move.EndX, move.EndY).GetColor() == PieceColor.White) ? 1 : 0] = false;
                }
                IPiece startPiece = board.GetPiece(move.StartX, move.StartY);
                IPiece endPiece = board.GetPiece(move.EndX, move.EndY);
                bool PriorMoveState = startPiece.GetHasMoved();
                board.DoMove(move, false);
                // Get score
                board.SwitchTurn();
                score = MiniMax(board, depth - 1, !isMax, _a, _b).Value;
                board.SwitchTurn();
                // Move back
                //Move.PieceMove moveString = new Move.PieceMove(move.EndX, move.EndY, move.StartX, move.StartY);
                board.DoMove(move, true);
                board.Squares[move.EndX, move.EndY].Piece = endPiece;
                startPiece.SetHasMoved(PriorMoveState);
                board.ActiveKings = OldKingStates;
            }
            return score;
        }

        #region Minimax
        public Move MiniMax(Board board, int depth, bool isMax, double _a, double _b) {
            Move bestMove = new Move();

            if ( isMax ) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if ( depth == 0 ) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            var moves = board.GetAllPossibleMoves();

            double score;
            foreach ( var move in moves ) {
                //for ( int i = 0; i < moves.Count; i++ ) {

                // Reference to BestMove required
                #region GetScore

                ThreadManager.GetScoreCallback callback = new ThreadManager.GetScoreCallback();
                ParallelProcessor.GetScore(board, move, depth, isMax, _a, _b, callback);

                //ParallelProcessor.WaitAll();
                //Thread.Sleep(5000);

                score = callback.Score;
            }
            ParallelProcessor.WaitAll();

            //if ( true ) {
            //    #region PostGetScore
            //    // See if better move
            //    if ( (isMax && score > bestMove.Value) || (!isMax && score < bestMove.Value) ) {
            //        bestMove.Value = score;
            //        bestMove.move = move;
            //    }

            //    // Alpha beta pruning
            //    if ( isMax )
            //        _a = Math.Max(bestMove.Value, _a);
            //    else
            //        _b = Math.Min(bestMove.Value, _b);

            //    if ( _a >= _b ) break;
            #endregion
            #endregion
            //}

            return bestMove;
        }


        #region OldSynced
        Move GetBestMoveFromList(List<Move> moves, Move initialOutput, bool _isMax, double _a, double _b) {
            Move output = initialOutput;
            foreach ( var move in moves ) {
                if ( _isMax && move.Value > output.Value || !_isMax && move.Value < output.Value )
                    output = move;

                // Alpha beta pruning
                if ( _isMax ) _a = Math.Max(output.Value, _a);
                else _b = Math.Min(output.Value, _b);

                if ( _a >= _b ) break;
            }
            return output;
        }

        List<Move> EvaluateAllPossibleMoves(Board board, int _depth, bool _isMax, double _a, double _b) {
            var results = new List<Move>();
            var moves = board.GetAllPossibleMoves();

            board.SwitchTurn();
            foreach ( var move in moves ) {
                IPiece endChar = board.Squares[move.EndX, move.EndY].Piece;
                board.MovePiece(move);

                // Get score
                results.Add(new Move() {
                    move = move,
                    Value = MiniMaxSync(board, _depth - 1, !_isMax, _a, _b).Value
                });

                //Move back
                board.MovePieceReverse(move);
                board.GetSquare(move.EndX, move.EndY).Piece = endChar;
            }
            board.SwitchTurn();
            return results;
        }

        public Move MiniMaxSync(Board board, int _depth, bool _isMax, double _a, double _b) {
            Move bestMove = new Move();
            if ( _isMax ) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if ( _depth == 0 ) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            var results = EvaluateAllPossibleMoves(board, _depth, _isMax, _a, _b);

            return GetBestMoveFromList(results, bestMove, _isMax, _a, _b);

        }
        #endregion


        private ThreadManager ParallelProcessor { get; set; }

        public int Depth { get; set; }
        public int Minimum { get; set; } = -1000000;
        public int Maximum { get; set; } = 1000000;
        public Board MainBoard { get; set; }


        public Move GetBestMove() {
            Board boardClone = MainBoard.Clone();
            return MiniMax(boardClone, Depth, true, Minimum, Maximum);
        }

        public AIParallel(Board board, int depth) {
            MainBoard = board;
            this.Depth = depth;

            ParallelProcessor = new ThreadManager(this);
        }
    }
}
