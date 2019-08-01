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
        public static void Abort(Thread Handle)
        {
            try
            {
                Handle.Abort();
            }
            catch { }
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
        private class WorkItem
        {
            public EventFunc Func;
            public object[] data;
        }

        private class ThreadItem
        {
            public Thread Handle;
            public int    Index;
            public bool   Shutdown;
        }

        /// <summary>
        /// 线程链表
        /// </summary>
        private List<ThreadItem> m_ThreadArrary   = new List<ThreadItem>();
        private List<ThreadItem> m_ThreadShutdown = new List<ThreadItem>();
        private Queue<WorkItem> m_Queue           = new Queue<WorkItem>();
        public ThreadPoolManager(int iTreadMaxNum = 5)
        {
            SetPoolSize(iTreadMaxNum);
        }

        /// <summary>
        /// 添加新事件
        /// </summary>
        public void AddWork(EventFunc Func, object[] data)
        {
            WorkItem aObj = new WorkItem() { Func = Func, data = data };
            m_Queue.Enqueue(aObj);
        }

        /// <summary>
        /// 清空线程池
        /// </summary>
        public void CloseAll(bool bIsImmediately = false)
        {
            for (int i = 0; i < m_ThreadShutdown.Count; i++)
                m_ThreadArrary[i].Shutdown = true;

            if (bIsImmediately)
            {
                for (int i = 0; i < m_ThreadArrary.Count; i++)
                    ThreadHelper.Abort(m_ThreadArrary[i].Handle);
                for (int i = 0; i < m_ThreadShutdown.Count; i++)
                    ThreadHelper.Abort(m_ThreadShutdown[i].Handle);
            }
            m_ThreadArrary.Clear();
            m_ThreadShutdown.Clear();
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
        /// <param name="iNewSize"></param>
        public void SetPoolSize(int iNewSize)
        {
            if (iNewSize < 0)
                return;

            int iIncreaseNum = iNewSize - m_ThreadArrary.Count;
            if (iIncreaseNum == 0)
                return;
            if (iIncreaseNum > 0)
            {
                for (int i = 0; i < iIncreaseNum; i++)
                {
                    ThreadItem aRecord = new ThreadItem();
                    aRecord.Index      = m_ThreadArrary.Count;
                    aRecord.Shutdown   = false;
                    aRecord.Handle     = ThreadHelper.Start(ThreadFuncWork, aRecord.Index);
                    m_ThreadArrary.Add(aRecord);
                }
            }
            if (iIncreaseNum < 0)
            {
                for (int i = 0; i > iIncreaseNum; i--)
                {
                    ThreadItem aRecord = m_ThreadArrary[m_ThreadArrary.Count - 1];
                    aRecord.Shutdown   = true;
                    m_ThreadShutdown.Add(aRecord);
                    m_ThreadArrary.RemoveAt(m_ThreadArrary.Count - 1);
                }
            }
        }

        /// <summary>
        /// 线程处理函数
        /// </summary>
        private void ThreadFuncWork(object[] data)
        {
            int iIndex       = (int)data[0];
            WorkItem aRecord = null;
            
            Thread.Sleep(500);
            while (true)
            {
                if (m_ThreadArrary[iIndex].Shutdown)
                    return;

                try
                {
                    if (m_Queue.Count <= 0)
                        goto POINT_SLEEP;

                    aRecord = null;
                    aRecord = (WorkItem)m_Queue.Dequeue();
                    if (aRecord == null)
                        goto POINT_SLEEP;
                    aRecord.Func(aRecord.data);
                    continue;
                }
                catch{}

            POINT_SLEEP:
                Thread.Sleep(1000);
            }
        }
    }

}
