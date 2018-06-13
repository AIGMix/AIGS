using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AIGS.Control
{
    public class CListView : ListView
    {
        public CListView()
        {
            /// 双缓冲ListView ，解决闪烁
            /// 使用方法是在ListView 所在窗体的InitializeComponent方法中，更改控件类型实例化语句将
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
    }
}
