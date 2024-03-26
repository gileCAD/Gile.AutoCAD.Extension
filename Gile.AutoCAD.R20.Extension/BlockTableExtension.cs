using Autodesk.AutoCAD.DatabaseServices;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the BlockTable type.
    /// </summary>
    public static class BlockTableExtension
    {
        /// <summary>
        /// Gets the objectId of a block definition (BlockTableRecord) from its name.
        /// If the block is not found in the block table, a dwg file is searched in the support paths and added to the block table.
        /// </summary>
        /// <param name="blockTable">Block table.</param>
        /// <param name="blockName">Block name.</param>
        /// <returns>The ObjectId of the block table record or ObjectId.Null if not found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="blockTable"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="blockName"/> is null or empty.</exception>
        public static ObjectId GetBlock(this BlockTable blockTable, string blockName)
        {
            Assert.IsNotNull(blockTable, nameof(blockTable));
            Assert.IsNotNullOrWhiteSpace(blockName, nameof(blockName));

            if (blockTable.Has(blockName))
                return blockTable[blockName];
            try
            {
                string blockPath = HostApplicationServices.Current.FindFile(blockName + ".dwg", blockTable.Database, FindFileHint.Default);
                using (var tmpDb = new Database(false, true))
                {
                    tmpDb.ReadDwgFile(blockPath, FileOpenMode.OpenForReadAndAllShare, true, null);
                    return blockTable.Database.Insert(blockName, tmpDb, true);
                }
            }
            catch
            {
                return ObjectId.Null;
            }
        }
    }
}
