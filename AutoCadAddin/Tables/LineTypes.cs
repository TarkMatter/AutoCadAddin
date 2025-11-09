using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadAddin.Tables
{
    public static class LineTypes
    {
        public static ObjectId GetLineTypeId(string lineTypeName)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LinetypeTable ltt = tr.GetObject(db.LinetypeTableId, OpenMode.ForRead) as LinetypeTable;
                if (ltt.Has(lineTypeName))
                {
                    return ltt[lineTypeName];
                }
                return ltt[Constant.LinetypeNameArray["Default"]];
            }
        }
    }
}
