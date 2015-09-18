// *******************************************************************
//  GoMove.cs - contains public enum MoveType and public struct GoMove
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// *******************************************************************


namespace Go
{
    // *******************************************************************
    //  enum:       public enum MoveType { None, Placement, Normal, Pass }
    //  purpose:    distinguish between different kinds of Go moves
    // *******************************************************************
    public enum MoveType { None, Placement, Normal, Pass };

    // *********************************************************************
    //  struct:     public struct GoMove
    //  purpose:    encapsulate the information from a parsed SGF move datum
    // *********************************************************************
    public struct GoMove
    {
        public readonly int y;          // y integral position on a Go board
        public readonly int x;          // x integral position on a Go board
        public readonly GoColour c;     // colour of the Go stone
        public readonly MoveType t;     // type of move played

        // ************************************************************************
        //  constructor:        public GoMove(int y, int x, GoColour c, MoveType t)
        //  purpose:            initialize fields with above values
        //  parameters:         int y
        //                      int x
        //                      GoColour c
        //                      MoveType t
        // ************************************************************************
        public GoMove(int y, int x, GoColour c, MoveType t)
        {
            this.y = y;
            this.x = x;
            this.c = c;
            this.t = t;
        }
    }
}