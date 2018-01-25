/*
 *  This file is part of upnp-clr.
 *  Copyright (c) 2018 Denis Rozhkov <denis@rozhkoff.com>
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
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AmberSystems.UPnP.Core.Services
{
	public class WanIpConnectionClient : ServiceClient, IDisposable
	{
		protected HttpClient m_httpClient = new HttpClient();


		public WanIpConnectionClient( string host, string path, string serviceUrn )
			: base( host, path, serviceUrn )
		{
		}

		public async Task<string> GetExternalIpAddress()
		{
			var actionName = "GetExternalIPAddress";

			var content = GetContent( actionName );

			var response = await m_httpClient.PostAsync( m_uri, content );
			var responseContent = await response.Content.ReadAsByteArrayAsync();

			var argValue = new XmlSerializable( "NewExternalIPAddress" );
			var argValues = new ServiceArgMap();
			argValues.Add( argValue );

			ParseResponse( responseContent, actionName, argValues );

			return argValue.ToXml();
		}

		public async Task AddPortMapping(
				string internalHost, int internalPort, int externalPort,
				ProtocolType protocol = ProtocolType.Tcp,
				string description = null,
				TimeSpan leaseDuration = new TimeSpan(),
				bool isEnabled = true,
				string remoteHost = "" )
		{
			var actionName = "AddPortMapping";

			if (string.IsNullOrEmpty( description ))
			{
				description = $"{remoteHost}:{externalPort} -> {internalHost}:{internalPort} via upnp-clr";
			}

			var args = new ServiceArgMap();
			args
				.Add( new XmlSerializable( "NewRemoteHost", remoteHost ) )
				.Add( new XmlSerializable( "NewExternalPort", externalPort ) )
				.Add( new XmlSerializable( "NewProtocol", ToProtocolName( protocol ) ) )
				.Add( new XmlSerializable( "NewInternalPort", internalPort ) )
				.Add( new XmlSerializable( "NewInternalClient", internalHost ) )
				.Add( new XmlSerializable( "NewEnabled", isEnabled ) )
				.Add( new XmlSerializable( "NewPortMappingDescription", description ) )
				.Add( new XmlSerializable( "NewLeaseDuration", (int)leaseDuration.TotalSeconds ) )
				;

			var content = GetContent( actionName, args );

			var response = await m_httpClient.PostAsync( m_uri, content );
			var responseContent = await response.Content.ReadAsByteArrayAsync();

			var s = System.Text.Encoding.UTF8.GetString( responseContent );
		}

		public void Dispose()
		{
			m_httpClient.Dispose();
		}
	}
}
