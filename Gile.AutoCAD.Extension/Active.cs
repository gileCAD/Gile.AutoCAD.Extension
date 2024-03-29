using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

using AcAp = Autodesk.AutoCAD.ApplicationServices;

// Inspired by Scott McFarlane
// https://www.autodesk.com/autodesk-university/class/Being-Remarkable-C-NET-AutoCAD-Developer-2015#handout

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Provides easy access to several "active" objects in AutoCAD runtime environment.
    /// </summary>
    public static class Active
    {
        /// <summary>
        /// Gets the active Document object.
        /// </summary>
        public static AcAp.Document Document => Application.DocumentManager.MdiActiveDocument;

        /// <summary>
        /// Gets the active Database object.
        /// </summary>
        public static Database Database => Document.Database;

        /// <summary>
        /// Gets the active Editor object.
        /// </summary>
        public static Editor Editor => Document.Editor;
    }
}
