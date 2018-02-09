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


using System.Xml;
using AmberSystems.UPnP.Core;
using AmberSystems.UPnP.Core.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmberSystems.UPnP.Test
{
	[TestClass]
	public class ServiceClientTest
	{
		class TestServiceClient : ServiceClient
		{
			public void TestHandleFault( XmlDocument doc )
			{
				HandleFault( doc );
			}
		}

		[TestMethod]
		public void ServiceResponseDeserialize()
		{
			string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<p:PortMappingList xmlns:p=""urn:schemas-upnp-org:gw:WANIPConnection"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""urn:schemas-upnp-org:gw:WANIPConnection http://www.upnp.org/schemas/gw/WANIPConnection-v2.xsd"">
<p:PortMappingEntry>
<p:NewRemoteHost>202.233.2.1</p:NewRemoteHost>
<p:NewExternalPort>2345</p:NewExternalPort>
<p:NewProtocol>TCP</p:NewProtocol>
<p:NewInternalPort>2345</p:NewInternalPort>
<p:NewInternalClient>192.168.1.137</p:NewInternalClient>
<p:NewEnabled>1</p:NewEnabled>
<p:NewDescription>dooom</p:NewDescription>
<p:NewLeaseTime>345</p:NewLeaseTime>
</p:PortMappingEntry>
<p:PortMappingEntry>
<p:NewRemoteHost>134.231.2.11</p:NewRemoteHost>
<p:NewExternalPort>12345</p:NewExternalPort>
<p:NewProtocol>TCP</p:NewProtocol>
<p:NewInternalPort>12345</p:NewInternalPort>
<p:NewInternalClient>192.168.1.137</p:NewInternalClient>
<p:NewEnabled>1</p:NewEnabled>
<p:NewDescription>dooom</p:NewDescription>
<p:NewLeaseTime>345</p:NewLeaseTime>
</p:PortMappingEntry>
</p:PortMappingList>
";
			xml = @"<s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
<s:Body>
<s:Fault>
<faultcode>s:Client</faultcode>
<faultstring>UPnPError</faultstring>
<detail>
<UPnPError xmlns=""urn:schemas-upnp-org:control-1-0"">
<errorCode>402</errorCode>
<errorDescription>Invalid Args</errorDescription>
</UPnPError>
</detail>
</s:Fault>
</s:Body>
</s:Envelope>
";

			var doc = new XmlDocument();
			doc.LoadXml( xml );

			var client = new TestServiceClient();

			var exception = Assert.ThrowsException<ServiceErrorUpnpClrException>( () => client.TestHandleFault( doc ) );
		}
	}
}
