// *************************************************
//  GobanLogic.cs - contains public class GobanLogic
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// *************************************************

using System;
using System.Collections.Generic;
using System.Linq;


namespace Go
{
    // *******************************************************************************************************************************
    //  class:      public class GobanLogic
    //  purpose:    inheritable library of methods workingwith GoStone[,] arrays with respect to the basic rules/logic of a game of Go
    //  notes:      - Ko/Triple Ko is not considered here as this would require multiple states (See GoEngine class)
    //              - New Zealand Ruleset is not compatible as self-capture is forbidden in the current logic
    // *******************************************************************************************************************************
    public class GobanLogic
    {
        #region board-centric methods

        // **************************************************************************************************
        //  method:         protected bool IsWithinDimensions(GoStone[,] board, int y, int x)
        //  purpose:        checks if there is a board and whether the given coordinates fall somewhere on it
        //  parameters:     GoStone[,] board
        //                  int y
        //                  int x
        //  returns:        bool of the aforementioned checks
        // **************************************************************************************************
        protected bool IsWithinDimensions(GoStone[,] board, int y, int x)
        {
            if (board == null)
                return false;

            return y > -1 && y < board.GetLength(0) && x > -1 && x < board.GetLength(1);
        }

        // *****************************************************************************************
        //  method:         protected bool AreEqual(GoStone[,] a, GoStone[,] b)
        //  purpose:        checks if the given two boards contain the same dimensions, and for each
        //                  board element, the equivalent field data
        //  parameters:     GoStone[,] a
        //                  GoStone[,] b
        //  returns:        bool of aforementioned checks
        // *****************************************************************************************
        protected bool AreEqual(GoStone[,] a, GoStone[,] b)
        {
            if (a == null)
                return a == null && b == null;
            else if (a.GetLength(0) != b.GetLength(0) || a.GetLength(1) != b.GetLength(1))
                return false;

            for (int j = 0; j < a.GetLength(0); ++j)
                for (int i = 0; i < a.GetLength(1); ++i)
                    if (!(a[j, i].IsEquivalentEncapsulation(b[j, i])))
                        return false;

            return true;
        }

        // ******************************************************************
        //  method:         protected GoStone[,] CloneBoard(GoStone[,] board)
        //  purpose:        produce a deep copy of the given board
        //  parameters:     GoStone[,] board
        //  returns:        GoStone[,] representing deep copy of given board
        // ******************************************************************
        protected GoStone[,] CloneBoard(GoStone[,] board)
        {
            if (board == null)
                return null;

            GoStone[,] copy = new GoStone[board.GetLength(0), board.GetLength(1)];

            for (int j = 0; j < copy.GetLength(0); ++j)
                for (int i = 0; i < copy.GetLength(1); ++i)
                    copy[j, i] = board[j, i].Clone();

            return copy;
        }

        // *****************************************************************
        //  method:         protected GoStone[,] GetEmptyBoard(int h, int w)
        //  purpose:        generates an empty board of the given dimensions
        //  parameters:     int h (height)
        //                  int w (width)
        //  returns:        GoStone[,] of generated empty board
        // *****************************************************************
        protected GoStone[,] GetEmptyBoard(int h, int w)
        {
            if (h < 1 || w < 1)
                throw new ArgumentException("Board must have positive dimensions");

            GoStone[,] board = new GoStone[h, w];

            for (int j = 0; j < h; ++j)
                for (int i = 0; i < w; ++i)
                    board[j, i] = new GoStone(j, i, GoColour.None);

            return board;
        }

        #endregion
        #region stone placement methods

        // ******************************************************************************************************************
        //  method:         protected GoStone[,] Place(GoStone[,] board, GoMove m)
        //  purpose:        generate a new board state by placing the GoStone defined by GoMove on the given board arbitrarily
        //  parameters:     GoStone[,] board
        //                  GoMove m
        //  returns:        GoStone[,] of new board state
        // ******************************************************************************************************************
        protected GoStone[,] Place(GoStone[,] board, GoMove m)
        {
            GoStone[,] temp = CloneBoard(board);

            if (IsWithinDimensions(board, m.y, m.x))
                temp[m.y, m.x] = new GoStone(m);

            return temp;
        }

        // ***********************************************************************************************
        //  method:         protected bool IsPlayable(GoStone[,] board, GoMove m)
        //  purpose:        verify if the GoStone defined by GoMove is playable on the current board state
        //  parameters:     GoStone[,] board
        //                  GoMove m
        //  notes:          - the placed stone will form a group and that group must:
        //                      (i)     have at least one liberty OR
        //                     (ii)     force an opponent's group adjacent to have no liberties
        //                    this will avoid self capture, although this can be circumvented by forcing
        //                    a check at the end to self-capture the group created by the new GoStone
        //  returns:        bool of whether move is playable or not on the given board state
        // ***********************************************************************************************
        protected bool IsPlayable(GoStone[,] board, GoMove m)
        {
            if (board[m.y, m.x].Colour != GoColour.None || board == null)
                return false;

            GoStone[,] temp = CloneBoard(board);
            temp[m.y, m.x] = new GoStone(m);

            List<Coordinate> group = GetGroup(temp, m.y, m.x);
            if (GetLiberties(temp, group).Count == 0)
                return GetBorder(temp, group).Any(b => GetLiberties(temp, GetGroup(temp, b.y, b.x)).Count == 0);

            return true;
        }

