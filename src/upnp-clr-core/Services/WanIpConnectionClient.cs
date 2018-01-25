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
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml;

namespace AmberSystems.UPnP.Core.Services
{
	public class WanIpConnectionClient : ServiceClient
	{
		public class PortMapping : XmlSerializable
		{
			public string RemoteHost { get; protected set; }
			public int ExternalPort { get; protected set; }
			public ProtocolType Protocol { get; protected set; }
			public int InternalPort { get; protected set; }
			public string InternalHost { get; protected set; }
			public bool IsEnabled { get; protected set; }
			public string Description { get; protected set; }
			public TimeSpan LeaseDuration { get; protected set; }


			public PortMapping()
				: base( "PortMapping" )
			{
			}

			public override void Deserialize( XmlNode node )
			{
				this.RemoteHost = node.SelectSingleNode( "NewRemoteHost" ).InnerXml;
				this.ExternalPort = int.Parse( node.SelectSingleNode( "NewExternalPort" ).InnerXml );
				this.Protocol = ToProtocolType( node.SelectSingleNode( "NewProtocol" ).InnerXml );
				this.InternalPort = int.Parse( node.SelectSingleNode( "NewInternalPort" ).InnerXml );
				this.InternalHost = node.SelectSingleNode( "NewInternalClient" ).InnerXml;
				this.IsEnabled = int.Parse( node.SelectSingleNode( "NewEnabled" ).InnerXml ) == 1;
				this.Description = node.SelectSingleNode( "NewPortMappingDescription" ).InnerXml;
				this.LeaseDuration = TimeSpan.FromSeconds( int.Parse( node.SelectSingleNode( "NewLeaseDuration" ).InnerXml ) );
			}
		}

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

			ParseResponse( responseContent, actionName );
		}

		public async Task DeletePortMapping(
				int externalPort,
				ProtocolType protocol = ProtocolType.Tcp,
				string remoteHost = "" )
		{
			var actionName = "DeletePortMapping";

			var args = new ServiceArgMap();
			args
				.Add( new XmlSerializable( "NewRemoteHost", remoteHost ) )
				.Add( new XmlSerializable( "NewExternalPort", externalPort ) )
				.Add( new XmlSerializable( "NewProtocol", ToProtocolName( protocol ) ) )
				;

			var content = GetContent( actionName, args );

			var response = await m_httpClient.PostAsync( m_uri, content );
			var responseContent = await response.Content.ReadAsByteArrayAsync();

			ParseResponse( responseContent, actionName );
		}

		public async Task<PortMapping> GetGenericPortMappingEntry( int index )
		{
			var actionName = "GetGenericPortMappingEntry";

			var args = new ServiceArgMap();
			args
				.Add( new XmlSerializable( "NewPortMappingIndex", index ) )
				;

			var content = GetContent( actionName, args );

			var response = await m_httpClient.PostAsync( m_uri, content );
			var responseContent = await response.Content.ReadAsByteArrayAsync();

			var result = new PortMapping();
			ParseResponse( responseContent, actionName, new ServiceArgMap().Add( result ) );

			return result;
		}
	}
}
