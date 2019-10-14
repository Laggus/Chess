using Chess.Classes.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Chess.Classes {
    class AI {
        private class MoveCallback {
            public Move BestMove { get; set; }
            public Move WorstMove { get; set; }
            public Board Board { get; set; }
            public Move InitialMove { get; set; }
            public MoveCallback(Board board, Move bestMove, Move worstMove, Move initialMove) {
                this.Board = board;
                this.BestMove = bestMove;
                this.WorstMove = worstMove;
                this.InitialMove = initialMove;
            }
        }


        double GetAlpha(bool _isMax, Move bestMove) {
            // Alpha beta pruning
            if ( _isMax )
                return Math.Max(bestMove.Value, bestMove.Alpha);
            else
                return Math.Min(bestMove.Value, bestMove.Beta);
        }

        Move MiniMax(Board board, int _depth, bool _isMax, PieceColor _currentTurn, bool _doingHE, double _a, double _b) {
            Move bestMove = new Move();
            if (_isMax) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if (_depth == 0)
            {
                bestMove.Value = board.EvaluateBoard(board.CurrentTurn == PieceColor.White);
                return bestMove;
            }
            var moves = board.GetAllPossibleMoves(_currentTurn);

            for (int i = 0; i < moves.Count; i++)
            {
                IPiece startChar = board.Squares[moves[i].StartX, moves[i].StartY].Piece;
                IPiece endChar = board.Squares[moves[i].EndX, moves[i].EndY].Piece;
                board.MovePiece(moves[i], startChar);

                // Get score
                double score = MiniMax(board, _depth - 1, !_isMax, _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White, _doingHE, _a, _b).Value;

                // Move back
                //if ( moves[i].Length == 4) {
                if (true)
                {
                    Move.PieceMove moveString = new Move.PieceMove(moves[i].EndX, moves[i].EndY, moves[i].StartX, moves[i].StartY);
                    board.MovePiece(moveString, startChar);
                    board.Squares[moves[i].EndX, moves[i].EndY].Piece = endChar;
                }
                //else MovePiece(moves[i], startChar);


                // See if better move
                if (_isMax && score > bestMove.Value)
                {
                    bestMove.Value = score;
                    bestMove.move = moves[i];
                }
                else if (!_isMax && score < bestMove.Value)
                {
                    bestMove.Value = score;
                    bestMove.move = moves[i];
                }

                // Alpha beta pruning
                if (_isMax) _a = Math.Max(bestMove.Value, _a);
                else _b = Math.Min(bestMove.Value, _b);
                if (_a >= _b) break;
            }
            return bestMove;
        }

        /*
        Move MiniMaxSeperateBoard(Board inputBoard, int _depth, bool _isMax, double alpha, double beta) {


            Move bestMove = new Move() {
                Value = _isMax ? Minimum : Maximum,
                Alpha = alpha,
                Beta = beta
            };
            Board board = inputBoard.Clone();

            if ( _depth == 0 ) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            foreach ( var move in board.GetAllPossibleMoves() ) {
                IPiece startChar = board.GetPiece(move.StartX, move.StartY);
                IPiece endChar = board.GetPiece(move.EndX, move.EndY);
                board.MovePiece(move);

                // Get score
                board.SwitchTurn();
                double score = MiniMaxSeperateBoard(board, _depth - 1, !_isMax, bestMove.Alpha, bestMove.Beta).Value;

                // Move back
                Move.PieceMove moveString = new Move.PieceMove(move.EndX, move.EndY, move.StartX, move.StartY);
                board.MovePiece(moveString);
                board.Squares[move.EndX, move.EndY].Piece = endChar;
                if ( endChar != null ) endChar.Active = true;

                // See if better move
                if (
                    _isMax && score > bestMove.Value ||
                    !_isMax && score < bestMove.Value
                ) {
                    bestMove.Value = score;
                    bestMove.move = move;
                }

                // Alpha beta pruning
                if ( _isMax )
                    bestMove.Alpha = Math.Max(bestMove.Value, bestMove.Alpha);
                else
                    bestMove.Alpha = Math.Min(bestMove.Value, bestMove.Beta);

                if ( bestMove.Alpha >= bestMove.Beta ) break;
            }

            return bestMove;
        }
        */
        Move MiniMaxThreaded(Board board, int _depth, bool _isMax, double alpha, double beta) {
            //board = board.Clone();

            Move bestMove = new Move() {
                Value = _isMax ? Minimum : Maximum,
                Alpha = alpha,
                Beta = beta
            };

            if ( _depth == 0 ) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            bool done = false;
            var taskList = new List<Task>();
            board = board.Clone();
            foreach ( var move in board.GetAllPossibleMoves() ) {
                if ( done ) break;

                lock ( board.ThreadLock )
                    board = board.Clone();


                taskList.Add(Task.Run(() => {

                    if ( done ) return;

                    lock ( board.ThreadLock ) {

                        IPiece startChar = board.GetPiece(move.StartX, move.StartY);
                        IPiece endChar = board.GetPiece(move.EndX, move.EndY);

                        if ( board.GetPiece(move) == null )
                            Console.WriteLine("Potatoes");
                        else
                            board.MovePiece(move);

                        // Get score
                        board.SwitchTurn();


                        Board boardClone;
                        //lock ( board ) {
                        boardClone = board.Clone();
                        //}

                        double score = MiniMaxThreaded(boardClone, _depth - 1, !_isMax, bestMove.Alpha, bestMove.Beta).Value;


                        // Move back
                        Move.PieceMove moveString = new Move.PieceMove(move.EndX, move.EndY, move.StartX, move.StartY);
                        //Move.PieceMove moveString = move;
                        if ( board.GetPiece(moveString) == null )
                            Console.WriteLine("Potatoes");
                        board.MovePiece(moveString);
                        board.Squares[move.EndX, move.EndY].Piece = endChar;
                        if ( endChar != null ) endChar.Active = true;

                        if ( done ) return;
                        if (
                            _isMax && score > bestMove.Value ||
                            !_isMax && score < bestMove.Value
                        ) {
                            bestMove.Value = score;
                            bestMove.move = move;
                        }

                        bestMove.Alpha = GetAlpha(_isMax, bestMove);

                        if ( bestMove.Alpha >= bestMove.Beta ) done = true;
                    }

                }));
            }
            Task.WaitAll(taskList.ToArray());
            return bestMove;
        }


        private async Task<Move> EvaluateMove(Board board, Move move) {
            return new Move();
        }

        private async Task<Move> MiniMaxTask(Board board, int _depth, bool _isMax, double alpha, double beta) {
            Move bestMove = new Move() {
                Value = _isMax ? Minimum : Maximum,
                Alpha = alpha,
                Beta = beta
            };
            //board = board.Clone();

            if ( _depth == 0 ) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            foreach ( var move in board.GetAllPossibleMoves() ) {
                //IPiece startChar = board.GetPiece(move.StartX, move.StartY);
                IPiece endChar = board.GetPiece(move.EndX, move.EndY);
                board.MovePiece(move);

                // Get score
                board.SwitchTurn();
                double score = (await MiniMaxTask(board, _depth - 1, !_isMax, bestMove.Alpha, bestMove.Beta)).Value;

                // Move back
                Move.PieceMove moveString = new Move.PieceMove(move.EndX, move.EndY, move.StartX, move.StartY);
                board.MovePiece(moveString);
                board.Squares[move.EndX, move.EndY].Piece = endChar;
                if ( endChar != null ) endChar.Active = true;

                // See if better move
                if (
                    _isMax && score > bestMove.Value ||
                    !_isMax && score < bestMove.Value
                ) {
                    bestMove.Value = score;
                    bestMove.move = move;

                }
                bestMove.Alpha = GetAlpha(_isMax, bestMove);

                if ( bestMove.Alpha >= bestMove.Beta ) break;
            }

            return bestMove;
        }

        /*
        private async Task<MoveCallback> GetBestMove(Board board, int depth, Move currentBestMove, Move currentWorstMove, Move initialMove) {
            if ( currentBestMove == null ) {
                currentBestMove = new Move();
                currentBestMove.Value = -1000000;
            }
            if ( currentWorstMove == null ) {
                currentWorstMove = new Move();
                currentWorstMove.Value = 1000000;
            }

            if ( depth == 0 ) {
                currentBestMove.Value = board.EvaluateBoard();

                return new MoveCallback(board, currentBestMove, currentWorstMove, initialMove);
            }




            var allPossibleMoves = board.GetAllPossibleMoves();

            //var tasks = new List<Task<MoveCallback>>();
            var tasks = new Task<MoveCallback>[allPossibleMoves.Count];

            int taskIndex = tasks.Length;

            foreach ( var move in allPossibleMoves ) {
                var boardClone = board.Clone();
                boardClone.MovePiece(move);
                //(new Thread(thread => {
                tasks[--taskIndex] = GetBestMove(boardClone, depth - 1, null, null, initialMove);
                //})).Start();
            }


            for ( var i = 0; i < tasks.Length; i++ ) {
                var task = tasks[Task.WaitAny(tasks)];
                if ( task.Status != TaskStatus.RanToCompletion )
                    continue;

                MoveCallback result = await task;
                if ( result.BestMove.Value > 0 ) {
                    Console.WriteLine("test");
                }
                else {
                    Console.WriteLine("Test");
                }
                if ( result.BestMove.Value > currentBestMove.Value ) currentBestMove = result.BestMove;
                if ( result.WorstMove.Value < currentWorstMove.Value ) currentWorstMove = result.WorstMove;

            }
            
            //Task.WaitAll(tasks.ToArray());


            //foreach(var task in tasks) {
            //    MoveCallback result = await task;
            //    if ( result.Move.Value > currentBestMove.Value ) {
            //        currentBestMove = result.Move;
            //    }
            //}
            

            return new MoveCallback(board, currentBestMove, currentWorstMove, initialMove);

        }
        private async Task<MoveCallback> GetBestMove(Board board, int depth) => await GetBestMove(board, depth, new Move(), new Move(), null);

        */
        private async Task<Move> GetBestMove(Board board, int depth, Move initialMove) {

            if ( depth == 0 ) {
                //currentBestMove.Value = board.EvaluateBoard();
                //if( initialMove != null) {
                    initialMove.Value = board.EvaluateBoard();
                    return initialMove;
                //}
                //return new Move() {
                //    Value = board.EvaluateBoard()
                //};
            }


            var allPossibleMoves = board.GetAllPossibleMoves();

            var tasks = new Task<Move>[allPossibleMoves.Count];

            int taskIndex = tasks.Length;

            foreach ( var move in allPossibleMoves ) {
                var boardClone = board.Clone();
                boardClone.SwitchTurn();
                boardClone.MovePiece(move);
                if(initialMove == null) {
                    tasks[--taskIndex] = GetBestMove(boardClone, depth - 1, new Move() { move = move });
                }
                else {
                    tasks[--taskIndex] = GetBestMove(boardClone, depth - 1, initialMove);
                }
            }

            Move bestMove = new Move() { Value = Minimum };

            Task.WaitAll();
            for ( var i = 0; i < tasks.Length; i++ ) {
                //var taskId = Task.WaitAny(tasks);
                var taskId = i;
                var task = tasks[taskId];
                if ( task.Status != TaskStatus.RanToCompletion )
                    continue;

                Move result = await task;

                if ( result.Value > bestMove.Value ) bestMove = result;
            }

            return bestMove;
        }



        public Board MainBoard { get; set; }
        public int MovesAhead { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public Move GetBestMove() {

            Board boardClone = MainBoard.Clone();
            //boardClone.SwitchTurn();

            //Task<Move> task = GetBestMove(boardClone, MovesAhead, null);
            //task.Wait();

            //Move taskResult = task.Result;

            //return taskResult;

            return MiniMax(MainBoard, MovesAhead, true, MainBoard.CurrentTurn, false, Minimum, Maximum);

            /*

            return MiniMax(MainBoard, MovesAhead, true, MainBoard.CurrentTurn, false, Minimum, Maximum);
            //return MiniMaxSeperateBoard(MainBoard, MovesAhead, true, Minimum, Maximum);
            //return MiniMaxThreaded(MainBoard.Clone(), MovesAhead, true, Minimum, Maximum);


            Board boardClone = MainBoard.Clone();
            boardClone.SwitchTurn();

            var task = MiniMaxTask(boardClone, MovesAhead, true, Minimum, Maximum);
            task.Wait();

            return task.Result;
            */
        }

        public AI(Board board, int movesAhead, int minimum, int maximum) {
            this.MainBoard = board;
            this.MovesAhead = movesAhead;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }
    }
}
