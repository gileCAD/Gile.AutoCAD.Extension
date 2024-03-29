using System;
using System.Collections.Generic;

namespace Gile.AutoCAD.R25.Extension
{
    /// <summary>
    /// Defines methods to add or removes items from a sequence of disposable objects.
    /// </summary>
    /// <typeparam name="T">Type of the items.</typeparam>
    public interface IDisposableCollection<T> : ICollection<T>, IDisposable
        where T : IDisposable
    {
        /// <summary>
        /// Adds items to the sequence.
        /// </summary>
        /// <param name="items">Items to add.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Removes items from the sequence.
        /// </summary>
        /// <param name="items">Items to remove.</param>
        /// <returns>The sequence of removed items.</returns>
        IEnumerable<T> RemoveRange(IEnumerable<T> items);
    }
}
