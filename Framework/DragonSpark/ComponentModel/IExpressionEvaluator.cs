using NReco.Linq;
using System.Collections.Generic;

namespace DragonSpark.ComponentModel
{
	public interface IExpressionEvaluator
	{
		object Evaluate( object context, string expression );
	}

	public class ExpressionEvaluator : IExpressionEvaluator
	{
		public static ExpressionEvaluator Default { get; } = new ExpressionEvaluator();

		const string Context = "context";

		public object Evaluate( object context, string expression ) => new LambdaParser().Eval( string.Concat( Context, ".", expression.TrimStart( '.' ) ), new Dictionary<string, object> { { Context, context } } );
	}
}