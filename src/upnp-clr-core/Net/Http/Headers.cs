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

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace AmberSystems.UPnP.Core.Net.Http
{
	public class Header
	{
		public class Name
		{
			public const string Host = "HOST";
			public const string Man = "MAN";
			public const string Mx = "MX";
			public const string St = "ST";
			public const string Location = "LOCATION";
		}
	}

	public class Headers : HttpHeaders
	{
		protected Regex s_regexHeader = new Regex( @"^([^:]+):\s*(\S*)" );

		public void ParseAndAdd( string s )
		{
			var regexMatch = s_regexHeader.Match( s );

			if (regexMatch.Success)
			{
				Add( regexMatch.Groups[1].Value.Trim(), regexMatch.Groups[2].Value.Trim() );
			}
		}

		public bool TryGet( string name, out string value )
		{
			bool result = false;
			IEnumerable<string> values;

			if ((result = TryGetValues( name, out values )))
			{
				value = values.First();
			}
			else
			{
				value = null;
			}

			return result;
		}
	}
}
