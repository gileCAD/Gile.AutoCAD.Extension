using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

using System.Collections.Generic;
using System.Linq;

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Provides extension methods for the BlockTableRecord type.
    /// </summary>
    public static class BlockTableRecordExtension
    {
        /// <summary>
        /// Opens the entities which type matches to the given one, and return them.
        /// </summary>
        /// <typeparam name="T">Type of objects to return.</typeparam>
        /// <param name="btr">Block table record.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="mode">Open mode to obtain in.</param>
        /// <param name="openErased">Value indicating whether to obtain erased objects.</param>
        /// <param name="forceOpenOnLockedLayers">Value indicating if locked layers should be opened.</param>
        /// <returns>The sequence of opened objects.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="btr"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static IEnumerable<T> GetObjects<T>(
          this BlockTableRecord btr,
          Transaction tr,
          OpenMode mode = OpenMode.ForRead,
          bool openErased = false,
          bool forceOpenOnLockedLayers = false) where T : Entity
        {
            Assert.IsNotNull(btr, nameof(btr));
            Assert.IsNotNull(tr, nameof(tr));

            var source = openErased ? btr.IncludingErased : btr;
            if (typeof(T) == typeof(Entity))
            {
                foreach (ObjectId id in source)
                {
                    yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                }
            }
            else
            {
                var rxClass = RXObject.GetClass(typeof(T));
                foreach (ObjectId id in source)
                {
                    if (id.ObjectClass.IsDerivedFrom(rxClass))
                    {
                        yield return (T)tr.GetObject(id, mode, openErased, forceOpenOnLockedLayers);
                    }
                }
            }
        }

        /// <summary>
        /// Appends the entities to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="entities">Sequence of entities.</param>
        /// <returns>The collection of added entities ObjectId.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entities"/> is null.</exception>
        public static ObjectIdCollection Add(this BlockTableRecord owner, Transaction tr, IEnumerable<Entity> entities)
        {
            Assert.IsNotNull(owner, nameof(owner));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(entities, nameof(entities));

            var ids = new ObjectIdCollection();
            using (var ents = new DisposableSet<Entity>(entities))
            {
                foreach (Entity ent in ents)
                {
                    ids.Add(owner.AppendEntity(ent));
                    tr.AddNewlyCreatedDBObject(ent, true);
                }
            }
            return ids;
        }

        /// <summary>
        /// Appends the entities to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="tr">Active transaction</param>
        /// <param name="entities">Collection of entities.</param>
        /// <returns>The collection of added entities ObjectId.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entities"/> is null.</exception>
        public static ObjectIdCollection AddRange(this BlockTableRecord owner, Transaction tr, params Entity[] entities)
        {
            Assert.IsNotNull(owner, nameof(owner));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(entities, nameof(entities));

            return owner.Add(tr, entities);
        }

        /// <summary>
        /// Appends the entity to the BlockTableRecord.
        /// </summary>
        /// <param name="owner">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="entity">Entity to add.</param>
        /// <returns>The ObjectId of added entity.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="owner"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="entity"/> is null.</exception>
        public static ObjectId Add(this BlockTableRecord owner, Transaction tr, Entity entity)
        {
            Assert.IsNotNull(owner, nameof(owner));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNull(entity, nameof(entity));

            try
            {
                var id = owner.AppendEntity(entity);
                tr.AddNewlyCreatedDBObject(entity, true);
                return id;
            }
            catch
            {
                entity.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Inserts a block reference.
        /// </summary>
        /// <param name="target">Instance to which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <param name="blkName">Nlock name.</param>
        /// <param name="insertPoint">Insertion point.</param>
        /// <param name="xScale">X scale factor.</param>
        /// <param name="yScale">Y scale factor.</param>
        /// <param name="zScale">Z scale factor.</param>
        /// <param name="rotation">Rotation</param>
        /// <param name="attribValues">Collection of key/value pairs (Tag/Value).</param>
        /// <returns>The newly created BlockReference.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name ="blkName"/> is null or empty.</exception>
        public static BlockReference InsertBlockReference(
            this BlockTableRecord target,
            Transaction tr,
            string blkName,
            Point3d insertPoint,
            double xScale = 1.0,
            double yScale = 1.0,
            double zScale = 1.0,
            double rotation = 0.0,
            Dictionary<string, string> attribValues = null)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));
            Assert.IsNotNullOrWhiteSpace(blkName, nameof(blkName));

            var db = target.Database;
            BlockReference br = null;
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var btrId = bt.GetBlock(blkName);
            if (btrId != ObjectId.Null)
            {
                br = new BlockReference(insertPoint, btrId) { ScaleFactors = new Scale3d(xScale, yScale, zScale), Rotation = rotation };
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                if (btr.Annotative == AnnotativeStates.True)
                {
                    var objectContextManager = db.ObjectContextManager;
                    var objectContextCollection = objectContextManager.GetContextCollection("ACDB_ANNOTATIONSCALES");
                    Autodesk.AutoCAD.Internal.ObjectContexts.AddContext(br, objectContextCollection.CurrentContext);
                }
                target.Add(tr, br);
                br.AddAttributeReferences(tr, attribValues);
            }
            return br;
        }

        /// <summary>
        /// Synchronizes the attributes of all block references.
        /// </summary>
        /// <param name="target">Instance which the method applies.</param>
        /// <param name="tr">Transaction or OpenCloseTransaction to use.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="target"/> is null.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name ="tr"/> is null.</exception>
        public static void SynchronizeAttributes(this BlockTableRecord target, Transaction tr)
        {
            Assert.IsNotNull(target, nameof(target));
            Assert.IsNotNull(tr, nameof(tr));

            AttributeDefinition[] attDefs = target.GetObjects<AttributeDefinition>(tr).ToArray();
            foreach (BlockReference br in target.GetBlockReferenceIds(true, false).GetObjects<BlockReference>(tr, OpenMode.ForWrite))
            {
                br.ResetAttributes(tr, attDefs);
            }
            if (target.IsDynamicBlock)
            {
                target.UpdateAnonymousBlocks();
                foreach (BlockTableRecord btr in target.GetAnonymousBlockIds().GetObjects<BlockTableRecord>(tr))
                {
                    attDefs = btr.GetObjects<AttributeDefinition>(tr).ToArray();
                    foreach (BlockReference br in btr.GetBlockReferenceIds(true, false).GetObjects<BlockReference>(tr, OpenMode.ForWrite))
                    {
                        br.ResetAttributes(tr, attDefs);
                    }
                }
            }
        }
    }
}
