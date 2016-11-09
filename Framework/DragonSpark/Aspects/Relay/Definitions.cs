using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	sealed class Definitions : AspectBuildDefinition
	{
		/*readonly ISpecification<Type> specification;*/
		public static Definitions Default { get; } = new Definitions();
		Definitions() : this( ApplyCommandRelayDefinition.Default, ApplySourceRelayDefinition.Default, ApplySpecificationRelayDefinition.Default ) {}
		Definitions( params IAspectBuildDefinition[] definitions ) : base( definitions.Concat().Fixed() ) {}

		/*readonly ImmutableArray<IRelayAspectSource> sources;

		public Definitions( params IRelayAspectSource[] sources ) : this( new AnySpecification<Type>( sources.SelectTypes().Select( TypeAssignableSpecification.Defaults.Get ).Fixed() ), sources ) {}

		[UsedImplicitly]
		public Definitions( ISpecification<Type> specification, params IRelayAspectSource[] sources )
		{
			this.specification = specification;
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<AspectInstance> Yield( Type parameter )
		{
			foreach ( var source in sources )
			{
				var instance = source.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}

		public bool IsSatisfiedBy( Type parameter ) => specification.IsSatisfiedBy( parameter );

		IEnumerable<AspectInstance> IParameterizedSource<Type, IEnumerable<AspectInstance>>.Get( Type parameter ) => ProvideAspects( parameter );*/
	}
}