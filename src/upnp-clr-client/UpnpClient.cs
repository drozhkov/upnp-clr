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
using AmberSystems.UPnP.Core.Ssdp;
using AmberSystems.UPnP.Core.Types;

namespace AmberSystems.UPnP
{
	public class UpnpClient
	{
		protected ConcurrentDictionary<string, ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>> m_discoveryResults =
			new ConcurrentDictionary<string, ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>>();


		public async Task<KeyValuePair<string, ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>>[]> Discover( TargetType targetType = TargetType.All, short ttl = 2 )
		{
			List<IPAddress> addressList = new List<IPAddress>();
			addressList.AddRange( Dns.GetHostEntryAsync( Dns.GetHostName() )
				.Result
				.AddressList
				.Where( a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ) );

			var message = new Message( MessageType.Search )
				.Mx( TimeSpan.FromSeconds( 3 ) )
				.St( targetType );

			var messageBin = message.ToByteArray();

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

							var result = receiveTask.Result;

							try
							{
								AddDiscoveryResult( a, result.Buffer, result.RemoteEndPoint );
							}
							catch (Exception x)
							{
							}
						}
					}
				} );

				tasks.Add( task );
			}

			await Task.WhenAll( tasks.ToArray() );

			return m_discoveryResults.ToArray();
		}

		protected void AddDiscoveryResult( IPAddress address, byte[] response, IPEndPoint remoteEp )
		{
			var result = Message.Parse( response, remoteEp );

			if (result != null)
			{
				var resultKey = address.ToString();
				var targetDict = m_discoveryResults.GetOrAdd( resultKey, new ConcurrentDictionary<TargetType, ConcurrentDictionary<string, Result>>() );
				var resultDict = targetDict.GetOrAdd( result.Target.Type, new ConcurrentDictionary<string, Result>() );

				resultDict.AddOrUpdate( result.Key(), result, ( key, value ) => result );
			}
		}
	}
}
