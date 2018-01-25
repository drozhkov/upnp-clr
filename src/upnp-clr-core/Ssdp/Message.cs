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

using AmberSystems.UPnP.Core.Exceptions;
using AmberSystems.UPnP.Core.Types;

namespace AmberSystems.UPnP.Core.Ssdp
{
	public enum MessageType
	{
		_undefined = 0,
		Search = 1,
		Notify,
		Response
	}

	public class Message : Result
	{
		protected class Man
		{
			public const string Discover = "\"ssdp:discover\"";
		}

		protected MessageType m_type;

		protected TimeSpan m_mx;


		public Message( MessageType type = MessageType.Search )
		{
			m_type = type;

			this.Host = EndPoint.SiteLocal;
		}

		public static Message Parse( byte[] data, IPEndPoint sourceAddress, IPAddress localAddress )
		{
			Message result = null;

			var httpMessage = Net.Http.Message.Parse( data );

			if (httpMessage.ResponseCode != 0)
			{
				result = new Message( MessageType.Response );
				result.LocalAddress = localAddress;
				result.Host = sourceAddress;
				result.Target = Target.Parse( httpMessage.GetHeaderValue( Net.Http.Header.Name.St ) );
				result.Location = new Uri( httpMessage.GetHeaderValue( Net.Http.Header.Name.Location ) );
			}
			else
			{
				throw new NotImplementedException();
			}

			return result;
		}

		public TimeSpan Mx()
		{
			return m_mx;
		}

		public Message Mx( TimeSpan t )
		{
			m_mx = t;

			return this;
		}

		public Message St( TargetType type )
		{
			if (type != TargetType.All && type != TargetType.RootDevice)
			{
				throw new ArgumentUpnpClrException();
			}

			this.Target = new Target( type );

			return this;
		}

		public byte[] ToByteArray()
		{
			var httpMessage = new Net.Http.Message();

			switch (m_type)
			{
				case MessageType.Search:
					httpMessage.SetRequestMethodName( Net.Http.Request.MethodName.Search )
						.SetRequestPath( "*" )
						.AddHeader( Net.Http.Header.Name.Host, ToString( this.Host ) )
						.AddHeader( Net.Http.Header.Name.Man, Man.Discover )
						.AddHeader( Net.Http.Header.Name.Mx, ToString( m_mx ) )
						.AddHeader( Net.Http.Header.Name.St, ToString( this.Target ) );
					break;

				default:
					throw new NotImplementedException();
			}

			return httpMessage.ToByteArray();
		}

		protected string ToString( IPEndPoint ip )
		{
			return ip.ToString();
		}

		protected string ToString( Target target )
		{
			return target.ToString();
		}

		protected string ToString( TimeSpan ts )
		{
			if (ts.TotalSeconds < 2)
			{
				throw new BadMxUpnpClrException();
			}

			return ((int)ts.TotalSeconds).ToString();
		}
	}
}
