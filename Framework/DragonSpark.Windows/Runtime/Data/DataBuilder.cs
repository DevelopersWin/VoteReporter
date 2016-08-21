using DragonSpark.Sources.Parameterized;
using PostSharp.Patterns.Contracts;
using System;
using System.Xml;
using System.Xml.XPath;

namespace DragonSpark.Windows.Runtime.Data
{
	public abstract class DocumentFactory<TParameter> : ParameterizedSourceBase<TParameter, IXPathNavigable>
	{
		readonly Action<XmlDocument, TParameter> load;

		protected DocumentFactory( [Required]Action<XmlDocument, TParameter> load )
		{
			this.load = load;
		}

		public override IXPathNavigable Get( TParameter parameter )
		{
			var result = new XmlDocument();
			load( result, parameter );
			return result;
		}
	}

	public class DocumentFactory : DocumentFactory<string>
	{
		public static DocumentFactory Default { get; } = new DocumentFactory();

		public DocumentFactory() : base( ( document, data ) => document.LoadXml( data ) )
		{}
	}

	public class RemoteDocumentFactory : DocumentFactory<Uri>
	{
		public static RemoteDocumentFactory Default { get; } = new RemoteDocumentFactory();

		public RemoteDocumentFactory() : base( ( document, data ) => document.Load( data.ToString() ) )
		{}
	}
}