// **********************************************************************
// InfoSGF - used for storing information retrieved from parsed SGF files
//
// Written by Jonathan Melcher
// Last updated Feb 04, 2015
// **********************************************************************

#region using directives

using System;
using System.Collections.Generic;

#endregion

namespace SGFParser
{
    // ********************************************************************************************
    // InfoSGF class -  used as a slightly-too-big struct to store the data associated with NodeSGF
    //                  contains fields and properties pertaining to move/game/placement properties
    //                  of the parsed SGF file
    // ********************************************************************************************
    public class InfoSGF
    {
        #region non-static fields

        private List<Tuple<bool, int, int, int>> moves = new List<Tuple<bool, int, int, int>> { };
        private List<string> comments = new List<string> { };
        private Dictionary<string, string> gameProperties = new Dictionary<string, string> { };
        private bool toClear = false;
        private bool isMove;

        #endregion
        #region constructor

        public InfoSGF(bool isMove)
        {
            this.isMove = isMove;
        }

        #endregion
        #region properties

        public Dictionary<string, string> GameProperties
        {
            get { return gameProperties; }
        }

        public List<Tuple<bool, int, int, int>> Moves
        {
            get { return moves; }
        }

        public List<string> Comments
        {
            get { return comments; }
        }

        public bool IsMove
        {
            get { return isMove; }
            set { isMove = value; }
        }

        public bool ToClear
        {
            get { return toClear; }
            set { toClear = value; }
        }

        #endregion
    }
}
