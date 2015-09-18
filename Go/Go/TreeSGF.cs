// *******************************************
//  TreeSGF.cs - contains public class TreeSGF
//
//  Written by Jonathan Melcher on 15/09/2015
//  Last updated 16/09/2015
// *******************************************

using System;
using System.Collections.Generic;


namespace Go
{
    // **********************************************************************************
    //  class:      public class TreeSGF
    //  purpose:    K-Tree data-structure using NodeSGF as nodes, to represent a complete
    //              branching (Go) SGF file with SGF information at each node
    // **********************************************************************************
    public class TreeSGF
    {
        private const string rootID = "M";                  // signifies 'Main' branch
        private NodeSGF root;

        // ********************************************************
        //  constructor:        public TreeSGF(NodeSGF root)
        //  purpose:            initialize fields with given values
        //  parameters:         NodeSGF root
        // ********************************************************
        public TreeSGF(NodeSGF root)
        {
            Root = root;
        }

        public string RootID { get { return rootID; } }     // for labeling the nodes
        public NodeSGF Cursor { get; set; }                 // for navigating the tree
        public NodeSGF Root
        {
            get { return root; }

            // changing the value of the root will set the cursor there automatically
            // it is possible to lose entire trees this way but the cursor will remain
            // with the root

            set
            {
                root = value;
                Cursor = root;
            }
        }

        // *****************************************************************************
        //  method:         public void ActDFS(Action<NodeSGF> actn = null)
        //  purpose:        apply an action to each node in the order of a DFS traversal
        //  parameters:     Action<NodeSGF> of which to apply to each NodeSGF
        //  notes:          the DFS goes left first (first child)
        //  returns:        nothing
        // *****************************************************************************
        public void ActDFS(Action<NodeSGF> actn = null)
        {
            Stack<NodeSGF> stack = new Stack<NodeSGF>();
            stack.Push(Root);

            while (stack.Count != 0)
            {
                Cursor = stack.Pop();
                actn(Cursor);
                Cursor.Children.ForEach(c => stack.Push(c));
            }
        }

        // *******************************************************************************************************
        //  method:         public void AddAtCursor(NodeSGF node, bool setCursorAtChild)
        //  purpose:        add the given node to the tree as the next child of the current cursor, with an option
        //                  to set the cursor to its new child at the end
        //  parameters:     NodeSGF node
        //                  bool setCursorAtChild
        //  notes:          if there is no child to add, nothing will happen, however a null cursor will throw
        //  returns:        nothing
        // *******************************************************************************************************
        public void AddAtCursor(NodeSGF node, bool setCursorAtChild)
        {
            if (node == null)
                return;
            else if (Cursor == null)
                throw new InvalidOperationException("There is no Cursor!");
                
            node.Parent = Cursor;
            Cursor.Children.Add(node);

            if (setCursorAtChild)
                Cursor = node;
        }

        // ***********************************************************************************************
        //  method:         public void SetNodeIDstoRoot()
        //  purpose:        assign meaningful IDs to each node in the tree for use by the SGFViewer
        //  parameters:     none
        //  notes:          the labeling goes from last child to first which differs from the previous DFS
        //  returns:        nothing
        // ***********************************************************************************************
        public void SetNodeIDsToRoot()
        {
            if (root == null)
                return;

            Stack<NodeSGF> stack = new Stack<NodeSGF> { };

            Root.ID = RootID;
            stack.Push(root);

            while (stack.Count != 0)
            {
                // set the cursor's last child (main branch) to the cursor's ID.  this gives us the main branch
                // we append a value to the other children's IDs to show their particular branch number
                int branchCount = 1;
                Cursor = stack.Pop();

                if (Cursor.Children.Count == 0)
                    continue;

                Cursor.Children[Cursor.Children.Count - 1].ID = Cursor.ID;
                stack.Push(Cursor.Children[Cursor.Children.Count - 1]);

                for (int i = Cursor.Children.Count - 2; i >= 0; --i, ++branchCount)
                {
                    Cursor.Children[i].ID = Cursor.ID + branchCount.ToString();
                    stack.Push(Cursor.Children[i]);
                }
            }
        }
    }
}
