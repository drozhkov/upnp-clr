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

using System.Net;

namespace AmberSystems.UPnP.Core.Ssdp
{
	public class EndPoint
	{
		public const int Port = 1900;

		public static readonly IPEndPoint SiteLocal = new IPEndPoint( IPAddress.Parse( "239.255.255.250" ), Port );
		public static readonly IPEndPoint LinkLocalV6 = new IPEndPoint( IPAddress.Parse( "FF02::C" ), Port );
		public static readonly IPEndPoint SiteLocalV6 = new IPEndPoint( IPAddress.Parse( "FF05::C" ), Port );
		public static readonly IPEndPoint OrgLocalV6 = new IPEndPoint( IPAddress.Parse( "FF08::C" ), Port );
		public static readonly IPEndPoint GlobalV6 = new IPEndPoint( IPAddress.Parse( "FF0E::C" ), Port );
	}
}
