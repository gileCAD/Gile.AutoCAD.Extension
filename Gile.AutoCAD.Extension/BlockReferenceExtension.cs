using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.Extension
{
    /// <summary>
    /// Provides extension methods for the BlockReference type.
    /// </summary>
    public static class BlockReferenceExtension
    {
        /// <summary>
        /// Gets the effective name of the block reference (name of the DynamicBlockTableRecord for anonymous dynamic blocks).
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>The effective name of the block reference.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static string GetEffectiveName(this BlockReference source)
        {
            Assert.IsNotNull(source, nameof(source));

            return source.DynamicBlockTableRecord.GetObject<BlockTableRecord>().Name;
        }

        /// <summary>
        /// Gets all the attributes values by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>Sequence of pairs Tag/Attribute.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static IEnumerable<KeyValuePair<string, AttributeReference>> GetAttributesByTag(this BlockReference source)
        {
            Assert.IsNotNull(source, nameof(source));
            return source
                .AttributeCollection.GetObjects()
                .Select(att => new KeyValuePair<string, AttributeReference>(att.Tag, att));
        }

        /// <summary>
        /// Gets all the attributes values by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <returns>Collection of pairs Tag/Value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        public static Dictionary<string, string> GetAttributesValues(this BlockReference source)
        {
            Assert.IsNotNull(source, nameof(source));
            return source.GetAttributesByTag().ToDictionary(p => p.Key, p => p.Value.TextString);
        }

        /// <summary>
        /// Sets the value to the attribute.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tag">Attribute tag.</param>
        /// <param name="value">New value.</param>
        /// <returns>The value if attribute was found, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>target</c> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <c>tag</c> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>value</c> is null.</exception>
        public static string SetAttributeValue(this BlockReference target, string tag, string value)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNullOrWhiteSpace(tag, nameof(tag));
            Assert.IsNotNull(value, nameof(value));

            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects())
            {
                if (attRef.Tag == tag)
                {
                    attRef.TextString = value;
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the values to the attributes.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attribs">Collection of pairs Tag/Value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>target</c> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>attribs</c> is null.</exception>
        public static void SetAttributeValues(this BlockReference target, Dictionary<string, string> attribs)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(attribs, nameof(attribs));

            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects())
            {
                if (attribs.ContainsKey(attRef.Tag))
                {
                    attRef.OpenForWrite();
                    attRef.TextString = attribs[attRef.Tag];
                }
            }
        }

        /// <summary>
        /// Adds the attribute references to the block reference and set their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attribValues">Collection of pairs Tag/Value.</param>
        /// <returns>The sequence of the newly created attribute references</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>target</c> is null.</exception>
        /// <exception cref="Autodesk.AutoCAD.Runtime.Exception">eNoActiveTransactions is thrown if there is no active transaction.</exception>
        public static IEnumerable<AttributeReference> AddAttributeReferences(this BlockReference target, Dictionary<string, string> attribValues)
        {
            Assert.IsNotNull(target, nameof(target));

            Transaction tr = target.Database.GetTopTransaction();

            BlockTableRecord btr = target.BlockTableRecord.GetObject<BlockTableRecord>();

            foreach (AttributeDefinition attDef in btr.GetObjects<AttributeDefinition>())
            {
                if (!attDef.Constant)
                {
                    var attRef = new AttributeReference();
                    attRef.SetAttributeFromBlock(attDef, target.BlockTransform);
                    if (attribValues != null && attribValues.ContainsKey(attDef.Tag.ToUpper()))
                    {
                        attRef.TextString = attribValues[attDef.Tag.ToUpper()];
                    }
                    target.AttributeCollection.AppendAttribute(attRef);
                    tr.AddNewlyCreatedDBObject(attRef, true);
                    yield return attRef;
                }
            }
        }

        /// <summary>
        /// Resets the attribute references keeping their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="attDefs">Sequence of attribute definitions.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>target</c> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>attDeffs</c> is null.</exception>
        internal static void ResetAttributes(this BlockReference target, IEnumerable<AttributeDefinition> attDefs)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(attDefs, nameof(attDefs));

            var attValues = new Dictionary<string, string>();
            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects(OpenMode.ForWrite))
            {
                attValues.Add(attRef.Tag,
                    attRef.IsMTextAttribute ? attRef.MTextAttribute.Contents : attRef.TextString);
                attRef.Erase();
            }
            foreach (AttributeDefinition attDef in attDefs)
            {
                var attRef = new AttributeReference();
                attRef.SetAttributeFromBlock(attDef, target.BlockTransform);
                if (attDef.Constant)
                {
                    attRef.TextString = attDef.IsMTextAttributeDefinition ?
                        attDef.MTextAttributeDefinition.Contents :
                        attDef.TextString;
                }
                else if (attValues.ContainsKey(attDef.Tag))
                {
                    attRef.TextString = attValues[attDef.Tag];
                }
                target.AttributeCollection.AppendAttribute(attRef);
                target.Database.TransactionManager.AddNewlyCreatedDBObject(attRef, true);
            }
        }

        /// <summary>
        /// Gets a dynamic property.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <c>propName</c> is null or empty.</exception>
        public static DynamicBlockReferenceProperty GetDynamicProperty(this BlockReference source, string propName)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));

            if (source.IsDynamicBlock)
                foreach (DynamicBlockReferenceProperty prop in source.DynamicBlockReferencePropertyCollection)
                    if (prop.PropertyName.Equals(propName))
                        return prop;
            return null;
        }

        /// <summary>
        /// Gets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property value or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <c>propName</c> is null or empty.</exception>
        public static object GetDynamicPropertyValue(this BlockReference source, string propName)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));

            DynamicBlockReferenceProperty prop = source.GetDynamicProperty(propName);
            if (prop == null) return null;
            return prop.Value;
        }

        /// <summary>
        /// Sets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <param name="value">New property value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>target</c> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <c>propName</c> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>value</c> is null.</exception>
        public static void SetDynamicPropertyValue(this BlockReference target, string propName, object value)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));
            Assert.IsNotNull(value, nameof(value));
            DynamicBlockReferenceProperty prop = target.GetDynamicProperty(propName);
            if (prop != null)
            {
                try { prop.Value = value; }
                catch { }
            }
        }

        /// <summary>
        /// Mirrors the block reference honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <exception cref="ArgumentNullException">Thrown if <c>source</c> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <c>axis</c> is null.</exception>
        public static void Mirror(this BlockReference source, Line3d axis, bool eraseSource)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(axis, nameof(axis));

            var db = source.Database;
            var tr = db.GetTopTransaction();

            BlockReference mirrored;
            if (eraseSource)
            {
                mirrored = source;
                if (!mirrored.IsWriteEnabled)
                {
                    tr.GetObject(mirrored.ObjectId, OpenMode.ForWrite);
                }
            }
            else
            {
                var ids = new ObjectIdCollection(new[] { source.ObjectId });
                var mapping = new IdMapping();
                db.DeepCloneObjects(ids, db.CurrentSpaceId, mapping, false);
                mirrored = (BlockReference)tr.GetObject(mapping[source.ObjectId].Value, OpenMode.ForWrite);
            }
            mirrored.TransformBy(Matrix3d.Mirroring(axis));

            if (!db.Mirrtext)
            {
                foreach (ObjectId id in mirrored.AttributeCollection)
                {
                    var attRef = (AttributeReference)tr.GetObject(id, OpenMode.ForWrite);
                    var pts = attRef.GetTextBoxCorners();
                    var cen = new LineSegment3d(pts[0], pts[2]).MidPoint;
                    var rotAxis = Math.Abs(axis.Direction.X) < Math.Abs(axis.Direction.Y) ?
                        pts[0].GetVectorTo(pts[3]) :
                        pts[0].GetVectorTo(pts[1]);
                    mirrored.TransformBy(Matrix3d.Rotation(Math.PI, rotAxis, cen));
                }
            }
        }
    }
}
