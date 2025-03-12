using Autodesk.AutoCAD.ApplicationServices;

using System;
using System.Threading.Tasks;

using AcRx = Autodesk.AutoCAD.Runtime;

namespace Gile.AutoCAD.R25.Extension
{
    /// <summary>
    /// Provides extension methods for the DocumentCollection type.
    /// </summary>
    public static class DocumentCollectionExtension
    {
        /// <summary>
        /// Invokes an action in the document context that can be safely called from the application context.
        /// Credit: Tony Tanzillo.
        /// </summary>
        /// <param name="docs">Documents collection.</param>
        /// <param name="action">Action to invoke.</param>
        /// <returns>Task.CompletedTask.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="docs"/> is null;</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null;</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoDocument thrown if no active document.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eInvalidContext thrown if not called from application context.</exception>
        /// <remark>Must be called in a try / catch statement.</remark>
        public static async Task InvokeAsCommandAsync(this DocumentCollection docs, Action<Document> action)
        {
            ArgumentNullException.ThrowIfNull(docs);

            ArgumentNullException.ThrowIfNull(action);

            Document doc = docs.MdiActiveDocument ?? throw new AcRx.Exception(AcRx.ErrorStatus.NoDocument);

            if (!docs.IsApplicationContext)
                throw new AcRx.Exception(AcRx.ErrorStatus.InvalidContext);

            Task task = Task.CompletedTask;
            await docs.ExecuteInCommandContextAsync(
                (_) =>
                {
                    try
                    {
                        action(doc);
                        return task;
                    }
                    catch (Exception ex)
                    {
                        return task = Task.FromException(ex);
                    }
                },
                null
            );
            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
    }
}
