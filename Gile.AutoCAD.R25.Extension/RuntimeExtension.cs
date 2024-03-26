using Autodesk.AutoCAD.Runtime;

using System.Runtime.CompilerServices;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension related to AutoCAD Runtime.
    /// </summary>
    public static class RuntimeExtension
    {
        /// <summary>
        /// Throws Autodesk.AutoCAD.Exception(errorStatus) it condition is true.
        /// Credit Tony Tanzillo http://www.theswamp.org/index.php?topic=59013.msg619828#msg619828
        /// </summary>
        /// <param name="errorStatus">Error status.</param>
        /// <param name="condition">Condition.</param>
        /// <param name="message">Message.</param>
        /// <exception cref="Exception"></exception>
        public static void ThrowIf(
            this ErrorStatus errorStatus,
            bool condition,
            [CallerArgumentExpression("condition")] string? message = null)
        {
            if (condition)
                throw new Exception(errorStatus, message);
        }
    }
}
