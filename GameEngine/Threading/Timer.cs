using System;
using System.Diagnostics;

namespace GameEngine.Threading
{
	public class Timer : ITickable
	{
		private Stopwatch _stopwatch;
		private bool _enabled;
		private Context<ITickable> _context;

		public event Action<Timer> PeriodElapsed;
		public long Period { get; set; }
		public bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; updateStatus(); }
		}

		public void Enable()
		{
			Enabled = true;
		}
		public void Disable()
		{
			Enabled = false;
		}

		private void updateStatus()
		{
			if (_enabled && !_stopwatch.IsRunning)
			{
				_context.Register(this);
				_stopwatch.Restart();
			}
			else if (!_enabled && _stopwatch.IsRunning)
			{
				_context.Unregister(this);
				_stopwatch.Stop(); 
			}
		}

		public Timer(Context<ITickable> context)
		{
			_context = context;
			_stopwatch = new Stopwatch();
		}

		public void Tick(IThreadContext context)
		{
			if (_enabled && _stopwatch.ElapsedMilliseconds > Period)
			{
				_stopwatch.Restart();
				PeriodElapsed?.Invoke(this);
			}
		}
	}
}
