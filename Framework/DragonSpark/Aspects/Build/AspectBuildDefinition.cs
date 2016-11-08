using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Aspects.Build
{
	public class AspectBuildDefinition : DelegatedSpecification<Type>, IAspectBuildDefinition
	{
		readonly static Func<IEnumerable<Type>, Func<Type, bool>> SpecificationSource = SpecificationFactory.Default.Get;

		readonly ImmutableArray<IAspectInstanceLocator> locators;

		public AspectBuildDefinition( 
			IParameterizedSource<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectInstanceLocator>> locatorSource,
			params ITypeDefinition[] definitions
			) : this( definitions.SelectTypes(), locatorSource.GetFixed( definitions ) ) {}

		public AspectBuildDefinition( IEnumerable<Type> types, params IAspectInstanceLocator[] locators ) : this( SpecificationSource( types ), locators ) {}

		public AspectBuildDefinition( Func<Type, bool> specification, params IAspectInstanceLocator[] locators ) : base( specification )
		{
			this.locators = locators.ToImmutableArray();
		}

		public IEnumerable<AspectInstance> Get( Type parameter )
		{
			foreach ( var locator in locators )
			{
				var instance = locator.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}
	}

	public sealed class AspectLocatorFactory<TType, TMethod> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectInstanceLocator>
		where TType : IAspect 
		where TMethod : IAspect
	{
		public static AspectLocatorFactory<TType, TMethod> Default { get; } = new AspectLocatorFactory<TType, TMethod>();
		AspectLocatorFactory() {}

		public override IEnumerable<IAspectInstanceLocator> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			TypeAspectLocatorFactory<TType>.Default.Yield( parameter ).Concat( MethodAspectLocatorFactory<TMethod>.Default.Yield( parameter ) );
	}

	public sealed class TypeAspectLocatorFactory<T> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectInstanceLocator>
		where T : IAspect
	{
		public static TypeAspectLocatorFactory<T> Default { get; } = new TypeAspectLocatorFactory<T>();
		TypeAspectLocatorFactory() {}

		public override IEnumerable<IAspectInstanceLocator> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			parameter.Select( definition => new TypeBasedAspectInstanceLocator<T>( definition ) );
	}

	public sealed class MethodAspectLocatorFactory<T> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectInstanceLocator>
		where T : IAspect
	{
		public static MethodAspectLocatorFactory<T> Default { get; } = new MethodAspectLocatorFactory<T>();
		MethodAspectLocatorFactory() {}

		public override IEnumerable<IAspectInstanceLocator> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			parameter.AsEnumerable().Concat().Select( store => new MethodBasedAspectInstanceLocator<T>( store ) );
	}

	/*public class AspectBuildDefinitionFactory : ParameterizedSourceBase<ImmutableArray<ITypeDefinition>, IAspectBuildDefinition> 
	{
		readonly Func<IEnumerable<Type>, Func<Type, bool>> specificationSource;
		readonly Func<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectInstanceLocator>> locatorsSource;

		public AspectBuildDefinitionFactory( Func<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectInstanceLocator>> locatorsSource ) : this( SpecificationSource, locatorsSource ) {}

		public AspectBuildDefinitionFactory( Func<IEnumerable<Type>, Func<Type, bool>> specificationSource, Func<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectInstanceLocator>> locatorsSource )
		{
			this.specificationSource = specificationSource;
			this.locatorsSource = locatorsSource;
		}

		public override IAspectBuildDefinition Get( ImmutableArray<ITypeDefinition> parameter )
		{
			var specification = specificationSource( parameter.Select( definition => definition.ReferencedType ) );
			var locators = locatorsSource( parameter );
			var result = new AspectBuildDefinition( specification, locators.ToArray() );
			return result;
		}
	}*/
}