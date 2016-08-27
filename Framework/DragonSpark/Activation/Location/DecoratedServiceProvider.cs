using DragonSpark.Application.Setup;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation.Location
{
	public class DecoratedServiceProvider : SpecificationBase<Type>, IServiceProvider, IServiceSpecification
	{
		readonly Func<Type, object> inner;

		public DecoratedServiceProvider( Func<IServiceProvider> provider ) : this( provider.Delegate<object>() ) {}

		public DecoratedServiceProvider( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedServiceProvider( Func<Type, object> inner )
		{
			this.inner = inner;
		}

		public virtual object GetService( Type serviceType ) => inner( serviceType );
		public override bool IsSatisfiedBy( Type parameter )
		{
			var specification = inner.Target as IServiceSpecification ?? inner.Target as ISpecification<Type>;
			var result = specification?.IsSatisfiedBy( parameter ) ?? false;
			return result;
		}
	}
}