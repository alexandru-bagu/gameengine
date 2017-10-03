using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace GameEngine.Networking
{
	public class DiscoveryResult : IAsyncResult
	{
		private object _asyncState;
		private EventWaitHandle _waitHandle;
		private List<IPAddress> _resultList;
		private IPAddress[] _results;
		private bool _completed, _async;

		public object AsyncState => _asyncState;
		public WaitHandle AsyncWaitHandle => _waitHandle;
		public bool CompletedSynchronously => !_async;
		public bool IsCompleted => _completed;
		public IPAddress[] Results => _results;

		public DiscoveryResult(object asyncState, bool @async)
		{
			_asyncState = asyncState;
			_async = @async;
			_waitHandle = new EventWaitHandle(true, EventResetMode.ManualReset);
			_results = new IPAddress[0];
			_resultList = new List<IPAddress>();
		}

		internal void Add(IPAddress address)
		{
			_resultList.Add(address);
		}

		internal void Complete()
		{
			_results = _resultList.ToArray();
			_completed = true;
			_waitHandle.Reset();
		}
	}
}
