using Journey.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journey.Tree.Layout
{
    public static class TreeExtensionMethods
    {
        #region Methods

        public static Task<TreeDiagram<T>> LayoutTree<T>(this TreeNode<T> tree)
        {
            var root = new TreeDiagram<T>(tree);
            var nodes = new Dictionary<TreeNode<T>, TreeDiagramNode<T>> { { tree, root } };
            var queue = new Queue<TreeNode<T>>();
            queue.Enqueue(tree);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in current.Children)
                {
                    var childDiagramNode = new TreeDiagramNode<T>(child);
                    nodes.Add(child, childDiagramNode);
                    nodes[current].Add(childDiagramNode);
                    queue.Enqueue(child);
                }
            }

            // Reingold-Tilford Algorithm
            // https://towardsdatascience.com/reingold-tilford-algorithm-explained-with-walkthrough-be5810e8ed93/


            // ensure no node is being drawn off screen
            TreeHelpers.CalculateNodePositions(root);

            return Task.FromResult(root);
        }

        #endregion
    }

    public class TreeDiagram<T> : TreeDiagramNode<T>
    {
        #region Construction

        internal TreeDiagram(TreeNode<T> node)
            : base(node)
        { }

        #endregion
    }

    public class TreeDiagramNode<T> : TreeNode<T>
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