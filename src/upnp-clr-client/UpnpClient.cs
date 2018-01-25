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


			public void Add( IPAddress address, byte[] response, IPEndPoint remoteEp )
			{
				var result = Message.Parse( response, remoteEp, address );

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
			addressList.AddRange( Dns.GetHostEntryAsync( Dns.GetHostName() )
				.Result
				.AddressList
				.Where( a => a.AddressFamily == AddressFamily.InterNetwork ) );

			var message = new Message( MessageType.Search )
				.Mx( TimeSpan.FromSeconds( 3 ) )
				.St( targetType );

			var messageBin = message.ToByteArray();

			DiscoveryResult result = new DiscoveryResult();
			List<Task> tasks = new List<Task>();

			foreach (var a in addressList)
			{
				var task = Task.Run( () =>
				{
					using (var client = new UdpClient( new IPEndPoint( a, 0 ) ))
					{
						client.Ttl = ttl;

						client.SendAsync( messageBin, messageBin.Length, message.Host );
						client.SendAsync( messageBin, messageBin.Length, message.Host );

						while (true)
						{
							var receiveTask = client.ReceiveAsync();

							if (!receiveTask.Wait( (int)(message.Mx().TotalMilliseconds * ttl) ))
							{
								return;
							}

							var taskResult = receiveTask.Result;

							try
							{
								result.Add( a, taskResult.Buffer, taskResult.RemoteEndPoint );
							}
							catch (Exception x)
							{
							}
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
				throw new UpnpException();
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

		public async Task AddPortMapping(
				int internalPort, int externalPort,
				string internalHost = null, ProtocolType protocol = ProtocolType.Tcp,
				string description = null,
				TimeSpan leaseDuration = new TimeSpan(),
				bool isEnabled = true,
				string remoteHost = "" )
		{
			List<string> result = new List<string>();

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
	}
}
