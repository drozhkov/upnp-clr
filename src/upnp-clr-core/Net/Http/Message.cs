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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AmberSystems.UPnP.Core.Net.Http
{
	public class Request
	{
		public class MethodName
		{
			public const string Search = @"M-SEARCH";
		}
	}

	public class Message
	{
		protected const string Eol = "\r\n";

		public int ResponseCode { get; protected set; }

		protected static Regex s_regexStatus = new Regex( @"^HTTP\/(\S+)\s+(\d+)\s+(\S+)$", RegexOptions.IgnoreCase );

		protected string m_methodName;
		protected string m_path;

		protected Headers m_headers = new Headers();
		protected List<byte> m_body = new List<byte>();

		protected Version m_version = new Version( 1, 1 );

		protected Encoding m_encoding = Encoding.UTF8;


		public Message()
		{
		}

		public Message( string methodName, string path )
		{
			m_methodName = methodName;
			m_path = path;
		}

		public Message AddHeader( string name, string value )
		{
			m_headers.Add( name, value );

			return this;
		}

		public Message SetRequestMethodName( string methodName )
		{
			m_methodName = methodName;

			return this;
		}

		public Message SetRequestPath( string path )
		{
			m_path = path;

			return this;
		}

		public string GetHeaderValue( string name )
		{
			return m_headers.GetValues( name ).First();
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			result.Append( $"{m_methodName} {m_path} HTTP/{m_version.ToString( 2 )}" + Eol );
			result.Append( m_headers.ToString() );
			result.Append( Eol );

			return result.ToString();
		}

		public byte[] ToByteArray()
		{
			List<byte> result = new List<byte>();
			result.AddRange( m_encoding.GetBytes( ToString() ) );
			result.AddRange( m_body );

			return result.ToArray();
		}

		public static Message Parse( byte[] data )
		{
			Message result = null;
			var s = Encoding.ASCII.GetString( data );

			var lines = s.Split( '\n' );

			Match regexMatch = null;
			Headers headers = new Headers();
			bool isValidHeader = false;

			if (lines.Length > 0 && (regexMatch = s_regexStatus.Match( lines[0].TrimEnd() )).Success)
			{
				for (int i = 1; i < lines.Length; i++)
				{
					var line = lines[i].TrimEnd();

					if (string.IsNullOrEmpty( line ))
					{
						isValidHeader = true;
						// body
						break;
					}

					headers.ParseAndAdd( line );
				}
			}

			if (isValidHeader)
			{
				result = new Message();
				result.m_headers = headers;
				result.ResponseCode = int.Parse( regexMatch.Groups[2].Value );
			}

			return result;
		}
	}
}
