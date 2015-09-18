// ********************************************************************
//  GoStone.cs - contains public enum GoColour and public class GoStone
//
//  Written by Jonathan Melcher on 14/09/2015
//  Last updated 16/09/2015
// ********************************************************************


namespace Go
{
    // *********************************************************
    //  enum:       public enum GoColour { None, Black, White }
    //  purpose:    represent all possible colours of a Go stone
    // *********************************************************
    public enum GoColour { None, Black, White }

    // ***********************************************************************
    //  class:      public class GoStone
    //  purpose:    provide mutable information about a position on a Go board
    // ***********************************************************************
    public class GoStone
    {
        // **********************************************************************************************
        //  constructors:       public GoStone(int y, int x, GoColour colour, bool seen = false)
        //                      public GoStone(Coordinate coordinate, GoColour colour, bool seen = false)
        //                      public GoStone(GoMove m, bool seen = false)
        //  purpose:            initialize fields using the above values
        //  parameters:         int y
        //                      int x
        //                      GoColour colour
        //                      bool seen [= false]
        // **********************************************************************************************
        public GoStone(int y, int x, GoColour colour, bool seen = false)
        {
            Coordinate = new Coordinate(y, x);
            Colour = colour;
            Seen = seen;
        }

        public GoStone(Coordinate coordinate, GoColour colour, bool seen = false) : 
                                 this(coordinate.y, coordinate.x, colour, seen) { }

        public GoStone(GoMove m, bool seen = false) : this(m.y, m.x, m.c, seen) { }

        public Coordinate Coordinate { get; private set; }
        public GoColour Colour { get; set; }
        public bool Seen { get; set; }

        // ********************************************
        //  method:     public GoStone Clone()
        //  purpose:    create a deep copy of the class
        //  parameters: none
        //  returns:    GoStone of deep copy of class
        // ********************************************
        public GoStone Clone()
        {
            return new GoStone(Coordinate, Colour, Seen);
        }

        // ****************************************************************
        //  method:     public bool IsEquivalentEncapsulation(GoStone that)
        //  purpose:    check if fields of both classes are equivalent
        //  parameters: GoStone that
        //  returns:    bool of whether fields are equivalent or not
        // ****************************************************************
        public bool IsEquivalentEncapsulation(GoStone that)
        {
            return this.Coordinate.Equals(that.Coordinate) && this.Colour == that.Colour && this.Seen == that.Seen;
        }

        // **************************************************
        //  method:         public override string ToString()
        //  purpose:        facilitate debugging via Console
        //  parameters:     none
        //  returns:        string representing GoStone
        // **************************************************
        public override string ToString()
        {
            return string.Format("({0}, {1})", Colour, Coordinate);
        }
    }
}