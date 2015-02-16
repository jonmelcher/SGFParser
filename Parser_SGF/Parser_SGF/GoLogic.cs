// *****************************************************************************************
// GoLogic - Container for the states of a Go game parsed from an SGF file using Parser_SGF.
//
// Written by Jonathan Melcher
// Feb 15, 2015
// *****************************************************************************************

#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

#endregion

namespace Parser_SGF
{
    // **************************************************************************************
    // GoLogic class -  constructor takes either a TreeSGF object or parses an SGF file using
    //                  ParserSGF to create a TreeSGF object.  Methods for identifying inval-
    //                  id board positions, clearing off prisoners, counting liberties, etc.
    //                  are available, as well as moving between states of the game.  Note
    //                  that we force the games to be Go games as well as 19x19 size.
    // **************************************************************************************
    public class GoLogic
    {
        #region constant fields

        public const int boardWidth = 19;
        public const int boardHeight = 19;
        public enum moveState { None, Black, White };

        #endregion
        #region gamestate/tree fields

        private List<moveState[,]> gameStates = new List<moveState[,]> { new moveState[19, 19] };
        private TreeSGF game;
        public NodeSGF cursor;

        #endregion
        #region game property fields

        private bool tryToCapture = true;
        private bool whitePass = false;
        private bool blackPass = false;
        private int branchNumber = 1;
        private int currentMove = 0;

        #endregion

        #region constructor

        // Constructor either parses an SGF file located at the filepath or takes in a TreeSGF
        // object.  Once the treeSGF is available, we must set each node's ID and extract their
        // data from the parsed keywords/markups.  We check if we have a Go game and that it is
        // size 19x19, and then initialize our gamestate/tree fields.
        public GoLogic(string filepath, TreeSGF gameSource = null)
        {
            try
            {
                if (gameSource != null)
                    game = gameSource;
                else
                    using (StreamReader sr = new StreamReader(filepath))
                        game = ParserSGF.Parse(sr.ReadToEnd());

                ParserSGF.ExtractDataFromNodes(game);
                game.SetNodeIDsToRoot();
                CheckIfGM1AndSZ19();
                cursor = game.Root;
            }
            catch (IOException ex)
            {
                Console.WriteLine("File Reading Error: {0}", ex.Message);
                game = new TreeSGF(new NodeSGF());
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("File Source Error: {0}", ex.Message);
                game = new TreeSGF(new NodeSGF());
            }
        }

        #endregion
        #region properties

        public List<moveState[,]> GameStates
        {
            get { return gameStates; }
        }

        public TreeSGF Game
        {
            get { return game; }
        }

        public int CurrentMove
        {
            get { return currentMove; }
        }

        public int Branch
        {
            get { return branchNumber; }
            set
            {
                if (value > 0 && value <= cursor.Children.Count)
                    branchNumber = value;
            }
        }

        #endregion
        #region get methods - retrieving value/positions from current gamestate

        // Tallying the number of liberties of a group based on a single member
        public int GetLiberties(moveState[,] board, Tuple<moveState, int, int> move)
        {
            int sum = 0;
            List<Tuple<moveState, int, int>> group = GetGroup(board, move);

            foreach (Tuple<moveState, int, int> groupMember in group)
            {
                for (int i = groupMember.Item2 - 1; i < groupMember.Item2 + 2; i += 2)
                    if (i >= 0 && i < boardWidth && board[i, groupMember.Item3] == moveState.None)
                        sum++;

                for (int j = groupMember.Item3 - 1; j < groupMember.Item3 + 2; j += 2)
                    if (j >= 0 && j < boardHeight && board[groupMember.Item2, j] == moveState.None)
                        sum++;
            }
            return sum;
        }

        // Finding all the opponent stones that are directly adjacent to a specified move
        public List<Tuple<moveState, int, int>> GetAdjacentOpponentMoves(moveState[,] board, Tuple<moveState, int, int> move)
        {
            List<Tuple<moveState, int, int>> adjOpp = new List<Tuple<moveState, int, int>>();
            moveState oppositeOfMove = move.Item1 == moveState.Black ? moveState.White : moveState.Black;
            for (int i = move.Item2 - 1; i < move.Item2 + 2; i += 2)
                if (i >= 0 && i < boardWidth && board[i, move.Item3] == oppositeOfMove)
                    adjOpp.Add(Tuple.Create(oppositeOfMove, i, move.Item3));

            for (int j = move.Item3 - 1; j < move.Item3 + 2; j += 2)
                if (j >= 0 && j < boardHeight && board[move.Item2, j] == oppositeOfMove)
                    adjOpp.Add(Tuple.Create(oppositeOfMove, move.Item2, j));

            return adjOpp;
        }

        // Finding all the connected stones from a single member using breadth-first search
        // Connected stones are either vertical or horizontal on the board
        public List<Tuple<moveState, int, int>> GetGroup(moveState[,] board, Tuple<moveState, int, int> groupMember)
        {
            List<Tuple<moveState, int, int>> group = new List<Tuple<moveState, int, int>>();
            Stack<Tuple<moveState, int, int>> stack = new Stack<Tuple<moveState, int, int>>();

            stack.Push(groupMember);
            while (stack.Count != 0)
            {
                Tuple<moveState, int, int> currentMember = stack.Pop();
                group.Add(currentMember);

                for (int i = currentMember.Item2 - 1; i < currentMember.Item2 + 2; i += 2)
                    if (i >= 0 && i < boardWidth && !group.Contains(
                        Tuple.Create(board[i, currentMember.Item3], i, currentMember.Item3)))
                        if (board[i, currentMember.Item3] == currentMember.Item1)
                            stack.Push(Tuple.Create(currentMember.Item1, i, currentMember.Item3));

                for (int j = currentMember.Item3 - 1; j < currentMember.Item3 + 2; j += 2)
                    if (j >= 0 && j < boardHeight && !group.Contains(
                        Tuple.Create(board[currentMember.Item2, j], currentMember.Item2, j)))
                        if (board[currentMember.Item2, j] == currentMember.Item1)
                            stack.Push(Tuple.Create(currentMember.Item1, currentMember.Item2, j));
            }
            return group;
        }

