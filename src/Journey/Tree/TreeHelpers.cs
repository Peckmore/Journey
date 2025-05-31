using System;
using System.Collections.Generic;
using System.Linq;

namespace Journey.Tree
{
    internal static class TreeHelper
    {
        // This class was adapted from the following article on Rachel Lim's blog:
        // https://rachel53461.wordpress.com/2014/04/20/algorithm-for-drawing-trees/
        // It is an implementation of the Reingold-Tilford algorithm.

        #region Constants

        private const int NodeXIncrement = 1;

        #endregion

        #region Methods

        #region Private Static

        private static void CalculateFinalPositions<T>(TreeNode<T> node, double modSum)
        {
            // Now that all offsets have been calculated, run through the tree and increment X by the Mod value of each node, and
            // recursively pass this Mod value down to child nodes as a base offset to add to their own Mod value.

            node.X += modSum;
            modSum += node.Mod;

            foreach (var child in node.Children)
            {
                CalculateFinalPositions(child, modSum);
            }
        }
        private static void CalculateInitialPositions<T>(TreeNode<T> node)
        {
            // Call this method recursively on all child nodes.
            foreach (var child in node.Children)
            {
                CalculateInitialPositions(child);
            }

            // Check whether the node has children or not, and assign the X value accordingly.

            if (node.IsLeaf)
            {
                // The node has no children.

                if (node.IsLeft)
                {
                    // This is the first child of it's parent, so set X to 0.
                    node.X = 0;
                }
                else
                {
                    // There is a previous sibling, so set X to the prevous sibling X value, plus node increment.
                    node.X = node.PreviousSibling!.X + NodeXIncrement;
                }
            }
            else if (node.Children.Count == 1)
            {
                // The node has a single child.

                if (node.IsLeft)
                {
                    // This is the first child of it's parent, so set X to it's first child's X value.
                    node.X = node.Children[0].X;
                }
                else
                {
                    // There is a previous sibling, so set X to the prevous sibling X value, plus node increment.
                    node.X = node.PreviousSibling!.X + NodeXIncrement;

                    // Set the nodes Mod value to the difference between it's X value and it's first child's X value.
                    node.Mod = node.X - node.Children[0].X;
                }
            }
            else
            {
                // The node has multiple children.

                // Calculate the midway point between the leftmost and rightmost children.
                var mid = (node.LeftChild!.X + node.RightChild!.X) / 2;

                if (node.IsLeft)
                {
                    // This is the first child of it's parent, so set X to the midway point of it's children.
                    node.X = mid;
                }
                else
                {
                    // There is a previous sibling, so set X to the prevous sibling X value, plus node increment.
                    node.X = node.PreviousSibling!.X + NodeXIncrement;

                    // Set the nodes Mod value to the difference between it's X value and the midway point of it's children.
                    node.Mod = node.X - mid;
                }
            }

            if (node.Children.Count > 0 && !node.IsLeft)
            {
                // Subtrees can overlap, so check for any overlaps.
                CheckForOverlaps(node);
            }

        }
        private static void CenterNodes<T>(TreeNode<T> leftNode, TreeNode<T> rightNode)
        {
            // Equally space all nodes between the two specified nodes.

            // Determine how nodes are between the two nodes.
            var numNodesBetween = rightNode.Index - leftNode.Index;

            // If there are no nodes between the two nodes, we don't need to do anything.
            if (numNodesBetween > 1)
            {
                // Calculate the distance between the two nodes, and then divide that by the number of nodes between them to get the
                // distance that each node should be spaced apart.
                var distanceBetweenNodes = (rightNode.X - leftNode.X) / numNodesBetween;

                // Loop through all nodes between the two nodes, and set their X values to be spaced by the calculated distance.
                var count = 1;
                for (var i = leftNode.Index + 1; i < rightNode.Index; i++)
                {
                    var middleNode = leftNode.Parent!.Children[i];

                    var desiredX = leftNode.X + distanceBetweenNodes * count;
                    var offset = desiredX - middleNode.X;
                    middleNode.X += offset;
                    middleNode.Mod += offset;

                    count++;
                }

                // Check for overlaps after centering the nodes.
                CheckForOverlaps(leftNode);
            }
        }
        private static void CheckForOverlaps<T>(TreeNode<T> node)
        {
            // A variable to track how much we need to shift the node to the right.
            var shiftValue = 0d;

            // Get the left contour of the current node.
            var leftContour = new Dictionary<int, double>();
            GetContour(node, 0, ref leftContour, false);

            // Get the leftmost sibling of the current node.
            var sibling = node.LeftSibling;

            // Loop through all left siblings until we reach a node with no left sibling.
            while (sibling != null && sibling != node)
            {
                // Get the right contour of the sibling node.
                var rightContour = new Dictionary<int, double>();
                GetContour(sibling, 0, ref rightContour, true);

                // Loop through the levels of the contours to check for conflicts.
                for (var level = node.Y + 1; level <= Math.Min(rightContour.Keys.Max(), leftContour.Keys.Max()); level++)
                {
                    // If the left contour of the current node is greater than the right contour of the sibling node,
                    // calculate the distance and update the shift value if necessary.
                    var distance = leftContour[level] - rightContour[level];
                    if (distance + shiftValue < NodeXIncrement)
                    {
                        shiftValue = NodeXIncrement - distance;
                    }
                }

                // If the shift value is greater than 0, update the current node's X and Mod values to increment them both by the shift
                // value.
                if (shiftValue > 0)
                {
                    node.X += shiftValue;
                    node.Mod += shiftValue;

                    // Center the nodes between the current node and the sibling node to ensure they are spaced correctly.
                    CenterNodes(sibling, node);

                    // Reset the shift value to 0 for the next iteration.
                    shiftValue = 0;
                }

                // Move to the next sibling.
                sibling = sibling.NextSibling;
            }
        }
        private static void GetContour<T>(TreeNode<T> node, double modSum, ref Dictionary<int, double> values, bool rightContour)
        {
            // Get the left or right contour of the current node.

            if (!values.TryGetValue(node.Y, out var value))
            {
                // If the Y value of the node is not already in the dictionary, add it with the node X value plus the mod sum.
                values.Add(node.Y, node.X + modSum);
            }
            else
            {
                // If the Y value is already in the dictionary, update it to be the minimum or maximum of the node X value, or the value
                // plus the mod sum, dependent upon whether we are getting the left or right contour.
                values[node.Y] = rightContour ? Math.Max(value, node.X + modSum) :
                                                Math.Min(value, node.X + modSum);
            }

            // Add the node's Mod value to the mod sum for the next recursive call.
            modSum += node.Mod;

            // Recursively call this method for each child node.
            foreach (var child in node.Children)
            {
                GetContour(child, modSum, ref values, rightContour);
            }
        }
        private static void OffsetToZero<T>(TreeNode<T> node)
        {
            // Once initial node positions have been calculated, some nodes could have a negative X value. We'll now go through the tree
            // and shift the specified node to the right so that the leftmost leaf node has an X value of 0.

            // Get the left contour of the root node, which will give us the minimum X value.
            var nodeContour = new Dictionary<int, double>();
            GetContour(node, 0, ref nodeContour, false);

            // Find the minimum X value in the contour.
            double shiftAmount = 0;
            foreach (var y in nodeContour.Keys)
            {
                // If the leftmost node has a negative X value, we need to increment shiftAmount by that amount.
                if (nodeContour[y] + shiftAmount < 0)
                {
                    shiftAmount = nodeContour[y] * -1;
                }
            }

            // If the shift amount is greater than 0, we need to shift the node to the right so that the leftmost node has an X value of 0.
            if (shiftAmount > 0)
            {
                node.X += shiftAmount;
                node.Mod += shiftAmount;
            }
        }

        #endregion

        #region Public Static

        public static void CalculateNodePositions<T>(TreeNode<T> rootNode)
        {
            // Calculate initial X and Mod values for each node in the tree.
            CalculateInitialPositions(rootNode);

            // Ensure that the leftmost node has an X value of 0.
            OffsetToZero(rootNode);

            // Calculate final X values for each node in the tree.
            CalculateFinalPositions(rootNode, 0);
        }

        #endregion

        #endregion
    }
}