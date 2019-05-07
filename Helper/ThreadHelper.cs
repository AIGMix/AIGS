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

        public delegate void EventFunc2(object[] data);
        private static void ThreadFuncInternal2(object data)
        {
            Common.Property aRecord = (Common.Property)data;
            EventFunc2 Func         = (EventFunc2)aRecord.Key;
            object[] para           = (object[])aRecord.Value;

            Func(para);
        }

        public static Thread Start(EventFunc2 ThreadFunc, params object[] data)
        {
            Thread aNewThread       = new Thread(new ParameterizedThreadStart(ThreadFuncInternal2));
            Common.Property aRecord = new Common.Property(ThreadFunc, data);

            aNewThread.Start(aRecord);
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
        /// 线程处理前后的操作
        /// </summary>
        /// <param name="data"></param>
        private void ThreadFuncWait(object[] data)
        {
            EventFunc2 aFunc   = (EventFunc2)data[0];
            object[] aFuncPara = (object[])data[1];

            if (aFunc != null)
                aFunc(aFuncPara);

            try
            {
                m_ThreadSem.Release();
            }
            catch { }
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
        /// 获取线程数
        /// </summary>
        public int GetCount()
        {
            return m_ThreadArrary.Count;
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
        public bool ThreadStart(EventFunc2 ThreadFunc, params object[] data)
        {
            int iIndex = GetFreeIndex();
            if (iIndex < 0)
                return false;

            m_ThreadArrary[iIndex] = Start(ThreadFuncWait, ThreadFunc, data);
            return true;
        }
        
        /// <summary>
        /// 阻塞启动
        /// </summary>
        /// <param name="ThreadFunc"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool ThreadStartWait(EventFunc2 ThreadFunc, int iMillisecondsTimeout = 0, params object[] data)
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
            m_ThreadArrary[iIndex] = Start(ThreadFuncWait, ThreadFunc, data);
            return true;
        }

        /// <summary>
        /// 等待全部线程的结束
        /// </summary>
        public void WaitAll()
        {
            while(!IsAllFree())
            {
                Thread.Sleep(1000);
            }
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

    public class ThreadPoolManager
    {
        public delegate void EventFunc(object[] data);

        private class RECORD
        {
            public EventFunc Func;
            public object[] data;
        }

        private class RECORD2
        {
            public Thread Handle;
            public int ShutdownIndex;
        }

        /// <summary>
        /// 线程链表
        /// </summary>
        private List<RECORD2> m_ThreadArrary = new List<RECORD2>();
        private Queue<RECORD> m_Queue       = new Queue<RECORD>();
        private List<bool> m_ThreadShutdown = new List<bool>();

        public ThreadPoolManager(int iTreadMaxNum = 5)
        {
            //初始化线程链表
            for (int i = 0; i < iTreadMaxNum; i++)
            {
                m_ThreadShutdown.Add(false);

                RECORD2 aObj = new RECORD2();
                aObj.ShutdownIndex = i;
                aObj.Handle        = ThreadHelper.Start(ThreadFuncWork, i);

                m_ThreadArrary.Add(aObj);
            }
        }
        
        /// <summary>
        /// 添加新事件
        /// </summary>
        public void AddWork(EventFunc Func, object[] data)
        {
            RECORD aObj = new RECORD() { Func = Func, data = data };
            m_Queue.Enqueue(aObj);
        }

        /// <summary>
        /// 清空线程池
        /// </summary>
        public void CloseAll()
        {
            for (int i = 0; i < m_ThreadShutdown.Count; i++)
                m_ThreadShutdown[i] = true;
            m_ThreadArrary.Clear();
        }

        /// <summary>
        /// 获取线程池大小
        /// </summary>
        public int GetPoolSize()
        {
            return m_ThreadArrary.Count;
        }

        /// <summary>
        /// 设置线程池大小
        /// </summary>
        /// <param name="iSize"></param>
        public void SetPoolSize(int iSize)
        {
            int iCmpSize = iSize - m_ThreadArrary.Count;
            if (iCmpSize == 0)
                return;
            if (iCmpSize > 0)
            {
                for (int i = 0; i < iCmpSize; i++)
                {
                    m_ThreadShutdown.Add(false);
                    RECORD2 aObj       = new RECORD2();
                    aObj.ShutdownIndex = m_ThreadShutdown.Count - 1;
                    aObj.Handle        = ThreadHelper.Start(ThreadFuncWork, aObj.ShutdownIndex);

                    m_ThreadArrary.Add(aObj);
                }
            }
            else
            {
                while(iCmpSize < 0)
                {
                    RECORD2 aObj = m_ThreadArrary[m_ThreadArrary.Count - 1];
                    m_ThreadShutdown[aObj.ShutdownIndex] = true;
                    m_ThreadArrary.RemoveAt(m_ThreadArrary.Count - 1);

                    iCmpSize++;
                }
            }
        }

        /// <summary>
        /// 线程处理函数
        /// </summary>
        private void ThreadFuncWork(object[] data)
        {
            RECORD aObj      = null;
            int iStatusIndex = (int)data[0];
            while (true)
            {
                if (m_ThreadShutdown[iStatusIndex])
                    return;
                try
                {
                    if(m_Queue.Count > 0)
                        aObj = (RECORD)m_Queue.Dequeue();
                }
                catch{}

                if(aObj == null)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                
                aObj.Func(aObj.data);
                aObj = null;
            }
        }

    }

}
