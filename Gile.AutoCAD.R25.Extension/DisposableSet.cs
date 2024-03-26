using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Describes a set of disposable values.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public class DisposableSet<T> : HashSet<T>, IDisposableCollection<T>
        where T : IDisposable
    {
        /// <summary>
        /// Creates a new empty instance using the default comparer.
        /// </summary>
        public DisposableSet()
        { }

        /// <summary>
        /// Creates a new instance using the default comparer by copying the sequence items.
        /// </summary>
        /// <param name="collection">Sequence whose elements are copied into the new set.</param>
        public DisposableSet(IEnumerable<T> collection)
            : base(collection)
        { }

        /// <summary>
        /// Creates a new empty instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">IEqualityComparer&lt;T&gt; implementation.</param>
        public DisposableSet(IEqualityComparer<T> comparer)
            : base(comparer)
        { }

        /// <summary>
        /// Creates a new instance using the specified comparer by copying the sequence items.
        /// </summary>
        /// <param name="collection">Sequence whose elements are copied into the new set.</param>
        /// <param name="comparer">IEqualityComparer&lt;T&gt; implementation.</param>
        public DisposableSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
            : base(collection, comparer)
        { }

        /// <summary>
        /// Disposes of all items.
        /// </summary>
        public void Dispose()
        {
            var list = this.ToList();
            Clear();
            list.DisposeAll();
        }

        /// <summary>
        /// Adds items to the active instance.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            UnionWith(items);
        }

        /// <summary>
        /// Removes items from the active instance.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        /// <returns>The sequence of effectively removed items.</returns>
        public IEnumerable<T> RemoveRange(IEnumerable<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            ExceptWith(items);
            return items;
        }
    }
}
