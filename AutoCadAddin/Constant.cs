using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;

namespace AutoCadAddin
{
    public enum Direction
    {
        NONE,
        HORIZONTAL,
        VERTICAL
    }

    public static class Constant
    {
        private static string[] holePrefixArray;
        private static string[] shaftPrefixArray;
        private static int[][] holeSurfixArray;
        private static int[][] shaftSurfixArray;
        private static int[] stepSection;
        private static double[][][] holeUpperLimitArray;
        private static double[][][] shaftUpperLimitArray;
        private static double[][][] holeLowerLimitArray;
        private static double[][][] shaftLowerLimitArray;
        private static Dictionary<string, LineWeight> lineWeightArray;
        private static Dictionary<string, short> autocadColorIndex;
        private static Dictionary<string, string> linetypeNameArray;

        private const string defaultLayerName = "0";

        public static string[] HolePrefixArray
        {
            get => holePrefixArray;
            private set => holePrefixArray = value;
        }

        public static string[] ShaftPrefixArray
        {
            get => shaftPrefixArray;
            private set => shaftPrefixArray = value;
        }
        public static int[] StepSection
        {
            get => stepSection;
            private set => stepSection = value;
        }

        public static int[][] HoleSurfixArray
        {
            get => holeSurfixArray;
            private set => holeSurfixArray = value;
        }
        public static int[][] ShaftSurfixArray
        {
            get => shaftSurfixArray;
            private set => shaftSurfixArray = value;
        }
        public static double[][][] HoleUpperLimitArray
        {
            get => holeUpperLimitArray;
            private set => holeUpperLimitArray = value;
        }
        public static double[][][] ShaftUpperLimitArray
        {
            get => shaftUpperLimitArray;
            private set => shaftUpperLimitArray = value;
        }
        public static double[][][] HoleLowerLimitArray
        {
            get => holeLowerLimitArray;
            set => holeLowerLimitArray = value;
        }
        public static double[][][] ShaftLowerLimitArray
        {
            get => shaftLowerLimitArray;
            set => shaftLowerLimitArray = value;
        }
        public static Dictionary<string, LineWeight> LineWeightArray
        {
            get => lineWeightArray;
            private set => lineWeightArray = value;
        }
        public static Dictionary<string, short> AutocadColorIndex
        {
            get => autocadColorIndex;
            private set => autocadColorIndex = value;
        }

        public static string DefaultLayerName => defaultLayerName;

        public static Dictionary<string, string> LinetypeNameArray
        {
            get => linetypeNameArray;
            private set => linetypeNameArray = value;
        }

