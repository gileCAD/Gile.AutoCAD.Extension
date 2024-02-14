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
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">Key of the entry in the dictionary.</param>
        /// <param name="obj">Output object.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetObject<T>(
            this DBDictionary source,
            Transaction tr,
            string key,
            out T obj,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false) where T : DBObject
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            obj = default;
            return source.Contains(key) && source.GetAt(key).TryGetObject(tr, out obj, mode, openErased);
        }

        ///<summary>
        /// Tries to get the named dictionary.
        /// </summary>
        /// <param name="parent">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">Name of the dictionary.</param>
        /// <param name="dictionary">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operations succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetNamedDictionary(
            this DBDictionary parent,
            Transaction tr,
            string key,
            out DBDictionary dictionary,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
        {
            Assert.IsNotNull(parent, nameof(parent));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            dictionary = default;
            if (parent.Contains(key))
            {
                var id = parent.GetAt(key);
                if (id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(DBDictionary))))
                {
                    dictionary = (DBDictionary)tr.GetObject(id, mode, openErased);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Opens the entities which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of returned objects.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of collected objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<T> GetObjects<T>(
            this DBDictionary source,
            Transaction tr,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
            where T : DBObject
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));

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
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="name">Name of the dictionary.</param>
        /// <returns>The found or newly created dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="parent"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="name"/> is null or empty.</exception>
        /// <exception cref="System.ArgumentException">Throw if the <paramref name="name"/> is not a DBDictionary.</exception>
        public static DBDictionary GetOrCreateNamedDictionary(
            this DBDictionary parent,
            Transaction tr,
            string name)
        {
            Assert.IsNotNull(parent, nameof(parent));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(name, nameof(name));

            if (parent.Contains(name))
            {
                var id = parent.GetAt(name);
                if (!id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(DBDictionary))))
                    throw new System.ArgumentException("Not a DBDictionary", nameof(name));
                return (DBDictionary)tr.GetObject(id, OpenMode.ForRead);
            }
            else
            {
                var dictionary = new DBDictionary();
                parent.OpenForWrite(tr);
                parent.SetAt(name, dictionary);
                tr.AddNewlyCreatedDBObject(dictionary, true);
                return dictionary;
            }
        }

        /// <summary>
        /// Tries to get the xrecord data.
        /// </summary>
        /// <param name="dictionary">Instance to which the method applies.</param>
        /// <param name="tr">Active transaction</param>
        /// <param name="key">Key of the xrecord.</param>
        /// <param name="data">Output data.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dictionary"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool TryGetXrecordData(
            this DBDictionary dictionary,
            Transaction tr,
            string key,
            out ResultBuffer data)
        {
            Assert.IsNotNull(dictionary, nameof(dictionary));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            data = default;
            if (dictionary.Contains(key))
            {
                var id = dictionary.GetAt(key);
                if (id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Xrecord))))
                {
                    var xrecord = (Xrecord)tr.GetObject(id, OpenMode.ForRead);
                    data = xrecord.Data;
                    return data != null;
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the xrecord data.
        /// </summary>
        /// <param name="dictionary">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it does not already exist.</param>
        /// <param name="data">Data</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dictionary"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="data"/> is null.</exception>
        public static void SetXrecordData(this DBDictionary dictionary, Transaction tr, string key, params TypedValue[] data)
        {
            Assert.IsNotNull(dictionary, nameof(dictionary));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            dictionary.SetXrecordData(tr, key, new ResultBuffer(data));
        }

        /// <summary>
        /// Sets the xrecord data.
        /// </summary>
        /// <param name="dictionary">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">Key of the xrecord, the xrecord is created if it does not already exist.</param>
        /// <param name="data">Data</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dictionary"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="data"/> is null.</exception>
        public static void SetXrecordData(this DBDictionary dictionary, Transaction tr, string key, ResultBuffer data)
        {
            Assert.IsNotNull(dictionary, nameof(dictionary));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));
            Assert.IsNotNull(data, nameof(data));
            Xrecord xrecord;
            if (dictionary.Contains(key))
            {
                var id = dictionary.GetAt(key);
                if (!id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(Xrecord))))
                    throw new System.ArgumentException("Not an Xrecord'", nameof(key));
                {
                    xrecord = (Xrecord)tr.GetObject(id, OpenMode.ForWrite);
                }
            }
            else
            {
                xrecord = new Xrecord();
                dictionary.OpenForWrite(tr);
                dictionary.SetAt(key, xrecord);
                tr.AddNewlyCreatedDBObject(xrecord, true);
            }
            xrecord.Data = data;
        }
    }
}
