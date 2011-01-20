//
// Copyright (c) 2011 Chip Bradford (cfbradford@gmail.com). All rights reserved.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace RetRotationSim.Collections
{
    /// <summary>
    /// Description of Heap.
    /// </summary>
    public sealed class Heap<T> : IPriorityQueue<T>
    {
        private readonly List<T> _heap; // TODO idealy this should be replaced with an array
        private readonly IComparer<T> _comparer;
        
        public Heap ()
            : this(new List<T>(), Comparer<T>.Default)
        {
        }
        
        public Heap (IEnumerable<T> collection)
            : this(new List<T>(collection), Comparer<T>.Default)
        {
            Contract.Requires(collection != null);
            
            Heapify();
        }
        
        public Heap (int capacity)
            : this(new List<T>(capacity), Comparer<T>.Default)
        {
            Contract.Requires(capacity >= 0);
        }
        
        public Heap (IComparer<T> comparer)
            : this(new List<T>(), comparer)
        {
            Contract.Requires(comparer != null);
        }
        
        public Heap (IEnumerable<T> collection, IComparer<T> comparer)
            : this(new List<T>(collection), comparer)
        {
            Contract.Requires(collection != null);
            Contract.Requires(comparer != null);
            
            Heapify();
        }
        
        public Heap (int capacity, IComparer<T> comparer)
            : this(new List<T>(capacity), comparer)
        {
            Contract.Requires(capacity >= 0);
            Contract.Requires(comparer != null);
        }
        
        private Heap (List<T> heap, IComparer<T> comparer)
        {
            Contract.Requires(heap != null);
            Contract.Requires(comparer != null);
            
            _heap = heap;
            _comparer = comparer;
        }
        
        private void Heapify ()
        {
            throw new NotImplementedException();
        }
        
        public int Count { get { return _heap.Count; } }
        
        bool ICollection<T>.IsReadOnly { get { return false; } }
        
        private void BubbleUp (int index, T value)
        {
            int parent = (index - 1) >> 1;
            
            while (index > 0 && _comparer.Compare(_heap[parent], value) < 0)
            {
                _heap[index] = _heap[parent];
                index = parent;
                parent = (index - 1) >> 1;
            }
            _heap[index] = value;
        }
        
        private void TrickleDown (int index, T value)
        {
            int child = (index << 1) + 1;
            
            while (child < Count)
            {
                if (child + 1 < Count && _comparer.Compare(_heap[child], _heap[child + 1]) < 0)
                    ++child;
                
                _heap[index] = _heap[child];
                index = child;
                child = (index << 1 ) + 1;
            }
            
            BubbleUp(index, value);
        }
        
        public void Push (T value)
        {
            _heap.Add(default(T));
            
            BubbleUp(Count - 1, value);
        }
        
        public T Peek ()
        {
            Contract.Requires(Count > 0);
            
            return _heap[0];
        }
        
        public T Pop ()
        {
            Contract.Requires(Count > 0);
            
            T result = _heap[0];
            
            int last = Count - 1;
            
            TrickleDown(0, _heap[last]);
            
            //_heap[last] = default(T);
            _heap.RemoveAt(Count - 1);
            
            return result;
        }
        
        public T[] ToArray ()
        {
            throw new NotImplementedException();
        }
        
        void ICollection<T>.Add (T item)
        {
            Push(item);
        }
        
        public void Clear ()
        {
            _heap.Clear();
        }
        
        public bool Contains (T item)
        {
            return _heap.Contains(item);
        }
        
        public void CopyTo (T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }
        
        bool ICollection<T>.Remove (T item)
        {
            return _heap.Remove(item);
        }
        
        public IEnumerator<T> GetEnumerator ()
        {
            return _heap.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return GetEnumerator();
        }
    }
}
