using System.Collections.ObjectModel;

namespace NetEx.Collections
{
    public class TreeNode<T>
    {
        #region Fields

        private readonly List<TreeNode<T>> _children;

        #endregion

        #region Construction

        internal TreeNode(T value, TreeNode<T>? parent, int id)
        {
            Value = value;
            Parent = parent;
            Id = id;

            _children = new();
        }

        #endregion

        #region Properties

        public ReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();
        public int Count => _children.Count;
        public int Id { get; private set; }
        public TreeNode<T> Left
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
        public int[] Path => Parent?.Path.Concat([Id]).ToArray() ?? [Id];
        public TreeNode<T>? Parent { get; private set; }
        public TreeNode<T> Right
        {
            get
            {
                var node = this;
                while (node.Children.Count > 0)
                {
                    node = node.Children[^1];
                }
                return node;
            }
        }
        public TreeNode<T> this[int i] => _children[i];
        public T Value { get; set; }

        #endregion

        #region Methods

        public TreeNode<T> Add(T value)
        {
            var node = new TreeNode<T>(value, this, Count);
            _children.Add(node);
            return node;
        }
        public IEnumerable<TreeNode<T>> AddRange(params T[] values)
        {
            return values.Select(Add);
        }
        public void Clear()
        {
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                child.Clear();
                _children.RemoveAt(i);
            }
        }
        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Concat(_children.SelectMany(x => x.Flatten()));
        }
        public bool Remove(TreeNode<T> node)
        {
            if (_children.Remove(node))
            {
                node.Parent = null;

                for (var index = 0; index < Children.Count; index++)
                {
                    Children[index].Id = index;
                }

                return true;
            }

            return false;
        }

        #endregion





        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

    }
}