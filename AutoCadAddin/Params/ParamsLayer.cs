using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoCadAddin.Tables;

namespace AutoCadAddin.Params
{
    public class ParamsLayer
    {
        private string _name;
        private short _colorIndex;
        private ObjectId _lineTypeId;
        private LineWeight _lineWeight;
        private bool _isPlotable;

        public ParamsLayer(string name, short idx, string lineTypeName, string weight, bool isPlott)
        {
            Name = name;
            ColorIndex = idx;
            LineTypeId = LineTypes.GetLineTypeId(lineTypeName);
            LineWeight = Constant.LineWeightArray[weight];
            IsPlotable = isPlott;
        }

        public string Name
        {
            get => _name;
            private set => _name = value;
        }

        public short ColorIndex
        {
            get => _colorIndex;
            private set => _colorIndex = value;
        }

        public ObjectId LineTypeId
        {
            get => _lineTypeId;
            private set => _lineTypeId = value;
        }

        public LineWeight LineWeight
        {
            get => _lineWeight;
            private set => _lineWeight = value;
        }

        public bool IsPlotable
        {
            get => _isPlotable;
            private set => _isPlotable = value;
        }
    }
}
