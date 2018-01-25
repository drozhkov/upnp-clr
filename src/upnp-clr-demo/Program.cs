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
using System.Linq;

namespace AmberSystems.UPnP.Demo
{
	public class Program
	{
		public static void Main( string[] args )
		{
			try
			{
				var client = new UpnpClient();
				var result = client.Discover( Core.Types.TargetType.All ).Result;

				foreach (var @interface in result.GetInterfaces())
				{
					Console.WriteLine( $"interface: {@interface}" );

					foreach (var type in result.GetTargetTypes( @interface ))
					{
						Console.WriteLine( $"- type: {type}" );

						foreach (var target in result.GetTargets( @interface, type ))
						{
							Console.WriteLine( $"-- target: {target.Target}" );
							Console.WriteLine( $"--- location: {target.Location}" );

							Console.WriteLine();
						}
					}
				}

				var extAddrList = client.GetExternalAddressList().Result;
				Console.WriteLine( $"external address(es): {extAddrList.Aggregate( ( a, b ) => a + ", " + b )}" );

				//client.AddPortMapping( 80, 81 ).Wait();
			}
			catch (Exception x)
			{
				Console.WriteLine( x );
			}
			finally
			{
				Console.WriteLine( "press any key..." );
				Console.ReadKey( true );
			}
		}
	}
}
