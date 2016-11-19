using DevelopersWin.VoteReporter.Properties;
using DragonSpark.Runtime.Data;
using DragonSpark.Windows.Runtime.Data;
using Transformer = System.Func<DragonSpark.Windows.Runtime.Data.DataTransformParameter, string>;

namespace DevelopersWin.VoteReporter
{
	public interface IVoteReportContentGenerator
	{
		string Generate( VoteReport report );
	}

	public sealed class VoteReportContentGenerator : IVoteReportContentGenerator
	{
		readonly Transformer transformer;
		readonly ISerializer serializer;
		readonly DocumentFactory factory;

		public VoteReportContentGenerator( DataTransformer transformer, ISerializer serializer ) : this( transformer.Get, serializer, DocumentFactory.Default ) {}

		VoteReportContentGenerator( Transformer transformer, ISerializer serializer, DocumentFactory factory )
		{
			this.transformer = transformer;
			this.serializer = serializer;
			this.factory = factory;
		}

		public string Generate( VoteReport report )
		{
			var stylesheet = factory.Get( Resources.Report );
			var data = serializer.Save( report );
			var source = factory.Get( data );
			var result = transformer( new DataTransformParameter( stylesheet, source ) );
			return result;
		}
	}
}