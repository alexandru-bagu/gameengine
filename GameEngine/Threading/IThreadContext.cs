using System;

namespace GameEngine.Threading
{
	public interface IThreadContext
	{
		void Invoke(Action action);
		void InvokeLazy(Action action, int timeout);
	}
}
