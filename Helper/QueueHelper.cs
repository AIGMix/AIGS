using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AIGS.Helper
{
    class QueueHelper
    {
        /// <summary>
        /// 队列容量 | 队列 | 当前队列中消息数量
        /// </summary>
        public readonly int SizeLimit = 0;
        private Queue<object> _inner_queue = null;
        public int Count
        {
            get { return _inner_queue.Count; }
        }

        /// <summary>
        /// 是否关闭
        /// </summary>
        private bool _IsShutdown = false;

        /// <summary>
        /// 同步锁
        /// </summary>
        private ManualResetEvent _enqueue_wait = null;
        private ManualResetEvent _dequeue_wait = null;


        public QueueHelper(int sizeLimit)
        {
            this.SizeLimit     = sizeLimit;
            this._inner_queue  = new Queue<object>(this.SizeLimit);
            this._enqueue_wait = new ManualResetEvent(false);
            this._dequeue_wait = new ManualResetEvent(false);
        }
        public void EnQueue(object item)
        {
            if (this._IsShutdown == true) throw new InvalidCastException("Queue was shutdown. Enqueue was not allowed.");
            while (true)
            {
                lock (this._inner_queue)
                {
                    if (this._inner_queue.Count < this.SizeLimit)
                    {
                        this._inner_queue.Enqueue(item);
                        this._enqueue_wait.Reset();
                        this._dequeue_wait.Set();
                        break;
                    }
                }
                this._enqueue_wait.WaitOne();
            }
        }
        public object DeQueue()
        {
            while (true)
            {
                if (this._IsShutdown == true)
                {
                    lock (this._inner_queue) return this._inner_queue.Dequeue();
                }
                lock (this._inner_queue)
                {
                    if (this._inner_queue.Count > 0)
                    {
                        object item = this._inner_queue.Dequeue();
                        this._dequeue_wait.Reset();
                        this._enqueue_wait.Set();
                        return item;
                    }
                }
                this._dequeue_wait.WaitOne();
            }
        }
        public void Shutdown()
        {
            this._IsShutdown = true;
            this._dequeue_wait.Set();
        }
    }

}