        #endregion
        #region grouping methods

        // **************************************************************************************************************
        //  method:         protected List<Coordinate> GetGroup(GoStone[,] board, int y, int x)
        //  purpose:        get a group of stones connected horizontally and vertically on the board by the colour of the
        //                  stone defined by the given coordinate, without repeats
        //  parameters:     GoStone[,] board
        //                  int y
        //                  int x
        //  returns:        List<Coordinate> of coordinates of the group generated by the given coordinate
        // **************************************************************************************************************
        protected List<Coordinate> GetGroup(GoStone[,] board, int y, int x)
        {
            List<Coordinate> group = new List<Coordinate>();

            if (board != null && IsWithinDimensions(board, y, x))
                group.AddRange(GetAdjacentCoordinatesWithSameColourAndMarkSeen(board, y, x, board[y, x].Colour));

            group.ForEach(s => board[s.y, s.x].Seen = false);
            return group;
        }

        // **************************************************************************************************
        //  method:         protected List<Coordinate> GetLiberties(GoStone[,] board, List<Coordinate> group)
        //  purpose:        get the liberties of the given group of stones without repeats
        //  parameters:     GoStone[,] board
        //                  List<Coordinate> group
        //  returns:        List<Coordinate> of coordinates of the liberties of the given group
        // **************************************************************************************************
        protected List<Coordinate> GetLiberties(GoStone[,] board, List<Coordinate> group)
        {
            List<Coordinate> libs = new List<Coordinate>();

            if (board != null && group != null)
            {
                group.ForEach(s => libs.AddRange(GetLibertiesAndMarkSeen(board, s.y, s.x)));
                libs.ForEach(s => board[s.y, s.x].Seen = false);
            }

            return libs;
        }

        // *********************************************************************************************************
        //  method:         protected List<Coordinate> GetBorder(GoStone[,] board, List<Coordinate> group)
        //  purpose:        get the bordering (non-liberty) stones of the given group without repeats
        //  parameters:     GoStone[,] board
        //                  List<Coordinate> group
        //  notes:          - the acceptable border colours depend on the group makeup:
        //                      black -> white
        //                      white -> black
        //                      none  -> white or black
        //  returns:        List<Coordinate> of coordinates of the bordering (non-liberty) stones of the given group
        // *********************************************************************************************************
        protected List<Coordinate> GetBorder(GoStone[,] board, List<Coordinate> group)
        {
            List<Coordinate> border = new List<Coordinate>();

            if (board != null && group != null)
            {
                group.ForEach(s => border.AddRange(GetBorderingCoordinatesAndMarkSeen(board, s)));
                border.ForEach(s => board[s.y, s.x].Seen = false);
            }

            return border;
        }

        // ****************************************************************************************************
        //  method:         protected void SetGroupColour(GoStone[,] board, List<Coordinate> group, GoColour c)
        //  purpose:        arbitrarily set the given group stones to the given GoColour
        //  parameters:     GoStone[,] board
        //                  List<Coordinate> group
        //                  GoColour c (colour)
        //  returns:        nothing
        // ****************************************************************************************************
        protected void SetGroupColour(GoStone[,] board, List<Coordinate> group, GoColour c)
        {
            if (board != null && group != null)
                group.ForEach(s => board[s.y, s.x].Colour = c);
        }

        #endregion
        #region helpers

        // *************************************************************************************************************
        //  helper method:      private void FilterBorderByColour(GoStone[,] board, List<Coordinate> border, GoColour c)
        //  purpose:            strip out coordinates from border which do not belong (see GetBorder)
        //                      - used in GetBorderingCoordinatesAndMarkSeen -> GetBorder
        //  parameters:         GoStone[,] board
        //                      List<Coordinate> border
        //                      GoColour c
        //  returns:            nothing
        // *************************************************************************************************************
        private void FilterBorderByColour(GoStone[,] board, List<Coordinate> border, GoColour c)
        {
            switch (c)
            {
                case GoColour.Black:
                    border.RemoveAll(coord => board[coord.y, coord.x].Colour != GoColour.White);
                    break;
                case GoColour.White:
                    border.RemoveAll(coord => board[coord.y, coord.x].Colour != GoColour.Black);
                    break;
                default:
                    border.RemoveAll(coord => board[coord.y, coord.x].Colour == GoColour.None);
                    break;
            }
        }

