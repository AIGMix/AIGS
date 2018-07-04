#region << 版本说明 >>
/****************************************************
 * Creator by  : Yaron (yaronhuang@foxmail.com)
 * Create date : 2018-1-30
 * Modification History :
 * Date           Programmer         Amendment
 * 
*******************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AIGS.Helper
{
    public class ScreenShotHelper
    {
        #region 静态方法

        /// <summary>
        /// 截取显示器屏幕
        /// </summary>
        /// <returns></returns>
        public static Bitmap CutScreen()
        {
            int iPicWidth = Screen.PrimaryScreen.Bounds.Width;
            int iPicHeigth = Screen.PrimaryScreen.Bounds.Height;

            return Cut(new Point(0, 0), new Point(0, 0), iPicWidth, iPicHeigth);
        }


        /// <summary>
        /// 截取显示器屏幕到文件
        /// </summary>
        /// <param name="sPath">图片保存路径</param>
        /// <param name="sFileName">图片文件名</param>
        /// <param name="bAddTimeInName">文件名后是否加时间后缀</param>
        /// <returns></returns>
        public static bool CutScreenToFile(string sPath = null, string sFileName = null, bool bAddTimeInName = false)
        {
            int iPicWidth = Screen.PrimaryScreen.Bounds.Width;
            int iPicHeigth = Screen.PrimaryScreen.Bounds.Height;

            string sPicPath = GetFilePath(sPath, sFileName, bAddTimeInName);
            return CutToFile(new Point(0, 0), new Point(0, 0), iPicWidth, iPicHeigth, sPicPath);
        }


        /// <summary>
        /// 截取窗体屏幕
        /// </summary>
        /// <param name="aThis">窗口句柄</param>
        /// <returns></returns>
        public static Bitmap CutForm(Form aThis)
        {
            int iPicWidth = aThis.Width;
            int iPicHeigth = aThis.Height;

            return Cut(aThis.Location, new Point(0, 0), iPicWidth, iPicHeigth);
        }

        /// <summary>
        /// 截取窗体屏幕到文件
        /// </summary>
        /// <param name="aThis">窗口句柄</param>
        /// <param name="sPath">图片保存路径</param>
        /// <param name="sFileName">图片文件名</param>
        /// <param name="bAddTimeInName">文件名后是否加时间后缀</param>
        /// <returns></returns>
        public static bool CutFormToFile(Form aThis, string sPath = null, string sFileName = null, bool bAddTimeInName = false)
        {
            int iPicWidth = aThis.Width;
            int iPicHeigth = aThis.Height;

            string sPicPath = GetFilePath(sPath, sFileName, bAddTimeInName);
            return CutToFile(aThis.Location, new Point(0, 0), iPicWidth, iPicHeigth, sPicPath);
        }
        #endregion

        #region 工具
        /// <summary>
        /// 获取文件路径
        /// </summary>
        /// <param name="sPath">路径</param>
        /// <param name="sFileName">文件名</param>
        /// <param name="bAddTimeInName">文件名后面是否添加时间</param>
        /// <returns></returns>
        private static string GetFilePath(string sPath, string sFileName, bool bAddTimeInName)
        {
            //如果没有输出路径和文件名，则以当前程序的路径和名称为准
            sPath       = String.IsNullOrWhiteSpace(sPath)      ? AIGS.Helper.SystemHelper.GetExeDirectoryName() : sPath;
            sFileName   = String.IsNullOrWhiteSpace(sFileName)  ? AIGS.Helper.SystemHelper.GetExeName() : sFileName;

            if (bAddTimeInName)
            {
                string sName = Path.GetFileNameWithoutExtension(sFileName);
                string sExtension = Path.GetExtension(sFileName);
                sName += "[" + DateTime.Now.ToString("yyyy-MM-dd HH点mm分ss秒") + "]";

                sFileName = sName + sExtension;
            }

            return sPath + "\\" + sFileName;
        }

        /// <summary>
        /// 截图到文件
        /// </summary>
        /// <param name="upperLeftSource">屏幕左上角</param>
        /// <param name="upperLeftDestination">图片左上角的点</param>
        /// <param name="blockRegionWidth">截图的宽度</param>
        /// <param name="blockRegionHight">截图的高度</param>
        /// <param name="savePath">文件保存路径</param>
        /// <returns></returns>
        private static bool CutToFile(Point upperLeftSource,
            Point upperLeftDestination,
            int blockRegionWidth,
            int blockRegionHight,
            string savePath)
        {
            try
            {
                Bitmap aBitmap = Cut(upperLeftSource, upperLeftDestination, blockRegionWidth, blockRegionHight);
                aBitmap.Save(savePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="upperLeftSource">屏幕左上角</param>
        /// <param name="upperLeftDestination">图片左上角的点</param>
        /// <param name="blockRegionWidth">截图的宽度</param>
        /// <param name="blockRegionHight">截图的高度</param>
        /// <returns></returns>
        private static Bitmap Cut(Point upperLeftSource,
            Point upperLeftDestination,
            int blockRegionWidth,
            int blockRegionHight)
        {
            Bitmap aBitmap = new Bitmap(blockRegionWidth, blockRegionHight);
            Graphics aGraphics = Graphics.FromImage(aBitmap);

            aGraphics.CopyFromScreen(upperLeftSource, upperLeftDestination, aBitmap.Size);
            aGraphics.Dispose();
            return aBitmap;
        }
        #endregion

        #region << 截图功能说明(类似QQ截图) >>
        /****************************************************
         * 使用说明： ScreenShotHelper aObject = new ScreenShotHelper(false, Func_CutPic);
         *          aObject.StartCut();
         *          
         * 获取截图： 定义回调接口void Func_CutPic(Bitmap aReturnpic);        
         * 附加说明： 如果没有设置回调接口，则在点击“确定”后会弹出保存图片的窗口
         *          可以调用GetCutPic()获取图片
         * 
         * 操作说明： 左键-框选、长按移动截图框
         *          右键-取消框选、退出
        *******************************************************/
        #endregion

        #region 参数
        /// <summary>
        /// 窗体主句柄和按钮
        /// </summary>
        Form m_CutForm = new Form();
        Button m_ButOK = new Button();
        Button m_ButCancle = new Button();

        /// <summary>
        /// 用来记录鼠标按下的坐标，用来确定绘图起点
        /// </summary>
        private Point m_StartPoint;

        /// <summary>
        /// 截图的矩形框
        /// </summary>
        Rectangle m_Rectangle;

        /// <summary>
        /// 截图开始
        /// </summary>
        private bool m_bCutStart = false;

        /// <summary>
        /// 按键响应事件定义
        /// </summary>
        public delegate void CallbackFuncCut(Bitmap aBitmap);

        /// <summary>
        /// 确定按钮响应
        /// </summary>
        CallbackFuncCut m_FuncOk;

        /// <summary>
        /// 点击确定时是否保存
        /// </summary>
        bool m_bIsSaveOnOkEvent;

        /// <summary>
        /// 图片
        /// </summary>
        Bitmap m_RetrunBitmap = null;
        #endregion

        #region 区域截图
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="bIsSaveOnOkEvent">点击确定时是否保存</param>
        /// <param name="aFunc">截图回调</param>
        public ScreenShotHelper(bool bIsSaveOnOkEvent = true, CallbackFuncCut aFunc = null)
        {
            //获取屏幕图片
            Bitmap aBitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics aGraphics = Graphics.FromImage(aBitmap);
            aGraphics.CopyFromScreen(new Point(0, 0), new Point(0, 0), aBitmap.Size);

            //设置窗口的样式
            m_CutForm.FormBorderStyle = FormBorderStyle.None;
            m_CutForm.WindowState = FormWindowState.Maximized;

            // 改变鼠标样式和窗口背景
            m_CutForm.BackgroundImage = aBitmap;
            m_CutForm.Cursor = Cursors.Default;

            //设置窗口的响应
            m_CutForm.MouseDown += Event_MouseDown;
            m_CutForm.MouseUp   += Event_MouseUp;
            m_CutForm.MouseMove += Event_MouseMove;

            //设置按钮
            InitCutBut();
            m_CutForm.Controls.Add(m_ButOK);
            m_CutForm.Controls.Add(m_ButCancle);

            //确定按钮响应
            m_FuncOk = aFunc;
            m_bIsSaveOnOkEvent = bIsSaveOnOkEvent;
        }

        /// <summary>
        /// 开始截图
        /// </summary>
        public void StartCut()
        {
            m_CutForm.ShowDialog();
        }

        /// <summary>
        /// 获取截图
        /// </summary>
        public Bitmap GetCutPic()
        {
            return m_RetrunBitmap;
        }

        #endregion

        #region 内部鼠标事件
        private void Event_MouseDown(object sender, MouseEventArgs e)
        {
            // 鼠标左键按下是开始画图，也就是截图
            if (e.Button == MouseButtons.Left)
            {
                m_bCutStart = true;
                m_StartPoint = new Point(e.X, e.Y);
                //设置按钮
                SetCutButVisible(false);
            }
        }

        private void Event_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_bCutStart = false;
                //设置按钮
                if (e.X != m_StartPoint.X || e.Y != m_StartPoint.Y)
                {
                    SetCutButVisible(true);
                    SetCutButPosition(e.X, e.Y);
                }
            }
        }

        private void Event_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_bCutStart)
                return;

            // 新建画板和画笔
            Bitmap aFormBmp = (Bitmap)m_CutForm.BackgroundImage.Clone();
            Graphics aGraphics = Graphics.FromImage(aFormBmp);
            Pen aPen = new Pen(Color.OrangeRed, 1);
            aPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            //画截图的矩形框到画板上（也就是画到BMP图片上）
            m_Rectangle = GetRectangle(m_StartPoint.X, m_StartPoint.Y, e.X, e.Y);
            aGraphics.DrawRectangle(aPen, m_Rectangle);

            // 释放目前的画板
            aGraphics.Dispose();
            aPen.Dispose();

            // 将刚才所画的图片画到截图窗体上
            aGraphics = m_CutForm.CreateGraphics();
            aGraphics.DrawImage(aFormBmp, new Point(0, 0));
            aGraphics.Dispose();

            // 释放拷贝图片，防止内存被大量消耗
            aFormBmp.Dispose();
        }

        /// <summary>
        /// 获取矩形
        /// </summary>
        /// <param name="iX1"></param>
        /// <param name="iY1"></param>
        /// <param name="iX2"></param>
        /// <param name="iY2"></param>
        /// <returns></returns>
        private static Rectangle GetRectangle(int iX1, int iY1, int iX2, int iY2)
        {
            int iWidth = Math.Abs(iX1 - iX2);
            int iHeight = Math.Abs(iY1 - iY2);

            int iStartX = iX1 < iX2 ? iX1 : iX2;
            int iStartY = iY1 < iY2 ? iY1 : iY2;
            return new Rectangle(iStartX, iStartY, iWidth, iHeight);
        }


        #endregion

        #region 按钮

        /// <summary>
        /// 初始化按钮
        /// </summary>
        private void InitCutBut()
        {
            //设置按钮
            m_ButOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            m_ButOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            m_ButOK.Location = new System.Drawing.Point(560, 112);
            m_ButOK.Name = "m_ButOK";
            m_ButOK.Size = new System.Drawing.Size(75, 23);
            m_ButOK.Text = "确定";
            m_ButOK.BackColor = Color.LightSkyBlue;
            m_ButOK.UseVisualStyleBackColor = true;
            m_ButOK.Click += new System.EventHandler(Event_ButOkClick);

            m_ButCancle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            m_ButCancle.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            m_ButCancle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            m_ButCancle.Location = new System.Drawing.Point(641, 112);
            m_ButCancle.Name = "m_ButCancle";
            m_ButCancle.Size = new System.Drawing.Size(75, 23);
            m_ButCancle.Text = "取消";
            m_ButCancle.BackColor = Color.LightSkyBlue;
            m_ButCancle.UseVisualStyleBackColor = true;

        }

        /// <summary>
        /// 设置按钮的可见
        /// </summary>
        /// <param name="bFlag"></param>
        private void SetCutButVisible(bool bFlag)
        {
            m_ButOK.Visible = bFlag;
            m_ButCancle.Visible = bFlag;
        }

        /// <summary>
        /// 设置按钮的位置（右下角的点）
        /// </summary>
        /// <param name="iEndX"></param>
        /// <param name="iEndY"></param>
        private void SetCutButPosition(int iEndX, int iEndY)
        {
            //按钮排列：         确定 取消
            //按钮长宽：         75  23
            //按钮间隔：         6
            //按钮与矩形间隔：    2
            //规则：            需要保证两个按钮在屏幕内
            //两个按钮组成的长宽： W = 75+75+6 = 156 ;  H = 23

            int iMaxX = Screen.PrimaryScreen.Bounds.Width;
            int iMaxY = Screen.PrimaryScreen.Bounds.Height;

            int iButX = iEndX - 156;
            int iButY = iEndY + 2;

            //查看Y轴是否超过,超过则放到反向（框里面）
            if (iButY + 23 > iMaxY)
                iButY = iButY - 2 - 23;

            //查看X轴是否少于0,如果是则置为0
            if (iButX < 0)
                iButX = 0;

            int XOK = iButX;
            int YOK = iButY;
            m_ButOK.Location = new System.Drawing.Point(XOK, YOK);

            int XCancle = iButX + 75 + 2;
            int YCancle = iButY;
            m_ButCancle.Location = new System.Drawing.Point(XCancle, YCancle);
        }

        /// <summary>
        /// 按钮“确定”响应
        /// </summary>
        private void Event_ButOkClick(object sender, EventArgs e)
        {
            try
            {
                m_RetrunBitmap = Cut(new Point(m_Rectangle.X, m_Rectangle.Y), new Point(0, 0), m_Rectangle.Width, m_Rectangle.Height);
                if (m_FuncOk == null && m_bIsSaveOnOkEvent)
                {
                    //如果响应函数没有设置，则弹出保存窗口
                    SaveFileDialog aDlg = new SaveFileDialog();
                    aDlg.Title = "保存截图";
                    aDlg.Filter = "JPEG|*.jpg|BMP|*.bmp";

                    if (aDlg.ShowDialog() == DialogResult.OK)
                        m_RetrunBitmap.Save(aDlg.FileName);
                }

                if (m_FuncOk != null)
                    m_FuncOk(m_RetrunBitmap);

                m_CutForm.Close();
            }
            catch
            {

            }
        }
        #endregion
    }
}
