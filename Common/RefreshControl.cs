using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace AIGS.Common
{
    /// <summary>
    /// 线程中刷新主线程的控件
    /// </summary>
    public class RefreshControl
    {
        #region 通过UI线程的SynchronizationContext的Post/Send方法更新
        /// <summary>
        /// 句柄
        /// </summary>
        private SynchronizationContext m_SyncContext;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RefreshControl()
        {
            //获取主进程的上下文，以便可以刷新主线程中的控件
            m_SyncContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// 阻塞刷新控件
        /// </summary>
        /// <param name="pFunc">刷控件响应函数</param>
        /// <param name="data">传参</param>
        public void Send(SendOrPostCallback pFunc, object data = null)
        {
            m_SyncContext.Send(pFunc, data);
        }

        /// <summary>
        /// 异步刷新控件
        /// </summary>
        /// <param name="pFunc"></param>
        /// <param name="data"></param>
        public void Post(SendOrPostCallback pFunc, object data = null)
        {
            m_SyncContext.Post(pFunc, data);
        }

        #endregion
    }
}