        // ******************************************************************************************************************
        //  helper method:      private void GetLibertyAndMarkSeen
        //  purpose:            verify that there is a board, the given coordinate lies on it, and the GoStone defined by the
        //                      coordinate qualifies as a liberty
        //                      - used in GetLibertiesAndMarkSeen -> GetLiberties
        //  parameters:         GoStone[,] board
        //                      List<Coordinate> libs
        //                      int y
        //                      int x
        //  returns:            nothing
        // ******************************************************************************************************************
        private void GetLibertyAndMarkSeen(GoStone[,] board, List<Coordinate> libs, int y, int x)
        {
            if (IsWithinDimensions(board, y, x) && board[y, x].Colour == GoColour.None && !board[y, x].Seen)
            {
                board[y, x].Seen = true;
                libs.Add(board[y, x].Coordinate);
            }
        }

        // *****************************************************************************************************
        //  helper method:      private List<Coordinate> GetLibertiesAndMarkSeen(GoStone[,] board, int y, int x)
        //  purpose:            retrieve all possible liberties of a GoStone defined by the given coordinate
        //                      - used in GetLiberties
        //  parameters:         GoStone[,] board
        //                      int y
        //                      int x
        //  notes:              each actual liberty has its property Seen marked true
        //  returns:            List<Coordinate> of coordinates of the actual liberties
        // *****************************************************************************************************
        private List<Coordinate> GetLibertiesAndMarkSeen(GoStone[,] board, int y, int x)
        {
            List<Coordinate> libs = new List<Coordinate>();

            if (IsWithinDimensions(board, y, x))
            {
                GetLibertyAndMarkSeen(board, libs, y + 1, x);
                GetLibertyAndMarkSeen(board, libs, y - 1, x);
                GetLibertyAndMarkSeen(board, libs, y, x + 1);
                GetLibertyAndMarkSeen(board, libs, y, x - 1);
            }

            return libs;
        }

        // *****************************************************************************************************************************************
        //  helper method:      private List<Coordinate> GetAdjacentCoordinatesWithSameColourAndMarkSeen(GoStone[,] board, int y, int x, GoColour c)
        //  purpose:            recursively retrieve all similarly coloured GoStones adjacent to each other
        //                      - used in GetGroup
        //  parameters:         GoStone[,] board
        //                      int y
        //                      int x
        //                      GoColour c
        //  notes:              each retrieved adjacent GoStone will have its property Seen marked true
        //  returns:            List<Coordinate> of coordinates of the adjacent GoStones
        // *****************************************************************************************************************************************
        private List<Coordinate> GetAdjacentCoordinatesWithSameColourAndMarkSeen(GoStone[,] board, int y, int x, GoColour c)
        {
            List<Coordinate> adjCoords = new List<Coordinate>();

            if (IsWithinDimensions(board, y, x) && board[y, x].Colour == c && !board[y, x].Seen)
            {
                board[y, x].Seen = true;
                adjCoords.Add(board[y, x].Coordinate);
                adjCoords.AddRange(GetAdjacentCoordinatesWithSameColourAndMarkSeen(board, y + 1, x, c));
                adjCoords.AddRange(GetAdjacentCoordinatesWithSameColourAndMarkSeen(board, y - 1, x, c));
                adjCoords.AddRange(GetAdjacentCoordinatesWithSameColourAndMarkSeen(board, y, x + 1, c));
                adjCoords.AddRange(GetAdjacentCoordinatesWithSameColourAndMarkSeen(board, y, x - 1, c));
            }

            return adjCoords;
        }

        // *************************************************************************************************************************
        //  helper method:      private List<Coordinate> GetBorderingCoordinatesAndMarkSeen(GoStone[,] board, Coordinate coordinate)
        //  purpose:            check if each possible bordering stone to a given coordinate is in the board, add it to a list, then
        //                      filter by the GoStone defined by the given coordinate's colour
        //                      - used in GetBorder
        //  parameters:         GoStone[,] board
        //                      Coordinate coordinate
        //  notes:              each GoStone which is in the group after filtering will have its property Seen marked true
        //  returns:            List<Coordinate> of coordinates of the GoStones along the direct border of the given coordinate
        // *************************************************************************************************************************
        private List<Coordinate> GetBorderingCoordinatesAndMarkSeen(GoStone[,] board, Coordinate coordinate)
        {
            List<Coordinate> border = new List<Coordinate>();
            int y = coordinate.y;
            int x = coordinate.x;

            if (IsWithinDimensions(board, y, x))
            {
                GoColour c = board[y, x].Colour;
                if (IsWithinDimensions(board, y + 1, x) && !board[y + 1, x].Seen)
                    border.Add(board[y + 1, x].Coordinate);
                if (IsWithinDimensions(board, y - 1, x) && !board[y - 1, x].Seen)
                    border.Add(board[y - 1, x].Coordinate);
                if (IsWithinDimensions(board, y, x + 1) && !board[y, x + 1].Seen)
                    border.Add(board[y, x + 1].Coordinate);
                if (IsWithinDimensions(board, y, x - 1) && !board[y, x - 1].Seen)
                    border.Add(board[y, x - 1].Coordinate);

                FilterBorderByColour(board, border, c);
            }

            border.ForEach(s => board[s.y, s.x].Seen = true);
            return border;
        }

        #endregion
    }
}