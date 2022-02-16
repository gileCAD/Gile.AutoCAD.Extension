using Autodesk.AutoCAD.DatabaseServices;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the SymbolTable type.
    /// </summary>
    public static class SymbolTableExtension
    {
        /// <summary>
        /// Opens the table records in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of returned object.</typeparam>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <returns>The sequence of records.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active Transaction.</exception>
        public static IEnumerable<T> GetObjects<T>(this SymbolTable source, OpenMode mode = OpenMode.ForRead, bool openErased = false) 
            where T : SymbolTableRecord
        {
            Assert.IsNotNull(source, nameof(source));
            Transaction tr = source.Database.GetTopTransaction();

            foreach (ObjectId id in openErased ? source.IncludingErased : source)
            {
                yield return (T)tr.GetObject(id, mode, openErased, false);
            }
        }

        /// <summary>
        /// Purges the unreferenced symbol table records.
        /// </summary>
        /// <param name="symbolTable">Instance to which the method applies.</param>
        /// <returns>The number of pruged records.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="symbolTable"/> is null.</exception>
        public static int Purge(this SymbolTable symbolTable)
        {
            Assert.IsNotNull(symbolTable, nameof(symbolTable));
            int cnt = 0;
            Database db = symbolTable.Database;
            var unpurgeable = new HashSet<ObjectId>();
            while (true)
            {
                var ids = new ObjectIdCollection(
                    symbolTable.Cast<ObjectId>().Except(unpurgeable).ToArray());
                db.Purge(ids);
                if (ids.Count == 0)
                    break;
                foreach (ObjectId id in ids)
                {
                    try
                    {
                        id.GetObject<DBObject>(OpenMode.ForWrite).Erase();
                        cnt++;
                    }
                    catch
                    {
                        unpurgeable.Add(id);
                    }
                }
            }
            return cnt;
        }
    }
}
