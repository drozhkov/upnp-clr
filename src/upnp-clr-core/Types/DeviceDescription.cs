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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace AmberSystems.UPnP.Core.Types
{
	[XmlRoot( "root", Namespace = @"urn:schemas-upnp-org:device-1-0" )]
	public class DeviceDescription : Serializable
	{
		[XmlElement( ElementName = "URLBase" )]
		public string BaseUrl { get; set; }

		[XmlElement( ElementName = "device" )]
		public Device Device { get; set; }


		public static DeviceDescription Deserialize( string xml )
		{
			return Deserialize<DeviceDescription>( xml );
		}

		public IEnumerable<Device> GetDevices( DeviceType type, Device device = null )
		{
			List<Device> result = new List<Device>();

			if (device == null)
			{
				result.AddRange( GetDevices( type, this.Device ) );
			}
			else
			{
				if (device.Type == type)
				{
					result.Add( device );
				}

				if (device.DeviceList != null)
				{
					foreach (var d in device.DeviceList)
					{
						result.AddRange( GetDevices( type, d ) );
					}
				}
			}

			return result;
		}

		public IEnumerable<Service> GetServices( ServiceType type, Device device = null )
		{
			List<Service> result = new List<Service>();

			if (device == null)
			{
				result.AddRange( GetServices( type, this.Device ) );
			}
			else
			{
				if (device.ServiceList != null)
				{
					foreach (var service in device.ServiceList)
					{
						if (service.Type == type)
						{
							result.Add( service );
						}
					}
				}

				if (device.DeviceList != null)
				{
					foreach (var d in device.DeviceList)
					{
						result.AddRange( GetServices( type, d ) );
					}
				}
			}

			return result;
		}
	}
}
