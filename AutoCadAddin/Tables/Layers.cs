using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;

using AutoCadAddin.Params;

namespace AutoCadAddin.Tables
{
    public static class Layers
    {
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 指定のレイヤーが存在するか確認して、なければ"0"画層を返却
        /// </summary>
        /// <param name="layerName">指定のレイヤー名</param>
        /// <returns></returns>
        public static string SetLayer(string layerName)
        {
            bool isExist = CheckLayer(layerName);

            return isExist ? layerName : Config.LayerName["Base"];
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 指定のレイヤー名が存在するか確認
        /// </summary>
        /// <param name="layerName">指定のレイヤー名</param>
        /// <returns></returns>
        private static bool CheckLayer(string layerName)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                LayerTable lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                return lt.Has(layerName);
            }
        }

        public static void SettingLayer(ParamsLayer layer)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    if (lt.Has(layer.Name))
                    {
                        LayerTableRecord ltr = tr.GetObject(lt[layer.Name], OpenMode.ForWrite) as LayerTableRecord;

                        ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, layer.ColorIndex);
                        ltr.LinetypeObjectId = layer.LineTypeId;
                        ltr.LineWeight = layer.LineWeight;
                        ltr.IsPlottable = layer.IsPlotable;
                    }
                    else
                    {
                        LayerTable acLyrTbl = tr.GetObject(db.LayerTableId, OpenMode.ForWrite) as LayerTable;

                        using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                        {
                            acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, layer.ColorIndex);
                            acLyrTblRec.Name = layer.Name;
                            acLyrTblRec.LinetypeObjectId = layer.LineTypeId;
                            acLyrTblRec.LineWeight = layer.LineWeight;
                            acLyrTblRec.IsPlottable = layer.IsPlotable;

                            acLyrTbl.Add(acLyrTblRec);
                            tr.AddNewlyCreatedDBObject(acLyrTblRec, true);
                        }
                    }
                    tr.Commit();
                }
                catch (Exception ex)
                {
                    tr.Abort();
                    ed.WriteMessage("\n" + "LayerClass: " + ex.Message);
                }
            }
            return;
        }
    }
}
