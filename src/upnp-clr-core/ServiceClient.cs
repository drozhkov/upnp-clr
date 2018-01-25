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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace AmberSystems.UPnP.Core
{
	public class XmlSerializable
	{
		public string Name { get; protected set; }
		protected string m_value;


		public XmlSerializable( string name, string value = null )
		{
			this.Name = name;
			m_value = value;
		}

		public XmlSerializable( string name, int value )
			: this( name, value.ToString() )
		{
		}

		public XmlSerializable( string name, bool value )
			: this( name, value ? 1 : 0 )
		{
		}

		public virtual void Deserialize( XmlNode node )
		{
			var valueNode = node.SelectSingleNode( $"//*[local-name()='{this.Name}']" );
			m_value = valueNode.InnerXml;
		}

		public virtual string ToXml()
		{
			return m_value;
		}
	}

	public class ServiceArgMap : Dictionary<string, XmlSerializable>
	{
		public ServiceArgMap Add( XmlSerializable arg )
		{
			Add( arg.Name, arg );

			return this;
		}

		public void Deserialize( XmlNode node )
		{
			foreach (var arg in this.Values)
			{
				arg.Deserialize( node );
			}
		}
	}

	public abstract class ServiceClient
	{
		protected Uri m_uri;
		protected string m_serviceUrn;


		public ServiceClient( string host, string path, string serviceUrn )
		{
			m_uri = new Uri( new Uri( host ), path );
			m_serviceUrn = serviceUrn;
		}

		protected byte[] GetBody( string actionName, ServiceArgMap args = null )
		{
			XmlDocument doc = new XmlDocument();

			var nodeEnvelope = doc.CreateElement( "s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/" );

			var attr = doc.CreateAttribute( "s", "encodingStyle", nodeEnvelope.NamespaceURI );
			attr.Value = "http://schemas.xmlsoap.org/soap/encoding/";
			nodeEnvelope.Attributes.Append( attr );

			var nodeBody = doc.CreateElement( "s", "Body", nodeEnvelope.NamespaceURI );

			var nodeAction = doc.CreateElement( "u", actionName, m_serviceUrn );

			if (args != null)
			{
				foreach (var arg in args)
				{
					var nodeArg = doc.CreateElement( arg.Key );
					nodeArg.InnerXml = arg.Value.ToXml();
					nodeAction.AppendChild( nodeArg );
				}
			}

			nodeBody.AppendChild( nodeAction );
			nodeEnvelope.AppendChild( nodeBody );

			doc.AppendChild( nodeEnvelope );

			using (var stream = new MemoryStream())
			using (var writer = XmlWriter.Create( stream, new XmlWriterSettings() { Encoding = new UTF8Encoding( false ) } ))
			{
				doc.Save( writer );
				writer.Flush();

				return stream.ToArray();
			}
		}

		protected HttpContent GetContent( string actionName, ServiceArgMap args = null )
		{
			var content = new ByteArrayContent( GetBody( actionName, args ) );
			content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue( "text/xml" );
			content.Headers.ContentType.CharSet = "\"utf-8\"";
			content.Headers.Add( "SOAPACTION", $"\"{m_serviceUrn}#{actionName}\"" );

			return content;
		}

		protected void ParseResponse( byte[] body, string actionName, ServiceArgMap argValues )
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml( Encoding.UTF8.GetString( body ) );

			foreach (var argValueName in argValues.Keys.ToArray())
			{
				var node = doc.SelectSingleNode( $"//*[local-name()='{actionName}Response']" );
				argValues.Deserialize( node );
			}
		}

		public static string ToProtocolName( ProtocolType protocol )
		{
			switch (protocol)
			{
				case ProtocolType.Tcp:
					return "TCP";

				case ProtocolType.Udp:
					return "UDP";

				default:
					return "?PROTO";
			}
		}
	}
}
