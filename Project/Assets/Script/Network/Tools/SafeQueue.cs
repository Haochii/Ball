using System.Collections.Generic;

namespace BaseFramework
{
    public class SafeQueue<T> where T : class
    {
        private Queue<T> mQueue = null;
        private object mLock = null;

        public SafeQueue()
        {
            mQueue = new Queue<T>();
            mLock = new object();
        }

        public SafeQueue(int count)
        {
            mQueue = new Queue<T>(count);
            mLock = new object();
        }

        public int Count
        {
            get
            {
                lock (mLock)
                {
                    return mQueue.Count;
                }
            }
        }

        public T Dequeue()
        {
            lock (mLock)
            {
                if (mQueue.Count > 0)
                {
                    return mQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Enqueue(T _t)
        {
            lock (mLock)
            {
                mQueue.Enqueue(_t);
            }
        }

        public void Clear()
        {
            lock (mLock)
            {
                mQueue.Clear();
            }
        }
    }
}