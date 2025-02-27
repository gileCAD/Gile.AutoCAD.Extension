﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Extension
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
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>The effective name of the block reference.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static string GetEffectiveName(this BlockReference source, Transaction tr)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));

            return ((BlockTableRecord)tr.GetObject(source.DynamicBlockTableRecord, OpenMode.ForRead)).Name;
        }

        /// <summary>
        /// Gets all the attributes by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>Sequence of pairs Tag/Attribute.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<KeyValuePair<string, AttributeReference>> GetAttributesByTag(this BlockReference source, Transaction tr)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));

            return source
                .AttributeCollection.GetObjects(tr)
                .Select(att => new KeyValuePair<string, AttributeReference>(att.Tag, att));
        }

        /// <summary>
        /// Gets all the attribute values by tag.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <returns>Collection of pairs Tag/Value.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static Dictionary<string, string> GetAttributesValues(this BlockReference source, Transaction tr)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));

            return source
                .GetAttributesByTag(tr)
                .ToDictionary(p => p.Key, p => p.Value.TextString);
        }

        /// <summary>
        /// Sets the value to the attribute.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">ActiveTransaction.</param>
        /// <param name="tag">Attribute tag.</param>
        /// <param name="value">New value.</param>
        /// <returns>The value if attribute was found, null otherwise.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="tag"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="value"/> is null.</exception>
        public static string SetAttributeValue(this BlockReference target, Transaction tr, string tag, string value)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(tag, nameof(tag));
            Assert.IsNotNull(value, nameof(value));

            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects(tr))
            {
                if (attRef.Tag == tag)
                {
                    tr.GetObject(attRef.ObjectId, OpenMode.ForWrite);
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
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="attribs">Collection of pairs Tag/Value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="attribs"/> is null.</exception>
        public static void SetAttributeValues(this BlockReference target, Transaction tr, Dictionary<string, string> attribs)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(attribs, nameof(attribs));

            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects(tr))
            {
                if (attribs.TryGetValue(attRef.Tag, out string value))
                {
                    tr.GetObject(attRef.ObjectId, OpenMode.ForWrite);
                    attRef.TextString = value;
                }
            }
        }

        /// <summary>
        /// Adds the attribute references to the block reference and set their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="attribValues">Collection of pairs Tag/Value.</param>
        /// <returns>A Dictionary containing the newly created attribute references by tag.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static Dictionary<string, AttributeReference> AddAttributeReferences(
            this BlockReference target, Transaction tr, Dictionary<string, string> attribValues)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));

            var btr = (BlockTableRecord)tr.GetObject(target.BlockTableRecord, OpenMode.ForRead);
            var attribs = new Dictionary<string, AttributeReference>();
            foreach (AttributeDefinition attDef in btr.GetObjects<AttributeDefinition>(tr))
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
                    attribs[attRef.Tag] = attRef;
                }
            }
            return attribs;
        }

        /// <summary>
        /// Resets the attribute references keeping their values.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="attDefs">Sequence of attribute definitions.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="attDefs"/> is null.</exception>
        internal static void ResetAttributes(this BlockReference target, Transaction tr, IEnumerable<AttributeDefinition> attDefs)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(attDefs, nameof(attDefs));

            var attValues = new Dictionary<string, string>();
            foreach (AttributeReference attRef in target.AttributeCollection.GetObjects(tr, OpenMode.ForWrite))
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
                else if (attValues.TryGetValue(attDef.Tag, out string value))
                {
                    attRef.TextString = value;
                }
                target.AttributeCollection.AppendAttribute(attRef);
                tr.AddNewlyCreatedDBObject(attRef, true);
            }
        }

        /// <summary>
        /// Gets a dynamic property.
        /// </summary>                                                                                                                                                                                                                
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        public static DynamicBlockReferenceProperty GetDynamicProperty(this BlockReference source, string propName)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));

            foreach (DynamicBlockReferenceProperty prop in source.DynamicBlockReferencePropertyCollection)
            {
                if (prop.PropertyName.Equals(propName, StringComparison.OrdinalIgnoreCase))
                {
                    return prop;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <returns>The dynamic property value or null if not found.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        public static object GetDynamicPropertyValue(this BlockReference source, string propName)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));

            var prop = source.GetDynamicProperty(propName);
            return prop?.Value;
        }

        /// <summary>
        /// Sets the value of a dynamic bloc property.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="propName">Dynamic property name.</param>
        /// <param name="value">New property value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name ="propName"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="value"/> is null.</exception>
        public static void SetDynamicPropertyValue(this BlockReference target, string propName, object value)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNullOrWhiteSpace(propName, nameof(propName));
            Assert.IsNotNull(value, nameof(value));

            var prop = target.GetDynamicProperty(propName);
            if (prop != null && prop.Value != value)
            {
                prop.Value = (prop.PropertyTypeCode == 1 && !(value is double)) ?
                    Convert.ToDouble(value) :
                    value;
            }
        }

        /// <summary>
        /// Mirrors the block reference honoring the value of MIRRTEXT system variable.
        /// </summary>
        /// <param name="source">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="axis">Axis of the mirroring operation.</param>
        /// <param name="eraseSource">Value indicating if the source block reference have to be erased.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="source"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name ="axis"/> is null.</exception>
        public static void Mirror(this BlockReference source, Transaction tr, Line3d axis, bool eraseSource)
        {
            Assert.IsNotNull(source, nameof(source));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(axis, nameof(axis));

            var db = source.Database;
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
