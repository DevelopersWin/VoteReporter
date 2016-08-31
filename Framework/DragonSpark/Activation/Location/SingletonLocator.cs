using DragonSpark.Composition;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Composition;

namespace DragonSpark.Activation.Location
{
	public sealed class SingletonLocator : DelegatedParameterizedSource<Type, object>, ISingletonLocator
	{
		readonly static ISpecification<Type> Specification = Specifications<Type>.Assigned.And( ContainsSingletonPropertySpecification.Default );
		
		[Export]
		public static ISingletonLocator Default { get; } = new SingletonLocator();
		SingletonLocator() : this( new Source( SingletonDelegates.Default.Get ).With( Specification ) ) {}

		public SingletonLocator( Func<Type, Func<object>> inner ) : this( new Source( inner ) ) {}

		SingletonLocator( IParameterizedSource<Type, object> source ) : base( source.With( ConventionTypeSelector.Default ).ToSourceDelegate().Cache() ) {}

		sealed class Source : ParameterizedSourceBase<Type, object>
		{
			readonly Func<Type, Func<object>> source;

			public Source( Func<Type, Func<object>> source )
			{
				this.source = source;
			}

			public override object Get( Type parameter ) => source( parameter )?.Invoke();
		}

		object IServiceProvider.GetService( Type serviceType ) => Get( serviceType );
	}
}