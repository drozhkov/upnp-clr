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

using AmberSystems.UPnP.Core.Types;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmberSystems.UPnP.Test
{
	[TestClass]
	public class DescriptionSerializationTest
	{
		[TestMethod]
		public void DeviceDescriptionSerialize()
		{
		}

		[TestMethod]
		public void DeviceDescriptionDeserialize()
		{
			string xml =
@"<?xml version=""1.0""?>
<root xmlns=""urn:schemas-upnp-org:device-1-0"">
	<specVersion>
		<major>1</major>
		<minor>0</minor>
	</specVersion>
	<device>
		<deviceType>urn:schemas-upnp-org:device:InternetGatewayDevice:1</deviceType>
		<friendlyName>Linux Internet Gateway Device</friendlyName>
		<manufacturer>Linux UPnP IGD Project</manufacturer>
		<manufacturerURL>http://linux-igd.sourceforge.net</manufacturerURL>
		<modelName>IGD Version 1.00</modelName>
		<UDN>uuid:75802409-bccb-40e7-8e6c-fa095ecce13e</UDN>
		<iconList>
			<icon>
				<mimetype>image/gif</mimetype>
				<width>118</width>
				<height>119</height>
				<depth>8</depth>
				<url>/ligd.gif</url>
			</icon>
		</iconList>
		<serviceList>
						<service>
								<serviceType>urn:schemas-dummy-com:service:Dummy:1</serviceType>
								<serviceId>urn:dummy-com:serviceId:dummy1</serviceId>
							<controlURL>/dummy</controlURL>
								<eventSubURL>/dummy</eventSubURL>
					<SCPDURL>/dummy.xml</SCPDURL>
						</service>
				</serviceList>
		<deviceList>
			<device>
				<deviceType>urn:schemas-upnp-org:device:WANDevice:1</deviceType>
				<friendlyName>WANDevice</friendlyName>
				<manufacturer>Linux UPnP IGD Project</manufacturer>
				<manufacturerURL>http://linux-igd.sourceforge.net</manufacturerURL>
				<modelDescription>WAN Device on Linux IGD</modelDescription>
				<modelName>Linux IGD</modelName>
				<modelNumber>1.00</modelNumber>
				<modelURL>http://linux-igd.sourceforge.net</modelURL>
				<serialNumber>1.00</serialNumber>
				<UDN>uuid:75802409-bccb-40e7-8e6c-fa095ecce13e</UDN>
				<UPC>Linux IGD</UPC>
				<serviceList>
					<service>
						<serviceType>urn:schemas-upnp-org:service:WANCommonInterfaceConfig:1</serviceType>
						<serviceId>urn:upnp-org:serviceId:WANCommonIFC1</serviceId>
						<controlURL>/upnp/control/WANCommonIFC1</controlURL>
						<eventSubURL>/upnp/control/WANCommonIFC1</eventSubURL>
						<SCPDURL>/gateicfgSCPD.xml</SCPDURL>
					</service>
				</serviceList>
				<deviceList>
					<device>
						<deviceType>urn:schemas-upnp-org:device:WANConnectionDevice:1</deviceType>
						<friendlyName>WANConnectionDevice</friendlyName>
						<manufacturer>Linux UPnP IGD Project</manufacturer>
						<manufacturerURL>http://linux-igd.sourceforge.net</manufacturerURL>
						<modelDescription>WanConnectionDevice on Linux IGD</modelDescription>
						<modelName>Linux IGD</modelName>
						<modelNumber>1.00</modelNumber>
						<modelURL>http://linux-igd.sourceforge.net</modelURL>
						<serialNumber>1.00</serialNumber>
						<UDN>uuid:75802409-bccb-40e7-8e6c-fa095ecce13e</UDN>
						<UPC>Linux IGD</UPC>
						<serviceList>
							<service>
								<serviceType>urn:schemas-upnp-org:service:WANIPConnection:1</serviceType>
								<serviceId>urn:upnp-org:serviceId:WANIPConn1</serviceId>
								<controlURL>/upnp/control/WANIPConn1</controlURL>
								<eventSubURL>/upnp/control/WANIPConn1</eventSubURL>
								<SCPDURL>/gateconnSCPD.xml</SCPDURL>
							</service>
						</serviceList>
					</device>
				</deviceList>
			</device>
		</deviceList>
	</device>
</root>";

			var result = DeviceDescription.Deserialize( xml );
		}
	}
}
