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
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
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
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomExtents(this Editor ed)
        {
            Assert.IsNotNull(ed, nameof(ed));

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
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomWindow(this Editor ed, Point3d p1, Point3d p2)
        {
            Assert.IsNotNull(ed, nameof(ed));

            var extents = new Extents3d();
            extents.AddPoint(p1);
            extents.AddPoint(p2);
            ed.Zoom(extents);
        }

        /// <summary>
        /// Zooms to the specified entity collection.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="ids">Collection of the entities ObjectId on which to zoom.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
        public static void ZoomObjects(this Editor ed, IEnumerable<ObjectId> ids)
        {
            Assert.IsNotNull(ed, nameof(ed));
            Assert.IsNotNull(ids, nameof(ids));

            using (Transaction tr = ed.Document.TransactionManager.StartOpenCloseTransaction())
            {
                Extents3d ext = ids
                    .GetObjects<Entity>(tr)
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
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
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
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="ed"/> is null.</exception>
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

        #region Selection

        /// <summary>
        /// Gets a selection set using the supplied prompt selection options, the supplied filter and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="options">Selection options.</param>
        /// <param name="filter">Selection filter</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options, SelectionFilter filter, System.Predicate<ObjectId> predicate) =>
            GetPredicatedSelection(ed, predicate, options, filter);

        /// <summary>
        /// Gets a selection set using the supplied prompt selection options and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="options">Selection options.</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, PromptSelectionOptions options, System.Predicate<ObjectId> predicate) =>
            GetPredicatedSelection(ed, predicate, options);

        /// <summary>
        /// Gets a selection set using the supplied filter and the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="filter">Selection filter</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, SelectionFilter filter, System.Predicate<ObjectId> predicate) =>
            GetPredicatedSelection(ed, predicate, null, filter);

        /// <summary>
        /// Gets a selection set using the supplied predicate.
        /// </summary>
        /// <param name="ed">Instance to which the method applies.</param>
        /// <param name="predicate">Selection predicate.</param>
        /// <returns>The selection result.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="ed"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="predicate"/> is null.</exception>
        public static PromptSelectionResult GetSelection(this Editor ed, System.Predicate<ObjectId> predicate) =>
            GetPredicatedSelection(ed, predicate, null, null);

        private static PromptSelectionResult GetPredicatedSelection(
            Editor ed,
            System.Predicate<ObjectId> predicate,
            PromptSelectionOptions options = null,
            SelectionFilter filter = null)
        {
            Assert.IsNotNull(ed, nameof(ed));
            Assert.IsNotNull(predicate, nameof(predicate));

            void onSelectionAdded(object sender, SelectionAddedEventArgs e)
            {
                var ids = e.AddedObjects.GetObjectIds();
                for (int i = 0; i < ids.Length; i++)
                {
                    if (!predicate(ids[i]))
                    {
                        e.Remove(i);
                    }
                }
            }

            PromptSelectionResult result;
            ed.SelectionAdded += onSelectionAdded;
            if (options == null)
            {
                if (filter == null)
                {
                    result = ed.GetSelection();
                }
                else
                {
                    result = ed.GetSelection(filter);
                }
            }
            else
            {
                if (filter == null)
                {
                    result = ed.GetSelection(options);
                }
                else
                {
                    result = ed.GetSelection(options, filter);
                }
            }
            ed.SelectionAdded -= onSelectionAdded;
            return result;
        }

        #endregion
    }
}
