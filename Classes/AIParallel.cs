using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chess.Classes
{

    class AIParallel
    {
        private class TaskManager
        {

            public class BoardBufferItem
            {
                public Board Board { get; set; } = new Board();
                public Task Task { get; set; }
                private object ThreadLock { get; set; } = new object();
                public TaskStatus TaskStatusFromDealer { get; set; }
                public bool IsFree {
                    get {
                        lock (ThreadLock)
                        {
                            if (Task == null) return false;
                            switch (Task.Status)
                            {
                                case TaskStatus.RanToCompletion:
                                    return true;
                                case TaskStatus.Faulted:
                                    return true;
                                case TaskStatus.Canceled:
                                    return true;
                                case TaskStatus.Running:
                                    return false;
                                case TaskStatus.WaitingForActivation:
                                    return false;
                                case TaskStatus.Created:
                                    return false;
                                case TaskStatus.WaitingForChildrenToComplete:
                                    return false;
                                case TaskStatus.WaitingToRun:
                                    return false;
                            }
                        }
                        throw new Exception($"Unhandled ThreadStatus {Task.Status}");

                        //if ( FirstRun ) return true;
                        //if ( Task == null ) return false;
                        //if ( Task.Status == TaskStatus.WaitingToRun ) return false;
                        //return Task.Status != TaskStatus.Running;
                    }
                }
            }
            private object BoardBufferLock { get; set; } = new object();
            private List<BoardBufferItem> BoardBuffer { get; set; } = new List<BoardBufferItem>();
            private BoardBufferItem AddNewBufferItem()
            {
                throw new NotImplementedException();
                lock (BoardBufferLock)
                {
                    foreach (var item in BoardBuffer) if (item.IsFree) return item;
                    //BoardBuffer.Add()
                    /*
                    var newBoardBuffer = new BoardBufferItem[BoardBuffer.Length + 1];
                    for (int i = 0; i < BoardBuffer.Length; i++) newBoardBuffer[i] = BoardBuffer[i];
                    newBoardBuffer[BoardBuffer.Length] = new BoardBufferItem();
                    BoardBuffer = newBoardBuffer;
                    return BoardBuffer[BoardBuffer.Length - 1];
                    */
                }
            }

            private int NumberOfManagedProcesses { get; set; }
            public AIParallel AIParallel { get; set; }


            public List<Task> ActiveTasks { get; set; } = new List<Task>();
            public TaskManager(AIParallel aIParallel)
            {
                this.AIParallel = aIParallel;

            }

            public void AbortAll()
            {
                throw new NotImplementedException();
                //Task.Factory.
                //ActiveTasks.
                //ActiveTasks.ForEach(t => t.Abort());
            }

            public void WaitAll()
            {
                throw new NotImplementedException();
                //foreach ( var thread in ActiveTasks ) {
                //    if ( thread.IsAlive )
                //        thread.Join();
                //}
                //WaitAll();
            }

            public BoardBufferItem GetBoardBufferItem(/*Task task*/)
            {
                lock (BoardBufferLock)
                {
                    foreach (var item in BoardBuffer) if (item.IsFree) { item.TaskStatusFromDealer = item.Task.Status; return item; }
                    var newObj = new BoardBufferItem();
                    BoardBuffer.Add(newObj);
                    return newObj;
                }


                //lock (BoardBufferLock)
                //{
                //    var tasks = new Task[BoardBuffer.Length];
                //    int i;
                //    for (i = 0; i < BoardBuffer.Length; i++)
                //    {
                //        if (BoardBuffer[i].IsFree)
                //        {
                //            return BoardBuffer[i];
                //        }
                //        tasks[i] = BoardBuffer[i].Task;
                //    }
                //    return AddNewBufferItem();
                //}
            }

        }

        private double GetScore(Board board, Move.PieceMove move, int depth, bool isMax, double _a, double _b)
        {
            double score;
            if (board.GetPiece(move) == null)
            {
                Console.WriteLine("Invalid move in Parallel - Evaluating score as 0");
                return 0;
            }
            if (move.IsCastling)
            {
                board.Castle(move);
                board.SwitchTurn();
                score = MiniMax(board, depth - 1, !isMax, _a, _b).Value;
                board.SwitchTurn();
                board.UndoCastle(move);
            }
            else
            {
                bool[] OldKingStates = (bool[])board.ActiveKings.Clone();
                if (board.GetPiece(move.EndX, move.EndY) != null && board.GetPiece(move.EndX, move.EndY).GetPieceType() == PieceType.King)
                {
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
                //board.DoMove(move, true);
                //board.Squares[move.EndX, move.EndY].Piece = endPiece;
                //startPiece.SetHasMoved(PriorMoveState);
                //board.ActiveKings = OldKingStates;
            }
            return score;
        }

        #region Minimax
        double MiniMaxThreadStartGetScore(ref Board board, ref TaskManager.BoardBufferItem boardBufferItem, ref Move.PieceMove[] moves, int localI, int depth, bool isMax, double _a, double _b)
        {
            //boardBufferItem.Task = scoreTasks[i] = new Task<double>(() => {
            lock (board) board.CloneToBoard(boardBufferItem.Board);
            return GetScore(boardBufferItem.Board, moves[localI], depth, isMax, _a, _b);
            //});
        }

        public Move MiniMax(Board board, int depth, bool isMax, double _a, double _b)
        {
            Move bestMove = new Move();

            if (isMax) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if (depth == 0)
            {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            var moves = board.GetAllPossibleMoves().ToArray();

            var scoreTasks = new Task<double>[moves.Length];

            for (int i = 0; i < moves.Length; i++)
            {
                int localI = i;
                var boardBufferItem = ParallelProcessor.GetBoardBufferItem();
                boardBufferItem.Task = null;

                //boardBufferItem.Task = scoreTasks[localI] = Task.Run(() => MiniMaxThreadStartGetScore(ref board, ref boardBufferItem, ref moves, localI, depth, isMax, _a, _b));
                //boardBufferItem.Task = scoreTasks[i] = Task.Factory.StartNew(() => MiniMaxThreadStartGetScore(ref board, ref boardBufferItem, ref moves, localI, depth, isMax, _a, _b));
                boardBufferItem.Task = scoreTasks[localI] = new Task<double>(() => MiniMaxThreadStartGetScore(ref board, ref boardBufferItem, ref moves, localI, depth, isMax, _a, _b));
                boardBufferItem.Task.Start();
                /*
                boardBufferItem.Task = scoreTasks[i] = new Task<double>(() => {
                    lock (board) board.CloneToBoard(boardBufferItem.Board);
                    return GetScore(boardBufferItem.Board, moves[localI], depth, isMax, _a, _b);
                });
                */
                //lock (boardBufferItem)
                //    if (boardBufferItem.Task.Status == TaskStatus.Created || boardBufferItem.Task.Status == TaskStatus.WaitingToRun)
                //        boardBufferItem.Task.Start();
            }
            //foreach (var t in scoreTasks) t.Start();

            //Task.WaitAll(scoreTasks);

            for (int LoopTTL = 0; LoopTTL < moves.Length; LoopTTL++)
            {
                Task<double>[] remainingTasks = scoreTasks.Where(k => k != null).ToArray();
                int i = Task.WaitAny(remainingTasks);
                double score = scoreTasks[i].Result;
                //var tmpScoreTasks = scoreTasks.ToList();
                //tmpScoreTasks.Remove(scoreTasks[i]);
                //scoreTasks = tmpScoreTasks.ToArray();

                // See if better move
                if ((isMax && score > bestMove.Value) || (!isMax && score < bestMove.Value))
                {
                    bestMove.Value = score;
                    bestMove.move = moves[i];
                }

                // Alpha beta pruning
                if (isMax)
                    _a = Math.Max(bestMove.Value, _a);
                else
                    _b = Math.Min(bestMove.Value, _b);

                if (_a >= _b) break;
            }

            return bestMove;
        }
        #endregion

        private TaskManager ParallelProcessor { get; set; }

        public int Depth { get; set; }
        public int Minimum { get; set; } = -1000000;
        public int Maximum { get; set; } = 1000000;
        public Board MainBoard { get; set; }


        public Move GetBestMove()
        {
            Board boardClone = MainBoard.Clone();
            var output = MiniMax(boardClone, Depth, true, Minimum, Maximum);
            if (MainBoard.GetPiece(output.move) == null)
            {
                Console.WriteLine("Invalid move from AIParallel - Retrying");
                return GetBestMove();
            }

            return output;
        }

        public AIParallel(Board board, int depth)
        {
            MainBoard = board;
            this.Depth = depth;

            ParallelProcessor = new TaskManager(this);
        }
        public AIParallel(Board board, int depth, int min, int max)
        {
            MainBoard = board;
            this.Depth = depth;

            ParallelProcessor = new TaskManager(this);
        }
    }
}
