// *************************************************************
// ParserSGF - used to parse a Go SGF file into a TreeSGF object
//
// Written by Jonathan Melcher
// Last updated Feb 04, 2015
// *************************************************************

#region using directives

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace SGFParser
{
    #region class description and introduction

    // ****************************************************************************************************************
    // ParserSGF class -    parses an SGF file into a TreeSGF object.  Only a subset of the total keywords are accepted
    //                      and are as follows:
    //                      
    //                      Game Properties:                Form:       Stored in the root of the TreeSGF object
    //                      PB : Black Player's name        [x]
    //                      BR : Black player's rank        [x]
    //                      PW : White Player's name        [x]
    //                      WR : White Player's rank        [x]
    //                      GN : Game name                  [x]
    //                      RE : Result                     [x]
    //
    //                      Movement Properties:                      Each move represents a node in the TreeSGF object
    //                      ;B : Black move                 [ab]      each lowercase letter represents a coordinate; we
    //                      ;W : White move                 [ab]      only use size 19^2 here so the alphabet is simply
    //                                                                "abcdefghijklmnopqrs"
    //
    //                      Placement Properties:                     Chain of moves represents a node with a specified
    //                      AB : Add Black Stones   [ab]+[ab:cd]      markup.  Chains can be condensed using slice not-
    //                      AW : Add White Stones   [ab]+[ab:cd]      ation or not, and are a concatenated combination
    //                      AE : Remove Stones      [ab]+[ab:cd]      of the forms.  AE removes stones and requires a
    //                      CR : Circle             [ab]+[ab:cd]      flag.
    //                      MA : X-through          [ab]+[ab:cd]
    //                      SQ : Square             [ab]+[ab:cd]
    //                      TR : Triangle           [ab]+[ab:cd]
    // ****************************************************************************************************************

    #endregion

    public class ParserSGF
    {
        #region non-static fields

        private string rawFile = "";

        #endregion
        #region constructor

        public ParserSGF(string filepath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(filepath))
                {
                    rawFile = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Error: {0}", e.Message));
            }
        }

        #endregion
        #region properties

        public string Raw
        {
            get { return rawFile; }
        }

        #endregion
        #region parse subroutine

        // Creates a tree by parsing out the game/move/placement keywords and storing the related information
        // Acts recursively on subnodes, merging back into the main game tree at the end of the call
        public TreeSGF ParseFile(string source, List<string> gamePropertyArgs,
            List<string> movePropertyArgs, List<string> placementPropertyArgs)
        {
            TreeSGF gameTree = new TreeSGF(new InfoSGF(false));
            string currentBranch = TrimToBranch(source);
            string temp = "";

            // Start one after the first branch character '('
            int index = 1;

            // Runs while at least two characters are left to read
            while (index < currentBranch.Length - 1)
            {
                string possibleKey = currentBranch.Substring(index, 2);
                    
                // Parser finds a gameProperty keyword to apply to the main root
                // Each keyword only works once since they are one-time and static
                if (gamePropertyArgs.Contains(possibleKey))
                {
                    // Shift index to leftbrace of keyword's index and then shift it
                    // to right after the keyword information
                    index += GetContents(currentBranch, index + 2, out temp);

                    if (!gameTree.Root.Data.GameProperties.ContainsKey(possibleKey))
                        gameTree.Root.Data.GameProperties.Add(possibleKey, temp);
                }

                // Parser finds a moveProperty keyword
                // Creates a child of the current cursor and stores pertinent information
                else if (movePropertyArgs.Contains(possibleKey))
                {
                    // Node creation and cursor placement
                    gameTree.AddAtCursor(new InfoSGF(true));
                    gameTree.Cursor = gameTree.Cursor.Children[gameTree.Cursor.Children.Count - 1];

                    // Shift index to leftbrace of keyword's index and then shift it
                    // to right after the keyword information
                    index += GetContents(currentBranch, index + 2, out temp);

                    // Parse string into coordinate and add to move list
                    gameTree.Cursor.Data.Moves.AddRange(MoveDataToCoordinate(new string[2] { possibleKey, temp }));
                }

                // Parser finds a placementProperty keyword
                // Creates a child of the current cursor and stores pertinent information. placementProperty keywords
                // can have condensed notation and chains so must accomodate for this
                else if (placementPropertyArgs.Contains(possibleKey))
                {
                    string placementArgs = "";
                    int markup = GetMarkup(possibleKey);

                    // Node creation and cursor placement
                    gameTree.AddAtCursor(new InfoSGF(false));
                    gameTree.Cursor = gameTree.Cursor.Children[gameTree.Cursor.Children.Count - 1];

                    // Shift index to leftbrace of keyword's index and then shift it
                    // to right after the entire chain of the keyword information
                    while (currentBranch[index + 2] == '[')
                    {
                        index += GetContents(currentBranch, index + 2, out temp);
                        placementArgs += temp;
                    }

                    // Split up placement argument chain and add each placement to the move list
                    foreach (string splitArg in SplitPlacementArgs(placementArgs))
                        gameTree.Cursor.Data.Moves.AddRange(MoveDataToCoordinate(new string[2] { possibleKey, splitArg }, markup));

                    // Check whether stones are being cleared (keyword 'AE')
                    if (possibleKey[1] == 'E')
                        gameTree.Cursor.Data.ToClear = true;
                }

                // Miscellaneous - comments, skipped keyword, and recursive branch case
                else
                    switch (currentBranch[index])
                    {
                        case 'C':
                            // Determine if a comment was added to a node - update current cursor with comment data
                            switch(currentBranch.Substring(index - 1, 3))
                            {
                                case "]C[":
                                case " C[":
                                case ")C[":
                                    // Index shifted over to character after data and then back one to avoid an else clause
                                    index += GetContents(currentBranch, index + 1, out temp);
                                    gameTree.Cursor.Data.Comments.Add(temp);
                                    break;
                                default:
                                    index++;
                                    break;
                            }
                            break;

                        // A keyword was ignored and we can skip the inside of the [] to avoid keyword false positives
                        case '[':
                            index += GetContents(currentBranch, index, out temp);
                            break;

                        // A new branch is created at this point -
                        // recursively make tree from branch and merge back into the main tree
                        case '(':
                            index += GetContents(currentBranch, index, out temp);
                            TreeSGF newBranch = ParseFile(temp, gamePropertyArgs, movePropertyArgs, placementPropertyArgs);
                            gameTree.MergeAtCursor(newBranch);
                            break;

                        default:
                            index++;
                            break;
                    }
            }
            return gameTree;
        }

        #endregion
        #region helper methods

        // Creates a Tuple (isBlack, xCoord, yCoord, markup).  The coords are found via their indices in alphabet
        public static List<Tuple<bool, int, int, int>> MoveDataToCoordinate(string[] args, int markup = 0)
        {
            string alphabet = "abcdefghijklmnopqrs";
            List<Tuple<bool, int, int, int>> result = new List<Tuple<bool, int, int, int>> { };

            switch (args[1].Length)
            {
                // Data is of form []
                case 2:
                    result.Add(Tuple.Create(args[0][1] == 'B', -1, -1, markup));
                    break;

                // Data is of form [xy]
                case 4:
                    result.Add(Tuple.Create(args[0][1] == 'B',
                        alphabet.IndexOf(args[1][2]) + 1, alphabet.IndexOf(args[1][3]) + 1, markup));
                    break;

                // Data is of form [ab:cd]
                case 7:
                    for (int j = alphabet.IndexOf(args[1][2]); j <= alphabet.IndexOf(args[1][5]); j++)
                        for (int i = alphabet.IndexOf(args[1][1]); i <= alphabet.IndexOf(args[1][4]); i++)
                            result.Add(Tuple.Create(args[0][1] == 'B', i + 1, j + 1, markup));
                    break;
            }
            return result;
        }

        // Getting markup value from keyword
        private int GetMarkup(string keyword)
        {
            // Determining markup value
            switch (keyword)
            {
                case "CR":          // Circle
                    return 1;
                case "MA":          // X-ed
                    return 2;
                case "TR":          // Triangle
                    return 3;
                case "SQ":          // Square
                    return 4;
                default:            // No markup
                    return 0;
            }
        }

        // Retrieve length of a valid node string and out it to an outside string
        private int GetContents(string source, int startIndex, out string inside)
        {
            // Guarding against invalid starting indices
            if (startIndex < 0 || startIndex >= source.Length)
            {
                inside = "";
                return 0;
            }
            else
            {
                char leftBrace = source[startIndex];
                char rightBrace = default(char);
                int leftBraceCount = 1;
                int currentIndex = startIndex + 1;

                // Discerning what kind of brace is being used - [] and () are used in SGF files
                // This also guards against strings not starting on a valid left brace
                switch (leftBrace)
                {
                    case '[':
                        rightBrace = ']';
                        break;

                    case '(':
                        rightBrace = ')';
                        break;

                    default:
                        currentIndex = source.Length;
                        break;
                }

                // Iterate through string, until the closing brace is found, calculating length
                while (currentIndex < source.Length && leftBraceCount != 0)
                {
                    if (source[currentIndex] == leftBrace)
                        leftBraceCount++;
                    else if (source[currentIndex] == rightBrace)
                        leftBraceCount--;
                    currentIndex++;
                }

                inside = source.Substring(startIndex, currentIndex - startIndex);
                return inside.Length;
            }
        }
        
        // Trims characters of string outside of outermost parseable node
        private string TrimToBranch(string source)
        {
            string trimmed = "";
            int i = 0;
            while (i < source.Length && source[i] != '(')
                i++;

            // trimmed includes outer braces
            GetContents(source, i, out trimmed);
            return trimmed;
        }

        // Splits a placementProperty argument chain into a list of arguments
        // Argument chain must be in some concatenated combination of [xx] and [xx:xx]
        private string[] SplitPlacementArgs(string source)
        {
            List<string> splitArgs = new List<string> { };
            int startIndex = 0;
            int finishIndex = 0;

            // Iterate through source until the end, shifting start and finish indices
            // to encapsulate each argument in the chain
            while (finishIndex < source.Length && startIndex < source.Length)
            {
                if (source[finishIndex] == ']')
                {
                    finishIndex++;
                    splitArgs.Add(source.Substring(startIndex, finishIndex - startIndex));
                    startIndex = finishIndex;
                }
                finishIndex++;
            }
            return splitArgs.ToArray();
        }

        #endregion
    }
}
