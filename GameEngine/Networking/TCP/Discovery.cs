using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameEngine.Networking.TCP
{
	public class Discovery
	{
		public static DiscoveryResult Search(int port)
		{
			var result = new DiscoveryResult(null, false);
			search(port, null, null, result);
			return result;
		}

		public static IAsyncResult Search(int port, Action<IPAddress> resultCallback, Action<IAsyncResult> callback, object state)
		{
			var result = new DiscoveryResult(state, true);
			Action<int, Action<IPAddress>, Action<IAsyncResult>, DiscoveryResult> asyncAction = search;
			asyncAction.BeginInvoke(port, resultCallback, callback, result, asyncAction.EndInvoke, null);
			return result;
		}

		private static void search(int port, Action<IPAddress> resultCallback, Action<IAsyncResult> callback, DiscoveryResult result)
		{
			var syncRoot = new object();

			Parallel.ForEach(getLocalAreaNetworkIPAddresses(), p =>
			{
				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				var asyncResult = socket.BeginConnect(new IPEndPoint(p, port), endConnect, socket);
				asyncResult.AsyncWaitHandle.WaitOne(1000);
				lock (syncRoot)
				{
					if (asyncResult.IsCompleted)
						resultCallback?.Invoke(p);
					result.Add(p);
				}
				socket.Close();
			});

			result.Complete();
			callback?.Invoke(result);
		}

		private static void endConnect(IAsyncResult result)
		{
			var socket = result.AsyncState as Socket;

			try
			{
				socket.EndConnect(result);
			}
			catch (Exception)
			{
				socket = null;
			}
		}

		//http://stackoverflow.com/a/7380401/1163478
		private static IEnumerable<IPAddress> getLocalAreaNetworkIPAddresses()
		{
			foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				//if it's loopback, skip it
				if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;
				//if the current interface doesn't have an IP, skip it
				if (!(networkInterface.GetIPProperties().GatewayAddresses.Count > 0))
					continue;
					//get current IP Address(es)
				foreach (var ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
				{
					//if it's ipv6, skip it
					if (ipAddressInformation.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
						continue;
						//get the subnet mask and the IP address as bytes
					byte[] subnetMask = ipAddressInformation.IPv4Mask.GetAddressBytes();
					byte[] ipAddr = ipAddressInformation.Address.GetAddressBytes();

					// we reverse the byte-array if we are dealing with little endian.
					if (BitConverter.IsLittleEndian)
					{
						Array.Reverse(subnetMask);
						Array.Reverse(ipAddr);
					}

					//we convert the subnet mask as uint (just for didactic purposes (to check everything is ok now and next - use thecalculator in programmer mode)
					uint maskAsInt = BitConverter.ToUInt32(subnetMask, 0);

					//we convert the ip addres as uint (just for didactic purposes (to check everything is ok now and next - use thecalculator in programmer mode)
					uint ipAsInt = BitConverter.ToUInt32(ipAddr, 0);

					//we negate the subnet to determine the maximum number of host possible in this subnet
					uint validHostsEndingMax = ~BitConverter.ToUInt32(subnetMask, 0);

					//we convert the start of the ip addres as uint (the part that is fixed wrt the subnet mask - from here we calculate each new address by incrementing with 1 and converting to byte[] afterwards 
					uint validHostsStart = BitConverter.ToUInt32(ipAddr, 0) & BitConverter.ToUInt32(subnetMask, 0);

					//we increment the startIp to the number of maximum valid hosts in this subnet and for each we check the intended port (refactoring needed)
					for (uint i = 1; i <= validHostsEndingMax; i++)
					{
						uint host = validHostsStart + i;
						byte[] hostBytes = BitConverter.GetBytes(host);
						if (BitConverter.IsLittleEndian)
							Array.Reverse(hostBytes);

						yield return new IPAddress(hostBytes);
					}
				}
			}
		}

		//http://stackoverflow.com/a/7380401/1163478
		public static IEnumerable<IPAddress> GetIPAddresses()
		{
			foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				//if it's loopback, skip it
				if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
					continue;
				//if the current interface doesn't have an IP, skip it
				if (!(networkInterface.GetIPProperties().GatewayAddresses.Count > 0))
					continue;
				//get current IP Address(es)
				foreach (var ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
				{
					//if it's ipv6, skip it
					if (ipAddressInformation.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
						continue;

					yield return ipAddressInformation.Address;
				}
			}
		}
	}
}
