using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Classes {

    class AIParallel {
        private class TaskManager {

            private int GetNumberOfLogicalCores() {
                return Environment.ProcessorCount;
            }

            private class BoardBufferItem {
                private Board Board { get; set; } = new Board();
                public Board GetBoard() {
                    FirstRun = false;
                    return this.Board;
                }
                public Task Task { get; set; }
                private bool FirstRun {get;set;} = true;
                public bool IsFree {
                    get {
                        if ( FirstRun ) return true;
                        if ( Task == null ) return false;
                        return Task.Status != TaskStatus.Running;
                    }
                }
            }
            private BoardBufferItem[] BoardBuffer { get; set; }
            private int NumberOfManagedProcesses { get; set; }
            public AIParallel AIParallel { get; set; }


            public List<Task> ActiveTasks { get; set; } = new List<Task>();
            public TaskManager(AIParallel aIParallel) {
                NumberOfManagedProcesses = GetNumberOfLogicalCores();
                BoardBuffer = new BoardBufferItem[NumberOfManagedProcesses];
                for ( int i = 0; i < NumberOfManagedProcesses; i++ ) {
                    BoardBuffer[i] = new BoardBufferItem();
                }
                this.AIParallel = aIParallel;

            }

            public class GetScoreCallback {
                public double Score { get; set; }
            }
            /*
            public void GetScore(Board board, Move.PieceMove move, int depth, bool isMax, double _a, double _b, GetScoreCallback callback) {
                var task = new Task(() => {
                    lock ( board ) {
                        callback.Score = AIParallel.GetScore(board, move, depth, isMax, _a, _b);
                    }

                });
                lock ( ActiveTasks ) ActiveTasks.Add(task);
                task.Start();

                //System.Threading.ThreadPool.QueueUserWorkItem(() => { })

            }
            private void AfterGetScore(ref double output, double value) {
                output = value;
            }
            */

            public void AbortAll() {
                throw new NotImplementedException();
                //Task.Factory.
                //ActiveTasks.
                //ActiveTasks.ForEach(t => t.Abort());
            }

            public void WaitAll() {
                throw new NotImplementedException();
                //foreach ( var thread in ActiveTasks ) {
                //    if ( thread.IsAlive )
                //        thread.Join();
                //}
                //WaitAll();
            }

            public Board GetBoard(Task task) {
                lock ( BoardBuffer ) {
                    var tasks = new Task[BoardBuffer.Length];
                    int i;
                    for (i = 0; i < BoardBuffer.Length; i++ ) {
                        if ( BoardBuffer[i].IsFree ) {
                            BoardBuffer[i].Task = task;
                            return BoardBuffer[i].GetBoard();
                        }
                        tasks[i] = BoardBuffer[i].Task;
                    }
                    i = Task.WaitAny(tasks);
                    BoardBuffer[i].Task = task;
                    return  BoardBuffer[i].GetBoard();
                }
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

            var moves = board.GetAllPossibleMoves().ToArray();

            var scoreTasks = new Task[moves.Length];
            var scores = new double[moves.Length];
            for ( int i = 0; i < moves.Length; i++ ) {
                new Task(() => {
                    scores[i] = GetScore(board, moves[i], depth, isMax, _a, _b);
                }).Start();
            }

            Task.WaitAll(scoreTasks);

            for ( int i = 0; i < moves.Length; i++ ) {
                #region PostGetScore
                // See if better move
                if ( (isMax && scores[i] > bestMove.Value) || (!isMax && scores[i] < bestMove.Value) ) {
                    bestMove.Value = scores[i];
                    bestMove.move = moves[i];
                }

                // Alpha beta pruning
                if ( isMax )
                    _a = Math.Max(bestMove.Value, _a);
                else
                    _b = Math.Min(bestMove.Value, _b);

                if ( _a >= _b ) break;
                #endregion
                #endregion
            }

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


        private TaskManager ParallelProcessor { get; set; }

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

            ParallelProcessor = new TaskManager(this);
        }
    }
}
