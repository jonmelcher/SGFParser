// ******************************************************
// TreeSGF - Tree structure container for NodeSGF objects
//
// Written by Jonathan Melcher
// Feb 12, 2015
// ******************************************************

using System;
using System.Collections.Generic;

namespace Parser_SGF
{
    public class TreeSGF
    {
        public const string rootID = "M";
        private NodeSGF root;
        public NodeSGF cursor;

        public TreeSGF(NodeSGF root)
        {
            this.root = root;
            this.cursor = this.root;
        }

        public NodeSGF Root
        {
            get { return root; }
            set { root = value; cursor = root; }
        }

        public void MapDFS(Action<NodeSGF> map = null)
        {
            Stack<NodeSGF> stack = new Stack<NodeSGF>();
            stack.Push(root);

            while (stack.Count != 0)
            {
                cursor = stack.Pop();
                map(cursor);
                foreach (NodeSGF child in cursor.Children)
                    stack.Push(child);
            }
        }

        public void AddAtCursor(NodeSGF node, bool setCursorAtChild)
        {
            if (cursor == null)
                return;
            
            node.Parent = cursor;
            cursor.Children.Add(node);

            if (setCursorAtChild)
                cursor = node;
        }

        public void SetNodeIDsToRoot()
        {
            if (root == null)
                return;

            Stack<NodeSGF> stack = new Stack<NodeSGF> { };

            root.ID = rootID;
            stack.Push(root);

            while (stack.Count != 0)
            {
                // Set the cursor's last child (main branch) to the cursors IDValue.  This gives us the main branch.
                // We append a value to the other children's IDs to show their particular branch number.
                int branchCount = 1;
                cursor = stack.Pop();

                if (cursor.Children.Count == 0)
                    continue;

                cursor.Children[cursor.Children.Count - 1].ID = cursor.ID;
                stack.Push(cursor.Children[cursor.Children.Count - 1]);

                for (int i = cursor.Children.Count - 2; i >= 0; i--, branchCount++)
                {
                    cursor.Children[i].ID = cursor.ID + branchCount.ToString();
                    stack.Push(cursor.Children[i]);
                }
            }
        }
    }
}