        // Checking (without capturing) whether a move would cause captures
        private bool IsInvalidPositionFromLastMove(
            moveState[,] board, Tuple<moveState, int, int> lastMove)
        {
            if (lastMove.Item1 == moveState.None)
                return false;

            return GetAdjacentOpponentMoves(board, lastMove).Any(
                x => GetLiberties(board, x) == 0) || GetLiberties(board, lastMove) == 0;
        }

        // Checking whether the given move is a pass (coordinates < 0)
        private bool IsPass(Tuple<moveState, int, int> move, ref bool black, ref bool white)
        {
            if (move.Item2 < 0 || move.Item3 < 0 && move.Item1 != moveState.None)
            {
                if (move.Item1 == moveState.Black)
                    black = true;
                else
                    white = true;
                return true;
            }
            return false;
        }

        // Checks the GameTree's root properties to determine if the SGF file was for a Go game of size 19x19
        public void CheckIfGM1AndSZ19()
        {
            if (game.Root.RootProperties.ContainsKey("GM") && int.Parse(game.Root.RootProperties["GM"]) != 1)
                throw new ArgumentException("Game is not a Go game");
            else if (game.Root.RootProperties.ContainsKey("SZ") && game.Root.RootProperties["SZ"] != "19")
            {
                string[] args = game.Root.RootProperties["SZ"].Split(':');
                args[0] = args[0].Trim();
                args[1] = args[1].Trim();
                if (args[0] != "19" || args[1] != "19")
                    throw new ArgumentException("Game is not size 19x19");
            }
        }

        #endregion
        #region set methods - changing the current gamestate

        // Removing a stone from the board with the option of updating score
        public void ClearStone(moveState[,] board, Tuple<moveState, int, int> coordinate)
        {
            board[coordinate.Item2, coordinate.Item3] = moveState.None;
        }

        // Removing a group of stones from the board with the option of updating score (overloaded)
        public void ClearGroup(moveState[,] board, List<Tuple<moveState, int, int>> group)
        {
            group.ForEach(move => ClearStone(board, move));
        }

        // Removing a group of stones from the board with the option of updating score (overloaded)
        public void ClearGroup(moveState[,] board, HashSet<Tuple<moveState, int, int>> group)
        {
            group.ToList<Tuple<moveState, int, int>>().ForEach(move => ClearStone(board, move));
        }

        // Clearing any captures caused by the given move
        private void ClearCapturesFromMove(moveState[,] board, Tuple<moveState, int, int> move)
        {
            if (!tryToCapture)
                return;

            HashSet<Tuple<moveState, int, int>> prisoners = new HashSet<Tuple<moveState, int, int>>();
            GetAdjacentOpponentMoves(board, move).ForEach(x =>
            {
                if (GetLiberties(board, x) == 0)
                    GetGroup(board, x).ForEach(y => prisoners.Add(y));
            });

            ClearGroup(board, prisoners);

            if (GetLiberties(board, move) == 0)
                ClearGroup(board, GetGroup(board, move));
        }

        #endregion
        #region traversal methods - moving between gamestates

        // Calculating the next gamestate and moving to it via cursor
        public void NextMove()
        {
            if (cursor.Children.Count == 0)
                return;

            moveState[,] newBoardState = gameStates.Last().Clone() as moveState[,];
            cursor = cursor.Children[cursor.Children.Count - branchNumber];
            foreach (Tuple<moveState, int, int> placement in cursor.Placement)
            {
                newBoardState[placement.Item2, placement.Item3] = placement.Item1;

                if (tryToCapture && placement.Item1 != moveState.None)
                    tryToCapture = !IsInvalidPositionFromLastMove(newBoardState, placement);
            }

            if (cursor.Move != null && !IsPass(cursor.Move, ref blackPass, ref whitePass))
            {
                newBoardState[cursor.Move.Item2, cursor.Move.Item3] = cursor.Move.Item1;
                ClearCapturesFromMove(newBoardState, cursor.Move);
                currentMove++;
            }

            gameStates.Add(newBoardState);
            branchNumber = 1;
        }

        // Removing the last state and moving to previous state via cursor
        public void PreviousMove()
        {
            if (cursor.Parent == null)
                return;
            else if (cursor.IsMove)
                currentMove--;

            cursor = cursor.Parent;
            gameStates.RemoveAt(gameStates.Count - 1);
            branchNumber = 1;
        }

        // Remove all but initial state and move to first state via cursor
        public void FirstMove()
        {
            gameStates = new List<moveState[,]> { new moveState[19, 19] };
            cursor = game.Root;
            currentMove = 0;
            branchNumber = 1;
            tryToCapture = true;
            blackPass = false;
            whitePass = false;
        }


        #endregion
    }
}
