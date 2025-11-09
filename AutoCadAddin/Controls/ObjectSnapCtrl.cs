using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadAddin
{
    public class ObjectSnapCtrl
    {
        private const string _regAppName = "TTIF_SNAP";

        private static OSOverrule _osOverrule = null;
        private static IntOverrule _geoOverrule = null;
        private static DimOverrule _dimOverrule = null;

        private static short osMode;
        private static short autoSnap;

        public static string RegAppName => _regAppName;

        private class OSOverrule : OsnapOverrule
        {
            public OSOverrule()
            {
                SetXDataFilter(RegAppName);
            }

            public override void GetObjectSnapPoints
            (

                Entity ent,
                ObjectSnapModes mode,
                IntPtr gsm,
                Point3d pick,
                Point3d last,
                Matrix3d view,
                Point3dCollection snap,
                IntegerCollection geomIds
            )
            {
            }

            public override void GetObjectSnapPoints
            (
                Entity ent,
                ObjectSnapModes mode,
                IntPtr gsm,
                Point3d pick,
                Point3d last,
                Matrix3d view,
                Point3dCollection snaps,
                IntegerCollection geomIds,
                Matrix3d insertion
            )
            {
            }

            public override bool IsContentSnappable(Entity entity)
            {
                return false;
            }
        }

        public class DimOverrule : OsnapOverrule
        {
            //public event 
            public DimOverrule()
            {
                SetXDataFilter(RegAppName);
            }

            public override void GetObjectSnapPoints
            (

                Entity ent,
                ObjectSnapModes mode,
                IntPtr gsm,
                Point3d pick,
                Point3d last,
                Matrix3d view,
                Point3dCollection snap,
                IntegerCollection geomIds
            )
            {
                if (ent is RotatedDimension)
                {
                    return;
                }
            }

            public override void GetObjectSnapPoints
            (
                Entity ent,
                ObjectSnapModes mode,
                IntPtr gsm,
                Point3d pick,
                Point3d last,
                Matrix3d view,
                Point3dCollection snaps,
                IntegerCollection geomIds,
                Matrix3d insertion
            )
            {
                if (ent is RotatedDimension)
                {
                    return;
                }
            }

            public override bool IsContentSnappable(Entity entity)
            {
                return true;
            }
        }
        private class IntOverrule : GeometryOverrule
        {
            public IntOverrule()
            {
                SetXDataFilter(RegAppName);
            }

            public override void IntersectWith
            (
                Entity ent1,
                Entity ent2,
                Intersect intType,
                Plane proj,
                Point3dCollection points,
                IntPtr thisGsm,
                IntPtr otherGsm
            )
            {
            }

            public override void IntersectWith
            (
                Entity ent1,
                Entity ent2,
                Intersect intType,
                Point3dCollection points,
                IntPtr thisGsm,
                IntPtr otherGsm
            )
            {
            }
        }

        //該当のキー名がなければ追加登録
        private static void CheckRegAppName()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            using (RegAppTable rat = tr.GetObject(db.RegAppTableId, OpenMode.ForRead) as RegAppTable)
            {
                if (!rat.Has(RegAppName))
                {

                    rat.UpgradeOpen();
                    var ratr = new RegAppTableRecord();
                    ratr.Name = RegAppName;
                    rat.Add(ratr);
                    tr.AddNewlyCreatedDBObject(ratr, true);
                }
                tr.Commit();
            }
        }

        private static void ToggleOverruling(bool on)
        {
            if (on)
            {
                if (_dimOverrule == null)
                {
                    _dimOverrule = new DimOverrule();
                    ObjectOverrule.AddOverrule
                    (
                        RXObject.GetClass(typeof(Entity)),
                        _dimOverrule,
                        false
                    );
                }

                ObjectOverrule.Overruling = true;
            }
            else
            {
                if (_osOverrule != null)
                {
                    ObjectOverrule.RemoveOverrule
                    (
                        RXObject.GetClass(typeof(Entity)),
                        _osOverrule
                    );
                    _osOverrule.Dispose();
                    _osOverrule = null;
                }

                if (_geoOverrule != null)
                {
                    ObjectOverrule.RemoveOverrule
                    (
                        RXObject.GetClass(typeof(Entity)),
                        _geoOverrule
                    );
                    _geoOverrule.Dispose();
                    _geoOverrule = null;
                }
            }
        }
        //----------------------------------------------------------------------------------------------------
        public static void AddNoSnapXdata(Entity entity)
        {
            //オブジェクトスナップ回避用のXDataを付与
            using (var rb = new ResultBuffer
                    (
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName),
                        new TypedValue((int)DxfCode.ExtendedDataInteger16, 1)
                    )
                )
            {
                ToggleOverruling(true);

                CheckRegAppName();

                if (entity != null)
                {
                    entity.XData = rb;
                }
            }
        }

        //----------------------------------------------------------------------------------------------------
        public static void RemoveNoSnapXdata(Entity entity)
        {
            using (var rb = new ResultBuffer
                    (
                        new TypedValue((int)DxfCode.ExtendedDataRegAppName, RegAppName)
                    )
                )
            {
                if (entity != null)
                {
                    entity.XData = rb;
                }

                ToggleOverruling(false);
            }
        }
    }
}
