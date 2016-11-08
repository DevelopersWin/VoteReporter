using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	sealed class Relays : AspectProviderBase<Type>, Build.IAspectBuildDefinition
	{
		readonly ISpecification<Type> specification;
		public static Relays Default { get; } = new Relays();
		Relays() : this( CommandDescriptor.Default, SourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		readonly ImmutableArray<IRelayAspectSource> sources;

		public Relays( params IRelayAspectSource[] sources ) : this( new AnySpecification<Type>( sources.SelectTypes().Select( TypeAssignableSpecification.Defaults.Get ).Fixed() ), sources ) {}

		[UsedImplicitly]
		public Relays( ISpecification<Type> specification, params IRelayAspectSource[] sources )
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

		IEnumerable<AspectInstance> IParameterizedSource<Type, IEnumerable<AspectInstance>>.Get( Type parameter ) => ProvideAspects( parameter );
	}
}