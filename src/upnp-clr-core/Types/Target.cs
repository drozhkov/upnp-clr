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
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

using AmberSystems.UPnP.Core.Exceptions;

namespace AmberSystems.UPnP.Core.Types
{
	public enum TargetType
	{
		_undefined = 0,
		All = 16,
		Device = 32,
		RootDevice,
		DeviceId,
		VendorDevice,
		Service = 64,
		VendorService
	}

	public class Target
	{
		protected class String
		{
			public const string All = "ssdp:all";
			public const string RootDevice = "upnp:rootdevice";
			public const string DeviceId = "uuid";
			public const string Urn = "urn";
			public const string UpnpDomain = "schemas-upnp-org";
			public const string Device = "device";
			public const string Service = "service";
		}

		[XmlIgnore]
		public TargetType Type { get; protected set; }

		[XmlIgnore]
		public string VendorDomainName { get; protected set; }
		[XmlIgnore]
		public int Version { get; protected set; }


		protected Target()
		{
		}

		public Target( TargetType type )
		{
			this.Type = type;
		}

		protected virtual void Init( TargetType type )
		{
		}

		protected virtual void Init( DeviceType type, Guid? uuid )
		{
		}

		protected virtual void Init( string domainName, string typeName, int version )
		{
		}

		// TODO: refactor
		public static Target Parse( string s, Target r = null )
		{
			Target result = r;

			var tokens = s.Split( ':' );

			if (tokens.Length < 2 || tokens.Length > 5)
			{
				throw new FormatUpnpClrException( s );
			}

			if (tokens.Length == 2)
			{
				if (tokens[0] == String.DeviceId)
				{
					if (result == null)
					{
						result = new Device();
					}

					result.Init( DeviceType.Id, Guid.Parse( tokens[1] ) );
				}
				else
				{
					if (s == String.All)
					{
						result = new Target( TargetType.All );
					}
					else if (s == String.RootDevice)
					{
						result = new Target( TargetType.RootDevice );
					}
					else
					{
						throw new FormatUpnpClrException( s );
					}
				}
			}
			else if (tokens[0] == String.Urn && tokens.Length == 5)
			{
				var version = int.Parse( tokens[4] );

				if (tokens[2] == String.Device)
				{
					if (result == null)
					{
						result = new Device();
					}

					result.Init( tokens[1], tokens[3], version );
				}
				else if (tokens[2] == String.Service)
				{
					if (result == null)
					{
						result = new Service();
					}

					result.Init( tokens[1], tokens[3], version );
				}
				else
				{
					throw new FormatUpnpClrException( s );
				}
			}
			else
			{
				throw new FormatUpnpClrException( s );
			}

			return result;
		}

		public virtual async Task<T> GetDescription<T>( Uri location ) where T : class
		{
			using (var httpClient = new HttpClient())
			{
				var response = await httpClient.GetAsync( location );
				var responseContent = await response.Content.ReadAsStringAsync();

				return Serializable.Deserialize<T>( responseContent );
			}
		}

		public override string ToString()
		{
			switch (this.Type)
			{
				case TargetType.All:
					return String.All;

				case TargetType.RootDevice:
					return String.RootDevice;
			}

			throw new NotImplementedException();
		}
	}
}
