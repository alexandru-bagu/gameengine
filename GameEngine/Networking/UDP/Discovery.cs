using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace GameEngine.Networking.UDP
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
			asyncAction.BeginInvoke(port, resultCallback, callback, result, endInvoke, asyncAction);
			return result;
		}

		static void endInvoke(IAsyncResult ar)
		{
			var state = (Action<int, Action<IPAddress>, Action<IAsyncResult>, DiscoveryResult>)ar.AsyncState;
			state.EndInvoke(ar);
		}

		private static void search(int port, Action<IPAddress> resultCallback, Action<IAsyncResult> callback, DiscoveryResult result)
		{
			var syncRoot = new object();
			var broadcasts = getLocalAreaNetworkBroadcasts();
			Parallel.ForEach(broadcasts, p =>
			{
				try
				{
					var buffer = new byte[16];
					byte[] recvBuffer = new byte[16];
					EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

					Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
					socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
					socket.ReceiveTimeout = 10000;
					socket.SendTo(buffer, new IPEndPoint(p, port));

					while (true)
					{
						try
						{
							var recvSize = socket.ReceiveFrom(recvBuffer, 16, SocketFlags.None, ref endPoint);
							var addr = ((IPEndPoint)endPoint).Address;
							if (recvSize != 0)
							{
								lock (syncRoot)
								{
									resultCallback?.Invoke(addr);
									result.Add(addr);
								}
							}
						}
						catch
						{
							break;
						}
					}
					socket.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			});

			try
			{
				foreach (var ip in GetIPAddresses())
				{
					result.Add(ip);
					resultCallback?.Invoke(ip);
					break;
				}

				result.Complete();
				callback?.Invoke(result);
			}
			catch(Exception e) 
			{
				Console.WriteLine(e);
			}
		}


		//http://stackoverflow.com/a/7380401/1163478
		private static IEnumerable<IPAddress> getLocalAreaNetworkBroadcasts()
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
					uint max = validHostsStart | validHostsEndingMax;
					byte[] hostBytes = BitConverter.GetBytes(max);
					if (BitConverter.IsLittleEndian) Array.Reverse(hostBytes);

					var ipAddress = new IPAddress(hostBytes);
					yield return ipAddress;
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