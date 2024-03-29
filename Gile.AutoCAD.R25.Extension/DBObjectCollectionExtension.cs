using Autodesk.AutoCAD.DatabaseServices;

using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the DBObjectCollection type.
    /// </summary>
    public static class DBObjectCollectionExtension
    {
        /// <summary>
        /// Disposes of all objects in the collections.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static void DisposeAll(this DBObjectCollection source)
        {
            System.ArgumentNullException.ThrowIfNull(source);

            var list = source.Cast<DBObject>().ToList();
            source.Clear();
            list.DisposeAll();
        }
    }
}
