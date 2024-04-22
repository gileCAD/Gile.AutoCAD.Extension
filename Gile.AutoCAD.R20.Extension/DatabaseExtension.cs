using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Provides extension methods for the Database type.
    /// </summary>
    public static class DatabaseExtension
    {
        /// <summary>
        /// Gets the ObjectId of the last nondeleted entity in the drawing. 
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The ObjectId of the last entity, ObjectId.Null if none.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        public static ObjectId EntLast(this Database db)
        {
            Assert.IsNotNull(db, nameof(db));

            var seed = db.Handseed.Value;
            var entityClass = RXObject.GetClass(typeof(Entity));
            using (var tr = new OpenCloseTransaction())
            {
                while (1 < seed)
                {
                    if (db.TryGetObjectId(new Handle(seed), out ObjectId id) &&
                        id.ObjectClass.IsDerivedFrom(entityClass) &&
                        !id.IsErased &&
                        id.ObjectClass.Name != "AcDbBlockEnd" &&
                        id.ObjectClass.Name != "AcDbBlockBegin")
                    {
                        var entity = (Entity)tr.GetObject(id, OpenMode.ForRead);
                        if (entity.IsOwnedByLayout(tr))
                            return id;
                    }
                    seed--;
                }
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Gets the named object dictionary.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The named object dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static DBDictionary GetNOD(this Database db, Transaction tr, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

            return (DBDictionary)tr.GetObject(db.NamedObjectsDictionaryId, mode);
        }

        /// <summary>
        /// Gets the model space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The model space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static BlockTableRecord GetModelSpace(this Database db, Transaction tr, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

            return (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), mode);
        }

        /// <summary>
        /// Gets the current space block table record.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The current space.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static BlockTableRecord GetCurrentSpace(this Database db, Transaction tr, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

            return (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, mode);
        }

        /// <summary>
        /// Gets the block table record of each layout.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <returns>The sequence of block table records.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<BlockTableRecord> GetLayoutBlockTableRecords(this Database db, Transaction tr, bool exceptModel = true, OpenMode mode = OpenMode.ForRead)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

            return db.GetLayouts(tr, exceptModel).Select(l => (BlockTableRecord)tr.GetObject(l.BlockTableRecordId, mode));
        }

        /// <summary>
        /// Gets the layouts.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="exceptModel">Value indicating if the model space layout is left out.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of layouts.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<Layout> GetLayouts(this Database db, Transaction tr, bool exceptModel = true, OpenMode mode = OpenMode.ForRead, bool openErased = false)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

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
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>The sequence of layout names.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<string> GetLayoutNames(this Database db, Transaction tr)
        {
            Assert.IsNotNull(db, nameof(db));
            Assert.IsNotNull(tr, nameof(tr));

            return db.GetLayouts(tr).OrderBy(l => l.TabOrder).Select(l => l.LayoutName);
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
            if (customProperties[key] is null)
                return null;
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

        /// <summary>
        /// Imports the symbol table record from the specified file.
        /// </summary>
        /// <typeparam name="T">Type of SymbolTable.</typeparam>
        /// <param name="targetDb">Instance to which the method applies.</param>
        /// <param name="sourceFileName">Complete path of the source file.</param>
        /// <param name="cloning">Input action for duplicate records</param>
        /// <param name="recordName">Name of the record to import.</param>
        /// <returns>The ObjectId of the imported record; ObjectId.Null if cloning failed.</returns>
        /// <exception cref="System.ArgumentNullException">Lancée si l'instance à laquelle s'applique la méthode est null.</exception>
        public static ObjectId ImportSymbolTableRecord<T>(
            this Database targetDb, string sourceFileName, DuplicateRecordCloning cloning, string recordName)
            where T : SymbolTable
        {
            if (targetDb is null)
                throw new ArgumentNullException(nameof(targetDb));

            using (var sourceDb = new Database(false, true))
            {
                sourceDb.ReadDwgFile(sourceFileName, FileOpenMode.OpenForReadAndAllShare, false, null);
                using (Transaction tr = sourceDb.TransactionManager.StartTransaction())
                {
                    var sourceTable = (T)tr.GetObject(sourceDb.GetTableId<T>(), OpenMode.ForRead);
                    if (!sourceTable.Has(recordName))
                        return ObjectId.Null;
                    ObjectId sourceTableRecordId = sourceTable[recordName];
                    var ids = new ObjectIdCollection { sourceTableRecordId };
                    var mapping = new IdMapping();
                    sourceDb.WblockCloneObjects(ids, targetDb.GetTableId<T>(), mapping, cloning, false);
                    tr.Commit();
                    return mapping[sourceTableRecordId].IsCloned ?
                        mapping[sourceTableRecordId].Value :
                        ObjectId.Null;
                }
            }
        }

        /// <summary>
        /// Imports the symbol table records from the specified file.
        /// </summary>
        /// <typeparam name="T">Type of SymbolTable.</typeparam>
        /// <param name="targetDb">Instance to which the method applies.</param>
        /// <param name="sourceFileName">Complete path of the source file.</param>
        /// <param name="cloning">Input action for duplicate records</param>
        /// <param name="recordNames">Names of the records to import.</param>
        /// <returns>A dictionary containing the names and ObjectIds of the imported records.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="targetDb"/> is null.</exception>
        public static Dictionary<string, ObjectId> ImportSymbolTableRecords<T>(
            this Database targetDb, string sourceFileName, DuplicateRecordCloning cloning, params string[] recordNames)
            where T : SymbolTable
        {
            if (targetDb is null)
                throw new ArgumentNullException(nameof(targetDb));

            using (var sourceDb = new Database(false, true))
            {
                sourceDb.ReadDwgFile(sourceFileName, FileOpenMode.OpenForReadAndAllShare, false, null);
                using (Transaction tr = sourceDb.TransactionManager.StartTransaction())
                {
                    var sourceTable = (T)tr.GetObject(sourceDb.GetTableId<T>(), OpenMode.ForRead);
                    var ids = new ObjectIdCollection(recordNames
                        .Where(n => sourceTable.Has(n))
                        .Select(n => sourceTable[n])
                        .ToArray());
                    var mapping = new IdMapping();
                    sourceDb.WblockCloneObjects(ids, targetDb.GetTableId<T>(), mapping, cloning, false);
                    tr.Commit();
                    return recordNames
                        .Where(n => sourceTable.Has(n))
                        .Select(n => new { Name = n, IdPair = mapping[sourceTable[n]] })
                        .Where(x => x.IdPair.IsCloned)
                        .ToDictionary(x => x.Name, x => x.IdPair.Value);
                }
            }
        }

        /// <summary>
        /// Gets the ObjectId of the specified SymbolTable.
        /// </summary>
        /// <typeparam name="T">Type of SymbolTable.</typeparam>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The ObjectId of the specified SymbolTable.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="db"/> is null.</exception>
        /// <exception cref="NotImplementedException"></exception>
        public static ObjectId GetTableId<T>(this Database db) where T : SymbolTable
        {
            if (db is null) throw new ArgumentNullException(nameof(db));

            switch (typeof(T).Name)
            {
                case nameof(BlockTable): return db.BlockTableId;
                case nameof(DimStyleTable): return db.DimStyleTableId;
                case nameof(LayerTable): return db.LayerTableId;
                case nameof(LinetypeTable): return db.LinetypeTableId;
                case nameof(RegAppTable): return db.RegAppTableId;
                case nameof(TextStyleTable): return db.TextStyleTableId;
                case nameof(UcsTable): return db.UcsTableId;
                case nameof(ViewTable): return db.ViewTableId;
                case nameof(ViewportTable): return db.ViewportTableId;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
