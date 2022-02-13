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
        /// Tries to get the object extension dictionary
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="dict">Output dictionary.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static bool TryGetExtensionDictionary(this DBObject source, out DBDictionary dict, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(source, nameof(source));

            dict = null;
            var dictId = source.ExtensionDictionary;
            if (dictId == ObjectId.Null)
            {
                return false;
            }
            dict = dictId.GetObject<DBDictionary>(mode);
            return true;
        }

        /// <summary>
        /// Gets or creates the extension dictionary.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>The extension dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static DBDictionary GetOrCreateExtensionDictionary(this DBObject source)
        {
            Assert.IsNotNull(source, nameof(source));
            if (source.ExtensionDictionary == ObjectId.Null)
            {
                source.OpenForWrite();
                source.CreateExtensionDictionary();
            }
            return source.ExtensionDictionary.GetObject<DBDictionary>();
        }

        /// <summary>
        /// Gets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="key">Xrecord key.</param>
        /// <returns>The xrecord data or null if the xrecord does not exists.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>key</c> is null or empty.</exception>
        public static ResultBuffer GetXDictionaryXrecordData(this DBObject source, string key)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));
            if (!source.TryGetExtensionDictionary(out DBDictionary xdict))
            {
                return null;
            }
            return xdict.GetXrecordData(key);
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="values">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>key</c> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, string key, params TypedValue[] values)
        {
            target.SetXDictionaryXrecordData(key, new ResultBuffer(values));
        }

        /// <summary>
        /// Sets the xrecord data of the extension dictionary of the object.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="key">The xrecord key.</param>
        /// <param name="data">The new xrecord data.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <c>key</c> is null or empty.</exception>
        public static void SetXDictionaryXrecordData(this DBObject target, string key, ResultBuffer data)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));
            target.GetOrCreateExtensionDictionary().SetXrecordData(key, data);
        }

        /// <summary>
        /// Sets the object extended data (xdata) for the application.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="data">Extended data (the first TypedValue must be: (1001, &lt;regAppName&gt;)).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>data</c> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        /// <exception cref="Exception">eBadDxfSequence is thrown if the result buffer is not valid.</exception>
        public static void SetXDataForApplication(this DBObject target, ResultBuffer data)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(data, nameof(data));
            Database db = target.Database;
            Transaction tr = db.GetTopTransaction();
            var typedValue = data.AsArray()[0];
            if (typedValue.TypeCode != 1001)
                throw new Exception(ErrorStatus.BadDxfSequence);
            string appName = (string)typedValue.Value;
            RegAppTable regAppTable = db.RegAppTableId.GetObject<RegAppTable>();
            if (!regAppTable.Has(appName))
            {
                var regApp = new RegAppTableRecord();
                regApp.Name = appName;
                regAppTable.OpenForWrite();
                regAppTable.Add(regApp);
                tr.AddNewlyCreatedDBObject(regApp, true);
            }
            target.XData = data;
        }

        /// <summary>
        /// Opens the object for write.
        /// </summary>
        /// <param name="dBObj">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>obj</c> is null.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there's no active transaction.</exception>
        public static void OpenForWrite(this DBObject dBObj)
        {
            Assert.IsNotNull(dBObj, nameof(dBObj));

            if (!dBObj.IsWriteEnabled)
                dBObj.Database.GetTopTransaction().GetObject(dBObj.ObjectId, OpenMode.ForWrite);
        }
    }
}
