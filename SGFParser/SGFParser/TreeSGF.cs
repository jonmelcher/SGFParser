// *********************************************************
// TreeSGF - used as the tree structure of a parsed SGF file
//
// Written by Jonathan Melcher
// Last updated Feb 04, 2015
// *********************************************************

#region using directives

using System;
using System.Collections.Generic;

#endregion

namespace SGFParser
{
    // ***********************************************************************************
    // TreeSGF class -  used to structure the nodes of information parsed from an SGF file
    //                  contains a root and a cursor and their properties, as well as a
    //                  counter for identifying each node.  Methods are available for set-
    //                  ting the cursor, root, adding nodes at the cursor, reinitializing
    //                  the IDs (to ensure uniqueness) and for merging two trees.  Depth-
    //                  first search is employed.
    // ***********************************************************************************
    public class TreeSGF
    {
        #region non-static fields

        private NodeSGF root;
        private NodeSGF cursor;
        private string idValue = "";

        #endregion
        #region constructor

        public TreeSGF(InfoSGF rootData)
        {
            root = new NodeSGF(rootData);
            cursor = root;
        }

        #endregion
        #region properties

        public NodeSGF Root
        {
            get { return root; }
        }

        public string IDValue
        {
            get { return idValue; }
            set { idValue = value; }
        }

        public NodeSGF Cursor
        {
            get { return cursor; }
            set { cursor = value; }
        }

        #endregion
        #region cursor methods

        // Adding a child - cannot set root from this method
        public void AddAtCursor(InfoSGF nodeData)
        {
            NodeSGF nodeToAdd = new NodeSGF(nodeData, cursor);
            cursor.Children.Add(nodeToAdd);
        }

        // Merging two trees
        public void MergeAtCursor(TreeSGF other)
        {
            other.Root.Parent = Cursor;
            Cursor.Children.Add(other.Root);
        }

        #endregion
        #region id methods

        // Resetting IDs using DFS - sets root.ID as the argument and increments from there
        public void ResetTheIDS(string rootID, bool test = false)
        {
            Stack<NodeSGF> stack = new Stack<NodeSGF> { };
            IDValue = rootID;
            root.ID = IDValue;

            stack.Push(root);
            if (test)
                Console.WriteLine(root.ID);

            while (stack.Count != 0)
            {
                // Set the cursor's last child (main branch) to the cursors IDValue.  This gives us the main branch.
                // We append a value to the other children's IDs to show their particular branch number.
                int branchCount = 1;
                Cursor = stack.Pop();

                if (cursor.Children.Count == 0)
                    continue;

                Cursor.Children[Cursor.Children.Count - 1].ID = Cursor.ID;
                stack.Push(Cursor.Children[Cursor.Children.Count - 1]);
                if (test)
                {
                    Console.WriteLine(Cursor.ID);
                    Console.WriteLine(Cursor.Children[Cursor.Children.Count - 1].ID);
                }
                    
                for (int i = Cursor.Children.Count - 2; i >= 0; i--, branchCount++)
                {
                    Cursor.Children[i].ID = Cursor.ID + branchCount.ToString();
                    if (test)
                        Console.WriteLine(Cursor.Children[i].ID);
                        
                    stack.Push(Cursor.Children[i]);
                }
            }
        }

        #endregion
    }
}
