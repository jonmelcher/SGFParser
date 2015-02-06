// *********************************************************************
// NodeSGF - used as objects in the tree structure of a parsed SGF file
//
// Written by Jonathan Melcher
// Last updated Feb 04, 2015
// *********************************************************************

#region using directives

using System;
using System.Collections.Generic;

#endregion

namespace SGFParser
{
    // *********************************************************************************
    // NodeSGF class -  Main object consisting of fields and properties used for storing
    //                  data parsed from an SGF file, and are used as members of TreeSGF  
    // *********************************************************************************
    public class NodeSGF
    {
        #region non-static fields

        private List<NodeSGF> children = new List<NodeSGF> { };
        private NodeSGF parent;
        private InfoSGF data;       // Stores all move/game/placement properties of node
        string id = "";             // Used for identification in TreeSGF
        
        #endregion
        #region constructor

        public NodeSGF(InfoSGF data, NodeSGF parent = null)
        {
            this.data = data;
            this.parent = parent;
        }

        #endregion
        #region properties

        public InfoSGF Data
        {
            get { return data; }
        }

        public List<NodeSGF> Children
        {
            get { return children; }
        }

        public NodeSGF Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        #endregion
    }
}
