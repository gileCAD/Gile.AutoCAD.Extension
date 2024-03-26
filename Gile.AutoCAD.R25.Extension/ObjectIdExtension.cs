using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

using System.Runtime.InteropServices;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the ObjectId type.
    /// </summary>
    public static partial class ObjectIdExtension
    {
        /// <summary>
        /// Tries to open an AutoCAD object of the given type in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the output object.</typeparam>
        /// <param name="id">ObjectId to open.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="obj">Output object.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayer">Value indicating if locked layers should be opened.</param>
        /// <returns><c>true</c>, if the operation succeeded; <c>false</c>, otherwise.</returns>
        /// <exception cref="Exception">eNullObjectId is thrown if <paramref name ="id"/> equals <c>ObjectId.Null</c>.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static bool TryGetObject<T>(
            this ObjectId id,
            Transaction tr,
            out T? obj,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayer = false)
            where T : DBObject
        {
            ErrorStatus.NullObjectId.ThrowIf(id.IsNull);
            System.ArgumentNullException.ThrowIfNull(tr);

            obj = default;
            if (!id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
            {
                return false;
            }
            obj = (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
            return true;
        }

        /// <summary>
        /// Opens an AutoCAD object of the given type in the given open mode.
        /// </summary>
        /// <typeparam name="T">Type of the object to return.</typeparam>
        /// <param name="id">ObjectId to open.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayer">Value indicating if locked layers should be opened.</param>
        /// <returns>The object opened in given open mode.</returns>
        /// <exception cref="System.InvalidCastException">Thrown if the object type does not match the given type</exception>
        /// <exception cref="Exception">eNullObjectId is thrown if <paramref name ="id"/> equals <c>ObjectId.Null</c>.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static T GetObject<T>(
            this ObjectId id,
            Transaction tr,
            OpenMode mode = OpenMode.ForRead,
            bool openErased = false,
            bool forceOpenOnLockedLayer = false)
            where T : DBObject
        {
            ErrorStatus.NullObjectId.ThrowIf(id.IsNull);
            System.ArgumentNullException.ThrowIfNull(tr);

            return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayer);
        }

        /// <summary>
        /// Get the DXF definition data (like AutoLISP entget function).
        /// </summary>
        /// <param name="id">Instance to which the method applies.</param>
        /// <returns>The DXF data.</returns>
        /// <exception cref="Exception">Thrown in case of invalid objectId.</exception>
        public static ResultBuffer? EntGet(this ObjectId id)
        {
            var errorStatus = acdbGetAdsName(out AdsName ename, id);
            if (errorStatus != ErrorStatus.OK)
            {
                throw new Exception(errorStatus);
            }
            var result = acdbEntGet(ename);
            if (result != nint.Zero)
            {
                return ResultBuffer.Create(result, true);
            }
            return null;
        }

        // Replace the DLL name according to the AutoCAD targeted version
        // 2013-2014:   "acdb19.dll"
        // 2015-2016:   "acdb20.dll"
        // 2017:        "acdb21.dll"
        // 2018:        "acdb22.dll"
        // 2019-2020:   "acdb23.dll"
        // 2021-2024:   "acdb24.dll"
        // Replace the EntryPoint according to AutoCAD plateform
        // 32 bits:     "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AAY01JVAcDbObjectId@@@Z"
        // 64 bits:     "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z"
        [System.Security.SuppressUnmanagedCodeSecurity]
        [LibraryImport("acdb24.dll", EntryPoint = "?acdbGetAdsName@@YA?AW4ErrorStatus@Acad@@AEAY01_JVAcDbObjectId@@@Z")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial ErrorStatus acdbGetAdsName(out AdsName ename, ObjectId id);

        [System.Security.SuppressUnmanagedCodeSecurity]
        [LibraryImport("accore.dll", EntryPoint = "acdbEntGet")]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static partial nint acdbEntGet(AdsName ename);
    }
}
