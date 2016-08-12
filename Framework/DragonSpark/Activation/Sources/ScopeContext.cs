using System;

namespace DragonSpark.Activation.Sources
{
	public class ScopeContext : FixedSource<object>
	{
		readonly Func<object> defaultScope;

		public ScopeContext() : this( (ISource<ISource>)Execution.Context ) {}

		public ScopeContext( ISource<ISource> defaultScope ) : this( defaultScope.Delegate() ) {}

		public ScopeContext( Func<object> defaultScope )
		{
			this.defaultScope = defaultScope;
		}

		public override object Get() => SourceCoercer<object>.Instance.Coerce( base.Get() ) ?? defaultScope();
	}
}