using Journey.Tree.Overby.Collections;

public class TreeNodeLayout<T>
{
    public TreeNode<T> Node { get; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Mod { get; set; } // Modifier for subtree alignment
    public TreeNodeLayout<T>? Parent { get; set; }
    public List<TreeNodeLayout<T>> Children { get; } = new();

    public TreeNodeLayout(TreeNode<T> node)
    {
        Node = node;
    }
}
