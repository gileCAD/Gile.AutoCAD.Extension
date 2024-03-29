using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the IEnumerable(T) type.
    /// </summary>
    public static class IEnumerableExtension
    {
        /// <summary>
        /// Opens the objects which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of object to return.</typeparam>
        /// <param name="source">Sequence of ObjectIds.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<T> GetObjects<T>(
          this IEnumerable<ObjectId> source,
          Transaction tr,
          OpenMode mode = OpenMode.ForRead,
          bool openErased = false,
          bool forceOpenOnLockedLayers = false) where T : DBObject
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(tr);

            if (source.Any())
            {
                var rxClass = RXObject.GetClass(typeof(T));
                foreach (ObjectId id in source)
                {
                    if (id.ObjectClass.IsDerivedFrom(rxClass) &&
                        (!id.IsErased || openErased))
                    {
                        yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                    }
                }
            }
        }

        /// <summary>
        /// Upgrades the open mode of all objects in the sequence.
        /// </summary>
        /// <typeparam name="T">Type of objects.</typeparam>
        /// <param name="source">Sequence of DBObjects to upgrade.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>The sequence of opened for write objects (objets on locked layers are discared).</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<T> UpgradeOpen<T>(this IEnumerable<T> source, Transaction tr) where T : DBObject
        {
            ArgumentNullException.ThrowIfNull(source);

            foreach (T item in source)
            {
                try
                {
                    item.OpenForWrite(tr);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    if (ex.ErrorStatus != ErrorStatus.OnLockedLayer)
                    {
                        throw;
                    }
                    continue;
                }
                yield return item;
            }
        }

        /// <summary>
        /// Disposes of all items of the sequence.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence of disposable objects.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static void DisposeAll<T>(this IEnumerable<T> source) where T : IDisposable
        {
            ArgumentNullException.ThrowIfNull(source);

            if (source.Any())
            {
                System.Exception? last = null;
                foreach (T item in source)
                {
                    if (item != null)
                    {
                        try
                        {
                            item.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            last ??= ex;
                        }
                    }
                }
                if (last != null)
                {
                    throw last;
                }
            }
        }

        /// <summary>
        /// Runs the action for each item of the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence to process.</param>
        /// <param name="action">Action to run.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="action"/> is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);

            foreach (T item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Runs the indexed action for each item of the collection.
        /// </summary>
        /// <typeparam name="T">Type of the items.</typeparam>
        /// <param name="source">Sequence to process.</param>
        /// <param name="action">Indexed action to run.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="action"/> is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(action);

            int i = 0;
            foreach (T item in source)
            {
                action(item, i++);
            }
        }

        /// <summary>
        /// Gets the greatest item of the sequence using <paramref name ="comparer"/> with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <param name="comparer">Comparer used for the <c>TKey</c> type; uses <c>Comparer&lt;TKey&gt;.Default</c> if null or omitted.</param>
        /// <returns>The greatest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        public static TSource MaxBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector,
            IComparer<TKey>? comparer = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            comparer ??= Comparer<TKey>.Default;
            using var iterator = source.GetEnumerator();
            if (!iterator.MoveNext())
            {
                throw new InvalidOperationException("Empty sequence");
            }
            var max = iterator.Current;
            var maxKey = selector(max);
            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentKey = selector(current);
                if (comparer.Compare(currentKey, maxKey) > 0)
                {
                    max = current;
                    maxKey = currentKey;
                }
            }
            return max;
        }

        /// <summary>
        /// Gets the smallest item of the sequence using the <paramref name ="comparer"/> with the <paramref name ="selector"/> function returned values.
        /// </summary>
        /// <typeparam name="TSource">Type the items.</typeparam>
        /// <typeparam name="TKey">Type of the returned value of <paramref name ="selector"/> function.</typeparam>
        /// <param name="source">Sequence to which the method applies.</param>
        /// <param name="selector">Mapping function from <c>TSource</c> to <c>TKey</c>.</param>
        /// <param name="comparer">Comparer used for the <c>TKey</c> type; uses <c>Comparer&lt;TKey&gt;.Default</c> if null or omitted.</param>
        /// <returns>The smallest item in the sequence.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="selector"/> is null.</exception>
        public static TSource MinBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> selector,
            IComparer<TKey>? comparer = null)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(selector);

            comparer ??= Comparer<TKey>.Default;
            using var iterator = source.GetEnumerator();
            if (!iterator.MoveNext())
            {
                throw new InvalidOperationException("Empty sequence");
            }
            var min = iterator.Current;
            var minKey = selector(min);
            while (iterator.MoveNext())
            {
                var current = iterator.Current;
                var currentKey = selector(current);
                if (comparer.Compare(currentKey, minKey) < 0)
                {
                    min = current;
                    minKey = currentKey;
                }
            }
            return min;
        }
    }
}
