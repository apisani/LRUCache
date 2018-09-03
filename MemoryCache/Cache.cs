using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace MemoryCache
{
    /// <summary>
    /// Interface to implement for Cache Fone Dymanic Challenge.
    /// </summary>
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// Adds the value to the cache against the specified key.
        /// If the key already exists, its value is updated.
        /// </summary>
        void AddOrUpdate(TKey key, TValue value);
        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// and returns true if the key existed in the cache.
        /// </summary>
        bool TryGetValue(TKey key, out TValue value);
    }

    /// <summary>
    /// Memory Cache Fone Dynamics challenge.
    /// </summary>
    /// <typeparam name="K">The key type.</typeparam>
    /// <typeparam name="V">The value type.</typeparam>
    public class Cache<TKey, TValue> :  ICache<TKey, TValue>
    {
        private readonly Dictionary<TKey, CacheNode> _entries;
        private readonly int _capacity;
        private CacheNode _head;
        private CacheNode _tail;
        private TimeSpan _ttl;
        private Timer _timer;
        private int _count;

        /// <summary>
        /// A least recently used cache using O(1) complexity
        /// </summary>
        /// <param name="capacity">
        /// The maximum number of entries the cache will be able to hold before updating its content
        /// </param>
        public Cache(int capacity)
        {
            this._capacity = capacity;
            this._entries = new Dictionary<TKey, CacheNode>(this._capacity);
            this._ttl = new TimeSpan(0, 0 , 0);

            //First item in cache
            this._head = null;
            //Last item in cache
            this._tail = null;

            this._count = 0;
            if (this._ttl > TimeSpan.Zero)
            {
                this._timer = new Timer(PurgeCache, null, (int)this._ttl.TotalMilliseconds, 6000); // 6 seconds
            }
        }

        private class CacheNode
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public DateTime LastAccessed { get; set; }
            public CacheNode Next { get; set; }
            public CacheNode Previous { get; set; }
        }

        /// <summary>
        /// Gets the current number of entries in the cache.
        /// </summary>
        public int Count
        {
            get { return _entries.Count; }
        }

        /// <summary>
        /// Gets the maximum number of entries in the cache.
        /// </summary>
        public int Capacity
        {
            get { return this._capacity; }
        }

        /// <summary>
        /// Gets if the cache if full or not.
        /// </summary>
        /// <returns>True if full. False otherwise.</returns>
        public bool IsFull
        {
            get { return this._count == this._capacity; }
        }

        /// <summary>
        /// Attempts to gets the value from the cache against the specified key
        /// </summary>
        /// <returns>True if the key existed in the cache. False otherwise.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            CacheNode entry;
            value = default(TValue);

            if (!this._entries.TryGetValue(key, out entry))
            {
               return false;
            }

            MoveToHead(entry);

            lock (entry)
            {
                value = entry.Value;
            }

            return true;
        }

        /// <summary>
        /// Adds the value to the cache against the specified key.
        /// If the key already exists, its value is updated.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value to set in the cache.</param>
        public void AddOrUpdate(TKey key, TValue value)
        {
            CacheNode entry;
            if (!this._entries.TryGetValue(key, out entry))
            {
                // Add the entry
                lock (this)
                {
                    if (!this._entries.TryGetValue(key, out entry))
                    {
                        if (this.IsFull)
                        {
                            // Re-use the CacheNode entry
                            entry = this._tail;
                            _entries.Remove(this._tail.Key);

                            // Reset with new values
                            entry.Key = key;
                            entry.Value = value;
                            entry.LastAccessed = DateTime.UtcNow;

                            // Next and Previous don't need to be reset.
                        }
                        else
                        {
                            this._count++;
                            entry = new CacheNode()
                            {
                                Key = key,
                                Value = value,
                                LastAccessed = DateTime.UtcNow
                            };
                        }
                        _entries.Add(key, entry);
                    }
                }
            }
            else
            {
                lock (entry)
                {
                    entry.Value = value;
                }
            }

            MoveToHead(entry);

            if (null == this._tail)
            {
                this._tail = this._head;
            }
        }


        /// <summary>
        /// Place the given entry to the head of the list.
        /// </summary>
        /// <param name="entry">The CacheNode to move up the list.</param>
        private void MoveToHead(CacheNode node)
        {
            if (node == this._head)
            {
                return;
            }

            // Lock is necessary here as we're modifying the entry
            // which is not thread safe.
            lock (this)
            {
                RemoveCacheNodeFromList(node);
                AddToHead(node);
            }
        }

        private void PurgeCache(object state)
        {
            if (this._ttl <= TimeSpan.Zero || this._count == 0)
            {
                return;
            }

            lock (this)
            {
                var current = this._tail;
                var now = DateTime.UtcNow;

                while (null != current && (now - current.LastAccessed) > this._ttl)
                {
                    RemoveCacheNode(current);
                    current = current.Previous;
                }
            }
        }

        private void AddToHead(CacheNode node)
        {
            node.Previous = null;
            node.Next = this._head;

            if (null != this._head)
            {
                this._head.Previous = node;
            }

            this._head = node;
        }

        private void RemoveCacheNodeFromList(CacheNode node)
        {
            var next = node.Next;
            var previous = node.Previous;

            if (null != next)
            {
                next.Previous = node.Previous;
            }
            if (null != previous)
            {
                previous.Next = node.Next;
            }

            if (this._head == node)
            {
                this._head = next;
            }

            if (this._tail == node)
            {
                this._tail = previous;
            }
        }

        private void RemoveCacheNode(CacheNode node)
        {
            RemoveCacheNodeFromList(node);
            this._entries.Remove(node.Key);
            this._count--;
        }
    }
}
