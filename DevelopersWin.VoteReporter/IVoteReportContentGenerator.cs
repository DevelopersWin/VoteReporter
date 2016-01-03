using DevelopersWin.VoteReporter.Properties;
using DragonSpark.Runtime;
using DragonSpark.Windows.Runtime.Data;
using PostSharp.Patterns.Contracts;

namespace DevelopersWin.VoteReporter
{
	public interface IVoteReportContentGenerator
	{
		string Generate( VoteReport report );
	}

	class VoteReportContentGenerator : IVoteReportContentGenerator
	{
		readonly IDataTransformer transformer;
		readonly ISerializer serializer;
		readonly DocumentFactory factory;

		public VoteReportContentGenerator( IDataTransformer transformer, ISerializer serializer ) : this( transformer, serializer, DocumentFactory.Instance )
		{}

		public VoteReportContentGenerator( [Required]IDataTransformer transformer, [Required]ISerializer serializer, [Required]DocumentFactory factory )
		{
			this.transformer = transformer;
			this.serializer = serializer;
			this.factory = factory;
		}

		public string Generate( VoteReport report )
		{
			var stylesheet = factory.Create( Resources.Report );
			var data = serializer.Save( report );
			var source = factory.Create( data );
			var result = transformer.ToString( stylesheet, source );
			return result;
		}
	}
}