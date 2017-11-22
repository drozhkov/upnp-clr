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

namespace AmberSystems.UPnP.Core.Types
{
	public enum DeviceType
	{
		_undefined = 0,
		Unknown,
		Vendor,
		Id,
		InternetGatewayDevice,
		WanConnectionDevice,
		WanDevice
	}

	public class Device : Target
	{
		class DeviceString
		{
			public const string InternetGatewayDevice = "InternetGatewayDevice";
			public const string WanConnectionDevice = "WANConnectionDevice";
			public const string WanDevice = "WANDevice";
		}

		public new DeviceType Type { get; protected set; }

		public Guid Uuid { get; protected set; }
		public string VendorDeviceType { get; protected set; }

		public Device( DeviceType type, Guid? uuid )
		{
			this.Type = type;

			base.Type = type == DeviceType.Id ? TargetType.DeviceId
				: type == DeviceType.Vendor ? TargetType.VendorDevice
				: TargetType.Device;

			if (uuid.HasValue)
			{
				this.Uuid = uuid.Value;
			}
		}

		public Device( DeviceType type )
			: this( type, null )
		{
		}

		public Device( string domainName, string deviceType, int version )
		{
			if (domainName != String.UpnpDomain)
			{
				this.Type = DeviceType.Vendor;
				this.VendorDomainName = domainName;
				this.VendorDeviceType = deviceType;

				base.Type = TargetType.VendorDevice;
			}
			else
			{
				this.Type = ToType( deviceType );

				base.Type = TargetType.Device;
			}

			this.Version = version;
		}

		public static DeviceType ToType( string s )
		{
			DeviceType result = DeviceType.Unknown;

			switch (s)
			{
				case DeviceString.InternetGatewayDevice:
					result = DeviceType.InternetGatewayDevice;
					break;

				case DeviceString.WanConnectionDevice:
					result = DeviceType.WanConnectionDevice;
					break;

				case DeviceString.WanDevice:
					result = DeviceType.WanDevice;
					break;
			}

			return result;
		}

		public static string ToString( DeviceType type )
		{
			string result = "unknown";

			switch (type)
			{
				case DeviceType.InternetGatewayDevice:
					result = DeviceString.InternetGatewayDevice;
					break;

				case DeviceType.WanConnectionDevice:
					result = DeviceString.WanConnectionDevice;
					break;

				case DeviceType.WanDevice:
					result = DeviceString.WanDevice;
					break;
			}

			return result;
		}

		public override string ToString()
		{
			switch (this.Type)
			{
				case DeviceType.Id:
					return string.Format( "{0}:{1}", String.DeviceId, Uuid );

				case DeviceType.Vendor:
					return string.Format( "{0}:{1}:{2}:{3}:{4}",
						String.Urn, VendorDomainName, String.Device, VendorDeviceType, Version );

				default:
					return string.Format( "{0}:{1}:{2}:{3}:{4}",
						String.Urn, String.UpnpDomain, String.Device, ToString( this.Type ), Version );
			}

			throw new NotImplementedException();
		}
	}
}
