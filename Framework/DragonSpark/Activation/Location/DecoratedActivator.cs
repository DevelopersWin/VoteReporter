using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;

namespace DragonSpark.Activation.Location
{
	public class DecoratedActivator : ActivatorBase, ISpecification<Type>
	{
		readonly Func<Type, object> inner;

		public DecoratedActivator( Func<IServiceProvider> provider ) : this( provider.Delegate<object>() ) {}

		public DecoratedActivator( IServiceProvider provider ) : this( provider.GetService ) {}

		public DecoratedActivator( Func<Type, object> inner )
		{
			this.inner = inner;
		}

		/*public virtual object GetService( Type serviceType ) => inner( serviceType );
		public override bool IsSatisfiedBy( Type parameter ) => inner.Target.Accepts( parameter );*/
		public override object Get( Type parameter ) => inner( parameter );

		public bool IsSatisfiedBy( Type parameter ) => inner.Target.Accepts( parameter );
		bool ISpecification.IsSatisfiedBy( object parameter ) => parameter is Type && IsSatisfiedBy( (Type)parameter );
	}
}