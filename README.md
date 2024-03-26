# Gile.AutoCAD.Extension
### AutoCAD Helpers Class Library
Thanks to all the [Swampers](http://www.theswamp.org/index.php) who helped me learning AutoCAD .NET programming, with a special mention to Tony 'TheMaster' Tanzillo and Thorsten 'kaefer' Meinecke for the discussions about the GetObject<T> and GetObjects<T> methods which were the start points of this library.

#### This library should help to write code in a more concise and declarative way.
Example with a method to erase lines in model space which are smaller than a given distance:
    
```c#
public void EraseShortLines(double minLength)
{
    var db = Application.DocumentManager.MdiActiveDocument.Database;
    using (var tr = db.TransactionManager.StartOpenCloseTransaction())
    {
        var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
        var modelSpace = (BlockTableRecord)tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);
        var lineClass = RXObject.GetClass(typeof(Line));
        foreach (ObjectId id in modelSpace)
        {
            if (id.ObjectClass == lineClass)
            {
                var line = (Line)tr.GetObject(id, OpenMode.ForRead);
                if (line.Length < minLength)
                {
                    tr.GetObject(id, OpenMode.ForWrite);
                    line.Erase();
                }
            }
        }
        tr.Commit();
    }
}
```
The same method can be written:

```c#
public void EraseShortLines(double minLength)
{
    var db = Active.Database;
    using (var tr = db.TransactionManager.StartOpenCloseTransaction())
    {
        db.GetModelSpace(tr)
            .GetObjects<Line>(tr)
            .Where(line => line.Length < minLength)
            .UpgradeOpen(tr)
            .ForEach(line => line.Erase());
        tr.Commit();
    }
}
```

Reference one of these assemblies in AutoCAD .NET projects setting the Copy Locale property to True.

Download the [assemblies](https://gilecad.azurewebsites.net/Resources/Gile.AutoCAD.Extension.zip) (Gile.AutoCAD.R20.Extension.dll for AutoCAD 2015 to 2024, Gile.AutoCAD.R25.Extension.dll for AutoCAD 2025+).

See the [documentation](https://gilecad.azurewebsites.net/Resources/AcadExtensionHelp/index.html).
