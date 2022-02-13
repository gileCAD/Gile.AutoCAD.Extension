using Autodesk.AutoCAD.DatabaseServices;

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
        /// <exception cref="System.ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static void DisposeAll(this DBObjectCollection source)
        {
            Assert.IsNotNull(source, nameof(source));
            if (source.Count > 0)
            {
                System.Exception last = null; 
                foreach (DBObject obj in source)
                {
                    if (obj != null)
                    {
                        try
                        {
                            obj.Dispose();
                        }
                        catch (System.Exception ex)
                        {
                            last = last ?? ex;
                        }
                    }
                }
                source.Clear();
                if (last != null)
                    throw last;
            }
            source.Dispose();
        }
    }
}
