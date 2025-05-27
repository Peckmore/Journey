namespace Journey.Collections
{
    internal sealed class Tree<T> : TreeNode<T>
    {
        #region Construction

        public Tree(T value)
            : base(value)
        { }

        #endregion
    }
}