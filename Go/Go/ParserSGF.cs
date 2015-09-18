using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Go
{
    // ******************************************************************************************
    // ParserSGF class -    Contains methods and helper functions for extracting data from an SGF
    //                      file into a TreeSGF object, based on the keyword lists provided.
    // ******************************************************************************************
    public static class ParserSGF
    {
        public static readonly List<string> p_Root = new List<string> { "GM", "SZ", "BR", "GN", "PB", "PW", "RE", "WR", "KO" };
        public static readonly List<string> p_Move = new List<string> { "B", "W" };
        public static readonly List<string> p_Plce = new List<string> { "CR", "MA", "SQ", "TR" };
        public static readonly List<string> p_Annt = new List<string> { "C" };

        // Wrapper for parsing from a string of raw SGF data - outside game trees are split apart
        // and then parsed into TreeSGF objects, merged into an empty TreeSGF object.
        public static TreeSGF Parse(string source)
        {
            TreeSGF tree = new TreeSGF(new NodeSGF());
            List<string> outsideTrees = GetOutsideGameTrees(source);

            foreach (string ot in outsideTrees)
            {
                TreeSGF parsedOT = ParseTree(ot);
                parsedOT.Root.Parent = tree.Root;
                tree.Root.Children.Add(parsedOT.Root);
            }

            return tree;
        }

        // Method for extracting a TreeSGF object out of the raw data of a single game tree.
        // Nested game trees are parsed and then merged back in at the node they appear in.
        private static TreeSGF ParseTree(string treeData)
        {
            TreeSGF baseTree = new TreeSGF(new NodeSGF());
            int i = 1;
            string keyword = "";
            while (i < treeData.Length)
            {
                switch (treeData[i])
                {
                    case ';':
                        keyword = "";
                        baseTree.AddAtCursor(new NodeSGF(), true);
                        break;
                    case '[':
                        if (keyword.Trim() == "" && baseTree.Cursor.SGFProperties.Count != 0)
                            keyword = baseTree.Cursor.SGFProperties.Last().Item1;
                        else if (keyword.Trim() == "")
                            throw new ArgumentException("Invalid tree source");
                        string markup = GetMarkup(treeData.Substring(i));
                        baseTree.Cursor.SGFProperties.Add(Tuple.Create(keyword.Trim(), markup));
                        keyword = "";
                        i += markup.Length - 1;
                        break;
                    case '(':
                        int nestedLength = 0;
                        TreeSGF nestedTree = ParseTree(GetNestedGameTree(
                            treeData.Substring(i), out nestedLength));
                        nestedTree.Root.Children.ForEach(
                            child => baseTree.AddAtCursor(child, false));
                        i += nestedLength - 1;
                        break;
                    default:
                        keyword += treeData[i];
                        break;
                }
                ++i;
            }
            return baseTree;
        }

        // Method for splitting raw SGF data into its different game trees
        private static List<string> GetOutsideGameTrees(string source)
        {
            List<string> outsideTrees = new List<string>();
            string outsideTree = "";
            int treeLength = 0;
            int i = 0;

            if (source.Length == 0)
                return outsideTrees;
            else if (source[0] != '(')
                return GetOutsideGameTrees(string.Join("", source.SkipWhile(x => x != '(')));

            while (i < source.Length)
            {
                outsideTree = GetNestedGameTree(source.Substring(i), out treeLength);
                if (treeLength != 0)
                {
                    outsideTrees.Add(outsideTree);
                    i += treeLength - 1;
                }
                ++i;
            }
            return outsideTrees;
        }

        // Method for finding a specific game tree starting at its first character
        private static string GetNestedGameTree(string source, out int length)
        {
            if (source.Length == 0)
            {
                length = 0;
                return "";
            }
            else if (source[0] != '(')
                return GetNestedGameTree(
                    string.Join("", source.SkipWhile(x => x != '(')), out length);

            int i = 1;
            int nestLength = 0;
            string gameTree = "(";
            bool treeFinished = false;

            while (!treeFinished && i < source.Length)
            {
                switch (source[i])
                {
                    case '(':
                        gameTree += GetNestedGameTree(source.Substring(i), out nestLength);
                        i += nestLength - 1;
                        break;
                    case ')':
                        gameTree += ")";
                        if (source[i - 1] != '\\')
                            treeFinished = true;
                        break;
                    case '[':
                        gameTree += GetMarkup(source.Substring(i));
                        i += GetMarkup(source.Substring(i)).Length - 1;
                        break;
                    default:
                        gameTree += source[i];
                        break;
                }
                ++i;
            }
            length = gameTree.Length;
            return gameTree;
        }

        // Method for retrieving contents from a [ ] markup.  All data in SGF is given
        // in the form "keyword[data]".  Escape sequences must be considered.
        private static string GetMarkup(string markup)
        {
            if (markup.Length == 0)
                return "";
            else if (markup[0] != '[')
                return GetMarkup(string.Join("", markup.SkipWhile(x => x != '[')));
            return string.Join("", markup.TakeWhile(
                (x, x_i) => x != ']' || (x == ']' && markup[x_i - 1] == '\\'))) + "]";
        }

        // Method for extracting a GoMove struct from parsed 'movement' data
        private static List<GoMove> GetMovesFromData(string keyword, string data)
        {
            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            GoColour moveColour = GetMoveStateFromKeyword(keyword);
            List<GoMove> moves = new List<GoMove>();

            switch (data.Length)
            {
                case 0:     // move is a pass
                    moves.Add(new GoMove(-1, -1, moveColour, MoveType.Pass));
                    break;
                case 2:     // move is of form xy
                    moves.Add(new GoMove(
                        alphabet.IndexOf(data[1]), alphabet.IndexOf(data[0]), moveColour, MoveType.Normal));
                    break;
                case 5:     // move is of form xy : rs
                    string[] args = data.Split(':');
                    args[0] = args[0].Trim();
                    args[1] = args[1].Trim();
                    for (int j = alphabet.IndexOf(args[0][1]); j <= alphabet.IndexOf(args[1][1]); ++j)
                        for (int i = alphabet.IndexOf(args[0][0]); i <= alphabet.IndexOf(args[1][0]); ++i)
                            moves.Add(new GoMove(j, i, moveColour, MoveType.Placement));
                    break;
            }

            return moves;
        }

        public static void ExtractDataFromNodes(TreeSGF tree)
        {
            tree.ActDFS(x =>
            {
                foreach (Tuple<string, string> property in x.SGFProperties)
                {
                    string value = property.Item2.Substring(1, property.Item2.Length - 2).Trim();
                    if (ParserSGF.p_Root.Contains(property.Item1))
                        tree.Root.RootProperties[property.Item1] = value;
                    else if (ParserSGF.p_Plce.Contains(property.Item1))
                        x.Placement.AddRange(ParserSGF.GetMovesFromData(property.Item1, value));
                    else if (ParserSGF.p_Annt.Contains(property.Item1))
                        x.Comment = value.Trim();
                    else if (ParserSGF.p_Move.Contains(property.Item1))
                        x.Move = ParserSGF.GetMovesFromData(property.Item1, value)[0];
                }
            });
        }

        // Method for converting keyword to moveState
        public static GoColour GetMoveStateFromKeyword(string keyword)
        {
            switch (keyword)
            {
                case "B":
                case "AB":
                    return GoColour.Black;
                case "W":
                case "AW":
                    return GoColour.White;
                default:
                    return GoColour.None;
            }
        }
    }
}
