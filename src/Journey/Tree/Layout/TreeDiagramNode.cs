using Journey.Collections;

namespace Journey.Tree.Layout
{
    internal class TreeDiagramNode<T> : TreeNode<T>
    {
        #region Construction

        internal TreeDiagramNode(TreeNode<T> node)
            : base(node.Value)
        {
            Mod = 0;
            X = node.Index;
            Y = node.Depth;
        }

        #endregion

        #region Properties

        #region Public

        public double Mod { get; internal set; }
        public double X { get; internal set; }
        public int Y { get; }

        #endregion

        #endregion
    }
}