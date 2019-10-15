/*
 * 
 * Todo:
 *      Add a buffer of boards, the same length as the number of CPU cores.
 *      Add function to board to copy another board, to reset it between each iteration
 * 
 */

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
                bool PriorMoveState = startChar.GetHasMoved();
                board.MovePiece(moves[i]);

                // Get score
                double score = MiniMax(board, _depth - 1, !_isMax, _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White, _doingHE, _a, _b).Value;

                // Move back
                Move.PieceMove moveString = new Move.PieceMove(moves[i].EndX, moves[i].EndY, moves[i].StartX, moves[i].StartY);
                board.MovePiece(moveString);
                board.Squares[moves[i].EndX, moves[i].EndY].Piece = endChar;
                startChar.SetHasMoved(PriorMoveState);

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
                /*
                // Alpha beta pruning
                if (_isMax) _a = Math.Max(bestMove.Value, _a);
                else _b = Math.Min(bestMove.Value, _b);
                if (_a >= _b) break;
                */
            }
            return bestMove;
        }


        Move GetBestMoveFromList(List<Move> moves, Move initialOutput, bool _isMax, double _a, double _b) {
            Move output = initialOutput;
            foreach(var move in moves) {
                if (_isMax && move.Value > output.Value || !_isMax && move.Value < output.Value)
                    output = move;

                // Alpha beta pruning
                if (_isMax) _a = Math.Max(output.Value, _a);
                else _b = Math.Min(output.Value, _b);

                if (_a >= _b) break;
            }
            return output;
        }

        List<Move> EvaluateAllPossibleMoves(Board board, int _depth, bool _isMax, double _a, double _b) {
            var results = new List<Move>();
            var moves = board.GetAllPossibleMoves();

            board.SwitchTurn();
            foreach(var move in moves) {
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

        Move MiniMaxSync(Board board, int _depth, bool _isMax, double _a, double _b) {
            Move bestMove = new Move();
            if (_isMax) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if (_depth == 0) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }

            var results = EvaluateAllPossibleMoves(board, _depth, _isMax, _a, _b);

            return GetBestMoveFromList(results, bestMove, _isMax, _a, _b);
            
        }

        async Task<Move> MiniMaxThreaded(Board board, int _depth, bool _isMax, PieceColor _currentTurn, double _a, double _b) {
            Move bestMove = new Move();
            if (_isMax) bestMove.Value = Minimum;
            else bestMove.Value = Maximum;

            if (_depth == 0) {
                bestMove.Value = board.EvaluateBoard();
                return bestMove;
            }
            var moves = board.GetAllPossibleMoves(_currentTurn);

            var tasks = new Task<Move>[moves.Count];
            
            for (int i = 0; i < moves.Count; i++) {
                var boardClone = board.Clone();
                IPiece startChar = boardClone.Squares[moves[i].StartX, moves[i].StartY].Piece;
                IPiece endChar = boardClone.Squares[moves[i].EndX, moves[i].EndY].Piece;
                boardClone.MovePiece(moves[i]);


                
                // Get score
                //(new Thread(() => {
                    tasks[i] = MiniMaxThreaded(board, _depth - 1, !_isMax, _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White, _a, _b);
                //})).Start();
            }

            //Task.WaitAll(tasks);

            for(int i=0; i<tasks.Length; i++) {
                int taskId = Task.WaitAny(tasks);
                var task = tasks[i];
                tasks[taskId] = null;
                double score = task.Result.Value;
                
                //double score = (await MiniMaxThreaded(boardClone, _depth - 1, !_isMax, _currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White, _a, _b)).Value;


                // See if better move
                if (_isMax && score > bestMove.Value || !_isMax && score < bestMove.Value) {
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



        public Board MainBoard { get; set; }
        public int MovesAhead { get; set; }
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public Move GetBestMove() {


            Board boardClone = MainBoard.Clone();
            return MiniMax(boardClone, MovesAhead, true, boardClone.CurrentTurn, true, Minimum, Maximum);

            //boardClone.SwitchTurn();

            //return MiniMaxSynced(boardClone, MovesAhead, true, MainBoard.CurrentTurn, Minimum, Maximum);

            return MiniMaxSync(boardClone, MovesAhead, true, Minimum, Maximum);

            Task<Move> task = MiniMaxThreaded(boardClone, MovesAhead, true, MainBoard.CurrentTurn, Minimum, Maximum);
            task.Wait();
            return task.Result;

        }

        public AI(Board board, int movesAhead, int minimum, int maximum) {
            this.MainBoard = board;
            this.MovesAhead = 1;// movesAhead;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }
    }
}
