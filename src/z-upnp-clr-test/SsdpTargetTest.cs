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

using AmberSystems.UPnP.Core.Types;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AmberSystems.UPnP.Test
{
	[TestClass]
	public class SsdpTargetTest
	{
		[TestMethod]
		public void SsdpTargetParse()
		{
			var target1 = Target.Parse( "urn:schemas-upnp-org:device:InternetGatewayDevice:1" );
			Assert.AreEqual( TargetType.Device, target1.Type );

			var target2 = Target.Parse( "upnp:rootdevice" );
			Assert.AreEqual( TargetType.RootDevice, target2.Type );

			var target3 = Target.Parse( "urn:schemas-upnp-org:device:WANConnectionDevice:1" );
			Assert.AreEqual( TargetType.Device, target3.Type );

			var target4 = Target.Parse( "urn:schemas-upnp-org:device:WANDevice:1" );
			Assert.AreEqual( TargetType.Device, target4.Type );
		}
	}
}
