using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the DBDictionary type.
    /// </summary>
    public static class DBDictionaryExtension
    {
        /// <summary>
        /// Tries to open the object of the dictionary corresponding to the given type, in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the returned object.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">Key of the entry in the dictionary.</param>
        /// <param name="obj">Output object.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetObject<T>(
            this DBDictionary source,
            string key,
            out T obj,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false) where T : DBObject
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            obj = default(T);
            if (source.Contains(key) && source.GetAt(key).TryGetObject(out obj, mode, openErased))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to get the named dictionary.
        /// </summary>
        /// <param name="parent">Instance to which the method applies.</param>
        /// <param name="key">Name of the dictionary.</param>
        /// <param name="dict">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetNamedDictionary(
            this DBDictionary parent,
            string key,
            out DBDictionary dict,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
        {
            Assert.IsNotNull(parent, nameof(parent));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            dict = null;
            if (parent.Contains(key) && parent.GetAt(key).TryGetObject(out dict, mode, openErased))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Opens the objects corresponding to the given type in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of returned objects.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of collected objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this DBDictionary source,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
            where T : DBObject
        {
            Assert.IsNotNull(source, nameof(source));
            var tr = source.Database.GetTopTransaction();
            var rxc = RXObject.GetClass(typeof(T));
            foreach (DBDictionaryEntry entry in openErased ? source.IncludingErased : source)
            {
                if (entry.Value.ObjectClass.IsDerivedFrom(rxc))
                {
                    yield return (T)tr.GetObject(entry.Value, mode, openErased, false);
                }
            }
        }

        /// <summary>
        /// Gets or creates the named dictionary.
        /// </summary>
        /// <param name="parent">Instance to which the method applies.</param>
        /// <param name="name">Name of the dictionary.</param>
        /// <returns>The found or newly created dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="name"/> is null or empty.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static DBDictionary GetOrCreateNamedDictionary(this DBDictionary parent, string name)
        {
            Assert.IsNotNull(parent, nameof(parent));
            Assert.IsNotNullOrWhiteSpace(name, nameof(name));
            var tr = parent.Database.GetTopTransaction();
            if (parent.Contains(name))
            {
                return parent.GetAt(name).GetObject<DBDictionary>();
            }
            parent.OpenForWrite();
            var dict = new DBDictionary();
            parent.SetAt(name, dict);
            tr.AddNewlyCreatedDBObject(dict, true);
            return dict;
        }

        /// <summary>
        /// Gets the xrecord data of the dictionary.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">la clé du xrecord.</param>
        /// <returns>The xrecord data or null if the xrecord does not exists.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static ResultBuffer GetXrecordData(this DBDictionary source, string key)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            if (!source.Contains(key))
            {
                return null;
            }
            var id = (ObjectId)source[key];
            if (!id.TryGetObject(out Xrecord xrec))
                return null;
            return xrec.Data;
        }

        /// <summary>
        /// Sets the xrecord data of the dictionary.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it did not already exist.</param>
        /// <param name="values">Xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXrecordData(this DBDictionary target, string key, params TypedValue[] values)
        {
            target.SetXrecordData(key, new ResultBuffer(values));
        }

        /// <summary>
        /// Sets the xrecord data of the dictionary.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it did not already exist.</param>
        /// <param name="data">Xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXrecordData(this DBDictionary target, string key, ResultBuffer data)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));
            Xrecord xrec;
            if (target.Contains(key))
            {
                xrec = ((ObjectId)target[key]).GetObject<Xrecord>(OpenMode.ForWrite);
            }
            else
            {
                target.OpenForWrite();
                xrec = new Xrecord();
                target.SetAt(key, xrec);
                target.Database.TransactionManager.TopTransaction.AddNewlyCreatedDBObject(xrec, true);
            }
            xrec.Data = data;
        }
    }
}
