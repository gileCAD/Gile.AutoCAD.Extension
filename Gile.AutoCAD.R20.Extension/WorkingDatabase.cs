using Autodesk.AutoCAD.DatabaseServices;

using System;

namespace Gile.AutoCAD.R20.Extension
{
    /// <summary>
    /// Provides a safe way to temporarily set the working database.
    /// Credits Tony Tanzillo: https://forums.autodesk.com/t5/net/want-to-have-c-program-to-modify-dwg-and-save-into-dwg-format/m-p/12439905/highlight/true#M79788
    /// </summary>
    public class WorkingDatabase : IDisposable
    {
        Database previous;

        /// <summary>
        /// Creates a new instance of WorkingDatabase.
        /// </summary>
        /// <param name="newWorkingDb">Database to be temporarily set as working database.</param>
        public WorkingDatabase(Database newWorkingDb)
        {
            Assert.IsNotNull(newWorkingDb, nameof(newWorkingDb));

            Database current = HostApplicationServices.WorkingDatabase;
            if (newWorkingDb != current)
            {
                previous = current;
                HostApplicationServices.WorkingDatabase = newWorkingDb;
            }
        }

        /// <summary>
        /// Restores the previous working database.
        /// </summary>
        public void Dispose()
        {
            if (previous != null)
            {
                HostApplicationServices.WorkingDatabase = previous;
                previous = null;
            }
        }
    }
}
