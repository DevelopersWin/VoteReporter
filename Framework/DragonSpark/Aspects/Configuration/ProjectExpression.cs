using DragonSpark.Sources.Parameterized;
using PostSharp.Extensibility;

namespace DragonSpark.Aspects.Configuration
{
	public class ProjectExpression : ParameterizedSourceBase<IProject, string>
	{
		readonly string expression;

		public ProjectExpression( string expression )
		{
			this.expression = expression;
		}

		public override string Get( IProject parameter ) => parameter.EvaluateExpression( expression ) ?? string.Empty;
	}
}