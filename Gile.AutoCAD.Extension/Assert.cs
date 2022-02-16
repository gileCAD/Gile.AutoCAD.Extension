using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;


namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides methods to throw an exception if an assertion is wrong.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Throws ArgumentNullException if the object is null.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="obj">The instance to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsNotNull<T>(T obj, string paramName) where T : class
        {
            if (obj == null)
                throw new System.ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws eNullObjectId if the <c>ObjectId</c> is null.
        /// </summary>
        /// <param name="id">The ObjectId to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsNotObjectIdNull(ObjectId id, string paramName)
        {
            if (id.IsNull)
                throw new Exception(ErrorStatus.NullObjectId, paramName);
        }

        /// <summary>
        /// Throws ArgumentException if the string is null or empty.
        /// </summary>
        /// <param name="str">The string to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsNotNullOrWhiteSpace(string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new System.ArgumentException("eNullOrWhiteSpace", paramName);
        }

        /// <summary>
        /// Throws eWrongObjectType if the<c>ObjectId</c> is not derived from <c>T</c>/>.
        /// </summary>
        /// <typeparam name="T">Type which <paramref name="id"/> is supposed to dreive from.</typeparam>
        /// <param name="id"><c>ObjectId</c> which the type have to be evaluated.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsDerivedFrom<T>(ObjectId id, string paramName) where T : DBObject
        {
            if (!id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(T))))
                throw new Exception(ErrorStatus.WrongObjectType, paramName);
        }
    }
}
