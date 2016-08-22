using DragonSpark.Activation;
using System;

namespace DragonSpark.Sources
{
	public class ScopeContext : FixedSource<object>
	{
		readonly Func<object> defaultScope;

		public ScopeContext() : this( Execution.Context ) {}

		public ScopeContext( ISource<ISource> defaultScope ) : this( defaultScope.Delegate() ) {}

		public ScopeContext( Func<object> defaultScope )
		{
			this.defaultScope = defaultScope;
		}

		public override object Get() => SourceCoercer<object>.Default.Coerce( base.Get() ) ?? defaultScope();
	}
}