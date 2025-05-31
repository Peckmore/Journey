namespace Journey.Tree
{
    /// <summary>
    /// Generates a new TreeNode instance that has no parent (i.e., it should be the root of the tree).
    /// </summary>
    internal sealed class Tree<T> : TreeNode<T>
    {
        #region Construction

        public Tree(T value)
            : base(value)
        { }

        #endregion
    }
}