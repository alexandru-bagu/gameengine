namespace GameEngine.Threading
{
	public interface ITickable
	{
		void Tick(IThreadContext context);
	}
}
