using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the ObjectId type.
    /// </summary>
    public static class ObjectIdExtension
    {
        /// <summary>
        /// Tries to open an AutoCAD object of the given type in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the output object.</typeparam>
        /// <param name="id">ObjectId to open.</param>
        /// <param name="obj">Output object.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayer">Value indicating if locked layers should be opened.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="Exception">eNullObjectId is thrown if <paramref name ="id"/> equals <c>ObjectId.Null</c>.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static bool TryGetObject<T>(
            this ObjectId id,
            out T obj,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayer = false)
            where T : DBObject
        {
            Assert.IsNotObjectIdNull(id, nameof(id));
            var tr = id.Database.GetTopTransaction();
            obj = default(T);

            if (!id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
                return false;

            obj = (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
            return true;
        }

        /// <summary>
        /// Opens an AutoCAD object of the given type in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the object to return.</typeparam>
        /// <param name="id">ObjectId to open.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayer">Value indicating if locked layers should be opened.</param>
        /// <returns>The object opened in given open mode.</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the object type does not match the given type</exception>
        /// <exception cref="Exception">eNullObjectId is thrown if <paramref name ="id"/> equals <c>ObjectId.Null</c>.</exception>
        /// <exception cref="Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static T GetObject<T>(
            this ObjectId id,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayer = false)
            where T : DBObject
        {
            Assert.IsNotObjectIdNull(id, nameof(id));
            var tr = id.Database.GetTopTransaction();

            return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
        }
    }
}
