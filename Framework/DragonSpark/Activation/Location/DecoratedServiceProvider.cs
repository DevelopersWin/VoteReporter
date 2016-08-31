using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation.Location
{
	public class DecoratedServiceProvider : SpecificationBase<Type>, IServiceProvider
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( Func<IServiceProvider> provider ) : this( provider.Delegate<object>() ) {}

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
		public override bool IsSatisfiedBy( Type parameter ) => inner.Target.Accepts( parameter );
	}
}