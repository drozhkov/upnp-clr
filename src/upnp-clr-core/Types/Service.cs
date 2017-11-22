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
	public enum ServiceType
	{
		_undefined = 0,
		Unknown,
		Vendor,
		Layer3Forwarding,
		WanCommonInterfaceConfig,
		WanIpConnection
	}

	public class Service : Target
	{
		class ServiceString
		{
			public const string Layer3Forwarding = "Layer3Forwarding";
			public const string WanCommonInterfaceConfig = "WANCommonInterfaceConfig";
			public const string WanIpConnection = "WANIPConnection";
		}

		public new ServiceType Type { get; protected set; }

		public string VendorServiceType { get; protected set; }

		public Service( string domainName, string serviceType, int version )
		{
			if (domainName != String.UpnpDomain)
			{
				this.Type = ServiceType.Vendor;
				this.VendorDomainName = domainName;
				this.VendorServiceType = serviceType;

				base.Type = TargetType.VendorService;
			}
			else
			{
				this.Type = ToType( serviceType );

				base.Type = TargetType.Service;
			}

			this.Version = version;
		}

		public static ServiceType ToType( string s )
		{
			ServiceType result = ServiceType.Unknown;

			switch (s)
			{
				case ServiceString.Layer3Forwarding:
					result = ServiceType.Layer3Forwarding;
					break;

				case ServiceString.WanCommonInterfaceConfig:
					result = ServiceType.WanCommonInterfaceConfig;
					break;

				case ServiceString.WanIpConnection:
					result = ServiceType.WanIpConnection;
					break;
			}

			return result;
		}

		public static string ToString( ServiceType type )
		{
			string result = "unknown";

			switch (type)
			{
				case ServiceType.Layer3Forwarding:
					result = ServiceString.Layer3Forwarding;
					break;

				case ServiceType.WanCommonInterfaceConfig:
					result = ServiceString.WanCommonInterfaceConfig;
					break;

				case ServiceType.WanIpConnection:
					result = ServiceString.WanIpConnection;
					break;
			}

			return result;
		}

		public override string ToString()
		{
			switch (this.Type)
			{
				case ServiceType.Vendor:
					return string.Format( "{0}:{1}:{2}:{3}:{4}",
						String.Urn, VendorDomainName, String.Service, VendorServiceType, Version );

				default:
					return string.Format( "{0}:{1}:{2}:{3}:{4}",
						String.Urn, String.UpnpDomain, String.Service, ToString( this.Type ), Version );
			}

			throw new NotImplementedException();
		}
	}
}
