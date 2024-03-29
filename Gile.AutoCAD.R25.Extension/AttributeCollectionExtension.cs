using Autodesk.AutoCAD.DatabaseServices;

using System.Collections.Generic;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the AttributeCollection type.
    /// </summary>
    public static class AttributeCollectionExtension
    {
        /// <summary>
        /// Opens the attribute references in the given open mode.
        /// </summary>
        /// <param name="source">Attribute collection.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of attribute references.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<AttributeReference> GetObjects(
            this AttributeCollection source,
            Transaction tr,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayers = false)
        {
            System.ArgumentNullException.ThrowIfNull(source);
            System.ArgumentNullException.ThrowIfNull(tr);

            if (source.Count > 0)
            {
                foreach (ObjectId id in source)
                {
                    if (!id.IsErased || openErased)
                    {
                        yield return (AttributeReference)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                    }
                }
            }
        }
    }
}
