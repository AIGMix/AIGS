using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;

namespace AIGS.Helper
{
    public class TimeHelper
    {
        #region 静态接口
        /// <summary>
        /// 秒数转字符串（0 -> 00:00:00）
        /// </summary>
        /// <param name="iSecond">秒</param>
        public static string ConverIntToString(int iSecond)
        {
            int iHour = iSecond / 3600;
            int iMin  = (iSecond - iHour * 3600) / 60;
            int iSec  = iSecond - iHour * 3600 - iMin * 60;

            string sHour = iHour < 10 ? 0 + iHour.ToString():iHour.ToString();
            string sMin  = iMin  < 10 ? 0 + iMin.ToString():iMin.ToString();
            string sSec  = iSec  < 10 ? 0 + iSec.ToString():iSec.ToString();

            string sText = sHour + ':' + sMin + ':' + sSec;
            return sText;
        }

        /// <summary>
        /// 将字符串转成整数秒
        /// </summary>
        /// <param name="sHour"></param>
        /// <param name="sMine"></param>
        /// <param name="sSecond"></param>
        /// <returns> 大于等于0表示正确 </returns>
        public static int ConverToSecond(string sHour, string sMine, string sSecond)
        {
            if (String.IsNullOrWhiteSpace(sHour))
                sHour = "0";
   
            if (String.IsNullOrWhiteSpace(sMine))
                sMine = "0";

            if (String.IsNullOrWhiteSpace(sSecond))
                sSecond = "0";
            
            int iHour, iMine, iSecond;
            if (!int.TryParse(sHour, out iHour)
                || !int.TryParse(sMine, out iMine)
                || !int.TryParse(sSecond, out iSecond))
                return -1;

            return iHour * 60 * 60 + iMine * 60 + iSecond;
        }

        /// <summary>
        /// 获取当前时间
        /// </summary>
        /// <returns></returns>
        public static System.DateTime GetCurrentTime()
        {
            return System.DateTime.Now;
        }

        /// <summary>
        /// 获取时间消耗（毫秒）
        /// </summary>
        /// <param name="aStartTime"></param>
        /// <returns></returns>
        public static long CalcConsumeTime(System.DateTime aStartTime)
        {
            System.DateTime aCurTime = GetCurrentTime();

            int iEndRet = aCurTime.Hour * 3600000 + aCurTime.Minute * 60000 + aCurTime.Second * 1000 + aCurTime.Millisecond;
            int iStartRet = aStartTime.Hour * 3600000 + aStartTime.Minute * 60000 + aStartTime.Second * 1000 + aStartTime.Millisecond;

            return iEndRet - iStartRet;
        }

        #endregion

        #region 定时器内部参数与接口
        /// <summary>
        /// 定时器响应的回调函数
        /// </summary>
        public delegate void FuncEventHandle(object sender, ElapsedEventArgs e, int iTimerKey, ref object data);

        /// <summary>
        /// 定时器句柄的节点
        /// </summary>
        private class TIMER_NODE
        {
            public Timer  aTimer;           //定时器
            public object data;             //可自带的数据
            public FuncEventHandle pFunc;   //响应接口
        }

        /// <summary>
        /// 定时器句柄
        /// </summary>
        Dictionary<int, TIMER_NODE> m_TimerHandle = new Dictionary<int, TIMER_NODE>();

        /// <summary>
        /// 回调响应函数
        /// </summary>
        private void Callback_ElapsedEventHandler(object sender, ElapsedEventArgs e)
        {
            //查找
            Timer aTimer = (Timer)sender;
            KeyValuePair<int, TIMER_NODE> aTor = m_TimerHandle.First(s => s.Value.aTimer.Equals(aTimer));
            
            //执行回调
            TIMER_NODE aTreeNode    = aTor.Value;
            object aObject          = aTreeNode.data;
            aTreeNode.pFunc(sender, e, aTor.Key, ref aObject);

            //赋值回去
            aTreeNode.data = aObject;
            m_TimerHandle[aTor.Key] = aTreeNode;
        }
        #endregion

