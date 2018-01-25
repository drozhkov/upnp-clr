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

namespace AmberSystems.UPnP.Core.Exceptions
{
	public class UpnpClrException : Exception
	{
		public UpnpClrException()
		{
		}

		public UpnpClrException( string message )
			: base( message )
		{
		}
	}

	public class ArgumentUpnpClrException : UpnpClrException
	{
	}

	public class BadMxUpnpClrException : UpnpClrException
	{
	}

	public class FormatUpnpClrException : UpnpClrException
	{
		public FormatUpnpClrException( string message )
			: base( message )
		{
		}
	}

	public class ServiceFaultUpnpClrException : UpnpClrException
	{
		public ServiceFaultUpnpClrException( string message )
			: base( message )
		{
		}
	}

	public class ServiceErrorUpnpClrException : ServiceFaultUpnpClrException
	{
		public int ErrorCode { get; protected set; }


		public ServiceErrorUpnpClrException( int errorCode )
			: base( errorCode.ToString() )
		{
			this.ErrorCode = errorCode;
		}
	}
}
