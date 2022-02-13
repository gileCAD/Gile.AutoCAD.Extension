using Autodesk.AutoCAD.DatabaseServices;

using System;

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
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws ArgumentNullException if the ObjectId is null.
        /// </summary>
        /// <param name="id">The ObjectId to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsNotObjectIdNull(ObjectId id, string paramName)
        {
            if (id.IsNull)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>
        /// Throws ArgumentException if the string is null or empty.
        /// </summary>
        /// <param name="str">The string to which the assertion applies.</param>
        /// <param name="paramName">Name of the parameter.</param>
        public static void IsNotNullOrWhiteSpace(string str, string paramName)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentException("eNullOrWhiteSpace", paramName);
        }
    }
}
