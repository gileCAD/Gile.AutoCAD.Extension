using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the DBObject type.
    /// </summary>
    public static class DBObjectExtension
    {
        /// <summary>
        /// Tries to get the object extension dictionary.
        /// </summary>
        /// <param name="dbObject">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="dictionary">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dbObject"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static bool TryGetExtensionDictionary(
            this DBObject dbObject,
            Transaction tr,
            out DBDictionary dictionary,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false)
        {
            Assert.IsNotNull(dbObject, nameof(dbObject));
            Assert.IsNotNull(tr, nameof(tr));

            dictionary = default;
            var id = dbObject.ExtensionDictionary;
            if (id.IsNull)
                return false;
            dictionary = (DBDictionary)tr.GetObject(id, mode, openErased);
            return true;
        }

        /// <summary>
        /// Gets or creates the extension dictionary.
        /// </summary>
        /// <param name="dbObject">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The extension dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dbObject"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static DBDictionary GetOrCreateExtensionDictionary(
            this DBObject dbObject,
            Transaction tr,
            OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(dbObject, nameof(dbObject));
            Assert.IsNotNull(tr, nameof(tr));

            if (dbObject.ExtensionDictionary.IsNull)
            {
                dbObject.OpenForWrite(tr);
                dbObject.CreateExtensionDictionary();
            }
            return (DBDictionary)tr.GetObject(dbObject.ExtensionDictionary, mode);
        }

        /// <summary>
        /// Tries to get the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">Xrecord key.</param>
        /// <param name="data">Output data.</param>
        /// <returns>The xrecord data or null if the xrecord does not exists.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static bool GetXDictionaryXrecordData(this DBObject source, Transaction tr, string key, out ResultBuffer data)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            data = default;
            return
                source.TryGetExtensionDictionary(tr, out DBDictionary xdict) &&
                xdict.TryGetXrecordData(tr, key, out data);
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="values">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, Transaction tr, string key, params TypedValue[] values)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            target.SetXDictionaryXrecordData(tr, key, new ResultBuffer(values));
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="data">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, Transaction tr, string key, ResultBuffer data)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            target.GetOrCreateExtensionDictionary(tr).SetXrecordData(tr, key, data);
        }

        /// <summary>
        /// Sets the object extended data (xdata) for the application.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <param name="data">Extended data (the first TypedValue must be: (1001, &lt;regAppName&gt;)).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="data"/> is null.</exception>
        /// <exception cref="Exception">eBadDxfSequence is thrown if the result buffer is not valid.</exception>
        public static void SetXDataForApplication(this DBObject target, Transaction tr, ResultBuffer data)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(data, nameof(data));

            var db = target.Database;
            var typedValue = data.AsArray()[0];
            if (typedValue.TypeCode != 1001)
                throw new Exception(ErrorStatus.BadDxfSequence);
            string appName = (string)typedValue.Value;
            var regAppTable = (RegAppTable)tr.GetObject(db.RegAppTableId, OpenMode.ForRead);
            if (!regAppTable.Has(appName))
            {
                var regApp = new RegAppTableRecord
                {
                    Name = appName
                };
                tr.GetObject(db.RegAppTableId, OpenMode.ForWrite);
                regAppTable.Add(regApp);
                tr.AddNewlyCreatedDBObject(regApp, true);
            }
            target.XData = data;
        }

        /// <summary>
        /// Opens the object for write.
        /// </summary>
        /// <param name="dbObj">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction tu use.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="dbObj"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static void OpenForWrite(this DBObject dbObj, Transaction tr)
        {
            Assert.IsNotNull(dbObj, nameof(dbObj));
            Assert.IsNotNull(tr, nameof(tr));

            if (!dbObj.IsWriteEnabled)
            {
                tr.GetObject(dbObj.ObjectId, OpenMode.ForWrite);
            }
        }
    }
}
