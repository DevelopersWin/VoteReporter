using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Relay
{
	public class RelaySupportDefinition : SupportDefinitionBase
	{
		public RelaySupportDefinition( Func<Type, bool> specification, params IAspectInstanceLocator[] locators ) : base( specification, locators ) {}
	}

	sealed class Relays : AspectProviderBase<Type>, Build.ISupportDefinition
	{
		readonly ISpecification<Type> specification;
		public static Relays Default { get; } = new Relays();
		Relays() : this( CommandDescriptor.Default, SourceDescriptor.Default, SpecificationDescriptor.Default ) {}

		readonly ImmutableArray<IRelayAspectSource> sources;

		public Relays( params IRelayAspectSource[] sources ) : this( new AnySpecification<Type>( sources.Select( source => new DecoratedSpecification<Type>( source ) ).Fixed() ), sources ) {}

		[UsedImplicitly]
		public Relays( ISpecification<Type> specification, params IRelayAspectSource[] sources )
		{
			this.specification = specification;
			this.sources = sources.ToImmutableArray();
		}

		/*public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var descriptor in descriptors )
			{
				var instances = descriptor.Get( parameter ).Fixed();
				if ( instances.Any() )
				{
					return instances;
				}
			}
			return Items<AspectInstance>.Default;
		}*/

		protected override IEnumerable<AspectInstance> Yield( Type parameter )
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

	public interface IRelayAspectSource : ISpecification<Type>, IParameterizedSource<Type, AspectInstance> { }

	public interface ISupportDefinition : Build.ISupportDefinition, ITypeAware, IParameterizedSource<IAspect> {}
	/*

	public class Support<TRelay> : SupportDefinitionBase, ISupportDefinition where TRelay
	{
		readonly Func<object, object> adapterSource;
		readonly Func<object, IAspect> aspectSource;
		
		public Support( Type source, ITypeAware destination, Type adapterType, Type introducedInterface, params IAspectInstanceLocator[] locators ) 
			: this( source,
					new AdapterFactorySource( destination.DeclaringType, adapterType ).Get, 
					ParameterConstructor<object, IAspect>.Make( introducedInterface, typeof(TRelay) ), 
					locators
			) {}

		Support( Type supportedType, Func<object, object> adapterSource, Func<object, IAspect> aspectSource, params IAspectInstanceLocator[] locators )
			: base( new Build.Specification( supportedType ).IsSatisfiedBy, locators )
		{
			this.adapterSource = adapterSource;
			this.aspectSource = aspectSource;
		}

		
		public IAspect Get( object parameter ) => aspectSource( adapterSource( parameter ) );
	}*/
}