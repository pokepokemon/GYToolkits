using GYLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GYLibs
{

    public static class ObjectPoolForData<T> where T : class, new()
    {
        /// <summary>Internal pool</summary>
        static List<T> pool = new List<T>();
        static readonly HashSet<T> inPool = new HashSet<T>();

        /// <summary>
        /// Claim a object.
        /// Returns a pooled object if any are in the pool.
        /// Otherwise it creates a new one.
        /// After usage, this object should be released using the Release function (though not strictly necessary).
        /// </summary>
        public static T Get()
        {
            lock (pool)
            {
                if (pool.Count > 0)
                {
                    T ls = pool[pool.Count - 1];
                    pool.RemoveAt(pool.Count - 1);
                    inPool.Remove(ls);

                    if (ls is IRecycle)
                    {
                        (ls as IRecycle).OnReUse();
                    }
                    // StatusPool.AddStat(ls.GetType().ToString());
                    return ls;
                }
                else
                {
                    return new T();
                }
            }
        }

        /// <summary>
        /// Releases an object.
        /// After the object has been released it should not be used anymore.
        /// The variable will be set to null to prevent silly mistakes.
        ///
        /// Throws: System.InvalidOperationException
        /// Releasing an object when it has already been released will cause an exception to be thrown.
        ///
        /// See: Get
        /// </summary>
        public static void Release(T obj)
        {
            lock (pool)
            {
                if (!inPool.Add(obj))
                {
                    throw new InvalidOperationException("You are trying to pool an object twice. Please make sure that you only pool it once.");
                }
                if (obj is IRecycle)
                {
                    (obj as IRecycle).OnRecycle();
                }
                pool.Add(obj);
            }
            obj = null;
        }

        /// <summary>
        /// Clears the pool for objects of this type.
        /// This is an O(n) operation, where n is the number of pooled objects.
        /// </summary>
        public static void Clear()
        {
            lock (pool)
            {
                inPool.Clear();
                pool.Clear();
            }
        }

        /// <summary>Number of objects of this type in the pool</summary>
        public static int GetSize()
        {
            return pool.Count;
        }
    }

    /*
    public static class StatusPool
    {
        public static long totalReuseCount = 0;
        public static Dictionary<string, long> statDict = new Dictionary<string, long>();

        public static void AddStat(string key)
        {
            totalReuseCount++;
            if (statDict.TryGetValue(key, out long value))
            {
                statDict[key] = value + 1;
            }
            else
            {
                statDict[key] = 1;
            }
        }

        public static void PrintReuseCount()
        {
            foreach (var key in statDict.Keys)
            {
                Debug.Log(string.Format("[{0}] = {1}", key, statDict[key]));
            }
        }
    }
    */
}