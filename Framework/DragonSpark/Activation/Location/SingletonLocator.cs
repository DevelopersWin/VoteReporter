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
		public static ISpecification<Type> Specification { get; } = Specifications<Type>.Assigned.And( ContainsSingletonPropertySpecification.Default );
		
		[Export]
		public static ISingletonLocator Default { get; } = new SingletonLocator();
		SingletonLocator() : this( SingletonDelegates.Default.Get ) {}

		public SingletonLocator( Func<Type, Func<object>> inner ) : base( new Source( inner ).With( Specification ).With( ConventionTypeSelector.Default ).ToSourceDelegate().Fix() ) {}

		sealed class Source : ParameterizedSourceBase<Type, object>
		{
			readonly Func<Type, Func<object>> delegateSource;

			public Source( Func<Type, Func<object>> delegateSource )
			{
				this.delegateSource = delegateSource;
			}

			public override object Get( Type parameter ) => delegateSource( parameter )?.Invoke();
		}
	}
}