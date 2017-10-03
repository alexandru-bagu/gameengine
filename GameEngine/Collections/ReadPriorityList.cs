using System;
using System.Collections;
using System.Collections.Generic;

namespace GameEngine.Collections
{
	public class ReadPriorityList<T> : IEnumerable<T>
	{
		private List<T> _list;
		private T[] _array;
		private object _syncRoot;

		public T[] Array => _array;
		public int Count => _array.Length;

		public ReadPriorityList(int capacity)
		{
			_list = new List<T>(capacity);
			_array = new T[0];
			_syncRoot = new object();
		}

		public ReadPriorityList()
			: this(0) { }

		public ReadPriorityList(IEnumerable<T> collection)
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
				lock (_syncRoot)
				{
					_list[index] = value;
					_array = _list.ToArray();
				}
			}
		}

		public void Add(T obj)
		{
			lock (_syncRoot)
			{
				_list.Add(obj);
				_array = _list.ToArray();
			}
		}

		public void Remove(T obj)
		{
			lock (_syncRoot)
			{
				_list.Remove(obj);
				_array = _list.ToArray();
			}
		}

		public void Insert(int index, T obj)
		{
			lock (_syncRoot)
			{
				_list.Insert(index, obj);
				_array = _list.ToArray();
			}
		}

		public void Clear()
		{
			lock (_syncRoot)
			{
				_list.Clear();
				_array = _list.ToArray();
			}
		}

		public int FindIndex(Predicate<T> p)
		{
			lock (_syncRoot)
			{
				var array = Array;
				for (int i = 0; i < array.Length; i++)
					if (p(array[i]))
						return i;
			}
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