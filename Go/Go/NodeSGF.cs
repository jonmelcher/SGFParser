// *******************************************
//  NodeSGF.cs - contains public class NodeSGF
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// *******************************************

using System;
using System.Collections.Generic;


namespace Go
{
    // *****************************************************************************
    //  class:      public class NodeSGF
    //  purpose:    encapsulate all node-centric information parsed from an SGF file
    //              serves as elements of a TreeSGF data-structure
    // *****************************************************************************
    public class NodeSGF
    {
        // **********************************************************************
        //  constructor:        public NodeSGF()
        //  purpose:            initialize all fields to a non-null default value
        //  parameters:         none
        // **********************************************************************
        public NodeSGF()
        {
            Children = new List<NodeSGF>();
            Placement = new List<GoMove>();
            RootProperties = new Dictionary<string, string>();
            SGFProperties = new List<Tuple<string, string>>();
            Move = new GoMove(-1, -1, GoColour.None, MoveType.None);
            Comment = "";
        }

        public NodeSGF Parent { get; set; }
        public List<NodeSGF> Children { get; private set; }
        public List<GoMove> Placement { get; private set; }
        public GoMove Move { get; set; }
        public string ID { get; set; }
        public string Comment { get; set; }
        public Dictionary<string, string> RootProperties { get; private set; }
        public List<Tuple<string, string>> SGFProperties { get; private set; }
    }
}
