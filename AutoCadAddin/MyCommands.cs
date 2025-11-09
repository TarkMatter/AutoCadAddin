using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCadAddin
{
    public class MyCommands
    {
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 中心線の作成
        /// </summary>
        [CommandMethod("CL", CommandFlags.Modal | CommandFlags.Redraw)]
        public void CL()
        {
            DrawLine.DrawCenterLine();
            return;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 中心線の作成2
        /// </summary>
        [CommandMethod("CE", CommandFlags.Modal | CommandFlags.Redraw)]
        public void CE()
        {
            DrawLine.DrawCenterLine2();
            return;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 円の中心線の作成
        /// </summary>
        [CommandMethod("CCE", CommandFlags.Modal | CommandFlags.Redraw)]
        public void CCE()
        {
            DrawLine.DrawCircleCenterLine();
            return;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 下書き線の作成
        /// </summary>
        [CommandMethod("XL", CommandFlags.Modal | CommandFlags.Redraw)]
        public void XL()
        {
            DrawLine.DrawXline(Direction.NONE);
            return;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 下書き線(水平)の作成
        /// </summary>
        [CommandMethod("XH", CommandFlags.Modal | CommandFlags.Redraw)]
        public void XH()
        {
            DrawLine.DrawXline(Direction.HORIZONTAL);
            return;
        }
        //----------------------------------------------------------------------------------------------------
        /// <summary>
        /// 下書き線(垂直)の作成
        /// </summary>
        [CommandMethod("XV", CommandFlags.Modal | CommandFlags.Redraw)]
        public void XV()
        {
            DrawLine.DrawXline(Direction.VERTICAL);
            return;
        }
    }
}
