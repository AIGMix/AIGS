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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AIGS.Common
{
    public class Message
    {
        /// <summary>
        /// 阻塞发送
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int Send(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int Send(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int Send(IntPtr hWnd, int Msg, int wParam, string lParam);


        /// <summary>
        /// 异步发送
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool Post(IntPtr hWnd, int Msg, int wParam, ref IntPtr lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool Post(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool Post(IntPtr hWnd, int Msg, int wParam, string lParam);

        #region 样例
        /// <summary>
        /// 消息定义样例
        /// </summary>
        private const int USER = 0x0500;
        public const int WM_Common = USER + 1;   


        /// <summary>
        /// 消息响应样例
        /// </summary>
        //protected override void DefWndProc(ref System.Windows.Forms.Message m)   


        /// <summary>
        /// 参数传递转换-指针转string
        /// </summary>
        //string ss = Marshal.PtrToStringAnsi(m.LParam);
        #endregion
    }
}
