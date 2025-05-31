using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Journey.Tree
{
    /// <summary>
    /// A basic "tree" implementation to support the Journey manager.
    /// </summary>
    internal class TreeNode<T>
    {
        #region Fields

        private readonly List<TreeNode<T>> _children;

        #endregion

        #region Construction

        internal TreeNode(T value)
        {
            Value = value;

            _children = new();
            Count = 0;
            Depth = 0;
            Index = 0;
            IsLeaf = true;
            IsLeft = true;
            IsRight = true;
            LeftChild = null;
            LeftSibling = this;
            Mod = 0;
            NextSibling = null;
            Parent = null;
            PreviousSibling = null;
            RightChild = null;
            RightSibling = this;
            Path = [0];
            X = -1;
            Y = Depth;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The children of this node, as a read-only collection.
        /// </summary>
        public ReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();
        /// <summary>
        /// The number of children this node has.
        /// </summary>
        public int Count { get; private set; }
        /// <summary>
        /// The depth of this node in the tree, where the root node is at depth 0.
        /// </summary>
        public int Depth { get; private set; }
        /// <summary>
        /// The index of this node in its parent's children collection, where the first node is at index 0.
        /// </summary>
        public int Index { get; private set; }
        /// <summary>
        /// Indicates whether this node is a leaf node (i.e., it has no children).
        /// </summary>
        public bool IsLeaf { get; private set; }
        /// <summary>
        /// Indicates whether this node is the leftmost child of its parent.
        /// </summary>
        public bool IsLeft { get; private set; }
        /// <summary>
        /// Indicates whether this node is the rightmost child of its parent.
        /// </summary>
        public bool IsRight { get; private set; }
        /// <summary>
        /// The leftmost child of this node, or null if it has no children.
        /// </summary>
        public TreeNode<T>? LeftChild { get; private set; }
        /// <summary>
        /// The leftmost leaf node in the subtree rooted at this node. This is the leftmost node that has no children.
        /// </summary>
        public TreeNode<T> LeftLeaf
        {
            get
            {
                var node = this;
                while (node.Children.Count > 0)
                {
                    node = node.Children[0];
                }
                return node;
            }
        }
        /// <summary>
        /// The leftmost sibling of this node, or itself if it has no left sibling.
        /// </summary>
        public TreeNode<T> LeftSibling { get; private set; }
        /// <summary>
        /// The "mod" value for this node, which is used in the Reingold-Tilford algorithm to adjust the position of nodes in the tree layout.
        /// </summary>
        public double Mod { get; internal set; }
        /// <summary>
        /// The next sibling of this node in its parent's children collection, or null if it is the last child.
        /// </summary>
        public TreeNode<T>? NextSibling { get; private set; }
        /// <summary>
        /// The path from the root node to this node, represented as an array of indices. The first element is always 0, and subsequent
        /// elements represent the index of each node in its parent's children collection.
        /// </summary>
        public int[] Path { get; private set; }
        /// <summary>
        /// The parent of this node, or null if it is the root node.
        /// </summary>
        public TreeNode<T>? Parent { get; private set; }
        /// <summary>
        /// The previous sibling of this node in its parent's children collection, or null if it is the first child.
        /// </summary>
        public TreeNode<T>? PreviousSibling { get; private set; }
        /// <summary>
        /// The rightmost child of this node, or null if it has no children.
        /// </summary>
        public TreeNode<T>? RightChild { get; private set; }
        /// <summary>
        /// The rightmost leaf node in the subtree rooted at this node. This is the rightmost node that has no children.
        /// </summary>
        public TreeNode<T> RightLeaf
        {
            get
            {
                var node = this;
                while (node._children.Count > 0)
                {
                    node = node._children[^1];
                }

                return node;
            }
        }
        /// <summary>
        /// The rightmost sibling of this node, or itself if it has no right sibling.
        /// </summary>
        public TreeNode<T> RightSibling { get; private set; }
        /// <summary>
        /// The value stored in this node.
        /// </summary>
        /// <param name="i">The index of the child node to access.</param>
        /// <returns>The child node at the specified index.</returns>
        public TreeNode<T> this[int i] => _children[i];
        /// <summary>
        /// The value stored in this node.
        /// </summary>
        public T Value { get; set; }
        /// <summary>
        /// The X coordinate of this node in the tree layout. This is used in the Reingold-Tilford algorithm to position nodes horizontally.
        /// </summary>
        public double X { get; internal set; }
        /// <summary>
        /// The Y coordinate of this node in the tree layout. This is used in the Reingold-Tilford algorithm to position nodes vertically.
        /// </summary>
        public int Y { get; private set; }

        #endregion

        #region Methods

        #region Private

        private TreeNode<T> InternalAdd(T value, bool recalculate)
        {
            // Add a new child node with the specified value to this node's children.

            var node = new TreeNode<T>(value);
            _children.Add(node);
            node.Parent = this;
            if (recalculate)
            {
                Recalculate();
            }
            return node;
        }
        private void Recalculate()
        {
            // Recalculate the properties of this node based on its current state and those of its children. We do this whenever the node
            // or it's children are modified to improve performance when traversing the tree. Alternatively, we could calculate these
            // properties when accessed (e.g., walk the tree for each property), but that would be slower. The tradeoff is it takes longer
            // to add and remove nodes at the point of insertion/removal, and higher memory usage to store the additional properties.

            Count = _children.Count;
            Depth = Parent?.Depth + 1 ?? 0;
            Index = Parent?._children.IndexOf(this) ?? 0;
            IsLeaf = _children.Count == 0;
            IsLeft = Index == 0;
            IsRight = Index == Parent?._children.Count - 1;
            LeftChild = _children.Count > 0 ? _children[0] : null;
            LeftSibling = Parent?._children[0] ?? this;
            NextSibling = Index < Parent?._children.Count - 1 ? Parent?._children[Index + 1] : null;
            Path = Parent?.Path.Concat([Index]).ToArray() ?? [Index];
            PreviousSibling = Index > 0 ? Parent?._children[Index - 1] : null;
            RightChild = _children.Count > 0 ? _children[^1] : null;
            RightSibling = Parent?._children[^1] ?? this;
            Y = Depth;

            foreach (var child in _children)
            {
                child.Recalculate();
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        /// <param name="node">The child node to add.</param>
        public void Add(TreeNode<T> node)
        {
            _children.Add(node);
            node.Parent = this;
            Recalculate();
        }
        /// <summary>
        /// Adds a new child node with the specified value to this node and returns the new node.
        /// </summary>
        /// <param name="value">The value to store in the new child node.</param>
        /// <returns>The newly created child node.</returns>
        public TreeNode<T> Add(T value)
        {
            var node = InternalAdd(value, true);
            return node;
        }
        /// <summary>
        /// Adds multiple child nodes with the specified values to this node and returns the new nodes.
        /// </summary>
        /// <param name="values">An array of values to add as child nodes.</param>
        /// <returns>An enumerable collection of the newly created child nodes.</returns>
        public IEnumerable<TreeNode<T>> AddRange(params T[] values)
        {
            var nodes = values.Select(node => InternalAdd(node, false));
            Recalculate();
            return nodes;
        }
        /// <summary>
        /// Clears all child nodes from this node.
        /// </summary>
        public void Clear()
        {
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                _children.RemoveAt(i);
            }
        }
        public void PerformLayout()
        {
            // Calculate positions for the nodes in this tree using the Reingold-Tilford algorithm.
            TreeHelper.CalculateNodePositions(this);
        }
        /// <summary>
        /// Removes a child node from this node.
        /// </summary>
        /// <param name="node">The child node to remove.</param>
        /// <returns><see langword="true" /> if the node was successfully removed; otherwise <see langword="false" />.</returns>
        public bool Remove(TreeNode<T> node)
        {
            if (_children.Remove(node))
            {
                node.Parent = null;
                Recalculate();
                return true;
            }

            return false;
        }
        /// <summary>
        /// Returns the sibling node at the specified index, or null if node is a root node.
        /// </summary>
        /// <param name="i">The index of the sibling node to return.</param>
        /// <returns>The sibling node at the specified index, or null if the node is a root node.</returns>
        public TreeNode<T>? Sibling(int i)
        {
            if (Parent != null)
            {
                return Parent[i];
            }

            return null;
        }
        /// <summary>
        /// Traverses the tree in Postorder and returns an enumerable collection of nodes in the order they were visited.
        /// </summary>
        /// <returns>An enumerable collection of nodes in Postorder traversal order.</returns>
        public IEnumerable<TreeNode<T>> Traverse()
        {
            // Do a Postorder traversal of the tree, which is what we need for the Reingold-Tilford algorithm.

            // We'll use a stack to keep track of the nodes we need to visit, and another stack to keep track of the order in which we need
            // to return them.

            // This stack is to keep track of nodes as we go down the tree, from root to leaf. As we encounter a node, we'll push it onto
            // this stack, then iterate through it's children.
            var traversalStack = new Stack<TreeNode<T>>();

            // We add nodes to this stack in the final "Postorder" order, so we'll just iterate through this stack, popping items off the
            // top and returning them in order.
            var returnStack = new Stack<TreeNode<T>>();

            // Push the root node onto the traversal stack to start the process.
            traversalStack.Push(this);

            // While there are nodes to traverse, we will continue to pop nodes off the traversal stack and push their children onto it.
            while (traversalStack.Count > 0)
            {
                // Pop the top node off the traversal stack, which is the next node we need to visit.
                var node = traversalStack.Pop();

                // Add the node to our return stack.
                returnStack.Push(node);

                // Push children (from left to right) onto the traversal stack. When we come to pop them off, the children will be popped
                // off "right to left", and eventually pushed onto the return stack in that order, which ultimately means they'll be popped
                // off in "left to right" order when we iterate through the return stack.
                foreach (var childNode in node._children)
                {
                    traversalStack.Push(childNode);
                }
            }

            // We've traversed the whole tree, and all nodes are on our return stack in the correct order, so just return them.
            return [.. returnStack];
        }

        #endregion

        #endregion
    }
}