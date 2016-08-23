using System;
using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Activation
{
	public sealed class Activator : CompositeActivator
	{
		public static ISource<IActivator> Default { get; } = new Scope<IActivator>( Factory.Global( () => new Activator() ) );
		Activator() : base( new Locator(), Constructor.Default ) {}

		public static T Activate<T>( Type type ) => Default.Get().Get<T>( type );

		sealed class Locator : LocatorBase
		{
			readonly static Func<Type, Type> Types = ConventionTypes.Default.Get;

			readonly Func<Type, Type> convention;
			readonly ISingletonLocator singleton;

			public Locator() : this( Types, SingletonLocator.Default ) {}

			Locator( Func<Type, Type> convention, ISingletonLocator singleton )
			{
				this.convention = convention;
				this.singleton = singleton;
			}

			public override object Get( LocateTypeRequest parameter ) => singleton.Get( convention( parameter.RequestedType ) ?? parameter.RequestedType );
		}
	}
}