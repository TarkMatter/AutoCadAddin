using AutoCadAddin.Params;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadAddin.Tables
{
    public static class Config
    {
        public struct CaptureSize
        {
            double _width;
            double _height;

            public CaptureSize(double w, double h)
            {
                _width = w;
                _height = h;
            }
            public double Width { get => _width; private set => _width = value; }
            public double Height { get => _height; private set => _height = value; }
        }

        public static CaptureSize captureSize = new CaptureSize(1920, 1080);

        //----------------------------------------------------------------------------------------------------

        const string CONFIG_FILE_NAME = "settings.config";

        private static bool _isOutputLog = false;
        private static bool _isInitLayer = true;

        private static bool _firstAwake = true;
        private static bool _addEventHandler = false;

        private static string _lineType = "ByLayer";

        private static Dictionary<string, string> _layerName = new Dictionary<string, string>()
        {
            {"Base",Constant.DefaultLayerName },
            {"Center","CEN" },
            {"Thin","THN" },
            {"Dim","DIM" },
            {"Dtext","DIMTEXT" },
            {"Construct","INFO" },
        };

        private static List<ParamsLayer> _defaultLayersInfo = new List<ParamsLayer>
            {
                new ParamsLayer("0",Constant.AutocadColorIndex["Green"],Constant.LinetypeNameArray["Default"],"040",true),
                new ParamsLayer("1",Constant.AutocadColorIndex["Yellow"],Constant.LinetypeNameArray["Default"],"030",true),
                new ParamsLayer("CEN",Constant.AutocadColorIndex["Cyan"],Constant.LinetypeNameArray["Center"],"013",true),
                new ParamsLayer("Defpoints",Constant.AutocadColorIndex["White"],Constant.LinetypeNameArray["Default"],"def",false),
                new ParamsLayer("DIM",Constant.AutocadColorIndex["Cyan"],Constant.LinetypeNameArray["Default"],"013",true),
                new ParamsLayer("DIMTEXT",Constant.AutocadColorIndex["White"],Constant.LinetypeNameArray["Default"],"013",true),
                new ParamsLayer("DOT",151,Constant.LinetypeNameArray["Default"],"013",true),
                new ParamsLayer("DUMMY",133,Constant.LinetypeNameArray["Default"],"013",false),
                new ParamsLayer("HATCH",Constant.AutocadColorIndex["Magenta"],Constant.LinetypeNameArray["Default"],"013",true),
                new ParamsLayer("HID",Constant.AutocadColorIndex["Yellow"],Constant.LinetypeNameArray["Dash"],"013",true),
                new ParamsLayer("HOJO",Constant.AutocadColorIndex["Blue"],Constant.LinetypeNameArray["Dash"],"013",false),
                new ParamsLayer("INFO",11,Constant.LinetypeNameArray["Default"],"013",false),
                new ParamsLayer("NAME",Constant.AutocadColorIndex["Yellow"],Constant.LinetypeNameArray["Default"],"013",false),
                new ParamsLayer("THN",Constant.AutocadColorIndex["Cyan"],Constant.LinetypeNameArray["Default"],"013",true),
                new ParamsLayer("WAKU",Constant.AutocadColorIndex["Green"],Constant.LinetypeNameArray["Default"],"030",true),
            };

        private static Dictionary<string, string> _textStyles = new Dictionary<string, string>()
        {
            {"Default" ,"STANDARD"},
            {"JI2","JI2"},
        };

        private static Dictionary<string, double> _values = new Dictionary<string, double>()
        {
            {"CenterLineOvershoot",3.0d},//中心線のオーバーシュート量
            {"DimInterval1st",11d },//直線寸法線同士の間隔(１段目)
            {"DimInterval",10d },//直線寸法同士の間隔(２段目以降)
            {"DimArrowSize",2.5d },//寸法線の矢印サイズ
            {"DimExtensionLineExtend",1d},//寸法補助線延長長さ
            {"DimFontUnderGap",0.8d},//寸法値文字オフセット
            {"DecimalDigit",2.0d },//寸法値の小数点桁数(int)
            {"DimGuideLineAmount",7.0d },//寸法線用のガイドライン本数(int)
        };

        private static List<string> _scaleRatioArray = new List<string>()
        {
            "1:1","1:1.5","1:2","1:4","1:5","1:8","1:10","1:20","1:30","1:40","1:50","1:100",
            "1.5:1","2:1","4:1","8:1","10:1","100:1",
            "1:1.25","1:2.5","1:3","1.25:1","2.5:1","3:1",
        };

        //----------------------------------------------------------------------------------------------------

        public static bool IsOutputLog
        {
            get => _isOutputLog;
            private set => _isOutputLog = value;
        }
        public static bool IsInitLayer
        {
            get => _isInitLayer;
            private set => _isInitLayer = value;
        }

        public static string LineType
        {
            get => _lineType;
            private set => _lineType = value;
        }

        public static Dictionary<string, string> LayerName
        {
            get => _layerName;
            private set => _layerName = value;
        }

        public static Dictionary<string, string> TextStyles
        {
            get => _textStyles;
            private set => _textStyles = value;
        }

        public static Dictionary<string, double> Values
        {
            get => _values;
            private set => _values = value;
        }
        internal static List<ParamsLayer> DefaultLayersInfo
        {
            get => _defaultLayersInfo;
            private set => _defaultLayersInfo = value;
        }
        public static bool FirstAwake
        {
            get => _firstAwake;
            private set => _firstAwake = value;
        }
        public static List<string> ScaleRatioArray
        {
            get => _scaleRatioArray;
            private set => _scaleRatioArray = value;
        }

        public static void FirstAwakeDone()
        {
            FirstAwake = false;
        }

        //----------------------------------------------------------------------------------------------------
        public static void SetValues(string key, double val)
        {
            if (Values.ContainsKey(key))
            {
                Values[key] = val;
            }
            return;
        }

        public static string GetConfigFileFullPath()
        {
            return Directory.GetParent(typeof(Config).Assembly.Location).FullName + @"\" + CONFIG_FILE_NAME;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// セッティングファイルのデータ読込～設定値の更新
        /// </summary>
        public static void InitializeConfig()
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            List<string> errors = new List<string>();
            bool _isFirstLayerInfo = true;

            string fullPath = GetConfigFileFullPath();
            if (!System.IO.File.Exists(fullPath))
            {
                ed.WriteMessage("\n" + fullPath + "は存在しません");
            }
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(fullPath, Encoding.UTF8);

                while (sr.Peek() != -1)
                {
                    string line = sr.ReadLine();
                    if (line.Length < 1 || line[0] == '*')
                    {
                        continue;
                    }

                    string[] words = line.Split(',');

                    double dNum;
                    short sCheck;
                    bool toggle;
                    switch (words[0])
                    {
                        case "Log":
                            if (bool.TryParse(words[1], out toggle))
                            {
                                IsOutputLog = toggle;
                            }
                            else
                            {
                                errors.Add(words[0] + "を設定できませんでした");
                            }
                            break;

                        case "InitLayer":
                            if (bool.TryParse(words[1], out toggle))
                            {
                                IsInitLayer = toggle;
                            }
                            else
                            {
                                errors.Add(words[0] + "を設定できませんでした");
                            }
                            break;

                        case "Layer":
                            if (LayerName.ContainsKey(words[1]))
                            {
                                LayerName[words[1]] = words[2];
                            }
                            else
                            {
                                errors.Add(words[1] + "のレイヤー名を設定できませんでした");
                            }
                            break;

                        case "LayerInfo":
                            short idx;
                            if (_isFirstLayerInfo)
                            {
                                DefaultLayersInfo.Clear();
                                _isFirstLayerInfo = false;
                            }

                            if (short.TryParse(words[2], out sCheck))
                            {
                                idx = sCheck;
                            }
                            else if (Constant.AutocadColorIndex.ContainsKey(words[2]))
                            {
                                idx = Constant.AutocadColorIndex[words[2]];
                            }
                            else
                            {
                                errors.Add(words[1] + "画層の設定をできませんでした");
                                break;
                            }

                            if (!Constant.LinetypeNameArray.ContainsKey(words[3]))
                            {
                                errors.Add(words[1] + "画層の設定をできませんでした");
                                break;
                            }

                            if (!Constant.LineWeightArray.ContainsKey(words[4]))
                            {
                                errors.Add(words[1] + "画層の設定をできませんでした");
                                break;
                            }

                            if (!bool.TryParse(words[5], out toggle))
                            {
                                errors.Add(words[1] + "画層の設定をできませんでした");
                                break;
                            }

                            DefaultLayersInfo.Add(
                                new ParamsLayer(
                                    words[1],
                                    idx,
                                    Constant.LinetypeNameArray[words[3]],
                                    words[4],
                                    bool.Parse(words[5]))
                                );
                            break;

                        case "LineType":
                            LineType = words[1];
                            break;

                        case "Values":
                            if (LayerName.ContainsKey(words[1]))
                            {
                                if (double.TryParse(words[2], out dNum))
                                {
                                    Values[words[1]] = dNum;
                                }
                                else
                                {
                                    errors.Add(words[1] + "の値を設定できませんでした");
                                }
                            }
                            break;
                        default:
                            errors.Add(words[1] + "は存在しません");
                            break;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                ed.WriteMessage("\n" + "\n" + ex.Message + "\n" + "ファイルが使用中です");
                return;
            }
            finally
            {
                sr?.Close();
            }

            if (errors.Count > 0 && IsOutputLog)
            {
                foreach (var error in errors)
                {
                    ed.WriteMessage("\n" + error.ToString());
                }
                return;
            }

            ed.WriteMessage("\n設定可能値は全て設定されました");
            return;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 画層のデフォルト値を作成
        /// </summary>
        public static void InitSettingLayers()
        {
            if (!IsInitLayer)
            {
                return;
            }

            using (Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument)
            {
                Editor ed = doc.Editor;
                try
                {
                    foreach (ParamsLayer layer in DefaultLayersInfo)
                    {
                        Layers.SettingLayer(layer);
                    }

                    ed.WriteMessage("\n" + "画層の追加・再設定を実行しました");

                    return;
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ed.WriteMessage(ex.Message);
                }

            }
            return;
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// ファイルの新規作成および既存ファイル開封時のイベント登録用メソッド
        /// </summary>
        /// <param name="senderObj"></param>
        /// <param name="docColDocActEvtArgs"></param>
        public static void OpenFileInitialize(object senderObj, DocumentCollectionEventArgs docColDocActEvtArgs)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }
            doc.CommandWillStart -= new CommandEventHandler(SettingByDwgFile);
            doc.CommandWillStart += new CommandEventHandler(SettingByDwgFile);
        }

        public static void SettingByDwgFile(object senderObj, CommandEventArgs docColDocActEvtArgs)
        {
            Config.InitializeConfig();
            InitSettingLayers();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }
            doc.CommandWillStart -= new CommandEventHandler(SettingByDwgFile);
            doc.CommandEnded -= new CommandEventHandler(SettingByDwgFile);
        }

        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// ファイルの新規作成および既存ファイル開封時のイベント登録
        /// </summary>
        public static void FileOpenHandler()
        {
            if (!_addEventHandler)
            {
                _addEventHandler = true;

                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.DocumentCreated -= new DocumentCollectionEventHandler(OpenFileInitialize);
                Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.DocumentCreated += new DocumentCollectionEventHandler(OpenFileInitialize);

                Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                doc.CommandWillStart -= new CommandEventHandler(SettingByDwgFile);
                doc.CommandWillStart += new CommandEventHandler(SettingByDwgFile);
            }
        }
    }
}
