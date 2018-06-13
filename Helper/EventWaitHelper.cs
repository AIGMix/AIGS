using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AIGS.Helper
{
    public class EventWaitHelper
    {
        #region 参数
        /// <summary>
        /// 等待句柄
        /// </summary>
        EventWaitHandle m_WatiHandle;

        /// <summary>
        /// 需要唤醒的次数
        /// </summary>
        int m_NeedAwakeSum;

        /// <summary>
        /// 已经唤醒的次数
        /// </summary>
        int m_AlreadyAwakeCount;

        /// <summary>
        /// 上锁
        /// </summary>
        object m_Lock = new object();
        #endregion


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bIsFirstWait">是否第一次就等待</param>
        /// <param name="iNeedAwakeSum">等待后需要唤醒的次数</param>
        public EventWaitHelper(bool bIsFirstWait = false, int iNeedAwakeSum = 1)
        {
            m_NeedAwakeSum = iNeedAwakeSum;
            m_WatiHandle = new AutoResetEvent(!bIsFirstWait);
        }

        /// <summary>
        /// 设置需要唤醒的次数
        /// </summary>
        /// <param name="iNeedAwakeSum"></param>
        public void SetNeedAwakeSum(int iNeedAwakeSum)
        {
            ResetAwakeCount();

            lock (m_Lock)
            {
                m_NeedAwakeSum = iNeedAwakeSum;
            }
        }

        /// <summary>
        /// 重置唤醒计数
        /// </summary>
        public void ResetAwakeCount()
        {
            lock (m_Lock)
            {
                m_AlreadyAwakeCount = 0;
            }
        }

        /// <summary>
        /// 等待唤醒
        /// </summary>
        public bool WaitOne(int iMillisecondsTimeout = -1)
        {
            lock (m_Lock)
            {
                if (m_NeedAwakeSum <= 0)
                    return true;
            }
            return m_WatiHandle.WaitOne(iMillisecondsTimeout);
        }

        /// <summary>
        /// 唤醒
        /// </summary>
        /// <returns></returns>
        public bool Set()
        {
            lock (m_Lock)
            {
                if (++m_AlreadyAwakeCount >= m_NeedAwakeSum)
                    return m_WatiHandle.Set();
            }
            return false;
        }

        /// <summary>
        /// 获取需要唤醒的次数
        /// </summary>
        public int GetNeedAwakeCount()
        {
            lock (m_Lock)
            {
                return m_NeedAwakeSum - m_AlreadyAwakeCount;
            }
        }
    }
}
