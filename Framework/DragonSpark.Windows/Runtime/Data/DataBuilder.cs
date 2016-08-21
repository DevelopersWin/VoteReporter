using DragonSpark.Sources.Parameterized;
using System;
using System.Xml;
using System.Xml.XPath;

namespace DragonSpark.Windows.Runtime.Data
{
	public abstract class DocumentFactory<T> : ParameterizedSourceBase<T, IXPathNavigable>
	{
		readonly Action<XmlDocument, T> load;

		protected DocumentFactory( Action<XmlDocument, T> load )
		{
			this.load = load;
		}

		public override IXPathNavigable Get( T parameter )
		{
			var result = new XmlDocument();
			load( result, parameter );
			return result;
		}
	}

	public class DocumentFactory : DocumentFactory<string>
	{
		public static DocumentFactory Default { get; } = new DocumentFactory();
		DocumentFactory() : base( ( document, data ) => document.LoadXml( data ) ) {}
	}

	public class DocumentResourceFactory : DocumentFactory<Uri>
	{
		public static DocumentResourceFactory Default { get; } = new DocumentResourceFactory();
		DocumentResourceFactory() : base( ( document, data ) => document.Load( data.ToString() ) ) {}
	}
}