using Autodesk.AutoCAD.DatabaseServices;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Enumeration of offset side options
    /// </summary>
    public enum OffsetSide
    {
        /// <summary>
        /// Inside.
        /// </summary>
        In,
        /// <summary>
        /// Outside.
        /// </summary>
        Out,
        /// <summary>
        /// Left side.
        /// </summary>
        Left,
        /// <summary>
        /// Right side.
        /// </summary>
        Right,
        /// <summary>
        /// Both sides.
        /// </summary>
        Both
    }

    // credits to Tony 'TheMaster' Tanzillo
    // http://www.theswamp.org/index.php?topic=31862.msg494503#msg494503

    /// <summary>
    /// Provides the Offset() extension method for the Polyline type
    /// </summary>
    public static class PolylineExtension
    {
        /// <summary>
        /// Offset the source polyline to specified side(s).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="offsetDist">Offset distance.</param>
        /// <param name="side">Offset side(s).</param>
        /// <returns>A polyline sequence resulting from the offset of the source polyline.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        public static IEnumerable<Polyline> Offset(this Polyline source, double offsetDist, OffsetSide side)
        {
            Assert.IsNotNull(source, nameof(source));

            offsetDist = Math.Abs(offsetDist);
            using (var plines = new DisposableSet<Polyline>())
            {
                IEnumerable<Polyline> offsetRight = source.GetOffsetCurves(offsetDist).Cast<Polyline>();
                plines.AddRange(offsetRight);
                IEnumerable<Polyline> offsetLeft = source.GetOffsetCurves(-offsetDist).Cast<Polyline>();
                plines.AddRange(offsetLeft);
                double areaRight = offsetRight.Select(pline => pline.Area).Sum();
                double areaLeft = offsetLeft.Select(pline => pline.Area).Sum();
                switch (side)
                {
                    case OffsetSide.In:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetRight : offsetLeft);
                    case OffsetSide.Out:
                        return plines.RemoveRange(
                           areaRight < areaLeft ? offsetLeft : offsetRight);
                    case OffsetSide.Left:
                        return plines.RemoveRange(offsetLeft);
                    case OffsetSide.Right:
                        return plines.RemoveRange(offsetRight);
                    case OffsetSide.Both:
                        plines.Clear();
                        return offsetRight.Concat(offsetLeft);
                    default:
                        return null;
                }
            }
        }
    }
}
