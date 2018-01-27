/*
 *  This file is part of upnp-clr.
 *  Copyright (c) 2016 Denis Rozhkov <denis@rozhkoff.com>
 *
 *  upnp-clr is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Lesser General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  upnp-clr is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with upnp-clr.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

using AmberSystems.UPnP.Core.Exceptions;
using AmberSystems.UPnP.Core.Ssdp;
using AmberSystems.UPnP.Core.Types;

namespace AmberSystems.UPnP
{
	public class UpnpClient
	{
		public class DiscoveryResult
		{
			protected ConcurrentDictionary<string, ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>> m_resultMap =
				new ConcurrentDictionary<string, ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>>();

			public bool IsEmpty { get { return m_resultMap.IsEmpty; } }


			public void Add( IPAddress address, byte[] response, System.Net.EndPoint remoteEp )
			{
				var result = Message.Parse( response, remoteEp as IPEndPoint, address );

				if (result != null)
				{
					var resultKey = address.ToString();
					var targetDict = m_resultMap.GetOrAdd( resultKey, new ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>() );
					var resultDict = targetDict.GetOrAdd( result.Target.Type, new ConcurrentDictionary<string, Result>() );

					resultDict.AddOrUpdate( result.Key(), result, ( key, value ) => result );
				}
			}

			public string[] GetInterfaces()
			{
				return m_resultMap.Keys.ToArray();
			}

			public TargetType[] GetTargetTypes( string iface )
			{
				return m_resultMap[iface].Keys.ToArray();
			}

			public Result[] GetTargets( string iface, TargetType type )
			{
				return m_resultMap[iface][type].Values.ToArray();
			}

			public IEnumerable<Result> GetResults( TargetType type )
			{
				return m_resultMap.SelectMany( a => a.Value.Values.SelectMany( b => b.Values ) ).Where( a => a.Target.Type == type );
			}
		}


		protected DiscoveryResult m_discoveryResult = new DiscoveryResult();


		public async Task<DiscoveryResult> Discover( TargetType targetType = TargetType.All, short ttl = 2 )
		{
			List<IPAddress> addressList = new List<IPAddress>();
			addressList.AddRange( NetworkInterface.GetAllNetworkInterfaces()
				.Where( a => a.NetworkInterfaceType != NetworkInterfaceType.Loopback && a.OperationalStatus == OperationalStatus.Up )
				.SelectMany( a => a.GetIPProperties().UnicastAddresses )
				.Where( a => a.Address.AddressFamily == AddressFamily.InterNetwork )
				.Select( a => a.Address ) );

			var message = new Message( MessageType.Search )
				.Mx( TimeSpan.FromSeconds( 3 ) )
				.St( targetType );

			var messageBin = message.ToByteArray();

			DiscoveryResult result = new DiscoveryResult();
			List<Task> tasks = new List<Task>();

			foreach (var a in addressList)
			{
				var task = Task.Run( async () =>
				{
					using (var client = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp ))
					{
						client.Ttl = ttl;
						client.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true );
						client.Bind( new IPEndPoint( a, 0 ) );

						var bufferIn = new ArraySegment<byte>( new byte[2048] );
						var receiveTask = client.ReceiveFromAsync( bufferIn, SocketFlags.None, new IPEndPoint( IPAddress.Any, 0 ) );

						ArraySegment<byte> bufferOut = new ArraySegment<byte>( messageBin );
						client.SendTo( bufferOut.Array, message.Host );
						client.SendTo( bufferOut.Array, message.Host );

						while (true)
						{
							if (!receiveTask.Wait( (int)(message.Mx().TotalMilliseconds * ttl) ))
							{
								return;
							}

							var taskResult = await receiveTask;

							try
							{
								result.Add( a, bufferIn.Take( taskResult.ReceivedBytes ).ToArray(), taskResult.RemoteEndPoint );
							}
							catch (Exception x)
							{
							}

							receiveTask = client.ReceiveFromAsync( bufferIn, SocketFlags.None, new IPEndPoint( IPAddress.Any, 0 ) );
						}
					}
				} );

				tasks.Add( task );
			}

			await Task.WhenAll( tasks );

			m_discoveryResult = result;

			return result;
		}

		protected async Task ExecuteService( DeviceType deviceType, ServiceType serviceType, Func<Result, Service, Task> f )
		{
			if (m_discoveryResult.IsEmpty)
			{
				await Discover();
			}

			if (m_discoveryResult.IsEmpty)
			{
				throw new UpnpClrException();
			}

			var devices = m_discoveryResult.GetResults( TargetType.Device ).Where( a => a.Target is Device && ((Device)a.Target).Type == deviceType );

			foreach (var device in devices)
			{
				var description = await device.GetDescription<DeviceDescription>();
				var services = description.GetServices( serviceType );

				foreach (var service in services)
				{
					await f( device, service );
				}
			}
		}

		public async Task<List<string>> GetExternalAddressList()
		{
			List<string> result = new List<string>();

			await ExecuteService( DeviceType.InternetGatewayDevice, ServiceType.WanIpConnection, async ( a, b ) =>
			{
				using (var serviceClient = new Core.Services.WanIpConnectionClient( a.Location.ToString(), b.ControlUrl, b.TypeName ))
				{
					result.Add( await serviceClient.GetExternalIpAddress() );
				}
			} );

			return result;
		}

		public async Task<List<Core.Services.WanIpConnectionClient.PortMapping>> GetGenericPortMappingEntry( int index )
		{
			List<Core.Services.WanIpConnectionClient.PortMapping> result = new List<Core.Services.WanIpConnectionClient.PortMapping>();

			await ExecuteService( DeviceType.InternetGatewayDevice, ServiceType.WanIpConnection, async ( a, b ) =>
			{
				using (var serviceClient = new Core.Services.WanIpConnectionClient( a.Location.ToString(), b.ControlUrl, b.TypeName ))
				{
					result.Add( await serviceClient.GetGenericPortMappingEntry( index ) );
				}
			} );

			return result;
		}

		public async Task AddPortMapping(
				int internalPort, int externalPort,
				string internalHost = null, ProtocolType protocol = ProtocolType.Tcp,
				string description = null,
				TimeSpan leaseDuration = new TimeSpan(),
				bool isEnabled = true,
				string remoteHost = "" )
		{
			await ExecuteService( DeviceType.InternetGatewayDevice, ServiceType.WanIpConnection, async ( a, b ) =>
			{
				if (string.IsNullOrEmpty( internalHost ))
				{
					internalHost = a.LocalAddress.ToString();
				}

				using (var serviceClient = new Core.Services.WanIpConnectionClient( a.Location.ToString(), b.ControlUrl, b.TypeName ))
				{
					await serviceClient.AddPortMapping( internalHost, internalPort, externalPort, protocol, description, leaseDuration, isEnabled, remoteHost );
				}
			} );
		}

		public async Task DeletePortMapping(
				int externalPort,
				ProtocolType protocol = ProtocolType.Tcp,
				string remoteHost = "" )
		{
			await ExecuteService( DeviceType.InternetGatewayDevice, ServiceType.WanIpConnection, async ( a, b ) =>
			{
				using (var serviceClient = new Core.Services.WanIpConnectionClient( a.Location.ToString(), b.ControlUrl, b.TypeName ))
				{
					await serviceClient.DeletePortMapping( externalPort, protocol, remoteHost );
				}
			} );
		}
	}
}
