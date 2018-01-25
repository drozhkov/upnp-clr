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

using System.IO;
using System.Xml.Serialization;

namespace AmberSystems.UPnP.Core
{
	public abstract class Serializable
	{
		public static T Deserialize<T>( string xml ) where T : class
		{
			using (var reader = new StringReader( xml ))
			{
				var serializer = new XmlSerializer( typeof( T ) );
				var result = serializer.Deserialize( reader );

				return (result as T);
			}
		}
	}
}
