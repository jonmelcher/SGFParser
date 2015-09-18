// *********************************************
//  GoEngine.cs - contains public class GoEngine
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// *********************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace Go
{
    // *******************************************************************************************************************************
    //  class:      public class GoEngine : GobanLogic
    //  purpose:    serves as an outward-facing class communicating between SGFViewer and ParserSGF to calculate and store boardstates
    //              boardstate calculation is done through the inherited class GobanLogic; navigation is done through navigating the
    //              TreeSGF structure after parsing any given SGF file via ParserSGF
    // *******************************************************************************************************************************
    public class GoEngine : GobanLogic
    {
        private List<GoStone[,]> boardStates;   // ordered list of all viewed board states
        private bool tryToCapture;              // flag to determine whether to analyze the next board state or place moves arbitrarily

        // ***********************************************************************
        //  constructor:        public GoEngine()
        //  purpose:            initialize fields to their non-null default values
        //  parameters:         none
        // ***********************************************************************
        public GoEngine()
        {
            boardStates = new List<GoStone[,]>();
            tryToCapture = true;
        }

        // *********************************************************************************************************************
        //  constructor:        public GoEngine(string filepath) : this()
        //  purpose:            initialize fields to their non-null default values as well as parses SGF file into the navigable
        //                      TreeSGF and initializes for navigation from the beginning
        //  parameters:         string filepath
        //  notes:              only SGF files for Go games of 19x19 are currently acceptable - everything else will parse into
        //                      an empty tree
        // *********************************************************************************************************************
        public GoEngine(string filepath) : this()
        {
            LoadAndParse(filepath);
        }

        public NodeSGF Cursor { get; set; }         // the cursor for navigating the TreeSGF
        public TreeSGF Game { get; set; }           // the TreeSGF structure to navigate
        public int CurrentMove { get; set; }        // the current move of the game            
        public int BranchNumber { get; set; }       // the current branch of the game

        // property for getting a deep copy of the current board state being analyzed
        public GoStone[,] CurrentState { get { return boardStates.Count != 0 ? CloneBoard(boardStates.Last()) : null; } }

        // **********************************************************************************************************
        //  method:         public void LoadAndParse(string filepath)
        //  purpose:        verify that filepath leads to an SGF file, dump its contents into a string and then parse
        //                  the raw contents using ParserSGF to generate a navigable TreeSGF structure
        //  parameters:     string filepath
        //  returns:        nothing (initializes Game / Game-related properties)
        // **********************************************************************************************************
        public void LoadAndParse(string filepath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                    Game = ParserSGF.Parse(sr.ReadToEnd());

                ParserSGF.ExtractDataFromNodes(Game);
                Game.SetNodeIDsToRoot();

                if (!IsGM1AndSZ19())
                    throw new ArgumentException("SGF is not a valid Go game of size 19x19");

                Cursor = Game.Root;
                boardStates.Add(GetEmptyBoard(19, 19));
                BranchNumber = 1;
            }
            catch (IOException ex)
            {
                Console.WriteLine("File Reading Error: {0}", ex.Message);
                Game = new TreeSGF(new NodeSGF());
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("File Source Error: {0}", ex.Message);
                Game = new TreeSGF(new NodeSGF());
            }
        }

        // **************************************************************************************************************
        //  method:         public void NextMove()
        //  purpose:        navigate to the next NodeSGF in the Game TreeSGF structure, updating all pertinent properties
        //                  and calculating the next boardstate
        //  parameters:     none
        //  returns:        none
        // **************************************************************************************************************
        public void NextMove()
        {
            // no forward move available
            if (Cursor.Children.Count == 0)
                return;

            // clone current board state for analysis and move Cursor
            GoStone[,] newState = CurrentState;
            Cursor = Cursor.Children[Cursor.Children.Count - BranchNumber] ?? Cursor.Children.Last();

            // place all arbitrary placements first
            foreach (GoMove m in Cursor.Placement)
                newState = Play(m);

            // play (and calculate new boardstate) move
            switch (Cursor.Move.t)
            {
                case MoveType.Normal:
                    newState = Play(Cursor.Move);
                    ++CurrentMove;
                    break;
                case MoveType.Pass:
                    ++CurrentMove;
                    break;
            }

            // add newly calculated state and reset BranchNumber
            boardStates.Add(newState);
            BranchNumber = 1;
        }

        // ********************************************************************************************
        //  method:         public void PreviousMove()
        //  purpose:        return to the previous board state and update/rollback pertinent properties
        //  parameters:     none
        //  returns:        nothing
        // ********************************************************************************************
        public void PreviousMove()
        {
            // only continue if there are moves available
            if (boardStates.Count == 1)
                return;
            else if (Cursor.Move.t == MoveType.Normal)
                CurrentMove--;

            // update/rollback
            Cursor = Cursor.Parent;
            boardStates.RemoveAt(boardStates.Count - 1);
            BranchNumber = 1;
        }

        // **********************************************************************************************
        //  method:         public void FirstMove()
        //  purpose:        resets board states and navigation to the start of the Game TreeSGF structure
        //  parameters:     none
        //  returns:        nothing
        // **********************************************************************************************
        public void FirstMove()
        {
            boardStates.Clear();
            boardStates.Add(GetEmptyBoard(19, 19));
            Cursor = Game.Root;
            CurrentMove = 0;
            BranchNumber = 1;
            tryToCapture = true;
        }

        // ********************************************************************************************************************
        //  method:         private GoStone[,] Play(GoMove m)
        //  purpose:        calculate the next board state based on the current one and the given GoMove
        //  parameters:     GoMove m
        //  notes:          if GoMove is not valid, tryToCapture is toggled off and all moves are placed arbitrarily henceforth
        //  returns:        GoStone[,] of new, calculated board state
        // ********************************************************************************************************************
        private GoStone[,] Play(GoMove m)
        {
            GoStone[,] temp = CurrentState;

            // GoMove is not valid - disable tryToCapture and place arbitrarily
            if (!IsPlayable(temp, m) && tryToCapture)
            {
                tryToCapture = false;
                return Place(temp, m);
            }

            // gather information for new board state analysis
            temp[m.y, m.x] = new GoStone(m);
            List<Coordinate> group = GetGroup(temp, m.y, m.x);
            List<Coordinate> border = GetBorder(temp, group);

            border.ForEach(b =>
            {
                if (temp[b.y, b.x].Colour != GoColour.None)
                {
                    List<Coordinate> grp = GetGroup(temp, b.y, b.x);
                    if (GetLiberties(temp, grp).Count == 0)
                        SetGroupColour(temp, grp, GoColour.None);
                }
            });

            return temp;
        }

        // ********************************************************************************************
        //  method:         private bool IsGM1AndSZ19()
        //  purpose:        verify that the parsed Game TreeSGF structure hosts a Go game of size 19x19
        //  parameters:     none
        //  returns:        bool for the aforementioned check
        // ********************************************************************************************
        private bool IsGM1AndSZ19()
        {
            if (Game.Root.RootProperties.ContainsKey("GM") && int.Parse(Game.Root.RootProperties["GM"]) != 1)
                return false;
            else if (Game.Root.RootProperties.ContainsKey("SZ") && Game.Root.RootProperties["SZ"] != "19")
            {
                string[] args = Game.Root.RootProperties["SZ"].Split(':');
                args[0] = args[0].Trim();
                args[1] = args[1].Trim();
                if (args[0] != "19" || args[1] != "19")
                    return false;
            }

            return true;
        }
    }
}
