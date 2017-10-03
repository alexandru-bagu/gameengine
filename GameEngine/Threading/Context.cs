using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using GameEngine.Collections;

namespace GameEngine.Threading
{
	public abstract class Context<T> : IContext<T>
		where T : ITickable
	{
		private ReadPriorityList<T> _tickables;
		private ConcurrentQueue<Action> _invokables;
		private ConcurrentQueue<Tuple<Action, int, Stopwatch>> _lazyInvokables;

		public T[] Tickables => _tickables.Array;

		protected Context()
		{
			_tickables = new ReadPriorityList<T>();
			_invokables = new ConcurrentQueue<Action>();
			_lazyInvokables = new ConcurrentQueue<Tuple<Action, int, Stopwatch>>();
		}

		public virtual void Tick(IThreadContext context)
		{
			Action invokable;
			while (_invokables.TryDequeue(out invokable))
				invokable.Invoke();

			Tuple<Action, int, Stopwatch> lazyInvokable;
			while (_lazyInvokables.TryPeek(out lazyInvokable))
			{
				if (lazyInvokable.Item3.ElapsedMilliseconds > lazyInvokable.Item2)
				{
					lazyInvokable.Item1.Invoke();
					_lazyInvokables.TryDequeue(out lazyInvokable);
				}
				else
				{
					break;
				}
			}

			foreach (var ticker in _tickables)
				ticker.Tick(context);
		}

		public void Invoke(Action action)
		{
			_invokables.Enqueue(action);
		}

		public void InvokeLazy(Action action, int timeout)
		{
			_lazyInvokables.Enqueue(Tuple.Create(action, timeout, Stopwatch.StartNew()));
		}

		public void Register(params T[] tickable)
		{
			foreach (var ticker in tickable)
				_tickables.Add(ticker);
		}

		public void Unregister(params T[] tickable)
		{
			foreach (var ticker in tickable)
				_tickables.Remove(ticker);
		}
	}
}
