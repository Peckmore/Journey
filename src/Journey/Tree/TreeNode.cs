using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Journey.Collections
{
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
            PreviousSibling = null;
            LeftChild = null;
            LeftSibling = this;
            Parent = null;
            NextSibling = null;
            RightChild = null;
            RightSibling = this;
            Path = [0];
        }

        #endregion

        #region Properties

        public ReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();
        public int Count { get; private set; }
        public int Depth { get; private set; }
        public int Index { get; private set; }
        public bool IsLeaf { get; private set; }
        public bool IsLeft { get; private set; }
        public bool IsRight { get; private set; }
        public TreeNode<T>? LeftChild { get; private set; }
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
        public TreeNode<T> LeftSibling { get; private set; }
        public TreeNode<T>? NextSibling { get; private set; }
        public int[] Path { get; private set; }
        public TreeNode<T>? Parent { get; private set; }
        public TreeNode<T>? PreviousSibling { get; private set; }
        public TreeNode<T>? RightChild { get; private set; }
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
        public TreeNode<T> RightSibling { get; private set; }
        public TreeNode<T> this[int i] => _children[i];
        public T Value { get; set; }

        #endregion

        #region Methods

        #region Private

        private TreeNode<T> InternalAdd(T value, bool recalculate)
        {
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

            foreach (var child in _children)
            {
                child.Recalculate();
            }
        }

        #endregion

        #region Public

        public void Add(TreeNode<T> node)
        {
            _children.Add(node);
            node.Parent = this;
            Recalculate();
        }
        public TreeNode<T> Add(T value)
        {
            var node = InternalAdd(value, true);
            return node;
        }
        public IEnumerable<TreeNode<T>> AddRange(params T[] values)
        {
            var nodes = values.Select(node => InternalAdd(node, false));
            Recalculate();
            return nodes;
        }
        public void Clear()
        {
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                _children.RemoveAt(i);
            }
        }
        public bool Remove(TreeNode<T> node)
        {
            var nodeIndex = _children.IndexOf(node);
            if (_children.Remove(node))
            {
                node.Parent = null;
                Recalculate();
                return true;
            }

            return false;
        }
        public TreeNode<T>? Sibling(int i)
        {
            if (Parent != null)
            {
                return Parent[i];
            }

            return null;
        }
        public IEnumerable<TreeNode<T>> Traverse()
        {
            // Do a Postorder traversal of the tree, which is what we need for the Reingold-Tilford algorithm.

            // We're going to use `yield` to implement this, so we'll try and do this without recursion otherwise
            // we'll need to wrap the recursive calls in `yield` calls as well.

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

            // We've traversed the whole tree, and all nodes are on our return stack in the correct order, so just start iterating through
            // returning them.
            while (returnStack.Count > 0)
            {
                yield return returnStack.Pop();
            }
        }

        #endregion

        #endregion
    }
}