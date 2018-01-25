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
using System.Net;
using System.Text;

using AmberSystems.UPnP.Core.Exceptions;
using AmberSystems.UPnP.Core.Ssdp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmberSystems.UPnP.Test
{
	[TestClass]
	public class SsdpTest
	{
		[TestMethod]
		public void SsdpMessageCreate()
		{
			var msg = new Message( MessageType.Search );

			Assert.ThrowsException<BadMxUpnpClrException>( () =>
			{
				var b = msg.ToByteArray();
			} );

			{
				var b = msg
					.St( Core.Types.TargetType.All )
					.Mx( TimeSpan.FromSeconds( 2 ) )
					.ToByteArray();

				Assert.AreEqual( "M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ssdp:all\r\n\r\n", Encoding.UTF8.GetString( b ) );
			}
		}

		[TestMethod]
		public void SsdpMessageParse()
		{
			var response =
@"HTTP/1.1 200 OK
CACHE-CONTROL: max-age=120
ST: urn:schemas-upnp-org:device:InternetGatewayDevice:1
USN: uuid:17a12880-1dd2-11b2-9090-c0a0bb81c885::urn:schemas-upnp-org:device:InternetGatewayDevice:1
EXT:
SERVER: D-Link/Russia UPnP/1.1 MiniUPnPd/1.8
LOCATION: http://192.168.0.1:53538/rootDesc.xml
OPT: ""http://schemas.upnp.org/upnp/1/0/""; ns=01
01-NLS: 1
BOOTID.UPNP.ORG: 1
CONFIGID.UPNP.ORG: 1337

";

			var result = Message.Parse( Encoding.ASCII.GetBytes( response ), new System.Net.IPEndPoint( 1, 0 ), IPAddress.Any );
			Assert.AreEqual( Core.Types.TargetType.Device, result.Target.Type );
			Assert.AreEqual( "http://192.168.0.1:53538/rootDesc.xml", result.Location.ToString() );
		}
	}
}
