using Journey.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Journey.Tree.Layout
{
    internal static class TreeExtensionMethods
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
}