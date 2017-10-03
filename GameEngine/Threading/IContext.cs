using System;

namespace GameEngine.Threading
{
	public interface IContext<T> : ITickable, IThreadContext
		where T : ITickable
	{
		void Register(params T[] tickable);
		void Unregister(params T[] tickable);
	}
}
