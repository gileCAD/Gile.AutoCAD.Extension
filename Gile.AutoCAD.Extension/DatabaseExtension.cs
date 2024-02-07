using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the Database type.
    /// </summary>
    public static class DatabaseExtension
    {
        /// <summary>
        /// Gets the database top transaction. Throws an exception if none.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The active top transaction.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static Transaction GetTopTransaction(this Database db)
        {
            Assert.IsNotNull(db, nameof(db));

            return db.TransactionManager.TopTransaction ?? throw new Exception(ErrorStatus.NoActiveTransactions);
        }

        /// <summary>
        /// Gets the named object dictionary.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The named object dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static DBDictionary GetNOD(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));

            return db.NamedObjectsDictionaryId.GetObject<DBDictionary>(mode);
        }

        /// <summary>
        /// Gets the model space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The model space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static BlockTableRecord GetModelSpace(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));

            return SymbolUtilityServices.GetBlockModelSpaceId(db).GetObject<BlockTableRecord>(mode);
        }

        /// <summary>
        /// Gets the current space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The current space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static BlockTableRecord GetCurrentSpace(this Database db, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));

            return db.CurrentSpaceId.GetObject<BlockTableRecord>(mode);
        }

        /// <summary>
        /// Gets the block table record of each layout.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The sequence of block table records.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static IEnumerable<BlockTableRecord> GetLayoutBlockTableRecords(this Database db, bool exceptModel = true, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));

            var tr = db.GetTopTransaction();
            return db.GetLayouts(exceptModel).Select(l => (BlockTableRecord)tr.GetObject(l.BlockTableRecordId, mode));
        }

        /// <summary>
        /// Gets the layouts.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of layouts.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static IEnumerable<Layout> GetLayouts(this Database db, bool exceptModel = true, OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            Assert.IsNotNull(db, nameof(db));

            var tr = db.GetTopTransaction();
            var layouts = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
            foreach (DBDictionaryEntry entry in layouts)
            {
                if ((entry.Key != "Model" || !exceptModel) && (!entry.Value.IsErased || openErased))
                {
                    yield return (Layout)tr.GetObject(entry.Value, mode, openErased);
                }
            }
        }

        /// <summary>
        /// Gets the layouts names.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The sequence of layout names.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static IEnumerable<string> GetLayoutNames(this Database db)
        {
            Assert.IsNotNull(db, nameof(db));

            return db.GetLayouts().OrderBy(l => l.TabOrder).Select(l => l.LayoutName);
        }

        /// <summary>
        /// Gets the value of the custom property.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="key">Custome property key.</param>
        /// <returns>The value of the custom property; or null, if it does not exist.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static string GetCustomProperty(this Database db, string key)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            return ((string)customProperties[key]).Trim();
        }

        /// <summary>
        /// Gets all the custom properties.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>A dictionary of custom properties.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static Dictionary<string, string> GetCustomProperties(this Database db)
        {
            Assert.IsNotNull(db, nameof(db));

            var result = new Dictionary<string, string>();
            var customPropertie = db.SummaryInfo.CustomProperties;
            while (customPropertie.MoveNext())
            {
                var entry = customPropertie.Entry;
                result.Add((string)entry.Key, ((string)entry.Value).Trim());
            }
            return result;
        }

        /// <summary>
        /// Sets the value of the custom property if it exists; otherwise, add the property.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="key">Property key.</param>
        /// <param name="value">Property value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="key"/> is null or empty.</exception>
        public static void SetCustomProperty(this Database db, string key, string value)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNullOrWhiteSpace(key, nameof(key));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            if (customProperties.Contains(key))
            {
                customProperties[key] = value;
            }
            else
            {
                customProperties.Add(key, value);
            }
            db.SummaryInfo = summaryInfoBuilder.ToDatabaseSummaryInfo();
        }

        /// <summary>
        /// Sets the values of the custom properties if they exist; otherwise, add them.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="values">KeyValue pairs for properties.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="values"/> is null.</exception>
        public static void SetCustomProperties(this Database db, params KeyValuePair<string, string>[] values)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(values, nameof(values));

            var summaryInfoBuilder = new DatabaseSummaryInfoBuilder(db.SummaryInfo);
            var customProperties = summaryInfoBuilder.CustomPropertyTable;
            foreach (KeyValuePair<string, string> pair in values)
            {
                string key = pair.Key;
                if (customProperties.Contains(key))
                {
                    customProperties[key] = pair.Value;
                }
                else
                {
                    customProperties.Add(key, pair.Value);
                }
            }
            db.SummaryInfo = summaryInfoBuilder.ToDatabaseSummaryInfo();
        }
    }
}
