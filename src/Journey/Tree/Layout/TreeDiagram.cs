using Journey.Collections;

namespace Journey.Tree.Layout
{
    internal sealed class TreeDiagram<T> : TreeDiagramNode<T>
    {
        #region Construction

        internal TreeDiagram(TreeNode<T> node)
            : base(node)
        { }

        #endregion
    }
}