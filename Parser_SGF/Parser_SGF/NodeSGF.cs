// *****************************************************************************
// NodeSGF - Used for holding information parsed from a SGF format source string
//
// Written by Jonathan Melcher
// Feb 12, 2015
// *****************************************************************************

using System;
using System.Collections.Generic;


namespace Parser_SGF
{
    public class NodeSGF
    {
        private List<Tuple<string, string>> data = new List<Tuple<string, string>>();
        private List<Tuple<GoLogic.moveState, int, int>> p_Placement = new List<Tuple<GoLogic.moveState, int, int>>();
        private Dictionary<string, string> p_Root = new Dictionary<string, string>();
        private Tuple<GoLogic.moveState, int, int> p_Move;
        private string p_Comm = "";
        private string id = "";

        private List<NodeSGF> children = new List<NodeSGF>();
        private NodeSGF parent = null;
        private bool isMove = false;

        public NodeSGF Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public List<NodeSGF> Children
        {
            get { return children; }
        }

        public List<Tuple<string, string>> Data
        {
            get { return data; }
        }

        public List<Tuple<GoLogic.moveState, int, int>> Placement
        {
            get { return p_Placement; }
        }

        public Tuple<GoLogic.moveState, int, int> Move
        {
            get { return p_Move; }
            set { p_Move = value; }
        }

        public Dictionary<string, string> RootProperties
        {
            get { return p_Root; }
        }

        public string Comment
        {
            get { return p_Comm; }
            set { p_Comm = value; }
        }

        public bool IsMove
        {
            get { return isMove; }
            set { isMove = value; }
        }

        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public void ClearExtractedData()
        {
            isMove = false;
            Move = null;
            RootProperties.Clear();
            Placement.Clear();
            Comment = "";
        }
    }
}