        static Constant()
        {
            //穴の交差域クラス(接頭辞)
            HolePrefixArray = new string[]
                {
                    "E", "F", "G", "H",
                    "JS", "K", "M", "N",
                };

            //軸の交差域クラス(接頭辞)
            ShaftPrefixArray = new string[]
                {
                    "e", "f", "g", "h",
                    "js", "k", "m", "n"
                };

            //穴の交差域クラス(接尾辞)
            HoleSurfixArray = new int[][]
                {
                    new int[]{ 7, 8, 9 },//E
                    new int[]{ 6, 7, 8 },//F
                    new int[]{ 6, 7 },//G
                    new int[]{ 5, 6, 7, 8, 9, 10, 11 },//H
                    new int[]{ 6,7 },//JS
                    new int[]{ 6,7 },//K
                    new int[]{ 6,7 },//M
                    new int[]{ 6,7 },//N
                };

            //軸の交差域クラス(接尾辞)
            ShaftSurfixArray = new int[][]
                {
                    new int[]{ 7, 8, 9 },//e
                    new int[]{ 6, 7, 8 },//f
                    new int[]{ 5, 6 },//g
                    new int[]{ 5, 6, 7, 8, 9 },//h
                    new int[]{5,6,7},//js
                    new int[]{ 5,6},//k
                    new int[]{ 5,6},//m
                    new int[]{6}//n
                };

            //基準寸法の区分
            StepSection = new int[]
                {
                    3, 6, 10, 14, 18, 24, 30, 40, 50, 65, 80, 100, 120, 140, 160, 180, 200, 225, 250
                };

            //穴の嵌めあい許容差上限
            HoleUpperLimitArray = new double[][][]
                {
                    new double[][]//E
                    {
                        new double[]{24, 32, 40, 50, 50, 61, 61, 75, 75, 90, 90, 107, 107, 125, 125, 125, 146, 146, 146 },//E7
                        new double[]{28, 38, 47, 59, 59, 73, 73, 89, 89, 106, 106, 126, 126, 148, 148, 148, 172, 172, 172 },//E8
                        new double[]{39, 50, 61, 75, 75, 92, 92, 112, 112, 134, 134, 159, 159, 185, 185, 185, 215, 215, 215},//E9
                    },
                    new double[][]//F
                    {
                        new double[]{12, 18, 22, 27, 27, 33, 33, 41, 41, 49, 49, 58, 58, 68, 68, 68, 79, 79, 79},//F6
                        new double[]{16, 22, 28, 34, 34, 41, 41, 50, 50, 60, 60, 71, 71, 83, 83, 83, 96, 96, 96},//F7
                        new double[]{20, 28, 35, 43, 43, 53, 53, 64, 64, 76, 76, 90, 90, 106, 106, 106, 122, 122, 122},//F8

                    },
                    new double[][]//G
                    {
                        new double[]{8, 12, 14, 17, 17, 20, 20, 25, 25, 29, 29, 34, 34, 39, 39, 39, 44, 44, 44},//G6
                        new double[]{12, 16, 20, 24, 24, 28, 28, 34, 34, 40, 40, 47, 47, 54, 54, 54, 61, 61, 61},//G7
                    },
                    new double[][]//H
                    {
                        new double[]{4, 5, 6, 8, 8, 9, 9, 11, 11, 13, 13, 15, 15, 18, 18, 18, 20, 20, 20},//H5
                        new double[]{6, 8, 9, 11, 11, 13, 13, 16, 16, 19, 19, 22, 22, 25, 25, 25, 29, 29, 29},//H6
                        new double[]{10, 12, 15, 18, 18, 21, 21, 25, 25, 30, 30, 35, 35, 40, 40, 40, 46, 46, 46},//H7
                        new double[]{14, 18, 22, 27, 27, 33, 33, 39, 39, 46, 46, 54, 54, 63, 63, 63, 72, 72, 72},//H8
                        new double[]{25, 30, 36, 43, 43, 52, 52, 62, 62, 74, 74, 87, 87, 100, 100, 100, 115, 115, 115},//H9
                        new double[]{40, 48, 58, 70, 70, 84, 84, 100, 100, 120, 120, 140, 140, 160, 160, 160, 185, 185, 185},//H10
                        new double[]{60, 75, 90, 110, 110, 130, 130, 16, 160, 190, 190, 220, 220, 250, 250, 250, 290, 290, 290},//H11
                    },
                    new double[][]//JS
                    {
                        new double[]{3, 4, 4.5, 5.5, 5.5, 6.5, 6.5, 8, 8, 9.5, 9.5, 11, 11, 12.5, 12.5, 12.5, 14.5, 14.5, 14.5},//JS6
                        new double[]{5, 6, 7, 9, 9, 10, 10, 12, 12, 15, 15, 17, 17, 20, 20, 20, 23, 23, 23},//JS7
                    },
                    new double[][]//K
                    {
                        new double[]{0, 2, 2, 2, 2, 2, 2, 3, 3, 4, 4, 4, 4, 4, 4, 4, 5, 5, 5},//K6
                        new double[]{0, 3, 5, 6, 6, 6, 6, 7, 7, 9, 9, 10, 10, 12, 12, 12, 13, 13, 13},//K7
                    },
                    new double[][]//M
                    {
                        new double[]{-2, -1, -3, -4, -4, -4, -4, -4, -4, -5, -5, -6, -6, -8, -8, -8, -8, -8, -8},//M6
                        new double[]{-2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//M7
                    },
                    new double[][]//N
                    {
                        new double[]{-4, -5, -7, -9, -9, -11, -11, -12, -12, -14, -14, -16, -16, -20, -20, -20, -22, -22, -22},//N6
                        new double[]{-4, -4, -4, -5, -5, -7, -7, -8, -8, -9, -9, -10, -10, -12, -12, -12, -14, -14, -14},//N7
                    },
                };

            //軸の嵌めあい許容差上限
            ShaftUpperLimitArray = new double[][][]
                {
                    new double[][]//e
                    {
                        new double[]{-14, -20, -25, -32, -32, -40, -40, -50, -50, -60, -60, -72, -72, -85, -85, -85, -100, -100, -100 },//e7
                        new double[]{-14, -20, -25, -32, -32, -40, -40, -50, -50, -60, -60, -72, -72, -85, -85, -85, -100, -100, -100},//e8
                        new double[]{-14, -20, -25, -32, -32, -40, -40, -50, -50, -60, -60, -72, -72, -85, -85, -85, -100, -100, -100}//e9
                    },
                    new double[][]//f
                    {
                        new double[]{-6, -10, -13, -16, -16, -20, -20, -25, -25, -30, -30, -36, -36, -43, -43, -43, -50, -50, -50},//f6
                        new double[]{-6, -10, -13, -16, -16, -20, -20, -25, -25, -30, -30, -36, -36, -43, -43, -43, -50, -50, -50},//f7
                        new double[]{-6, -10, -13, -16, -16, -20, -20, -25, -25, -30, -30, -36, -36, -43, -43, -43, -50, -50, -50},//f8
                    },
                    new double[][]//g
                    {
                        new double[]{-2, -4, -5, -6, -6, -7, -7, -9, -9, -10, -10, -12, -12, -14, -14, -14, -15, -15, -15},//g5
                        new double[]{-2, -4, -5, -6, -6, -7, -7, -9, -9, -10, -10, -12, -12, -14, -14, -14, -15, -15, -15},//g6
                    },
                    new double[][]//h
                    {
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//h5
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//h6
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//h7
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//h8
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//h9
                    },
                    new double[][]//js
                    {
                        new double[]{2, 2.5, 3, 4, 4, 4.5, 4.5, 5.5, 5.5, 6.5, 6.5, 7.5, 7.5, 9, 9, 9, 10, 10, 10},//js5
                        new double[]{3, 4, 4.5, 5.5, 5.5, 6.5, 6.5, 8, 8, 9.5, 9.5, 11, 11, 12.5, 12.5, 12.5, 14.5, 14.5, 14.5},//js6
                        new double[]{5, 6, 7, 9, 9, 10, 10, 12, 12, 15, 15, 17, 17, 20, 20, 20, 23, 23, 23},//js7
                    },
                    new double[][]//k
                    {
                        new double[]{4, 6, 7, 9, 9, 11, 11, 13, 13, 15, 15, 18, 18, 21, 21, 21, 24, 24, 24},//k5
                        new double[]{6, 9, 10, 12, 12, 15, 15, 18, 18, 21, 21, 25, 25, 28, 28, 28, 33, 33, 33},//k6
                    },
                    new double[][]//m
                    {
                        new double[]{6, 9, 12, 15, 15, 17, 17, 20, 20, 24, 24, 28, 28, 33, 33, 33, 37, 37, 37},//m5
                        new double[]{8, 12, 15, 18, 18, 21, 21, 25, 25, 30, 30, 35, 35, 40, 40, 40, 46, 46, 46},//m6
                    },
                    new double[][]//n
                    {
                        new double[]{10, 16, 19, 23, 23, 28, 28, 33, 33, 39, 39, 45, 45, 52, 52, 52, 60, 60, 60},//n6
                    },
                };

            //穴の嵌めあい許容差下限
            HoleLowerLimitArray = new double[][][]
                {
                    new double[][]//E
                    {
                        new double[]{14, 20, 25, 32, 32, 40, 40, 50, 50, 60, 60, 72, 72, 85, 85, 85, 100, 100, 100},//E7
                        new double[]{14, 20, 25, 32, 32, 40, 40, 50, 50, 60, 60, 72, 72, 85, 85, 85, 100, 100, 100},//E8
                        new double[]{14, 20, 25, 32, 32, 40, 40, 50, 50, 60, 60, 72, 72, 85, 85, 85, 100, 100, 100},//E9

                    },
                    new double[][]//F
                    {
                        new double[]{6, 10, 13, 16, 16, 20, 20, 25, 25, 30, 30, 36, 36, 43, 43, 43, 50, 50, 50},//F6
                        new double[]{6, 10, 13, 16, 16, 20, 20, 25, 25, 30, 30, 36, 36, 43, 43, 43, 50, 50, 50},//F7
                        new double[]{6, 10, 13, 16, 16, 20, 20, 25, 25, 30, 30, 36, 36, 43, 43, 43, 50, 50, 50},//F8
                    },
                    new double[][]//G
                    {
                        new double[]{2, 4, 5, 6, 6, 7, 7, 9, 9, 10, 10, 12, 12, 14, 14, 14, 15, 15, 15},//G6
                        new double[]{2, 4, 5, 6, 6, 7, 7, 9, 9, 10, 10, 12, 12, 14, 14, 14, 15, 15, 15} ,//G7
                    },
                    new double[][]//H
                    {
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H5
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H6
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H7
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H8
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H9
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H10
                        new double[]{0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},//H11
                    },
                    new double[][]//JS
                    {
                        new double[]{-3, -4, -4.5, -5.5, -5.5, -6.5, -6.5, -8, -8, -9.5, -9.5, -11, -11, -12.5, -12.5, -12.5, -14.5, -14.5, -14.5},//JS6
                        new double[]{-5, -6, -7, -9, -9, -10, -10, -12, -12, -15, -15, -17, -17, -20, -20, -20, -23, -23, -23 },//JS7
                    },
                    new double[][]//K
                    {
                        new double[]{-6, -6, -7, -9, -9, -11, -11, -13, -13, -15, -15, -18, -18, -21, -21, -21, -24, -24, -24 },//K6
                        new double[]{-10, -9, -10, -12, -12, -15, -15, -18, -18, -21, -21, -25, -25, -28, -28, -28, -33, -33, -33 },//K7
                    },
                    new double[][]//M
                    {
                        new double[]{-8, -9, -12, -15, -15, -17, -17, -20, -20, -24, -24, -28, -28, -33, -33, -33, -37, -37, -37 },//M6
                        new double[]{-12, -12, -15, -18, -18, -21, -21, -25, -25, -30, -30, -35, -35, -40, -40, -40, -46, -46, -46 },//M7
                    },
                    new double[][]//N
                    {
                        new double[]{-10, -13, -16, -20, -20, -24, -24, -28, -28, -33, -33, -38, -38, -45, -45, -45, -51, -51, -51 },//N6
                        new double[]{-14, -16, -19, -23, -23, -28, -28, -33, -33, -39, -39, -45, -45, -52, -52, -52, -60, -60, -60 },//N7
                    },
                };

            //軸の嵌めあい許容差下限
            ShaftLowerLimitArray = new double[][][]
                {
                    new double[][]//e
                    {
                        new double[]{-24, -32, -40, -50, -50, -61, -61, -75, -75, -90, -90, -107, -107, -125, -125, -125, -146, -146, -146 },//e7
                        new double[]{-28, -38, -47, -59, -59, -73, -73, -89, -89, -106, -106, -126, -126, -148, -148, -148, -172, -172, -172},//e8
                        new double[]{-39, -50, -61, -75, -75, -92, -92, -112, -112, -134, -134, -159, -159, -185, -185, -185, -215, -215, -215},//e9
                    },
                    new double[][]//f
                    {
                        new double[]{-12, -18, -22, -27, -27, -33, -33, -41, -41, -49, -49, -58, -58, -68, -68, -68, -79, -79, -79 },//f6
                        new double[]{-16, -22, -28, -34, -34, -41, -41, -50, -50, -60, -60, -71, -71, -83, -83, -83, -96, -96, -96},//f7
                        new double[]{-20, -28, -35, -43, -43, -53, -53, -64, -64, -76, -76, -90, -90, -106, -106, -106, -122, -122, -122},//f8
                    },
                    new double[][]//g
                    {
                        new double[]{-6, -9, -11, -14, -14, -16, -16, -20, -20, -23, -23, -27, -27, -32, -32, -32, -35, -35, -35 },//g5
                        new double[]{-8, -12, -14, -17, -17, -20, -20, -25, -25, -29, -29, -34, -34, -39, -39, -39, -44, -44, -44},//g6
                    },
                    new double[][]//h
                    {
                        new double[]{-4, -5, -6, -8, -8, -9, -9, -11, -11, -13, -13, -15, -15, -18, -18, -18, -20, -20, -20 },//h5
                        new double[]{-6, -8, -9, -11, -11, -13, -13, -16, -16, -19, -19, -22, -22, -25, -25, -25, -29, -29, -29},//h6
                        new double[]{-10, -12, -15, -18, -18, -21, -21, -25, -25, -30, -30, -35, -35, -40, -40, -40, -46, -46, -46},//h7
                        new double[]{-14, -18, -22, -27, -27, -33, -33, -39, -39, -46, -46, -54, -54, -63, -63, -63, -72, -72, -72},//h8
                        new double[]{-25, -30, -36, -43, -43, -52, -52, -62, -62, -74, -74, -87, -87, -100, -100, -100, -115, -115, -115},//h9
                    },
                    new double[][]//js
                    {
                        new double[]{-2, -2.5, -3, -4, -4, -4.5, -4.5, -5.5, -5.5, -6.5, -6.5, -7.5, -7.5, -9, -9, -9, -10, -10, -10 },//js5
                        new double[]{-3, -4, -4.5, -5.5, -5.5, -6.5, -6.5, -8, -8, -9.5, -9.5, -11, -11, -12.5, -12.5, -12.5, -14.5, -14.5, -14.5},//js6
                        new double[]{-5, -6, -7, -9, -9, -10, -10, -12, -12, -15, -15, -17, -17, -20, -20, -20, -23, -23, -23},//js7
                    },
                    new double[][]//k
                    {
                        new double[]{0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4 },//k5
                        new double[]{0, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4},//k6
                    },
                    new double[][]//m
                    {
                        new double[]{2, 4, 6, 7, 7, 8, 8, 9, 9, 11, 11, 13, 13, 15, 15, 15, 17, 17, 17 },//m5
                        new double[]{2, 4, 6, 7, 7, 8, 8, 9, 9, 11, 11, 13, 13, 15, 15, 15, 17, 17, 17},//m6
                    },
                    new double[][]
                    {
                        new double[]{4, 8, 10, 12, 12, 15, 15, 17, 17, 20, 20, 23, 23, 27, 27, 27, 31, 31, 31 },//'n6
                    },
                };

            LineWeightArray = new Dictionary<string, LineWeight>()
            {
                {"def",LineWeight.ByLineWeightDefault },
                {"000", LineWeight.LineWeight000 },
                {"005", LineWeight.LineWeight005 },
                {"009", LineWeight.LineWeight009 },
                {"013", LineWeight.LineWeight013 },
                {"015", LineWeight.LineWeight015 },
                {"018", LineWeight.LineWeight018 },
                {"020", LineWeight.LineWeight020 },
                {"025", LineWeight.LineWeight025 },
                {"030", LineWeight.LineWeight030 },
                {"035", LineWeight.LineWeight035 },
                {"040", LineWeight.LineWeight040 },
                {"050", LineWeight.LineWeight050 },
                {"053", LineWeight.LineWeight053 },
                {"060", LineWeight.LineWeight060 },
                {"070", LineWeight.LineWeight070 },
                {"080", LineWeight.LineWeight080 },
                {"090", LineWeight.LineWeight090 },
                {"100", LineWeight.LineWeight100 },
                {"106", LineWeight.LineWeight106 },
                {"120", LineWeight.LineWeight120 },
                {"140", LineWeight.LineWeight140 },
                {"158", LineWeight.LineWeight158 },
                {"200", LineWeight.LineWeight200 },
                {"211", LineWeight.LineWeight211 },
            };

            AutocadColorIndex = new Dictionary<string, short>()
            {
                {"Red", 1},
                {"Yellow",2},
                {"Green", 3},
                {"Cyan",4},
                {"Blue", 5},
                {"Magenta",6},
                {"White", 7},
            };

            LinetypeNameArray = new Dictionary<string, string>()
            {
                {"Default","CONTINUOUS"},
                {"Center","CENTER"},
                {"Dash","DASHED"},
            };
        }
    }
}
