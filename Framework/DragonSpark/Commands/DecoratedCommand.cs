using DragonSpark.Specifications;

namespace DragonSpark.Commands
{
	public class DecoratedCommand : DecoratedCommand<object>
	{
		public DecoratedCommand( ICommand<object> inner ) : base( inner ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer ) : base( inner, coercer ) {}
		public DecoratedCommand( ICommand<object> inner, ISpecification<object> specification ) : base( inner, specification ) {}
		public DecoratedCommand( ICommand<object> inner, Coerce<object> coercer, ISpecification<object> specification ) : base( inner, coercer, specification ) {}
	}
}