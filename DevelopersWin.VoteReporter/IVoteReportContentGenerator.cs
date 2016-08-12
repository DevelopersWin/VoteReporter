using DevelopersWin.VoteReporter.Properties;
using DragonSpark.Runtime;
using DragonSpark.Windows.Runtime.Data;
using PostSharp.Patterns.Contracts;
using Transformer = System.Func<DragonSpark.Windows.Runtime.Data.DataTransformParameter, string>;

namespace DevelopersWin.VoteReporter
{
	public interface IVoteReportContentGenerator
	{
		string Generate( VoteReport report );
	}

	class VoteReportContentGenerator : IVoteReportContentGenerator
	{
		readonly Transformer transformer;
		readonly ISerializer serializer;
		readonly DocumentFactory factory;

		public VoteReportContentGenerator( DataTransformer transformer, ISerializer serializer ) : this( transformer.Get, serializer, DocumentFactory.Instance ) {}

		protected VoteReportContentGenerator( [Required]Transformer transformer, [Required]ISerializer serializer, [Required]DocumentFactory factory )
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