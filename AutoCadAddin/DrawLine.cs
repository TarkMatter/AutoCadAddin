using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoCadAddin.Tables;

namespace AutoCadAddin
{
    public static class DrawLine
    {
        static private Point3d _startPoint;
        static private Point3d _lastPoint;
        static private ObjectId _oId = ObjectId.Null;
        static private Direction dir;

        static private List<ObjectId> _oIds = new List<ObjectId>();

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 中心線の作成２
        /// </summary>
        public static void DrawCenterLine2()
        {
            //ドキュメントの作成
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            //事前選択のリセット
            ObjectId[] emptys = new ObjectId[0];
            ed.SetImpliedSelection(emptys);

            Point3d _line1st_StartPoint = new Point3d();
            Point3d _line1st_EndPoint = new Point3d();
            Point3d _line2nd_StartPoint = new Point3d();
            Point3d _line2nd_EndPoint = new Point3d();

            PromptEntityOptions peo = new PromptEntityOptions("\n1本目の線分を選択:");
            peo.SetRejectMessage("\n線分オブジェクトではありません.");
            peo.AddAllowedClass(typeof(Line), true);
            PromptEntityResult pent1 = ed.GetEntity(peo);

            if (pent1.Status != PromptStatus.OK)
            {
                return;
            }

            peo = new PromptEntityOptions("\n2本目の線分を選択:");
            peo.SetRejectMessage("\n線分オブジェクトではありません.");
            peo.AddAllowedClass(typeof(Line), true);
            PromptEntityResult pent2 = ed.GetEntity(peo);

            if (pent2.Status != PromptStatus.OK)
            {
                return;
            }

            if (pent1.ObjectId == pent2.ObjectId)
            {
                return;
            }

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                Entity ent = tr.GetObject(pent1.ObjectId, OpenMode.ForRead) as Entity;
                Line line1 = new Line();
                Line line2 = new Line();
                Line ccLine = new Line();

                if (ent.GetType() == typeof(Line))
                {
                    line1 = ent as Line;
                    _line1st_StartPoint = line1.StartPoint;
                    _line1st_EndPoint = line1.EndPoint;
                }

                ent = tr.GetObject(pent2.ObjectId, OpenMode.ForRead) as Entity;
                if (ent.GetType() == typeof(Line))
                {
                    line2 = ent as Line;
                    _line2nd_StartPoint = line2.StartPoint;
                    _line2nd_EndPoint = line2.EndPoint;
                }

                //線分の端点同士を接続した際の交差関係確認
                Line cLine1 = new Line(_line1st_StartPoint, _line2nd_StartPoint);
                Line cLine2 = new Line(_line1st_EndPoint, _line2nd_EndPoint);

                //端点接続線分が交差しているか判定
                Point3dCollection intPoints = new Point3dCollection();
                cLine1.IntersectWith(cLine2, Intersect.OnBothOperands, intPoints, IntPtr.Zero, IntPtr.Zero);

                //端点接続線分が交差している場合は、終点を入れ替える
                if (intPoints.Count != 0)
                {
                    cLine1.EndPoint = _line2nd_EndPoint;
                    cLine2.EndPoint = _line2nd_StartPoint;
                }

                //第１線分の角度を取得
                Point2d cPoint = new Point2d(
                    (_line1st_StartPoint - _line1st_EndPoint).TransformBy(ed.CurrentUserCoordinateSystem).X,
                    (_line1st_StartPoint - _line1st_EndPoint).TransformBy(ed.CurrentUserCoordinateSystem).Y
                    );
                Vector3d vec1 = line1.EndPoint - line1.StartPoint;
                vec1 = vec1.TransformBy(ed.CurrentUserCoordinateSystem);
                double ang1 = vec1.GetAngleTo(ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis);

                //第２線分の角度を取得
                cPoint = new Point2d(
                    (_line2nd_StartPoint - _line2nd_EndPoint).TransformBy(ed.CurrentUserCoordinateSystem).X,
                    (_line2nd_StartPoint - _line2nd_EndPoint).TransformBy(ed.CurrentUserCoordinateSystem).Y
                    );
                Vector3d vec2 = line2.EndPoint - line2.StartPoint;
                vec2 = vec2.TransformBy(ed.CurrentUserCoordinateSystem);
                double ang2 = vec2.GetAngleTo(ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis);

                Point3d cPoint1 = new Point3d();
                Point3d cPoint2 = new Point3d();

                if ((ang1 >= Math.PI ? ang1 - Math.PI : ang1) == (ang2 >= Math.PI ? ang2 - Math.PI : ang2))
                {
                    //角度が同じ場合＝平行の場合
                    //端点接続線分１の中点を取得
                    cPoint1 = new Point3d(
                        (cLine1.StartPoint.X + cLine1.EndPoint.X) / 2d,
                        (cLine1.StartPoint.Y + cLine1.EndPoint.Y) / 2d,
                        (cLine1.StartPoint.Z + cLine1.EndPoint.Z) / 2d
                        );
                    //端点接続線分２の中点を取得
                    cPoint2 = new Point3d(
                        (cLine2.StartPoint.X + cLine2.EndPoint.X) / 2d,
                        (cLine2.StartPoint.Y + cLine2.EndPoint.Y) / 2d,
                        (cLine2.StartPoint.Z + cLine2.EndPoint.Z) / 2d
                        );
                }
                else
                {
                    //角度が違う場合＝延長線上で交差する場合
                    intPoints = new Point3dCollection();
                    line1.IntersectWith(line2, Intersect.ExtendBoth, intPoints, IntPtr.Zero, IntPtr.Zero);

                    //交差点が見つからなければ終了
                    if (intPoints.Count == 0)
                    {
                        ed.WriteMessage("\nオブジェクト同士が同一平面にない可能性があります");
                        return;
                    }

                    //現在の描画空間を判定
                    string spaceName = LayoutManager.Current.CurrentLayout == "Model" ? BlockTableRecord.ModelSpace : BlockTableRecord.PaperSpace;

                    using (BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable)
                    using (BlockTableRecord btr = tr.GetObject(bt[spaceName], OpenMode.ForWrite) as BlockTableRecord)
                    using (Xline xl = new Xline())
                    {
                        //線分の仮想交点を取得
                        Point3d crossPoint = intPoints[0];

                        Point3d targetPoint = crossPoint.DistanceTo(line1.StartPoint) > crossPoint.DistanceTo(line1.EndPoint) ? line1.StartPoint : line1.EndPoint;
                        vec1 = targetPoint - crossPoint;
                        vec1 = vec1.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());
                        ang1 = vec1.GetAngleTo(Vector3d.XAxis);
                        if (vec1.CrossProduct(Vector3d.XAxis).Z > 0)
                        {
                            ang1 = 2 * Math.PI - ang1;
                        }

                        targetPoint = targetPoint = crossPoint.DistanceTo(line2.StartPoint) > crossPoint.DistanceTo(line2.EndPoint) ? line2.StartPoint : line2.EndPoint;
                        vec2 = targetPoint - crossPoint;
                        vec2 = vec2.TransformBy(ed.CurrentUserCoordinateSystem.Inverse());
                        ang2 = vec2.GetAngleTo(Vector3d.XAxis);
                        if (vec2.CrossProduct(Vector3d.XAxis).Z > 0)
                        {
                            ang2 = 2 * Math.PI - ang2;
                        }

                        //２つの線分の２等分角を取得
                        double difAng = (ang1 + ang2) / 2d;
                        if (difAng > (2d * Math.PI))
                        {
                            difAng -= (2d * Math.PI);
                        }

                        int i = 0;
                        do
                        {
                            i++;
                            //仮想交点から２等分角へ伸びる無限線を仮想で作成
                            xl.BasePoint = crossPoint;
                            Vector3d directionVector = new Vector3d(Math.Cos(difAng), Math.Sin(difAng), 0);
                            Vector3d transformedDirection = directionVector.TransformBy(ed.CurrentUserCoordinateSystem);
                            xl.SecondPoint = crossPoint + transformedDirection;

                            if (i > 100)
                            {
                                ed.WriteMessage("\n中心線を作成できませんでした");
                                return;
                            }

                            intPoints = new Point3dCollection();
                            xl.IntersectWith(cLine1, Intersect.ExtendThis, intPoints, IntPtr.Zero, IntPtr.Zero);
                            if (intPoints.Count == 0)
                            {
                                difAng += (Math.PI / 2d);
                                difAng = difAng % (2 * Math.PI);
                                continue;
                            }
                            intPoints = new Point3dCollection();
                            xl.IntersectWith(cLine2, Intersect.ExtendThis, intPoints, IntPtr.Zero, IntPtr.Zero);
                            if (intPoints.Count == 0)
                            {
                                difAng += (Math.PI / 2d);
                                difAng %= (2 * Math.PI);
                                continue;
                            }

                            break;
                        }
                        while (true);

                        //線分１と無限線との交点を取得
                        intPoints = new Point3dCollection();
                        xl.IntersectWith(cLine1, Intersect.ExtendBoth, intPoints, IntPtr.Zero, IntPtr.Zero);
                        cPoint1 = new Point3d(intPoints[0].X, intPoints[0].Y, intPoints[0].Z);

                        //線分２と無限線との交点を取得
                        intPoints = new Point3dCollection();
                        xl.IntersectWith(cLine2, Intersect.ExtendBoth, intPoints, IntPtr.Zero, IntPtr.Zero);
                        cPoint2 = new Point3d(intPoints[0].X, intPoints[0].Y, intPoints[0].Z);
                    }
                }

