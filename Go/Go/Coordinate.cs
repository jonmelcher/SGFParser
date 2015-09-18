// **************************************************
//  Coordinate.cs - contains public struct Coordinate
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// **************************************************


namespace Go
{
    // ********************************************************************************************
    //  struct:     public struct Coordinate
    //  purpose:    represent an integral point in 2D space, in particular a location on a Go board
    // ********************************************************************************************
    public struct Coordinate
    {
        public readonly int y;      // y position in integral 2D space
        public readonly int x;      // x position in integral 2D space

        // ****************************************************
        //  constructor:    public Coordinate(int y, int x)
        //  purpose:        initialize fields with above values
        //  parameters:     int y
        //                  int x
        // ****************************************************
        public Coordinate(int y, int x)
        {
            this.y = y;
            this.x = x;
        }

        // **************************************************
        //  method:         public override string ToString()
        //  purpose:        facilitate debugging via Console
        //  parameters:     none
        //  returns:        string representing coordinate
        // **************************************************
        public override string ToString()
        {
            return string.Format("({0}, {1})", y, x);
        }
    }
}