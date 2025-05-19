namespace NetEx.Collections
{
    public class Tree<T> : TreeNode<T>
    {
        #region Construction

        public Tree(T value)
            : base(value, null, 0)
        { }

        #endregion
    }
}