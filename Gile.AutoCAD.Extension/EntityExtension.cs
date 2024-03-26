using Autodesk.AutoCAD.DatabaseServices;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the Entity type.
    /// </summary>
    public static class EntityExtension
    {
        /// <summary>
        /// Evaluates if the owner of <paramref name ="entity"/> is a Layout.
        /// </summary>
        /// <param name="entity">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>true, if the owner is a Layout; false, otherwise.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entity"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static bool IsOwnedByLayout(this Entity entity, Transaction tr)
        {
            Assert.IsNotNull(entity, nameof(entity));
            Assert.IsNotNull(entity, nameof(tr));

            var owner = tr.GetObject(entity.OwnerId, OpenMode.ForRead);
            return owner is BlockTableRecord btr && btr.IsLayout;
        }
    }
}
