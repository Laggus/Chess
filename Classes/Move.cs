using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    class Move
    {
        public class PieceMove
        {
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int EndX { get; set; }
            public int EndY { get; set; }

            public bool IsCastling { get; set; }
            public bool IsPromoting { get; set; }

            public static PieceMove ConvertToMove(int startPosX, int startPosY, int endPosX, int endPosY) {
                return new PieceMove(startPosX, startPosY, endPosX, endPosY);
            }
            public PieceMove(int startPosX, int startPosY, int endPosX, int endPosY) {
                StartX = startPosX;
                StartY = startPosY;
                EndX = endPosX;
                EndY = endPosY;
            }
            public new string ToString() {
                return $"Move [{StartX},{StartY}] -> [{EndX},{EndY}]";
            }
        }

        public PieceMove move { get; set; }
        public double Value { get; set; }
        public double Alpha { get; set; }
        public double Beta { get; set; }

        public new string ToString() {
            if ( move == null )
                return "No move";
            else
                return move.ToString();
        }
    }
}