                //中心線の作成
                CreateCenterLine(db, tr, cPoint1, cPoint2);

                tr.Commit();
            }


        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 中心線の作成
        /// </summary>
        /// <param name="db">データベース</param>
        /// <param name="tr">トランザクション</param>
        /// <param name="sPoint">始点</param>
        /// <param name="ePoint">終点</param>
        private static bool CreateCenterLine(Database db, Transaction tr, Point3d sPoint, Point3d ePoint)
        {
            try
            {
                //交点同士の方向ベクトルを取得・単位ベクトル化
                Vector3d direction = sPoint - ePoint;
                direction = direction.GetNormal();

                //各交点から単位ベクトルの正負方向に中心線のオーバーシュート長さ分の位置を取得
                sPoint += direction * Config.Values["CenterLineOvershoot"];
                ePoint -= direction * Config.Values["CenterLineOvershoot"];

                //現在の描画空間を判定
                string spaceName = LayoutManager.Current.CurrentLayout == "Model" ? BlockTableRecord.ModelSpace : BlockTableRecord.PaperSpace;

                //線分を追加
                using (BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable)
                using (BlockTableRecord btr = tr.GetObject(bt[spaceName], OpenMode.ForWrite) as BlockTableRecord)
                using (Line line = new Line(sPoint, ePoint))
                {
                    btr.AppendEntity(line);
                    line.Layer = Layers.SetLayer(Config.LayerName["Center"]);
                    line.Linetype = Config.LineType;
                    line.LinetypeScale = 1d / db.Cannoscale.Scale;
                    tr.AddNewlyCreatedDBObject(line, true);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                throw ex;
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
            return true;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 中心線の描画
        /// </summary>
        public static void DrawCenterLine()
        {
            //ドキュメントの作成
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            //事前選択のリセット
            ObjectId[] emptys = new ObjectId[0];
            ed.SetImpliedSelection(emptys);

            PromptPointResult pPtRes;
            PromptPointOptions pPtOpts = new PromptPointOptions("");

            //始点をプロンプトで指定
            pPtOpts.Message = "\n中心線の始点を指定: ";
            pPtRes = doc.Editor.GetPoint(pPtOpts);
            Point3d ptStart = pPtRes.Value;

            //ESCキーが押されるかキャンセルされたら終了
            if (pPtRes.Status == PromptStatus.Cancel)
            {
                return;
            }

            //終点をプロンプトで指定
            pPtOpts.Message = "\n中心線の終点を指定: ";
            pPtOpts.UseBasePoint = true;
            pPtOpts.BasePoint = ptStart;
            pPtRes = doc.Editor.GetPoint(pPtOpts);
            Point3d ptEnd = pPtRes.Value;

            //ESCキーが押されるかキャンセルされたら終了
            if (pPtRes.Status == PromptStatus.Cancel)
            {
                return;
            }

            //現在の座標系に変換
            ptStart = ptStart.TransformBy(ed.CurrentUserCoordinateSystem);
            ptEnd = ptEnd.TransformBy(ed.CurrentUserCoordinateSystem);

            Vector3d direction = ptStart - ptEnd;
            direction = direction.GetNormal();

            ptStart += direction * Config.Values["CenterLineOvershoot"];
            ptEnd -= direction * Config.Values["CenterLineOvershoot"];

            //トランザクション開始
            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    ////線分を追加
                    CreateCenterLine(db, tr, ptStart, ptEnd);

                    //コミット
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    tr.Abort();
                    ed.WriteMessage("\n" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name
                                    + " / " + System.Reflection.MethodBase.GetCurrentMethod().Name + "(Acad) : " + ex.Message);
                }
                catch (System.Exception ex)
                {
                    tr.Abort();
                    ed.WriteMessage("\n" + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name
                                    + " / " + System.Reflection.MethodBase.GetCurrentMethod().Name + "(System) : " + ex.Message);
                }
            }
            return;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 円の中心線の描画
        /// </summary>
        public static void DrawCircleCenterLine()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptSelectionResult selRes = ed.SelectImplied();
            SelectionSet sset;

            if (selRes.Value == null)
            {
                selRes = ed.GetSelection();
                sset = selRes.Value;


                //キャンセルされたら終了
                if (selRes.Status != PromptStatus.OK)
                {
                    return;
                }
            }
            else
            {
                selRes = ed.GetSelection();
                sset = selRes.Value;
            }

            using (Database db = doc.Database)
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (SelectedObject so in sset)
                {
                    Entity target = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;

                    Point3d center;
                    double radius;

                    Vector3d normal;

                    if (target.GetType() != typeof(Circle) && target.GetType() != typeof(Arc))
                    {
                        continue;
                    }

                    if (target.GetType() == typeof(Circle))
                    {
                        Circle obj = target as Circle;
                        normal = obj.Normal;
                        center = obj.Center;
                        radius = obj.Radius;
                    }
                    else
                    {
                        Arc obj = target as Arc;
                        normal = obj.Normal;
                        center = obj.Center;
                        radius = obj.Radius;
                    }

                    //水平の中心線を作成
                    Point3d startPoint = center + radius * normal.GetPerpendicularVector();
                    Point3d endPoint = center - radius * normal.GetPerpendicularVector();
                    CreateCenterLine(db, tr, startPoint, endPoint);

                    //垂直の中心線を作成
                    normal = normal.CrossProduct(normal.GetPerpendicularVector());
                    startPoint = center + radius * normal;
                    endPoint = center - radius * normal;
                    CreateCenterLine(db, tr, startPoint, endPoint);

                }


                //コミット
                tr.Commit();

            }
            return;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 下書き線の作成
        /// </summary>
        /// <param name="d">作成方向</param>
        public static void DrawXline(Direction d)
        {
            //ドキュメントの作成
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptPointOptions opts;
            PromptPointResult res;
            bool _isAddHandler = false;

            //事前選択のリセット
            ObjectId[] emptys = new ObjectId[0];
            ed.SetImpliedSelection(emptys);

            string selectContents = "[任意(D) /水平(H) /垂直(V)]: ";
            string selectOptions = "D H V";

            dir = d;
            while (true)
            {
                opts = new PromptPointOptions("\n点を指定 または " + selectContents, selectOptions);
                opts.Keywords.Default = "D";

                //初期値が任意(D)以外ならカーソル追跡開始
                if (dir != Direction.NONE)
                {
                    _isAddHandler = ToggleTrackXline(ed, _isAddHandler, true);
                }

                //キーワードを入力する限りループ
                //ESCキーで終了
                //任意の点をクリックで次へ
                do
                {
                    //キーワードもしくはクリックを入力要請
                    switch (dir)
                    {
                        case Direction.NONE:
                            opts = new PromptPointOptions("\n点を指定 または " + selectContents, selectOptions);
                            opts.Keywords.Default = "D";
                            break;
                        case Direction.HORIZONTAL:
                            opts = new PromptPointOptions("\n通過点を指定 または " + selectContents, selectOptions);
                            opts.Keywords.Default = "H";
                            break;
                        case Direction.VERTICAL:
                            opts = new PromptPointOptions("\n通過点を指定 または " + selectContents, selectOptions);
                            opts.Keywords.Default = "V";
                            break;
                    }
                    res = ed.GetPoint(opts);

                    //キーワードが入力されたら
                    if (res.Status == PromptStatus.Keyword)
                    {
                        //一度イベントハンドラを削除
                        _isAddHandler = ToggleTrackXline(ed, _isAddHandler, false);

                        if (!_oId.IsNull)
                        {
                            //最後に描画していたオブジェクトを削除
                            using (Database db = doc.Database)
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {
                                Entity ent = tr.GetObject(_oId, OpenMode.ForWrite) as Entity;
                                ent.Erase();
                                tr.Commit();
                            }
                            //更新用オブジェクトIDフィールドを初期化
                            _oId = ObjectId.Null;
                        }

                        //選択されたオプションごとに挙動を変更
                        switch (res.StringResult)
                        {
                            case "D":
                                dir = Direction.NONE;
                                break;

                            case "H":
                                dir = Direction.HORIZONTAL;
                                _isAddHandler = ToggleTrackXline(ed, _isAddHandler, true);
                                break;

                            case "V":
                                dir = Direction.VERTICAL;
                                _isAddHandler = ToggleTrackXline(ed, _isAddHandler, true);
                                break;
                        }
                    }
                    //入力がキーワードでもクリックでもない場合、終了
                    if (res.Status != PromptStatus.Keyword && res.Status != PromptStatus.OK)
                    {
                        _isAddHandler = ToggleTrackXline(ed, _isAddHandler, false);

                        //キャンセルされたら最後に描画していたオブジェクトを削除
                        if (!_oId.IsNull)
                        {
                            //最後に描画していたオブジェクトを削除
                            using (Database db = doc.Database)
                            using (Transaction tr = db.TransactionManager.StartTransaction())
                            {
                                Entity ent = tr.GetObject(_oId, OpenMode.ForWrite) as Entity;
                                ent.Erase();
                                tr.Commit();
                            }
                            //更新用オブジェクトIDフィールドを初期化
                            _oId = ObjectId.Null;
                        }

                        return;
                    }

                    if (res.Status == PromptStatus.OK && dir == Direction.NONE)
                    {
                        _startPoint = res.Value;
                    }

                } while (res.Status == PromptStatus.Keyword);

                //任意方向の場合、2点目を入力要請
                if (dir == Direction.NONE)
                {
                    //終点に始点を代入
                    _lastPoint = _startPoint;

                    //マウスイベントに下書き戦の常時描画更新メソッドを登録
                    _isAddHandler = ToggleTrackXline(ed, _isAddHandler, true);

                    //終点を入力要請
                    opts = new PromptPointOptions("\n通過点を指定 : ");
                    res = ed.GetPoint(opts);

                    //キャンセルされたら最後に描画していたオブジェクトを削除
                    if (res.Status != PromptStatus.OK)
                    {
                        using (Database db = doc.Database)
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            Entity ent = tr.GetObject(_oId, OpenMode.ForWrite) as Entity;
                            ent.Erase();
                            tr.Commit();
                        }
                    }
                    else
                    {
                        using (Database db = doc.Database)
                        using (Transaction tr = db.TransactionManager.StartTransaction())
                        {
                            try
                            {
                                Xline ent = tr.GetObject(_oId, OpenMode.ForWrite) as Xline;

                                Vector3d normal;
                                switch (dir)
                                {
                                    case Direction.NONE:
                                        ent.SecondPoint = res.Value;
                                        break;
                                    case Direction.HORIZONTAL:
                                        ent.BasePoint = res.Value;
                                        normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis;
                                        ent.SecondPoint = new Point3d(res.Value.X + normal.X, res.Value.Y + normal.Y, res.Value.Z + normal.Z);
                                        break;
                                    case Direction.VERTICAL:
                                        ent.BasePoint = res.Value;
                                        normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis;
                                        ent.SecondPoint = new Point3d(res.Value.X + normal.X, res.Value.Y + normal.Y, res.Value.Z + normal.Z);
                                        break;
                                    default:
                                        break;
                                }

                                //オブジェクトスナップ回避用のXDataを削除
                                ObjectSnapCtrl.RemoveNoSnapXdata(ent);

                                //コミット
                                tr.Commit();
                            }
                            catch (Autodesk.AutoCAD.Runtime.Exception ex)
                            {
                                tr.Abort();
                                ed.WriteMessage("\n" + "DrawLineClass / DrawXline(XL)(Acad): " + ex.Message);
                            }
                            catch (System.Exception ex)
                            {
                                tr.Abort();
                                ed.WriteMessage("\n" + "DrawLineClass / DrawXline(XL)(System): " + ex.Message);
                            }

                        }
                    }
                }
                else
                {
                    using (Database db = doc.Database)
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            Xline ent = tr.GetObject(_oId, OpenMode.ForWrite) as Xline;

                            //オブジェクトスナップ回避用のXDataを削除
                            ObjectSnapCtrl.RemoveNoSnapXdata(ent);

                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            tr.Abort();
                            ed.WriteMessage("\n" + "DrawLineClass / DrawXline(XV,XH):(Acad) " + ex.Message);
                        }
                        catch (System.Exception ex)
                        {
                            tr.Abort();
                            ed.WriteMessage("\n" + "DrawLineClass / DrawXline(XV,XH)(System): " + ex.Message);
                        }
                        tr.Commit();
                    }
                }

