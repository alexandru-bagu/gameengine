using System;
using System.Collections;
using System.Collections.Generic;
using GameEngine.Collision;

namespace GameEngine.Collections
{
	public class ReadOptimizedList<T> : IEnumerable<T>
	{
		private List<T> _list;
		private T[] _array;

		public T[] Array => _array;
		public int Count => _array.Length;

		public ReadOptimizedList(int capacity)
		{
			_list = new List<T>(capacity);
			_array = new T[0];
		}

		public ReadOptimizedList()
			: this(0) { }

		public ReadOptimizedList(IEnumerable<T> collection)
			: this()
		{
			_list.AddRange(collection);
		}

		public T this[int index]
		{
			get
			{
				return _array[index];
			}
			set
			{
				_list[index] = value;
				_array = _list.ToArray();
			}
		}

		public void Add(T obj)
		{
			_list.Add(obj);
			_array = _list.ToArray();
		}

		public void Remove(T obj)
		{
			_list.Remove(obj);
			_array = _list.ToArray();
		}

		public void Insert(int index, T obj)
		{
			_list.Insert(index, obj);
			_array = _list.ToArray();
		}

		public void Clear()
		{
			_list.Clear();
			_array = _list.ToArray();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);
            _array = _list.ToArray();
        }

        public int FindIndex(Predicate<T> p)
		{
			var array = Array;
			for (int i = 0; i < array.Length; i++)
				if (p(array[i]))
					return i;
			return -1;
		}

		public IEnumerator<T> GetEnumerator()
		{
			var obj = _array;//copy reference
			for (int i = 0; i < obj.Length; i++)
				yield return obj[i];
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
    }
}