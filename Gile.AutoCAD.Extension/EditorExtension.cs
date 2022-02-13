using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the Editor type.
    /// </summary>
    public static class EditorExtension
    {
        #region Zoom

        /// <summary>
        /// Zooms to given extents in the current viewport.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="ext">Extents of the zoom.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void Zoom(this Editor ed, Extents3d ext)
        {
            Assert.IsNotNull(ed, nameof(ed));
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                ext.TransformBy(view.WorldToEye());
                view.Width = ext.MaxPoint.X - ext.MinPoint.X;
                view.Height = ext.MaxPoint.Y - ext.MinPoint.Y;
                view.CenterPoint = new Point2d(
                    (ext.MaxPoint.X + ext.MinPoint.X) / 2.0,
                    (ext.MaxPoint.Y + ext.MinPoint.Y) / 2.0);
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// Zooms to the extents of the current viewport.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void ZoomExtents(this Editor ed)
        {
            Database db = ed.Document.Database;
            db.UpdateExt(false);
            Extents3d ext = (short)Application.GetSystemVariable("cvport") == 1 ?
                new Extents3d(db.Pextmin, db.Pextmax) :
                new Extents3d(db.Extmin, db.Extmax);
            ed.Zoom(ext);
        }

        /// <summary>
        /// Zooms to the given window.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="p1">First window corner.</param>
        /// <param name="p2">Opposite window corner.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void ZoomWindow(this Editor ed, Point3d p1, Point3d p2)
        {
            using (var line = new Line(p1, p2))
            {
                ed.Zoom(line.GeometricExtents);
            }
        }

        /// <summary>
        /// Zooms to the specified entity collection.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="ids">Collection of the entities ObjectId on which to zoom.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void ZoomObjects(this Editor ed, IEnumerable<ObjectId> ids)
        {
            Assert.IsNotNull(ed, nameof(ed));
            Assert.IsNotNull(ids, nameof(ids));
            using (Transaction tr = ed.Document.TransactionManager.StartTransaction())
            {
                //Extents3d ext = ids
                //    .GetObjects<Entity>()
                //    .Select(ent => ent.GeometricExtents)
                //    .Aggregate((e1, e2) => { e1.AddExtents(e2); return e1; });
                Extents3d ext = ids
                    .GetObjects<Entity>()
                    .Select(ent => ent.Bounds)
                    .Where(b => b.HasValue)
                    .Select(b => b.Value)
                    .Aggregate((e1, e2) => { e1.AddExtents(e2); return e1; });
                ed.Zoom(ext);
                tr.Commit();
            }
        }

        /// <summary>
        /// Zooms in current viewport to the specified scale.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="scale">Scale.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void ZoomScale(this Editor ed, double scale)
        {
            Assert.IsNotNull(ed, nameof(ed));
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                view.Width /= scale;
                view.Height /= scale;
                ed.SetCurrentView(view);
            }
        }

        /// <summary>
        /// Zooms in current viewport to the specified scale and center. 
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="center">Viewport center.</param>
        /// <param name="scale">Scale (default = 1).</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <c>ed</c> is null.</exception>
        public static void ZoomCenter(this Editor ed, Point3d center, double scale = 1.0)
        {
            Assert.IsNotNull(ed, nameof(ed));
            using (ViewTableRecord view = ed.GetCurrentView())
            {
                center = center.TransformBy(view.WorldToEye());
                view.Height /= scale;
                view.Width /= scale;
                view.CenterPoint = new Point2d(center.X, center.Y);
                ed.SetCurrentView(view);
            }
        }

        #endregion
    }
}
