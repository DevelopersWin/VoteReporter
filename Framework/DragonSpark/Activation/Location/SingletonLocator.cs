using DragonSpark.Composition;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Composition;

namespace DragonSpark.Activation.Location
{
	public sealed class SingletonLocator : ActivatorBase, ISingletonLocator
	{
		readonly static ISpecification<Type> Specification = Specifications<Type>.Assigned.And( ContainsSingletonPropertySpecification.Default );
		
		[Export]
		public static ISingletonLocator Default { get; } = new SingletonLocator();
		SingletonLocator() : this( Specification, new Source( SingletonDelegates.Default.Get ) ) {}

		public SingletonLocator( Func<Type, Func<object>> inner ) : this( Specification, inner ) {}
		public SingletonLocator( ISpecification<Type> specification, Func<Type, Func<object>> inner ) : this( specification, new Source( inner ) ) {}
		SingletonLocator( ISpecification<Type> specification, IParameterizedSource<Type, object> source ) : base( specification, source.With( ConventionTypeSelector.Default ).ToCache().ToSourceDelegate() ) {}

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