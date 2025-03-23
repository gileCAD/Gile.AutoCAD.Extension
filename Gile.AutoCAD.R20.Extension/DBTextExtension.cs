﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Runtime.InteropServices;

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Provides extension methods for the DBText type.
    /// </summary>
    public static class DBTextExtension
    {
        /// <summary>
        /// Gets the 'text box' of the DBText instance.
        /// </summary>
        /// <param name="dbText">Instance to which the method applies.</param>
        /// <param name="point1">Lower left corner of the box ('DBText coordinate system').</param>
        /// <param name="point2">Upper right corner of the box ('DBText coordinate system')</param>
        /// <param name="transform">Transformation matrix from 'DBText coordinate system' to WCS.</param>
        public static void GetTextBox(this DBText dbText, out Point3d point1, out Point3d point2, out Matrix3d transform)
        {
            Assert.IsNotNull(dbText, nameof(dbText));

            int mirrored = dbText.IsMirroredInX ? 2 : 0;
            mirrored |= dbText.IsMirroredInY ? 4 : 0;
            var rb = new ResultBuffer(
                    new TypedValue(1, dbText.TextString),
                    new TypedValue(40, dbText.Height),
                    new TypedValue(41, dbText.WidthFactor),
                    new TypedValue(51, dbText.Oblique),
                    new TypedValue(7, dbText.TextStyleName),
                    new TypedValue(71, mirrored),
                    new TypedValue(72, (int)dbText.HorizontalMode),
                    new TypedValue(73, (int)dbText.VerticalMode));
            var pt1 = new double[3];
            var pt2 = new double[3];
            acedTextBox(rb.UnmanagedObject, pt1, pt2);
            point1 = new Point3d(pt1);
            point2 = new Point3d(pt2);
            transform =
                Matrix3d.Displacement(dbText.Position.GetAsVector()) *
                Matrix3d.Rotation(dbText.Rotation, dbText.Normal, Point3d.Origin) *
                Matrix3d.PlaneToWorld(new Plane(Point3d.Origin, dbText.Normal));
        }

        /// <summary>
        /// Gets the center of the text bounding box.
        /// </summary>
        /// <param name="dbText">Instance to which the method applies.</param>
        /// <returns>The center point of the text.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="dbText"/> is null.</exception>
        public static Point3d GetTextBoxCenter(this DBText dbText)
        {
            dbText.GetTextBox(out Point3d point1, out Point3d point2, out Matrix3d xform);
            return new Point3d(
                (point1.X + point2.X) / 2.0,
                (point1.Y + point2.Y) / 2.0,
                (point1.Z + point2.Z) / 2.0)
                .TransformBy(xform);
        }

        /// <summary>
        /// Gets the points at corners of the text bounding box.
        /// </summary>
        /// <param name="dbText">Instance to which the method applies.</param>
        /// <returns>The points(counter-clockwise from lower left).</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="dbText"/> is null.</exception>
        public static Point3d[] GetTextBoxCorners(this DBText dbText)
        {
            dbText.GetTextBox(out Point3d point1, out Point3d point2, out Matrix3d xform);
            return new[]
            {
                point1.TransformBy(xform),
                new Point3d(point2.X, point1.Y, 0.0).TransformBy(xform),
                point2.TransformBy(xform),
                new Point3d(point1.X, point2.Y, 0.0).TransformBy(xform)
            };
        }

        /// <summary>
        /// Mirrors the text honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="axis"/> is null.</exception>
        public static void Mirror(this DBText source, Transaction tr, Line3d axis, bool eraseSource)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(axis, nameof(axis));

            var db = source.Database;
            DBText mirrored;
            if (eraseSource)
            {
                mirrored = source;
                mirrored.OpenForWrite(tr);
            }
            else
            {
                var ids = new ObjectIdCollection(new[] { source.ObjectId });
                var mapping = new IdMapping();
                db.DeepCloneObjects(ids, db.CurrentSpaceId, mapping, false);
                mirrored = (DBText)tr.GetObject(mapping[source.ObjectId].Value, OpenMode.ForWrite);
            }
            mirrored.TransformBy(Matrix3d.Mirroring(axis));
            if (!db.Mirrtext)
            {
                var pts = mirrored.GetTextBoxCorners();
                var cen = new LineSegment3d(pts[0], pts[2]).MidPoint;
                var rotAxis = Math.Abs(axis.Direction.X) < Math.Abs(axis.Direction.Y) ?
                    pts[0].GetVectorTo(pts[3]) :
                    pts[0].GetVectorTo(pts[1]);
                mirrored.TransformBy(Matrix3d.Rotation(Math.PI, rotAxis, cen));
            }
        }

        /// <summary>
        /// P/Invokes the unmanaged acedTextBox method.
        /// </summary>
        /// <param name="rb">ResultBuffer cotaining the DBtext DXF definition.</param>
        /// <param name="point1">Minimum point of the bounding box.</param>
        /// <param name="point2">Maximum point of the bounding box.</param>
        /// <returns>RTNORM, if succeeds; RTERROR, otherwise.</returns>
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("accore.dll", CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.Cdecl, EntryPoint = "acedTextBox")]
        static extern IntPtr acedTextBox(IntPtr rb, double[] point1, double[] point2);
    }
}
