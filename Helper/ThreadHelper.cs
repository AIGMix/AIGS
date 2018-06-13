using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AIGS.Helper
{
    public class ThreadHelper
    {
        #region 静态接口
        /// <summary>
        /// 启动一条线程
        /// </summary>
        /// <param name="ThreadFunc">线程回调</param>
        /// <param name="data">线程传参</param>
        /// <returns></returns>
        public static Thread Start(EventFunc ThreadFunc, object data = null)
        {
            Thread aNewThread = new Thread(new ParameterizedThreadStart(ThreadFunc));
            aNewThread.Start(data);
            
            return aNewThread;
        }

        #endregion


        #region 动态接口

        /// <summary>
        /// 线程链表
        /// </summary>
        private List<Thread> m_ThreadArrary;

        /// <summary>
        /// 信号量-用于等待
        /// </summary>
        private Semaphore m_ThreadSem;

        /// <summary>
        /// 自定义线程处理函数
        /// </summary>
        /// <param name="data"></param>
        public delegate void EventFunc(object data);

        /// <summary>
        /// 线程处理前后的操作
        /// </summary>
        /// <param name="data"></param>
        private void Thread_Func(object data)
        {
            KeyValuePair<EventFunc, object> aRecord = (KeyValuePair<EventFunc, object>)data;
            EventFunc aFunc = aRecord.Key;
            object aFuncPara = aRecord.Value;

            if (aFunc != null)
                aFunc(aFuncPara);

            m_ThreadSem.Release();
        }

        
        /// <summary>
        /// 集合初始化
        /// </summary>
        /// <param name="iTreadNum">集合线程数量</param>
        public ThreadHelper(int iTreadMaxNum = 5)
        {
            //初始化线程链表
            m_ThreadArrary  = new List<Thread>();
            for (int i = 0; i < iTreadMaxNum; i++)
                m_ThreadArrary.Add(null);

            //初始化信号量
            m_ThreadSem = new Semaphore(iTreadMaxNum, iTreadMaxNum);
        }


        /// <summary>
        /// 查看集合中是否有空闲线程
        /// </summary>
        /// <returns></returns>
        public bool HaveFree()
        {
            return GetFreeIndex() >= 0 ? true: false;
        }
        

        /// <summary>
        /// 开始处理
        /// </summary>
        /// <param name="ThreadFunc">线程回调</param>
        /// <param name="data">线程传参</param>
        /// <returns></returns>
        public bool ThreadStart(EventFunc ThreadFunc, object data = null)
        {
            int iIndex = GetFreeIndex();
            if (iIndex < 0)
                return false;

            //启动线程
            KeyValuePair<EventFunc, object> aRecord = new KeyValuePair<EventFunc, object>(ThreadFunc, data);
            m_ThreadArrary[iIndex] = Start(Thread_Func, aRecord);
            return true;
        }
        
        /// <summary>
        /// 阻塞启动
        /// </summary>
        /// <param name="ThreadFunc"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ThreadStartWait(EventFunc ThreadFunc, object data = null, int iMillisecondsTimeout = 0)
        {
            //等待
            bool bFlag;
            if (iMillisecondsTimeout > 0)
                bFlag = m_ThreadSem.WaitOne(iMillisecondsTimeout);
            else
                bFlag = m_ThreadSem.WaitOne();

            //获取空闲的线程序号
            int iIndex = GetFreeIndex();
            
            //为了保险起见，如果没有获得，则在此循环3次
            int iRetryCount = 3;
            while (iIndex < 0 && iRetryCount-- > 0)
            {
                Thread.Sleep(100);
                iIndex = GetFreeIndex();
            }

            //如果没有获取到则返回失败
            if (iIndex < 0)
                return false;

            //启动线程
            KeyValuePair<EventFunc, object> aRecord = new KeyValuePair<EventFunc, object>(ThreadFunc, data);
            m_ThreadArrary[iIndex] = Start(Thread_Func, aRecord);
            return true;
        }

        /// <summary>
        /// 释放所有线程
        /// </summary>
        public void AbortAll()
        {
            int iCount = m_ThreadArrary.Count;
            for (int i = 0; i < iCount; i++)
            {
                if (m_ThreadArrary[i] == null)
                    continue;

                try
                {
                    if (m_ThreadArrary[i].ThreadState       != ThreadState.Stopped
                        && m_ThreadArrary[i].ThreadState    != ThreadState.Aborted
                        && m_ThreadArrary[i].ThreadState    != ThreadState.Unstarted)
                        m_ThreadArrary[i].Abort();
                }
                catch
                { }
            }
        }
       
        /// <summary>
        /// 获取空闲线程的序号
        /// </summary>
        /// <returns></returns>
        private int GetFreeIndex()
        {
            int iCount = m_ThreadArrary.Count;
            for (int i = 0; i < iCount; i++)
            {
                if (m_ThreadArrary[i] == null)
                    return i;

                if (IsFreeThreadByIndex(i))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// 查看线程集合是否都是空闲的
        /// </summary>
        public bool IsAllFree()
        {
            int iCount = m_ThreadArrary.Count;
            for (int i = 0; i < iCount; i++)
            {
                if (m_ThreadArrary[i] == null)
                    continue;

                if (!IsFreeThreadByIndex(i))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 是否为空闲的线程
        /// </summary>
        /// <param name="iIndex"></param>
        /// <returns></returns>
        private bool IsFreeThreadByIndex(int iIndex)
        {
            try
            {
                if (m_ThreadArrary[iIndex].ThreadState == System.Threading.ThreadState.Unstarted ||
                    m_ThreadArrary[iIndex].ThreadState == System.Threading.ThreadState.Aborted)
                    return true;

                if (m_ThreadArrary[iIndex].ThreadState == System.Threading.ThreadState.Stopped)
                {
                    m_ThreadArrary[iIndex].Abort();
                    return true;
                }
            }
            catch
            { }

            return false;
        }

        #endregion







    }

}
