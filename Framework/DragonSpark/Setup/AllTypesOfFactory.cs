using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Setup
{
	[Persistent]
	public class AllTypesOfFactory : FactoryBase<Type, Array>
	{
		readonly Func<ImmutableArray<Type>> typeSource;
		readonly IActivator activator;

		public AllTypesOfFactory( IActivator activator ) : this( ApplicationTypes.Instance.Get, activator ) {}

		public AllTypesOfFactory( Func<ImmutableArray<Type>> typeSource, IActivator activator )
		{
			this.typeSource = typeSource;
			this.activator = activator;
		}

		public T[] Create<T>() => Create( typeof(T) ).Cast<T>().ToArray();

		public override Array Create( Type parameter ) => activator.ActivateMany<object>( parameter, typeSource().AsEnumerable() );
	}
}