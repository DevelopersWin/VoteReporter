using System.Xml.XPath;

namespace DragonSpark.Windows.Runtime.Data
{
	public struct DataTransformParameter
	{
		public DataTransformParameter( IXPathNavigable stylesheet, IXPathNavigable source )
		{
			Stylesheet = stylesheet;
			Source = source;
		}

		public IXPathNavigable Source { get; }

		public IXPathNavigable Stylesheet { get; }
	}
}