        #region 定时器动态接口

        /// <summary>
        /// 查看定时器是否已经存在
        /// </summary>
        /// <param name="iTimerKey"></param>
        /// <returns></returns>
        public bool TimerContains(int iTimerKey)
        {
            return m_TimerHandle.ContainsKey(iTimerKey);
        }

        /// <summary>
        /// 定时器初始化
        /// </summary>
        /// <param name="iMilliSecond">毫秒限制</param>
        /// <param name="data">可添加的变量</param>
        /// <param name="pFunc">处理函数</param>
        /// <param name="iTimerKey">定时器的关键字</param>
        public  void TimerInit(int iMilliSecond, object data, FuncEventHandle pFunc, int iTimerKey = -1)
        {
            //新建定时器
            Timer aTime = new Timer(iMilliSecond);
            aTime.Elapsed += Callback_ElapsedEventHandler;
            aTime.Stop();

            //新建节点
            TIMER_NODE pNode = new TIMER_NODE();
            pNode.aTimer    = aTime;
            pNode.data      = data;
            pNode.pFunc     = pFunc;

            //添加到哈希表句柄中
            m_TimerHandle.Remove(iTimerKey);
            m_TimerHandle.Add(iTimerKey, pNode);
        }

        /// <summary>
        /// 获取定时器的附件数据
        /// </summary>
        public object TimerGetData(int iTimerKey = -1)
        {
            if (m_TimerHandle.ContainsKey(iTimerKey))
                return m_TimerHandle[iTimerKey].data;

            return null;
        }

        /// <summary>
        /// 设置定时器的附件数据
        /// </summary>
        /// <param name="iTimerKey"></param>
        /// <param name="data"></param>
        public void TimerSetData(object data, int iTimerKey = -1)
        {
            if (m_TimerHandle.ContainsKey(iTimerKey))
                m_TimerHandle[iTimerKey].data = data;
        }


        /// <summary>
        /// 启动定时器
        /// </summary>
        /// <param name="iTimerKey">定时器的关键字</param>
        /// <returns></returns>
        public bool TimerStart(int iTimerKey = -1)
        {
            if (m_TimerHandle.ContainsKey(iTimerKey))
            {
                m_TimerHandle[iTimerKey].aTimer.Start();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        /// <param name="iTimerKey">定时器的关键字</param>
        /// <returns></returns>
        public bool TimerEnd(int iTimerKey = -1)
        {
            if (m_TimerHandle.ContainsKey(iTimerKey))
            {
                m_TimerHandle[iTimerKey].aTimer.Stop();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 停止全部定时器
        /// </summary>
        public void TimerAllEnd()
        {
            for (int i = 0; i < m_TimerHandle.Count; i++)
            {
                int iKey = m_TimerHandle.ElementAt(i).Key;
                m_TimerHandle[iKey].aTimer.Stop();
            }
        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        public bool TimerRemove(int iTimerKey = -1)
        {
            if (m_TimerHandle.ContainsKey(iTimerKey))
            {
                TimerEnd(iTimerKey);
                m_TimerHandle.Remove(iTimerKey);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 移除全部定时器
        /// </summary>
        public void TimerAllRemove()
        {
            TimerAllEnd();
            m_TimerHandle.Clear();
        }

        #endregion







        #region 计时器动态接口
        /// <summary>
        /// 计时器句柄
        /// </summary>
        Stopwatch m_WatchHandle = new Stopwatch();

        /// <summary>
        /// 计时开始
        /// </summary>
        public void WatchStart()
        {
            m_WatchHandle.Start();
        }

        /// <summary>
        /// 计时结束
        /// </summary>
        /// <returns></returns>
        public string WatchEnd()
        {
            m_WatchHandle.Stop();
            return ConverIntToString((int)(m_WatchHandle.ElapsedMilliseconds / 1000));
        }

        /// <summary>
        /// 计时器重置
        /// </summary>
        public void WatchReset()
        {
            m_WatchHandle.Reset();
        }

        #endregion


    }
}