                //マウスイベントから下書き線の常時描画更新メソッドを削除
                _isAddHandler = ToggleTrackXline(ed, _isAddHandler, false);

                //更新用オブジェクトIDフィールドを初期化
                _oId = ObjectId.Null;
            }
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// ポインタモニタイベントへのイベントハンドラ追加および削除
        /// </summary>
        /// <param name="ed">エディタ</param>
        /// <param name="isAdd">現在の追加削除状態</param>
        /// <param name="onOff">追加・削除指示</param>
        /// <returns>変更した追加削除状態</returns>
        private static bool ToggleTrackXline(Editor ed, bool isAdd, bool onOff)
        {
            if (onOff && !isAdd)
            {
                ed.PointMonitor += TrackXline;
                isAdd = true;
            }
            if (!onOff && isAdd)
            {
                ed.PointMonitor -= TrackXline;
                isAdd = false;
            }
            return isAdd;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 下書き線の常時描画更新イベントハンドラ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void TrackXline(object sender, PointMonitorEventArgs e)
        {
            //ドキュメントの作成
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            if (e.Context.History == PointHistoryBits.LastPoint)
            {
                return;
            }

            //スナップ状態ならスナップ点を、そうでないならカーソル点を
            bool snapped = (e.Context.History & PointHistoryBits.ObjectSnapped) > 0;
            Point3d currentPoint = snapped ? e.Context.ObjectSnappedPoint : e.Context.RawPoint;

            //現在の描画空間を判定
            string spaceName = LayoutManager.Current.CurrentLayout == "Model" ? BlockTableRecord.ModelSpace : BlockTableRecord.PaperSpace;

            if (currentPoint != _lastPoint)
            {
                using (Database db = doc.Database)
                using (Transaction tr = db.TransactionManager.StartTransaction())
                using (BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable)
                using (BlockTableRecord btr = tr.GetObject(bt[spaceName], OpenMode.ForWrite) as BlockTableRecord)
                {
                    try
                    {
                        //１回目は下書き線の作成
                        if (_oId.IsNull)
                        {
                            using (Xline xl = new Xline())
                            {
                                Vector3d normal;
                                btr.AppendEntity(xl);
                                switch (dir)
                                {
                                    case Direction.NONE:
                                        xl.BasePoint = _startPoint;
                                        xl.SecondPoint = currentPoint;
                                        break;
                                    case Direction.HORIZONTAL:
                                        xl.BasePoint = currentPoint;
                                        normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis;
                                        xl.SecondPoint = new Point3d(currentPoint.X + normal.X, currentPoint.Y + normal.Y, currentPoint.Z + normal.Z);
                                        break;
                                    case Direction.VERTICAL:
                                        xl.BasePoint = currentPoint;
                                        normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis;
                                        xl.SecondPoint = new Point3d(currentPoint.X + normal.X, currentPoint.Y + normal.Y, currentPoint.Z + normal.Z);
                                        break;
                                }
                                xl.Layer = Layers.SetLayer(Config.LayerName["Construct"]);
                                xl.Linetype = Config.LineType;
                                xl.LinetypeScale = 1d / db.Cannoscale.Scale;
                                _oId = xl.ObjectId;

                                //オブジェクトスナップ回避用のXDataを付与
                                ObjectSnapCtrl.AddNoSnapXdata(xl);

                                tr.AddNewlyCreatedDBObject(xl, true);
                            }
                        }
                        //２回目以降は作成した下書き線の移動
                        else
                        {
                            Xline ent = tr.GetObject(_oId, OpenMode.ForWrite) as Xline;
                            Vector3d normal;
                            switch (dir)
                            {
                                case Direction.NONE:
                                    ent.BasePoint = _startPoint;
                                    ent.SecondPoint = currentPoint;
                                    break;
                                case Direction.HORIZONTAL:
                                    ent.BasePoint = currentPoint;
                                    normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Xaxis;
                                    ent.SecondPoint = new Point3d(currentPoint.X + normal.X, currentPoint.Y + normal.Y, currentPoint.Z + normal.Z);
                                    break;
                                case Direction.VERTICAL:
                                    ent.BasePoint = currentPoint;
                                    normal = ed.CurrentUserCoordinateSystem.CoordinateSystem3d.Yaxis;
                                    ent.SecondPoint = new Point3d(currentPoint.X + normal.X, currentPoint.Y + normal.Y, currentPoint.Z + normal.Z);
                                    break;
                            }
                        }
                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        tr.Abort();
                        ed.WriteMessage("\n" + "DrawLineClass / TrackXline:(Acad) " + ex.Message);
                    }
                    catch (System.Exception ex)
                    {
                        tr.Abort();
                        ed.WriteMessage("\n" + "DrawLineClass / TrackXline:(System) " + ex.Message);
                    }
                }
                _lastPoint = currentPoint;
            }
            return;
        }
    }
}
