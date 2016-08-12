using DragonSpark.Activation;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.Linq;
using Type = System.Type;

namespace DragonSpark.Setup
{
	[Persistent]
	public class AllTypesOfFactory : ParameterizedSourceBase<Type, Array>
	{
		readonly ImmutableArray<Type> types;
		readonly IActivator activator;

		public AllTypesOfFactory( ImmutableArray<Type> types, IActivator activator )
		{
			this.types = types;
			this.activator = activator;
		}

		public ImmutableArray<T> Create<T>() => ImmutableArray.CreateRange( Get( typeof(T) ).Cast<T>() );

		public override Array Get( Type parameter ) => activator.ActivateMany<object>( parameter, types.ToArray() ).ToArray();
	}

	/*[Persistent]
	public class AllTypesOfFactory : FactoryBase<Type, Array>
	{
		readonly Func<ImmutableArray<Type>> typeSource;
		readonly IActivator activator;
		readonly static Func<ImmutableArray<Type>> TypeSource = ApplicationTypes.Instance.Get;

		public AllTypesOfFactory( IActivator activator ) : this( TypeSource, activator ) {}

		public AllTypesOfFactory( Func<ImmutableArray<Type>> typeSource, IActivator activator )
		{
			this.typeSource = typeSource;
			this.activator = activator;
		}

		public T[] Create<T>() => Create( typeof(T) ).Cast<T>().ToArray();

		public override Array Create( Type parameter ) => activator.ActivateMany<object>( parameter, typeSource().ToArray() );
	}*/
}