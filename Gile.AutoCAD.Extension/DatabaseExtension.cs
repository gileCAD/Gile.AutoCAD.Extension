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
            var tr = db.TransactionManager.TopTransaction;
            if (tr == null)
                throw new Exception(ErrorStatus.NoActiveTransactions);
            return tr;
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
            return db.GetLayouts(exceptModel).Select(l => l.BlockTableRecordId.GetObject<BlockTableRecord>(mode));
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
            Transaction tr = db.GetTopTransaction();
            foreach (DBDictionaryEntry entry in db.LayoutDictionaryId.GetObject<DBDictionary>())
            {
                if ((entry.Key != "Model" || !exceptModel) && (!entry.Value.IsErased || openErased))
                    yield return entry.Value.GetObject<Layout>(mode, openErased);
            }
        }

        /// <summary>
        /// Gets the layouts names.
        /// </summary>
        /// <param name="db">Instance to which the method applies.</param>
        /// <returns>The sequence of layout names.</returns>
        public static IEnumerable<string> GetLayoutNames(this Database db) =>
            db.GetLayouts().OrderBy(l => l.TabOrder).Select(l => l.LayoutName);
    }
}
