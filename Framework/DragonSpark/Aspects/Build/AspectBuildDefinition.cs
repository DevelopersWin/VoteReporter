using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class AspectBuildDefinition : SpecificationParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>>, IAspectBuildDefinition
	{
		public AspectBuildDefinition( params IAspectSelector[] sources ) : this( Common<TypeInfo>.Assigned, sources ) {}
		public AspectBuildDefinition( ISpecification<TypeInfo> specification, params IAspectSelector[] sources ) : base( specification, new SourcedAspectProvider<TypeInfo>( sources ).Get ) {}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (TypeInfo)targetElement );

		/*readonly ImmutableArray<IAspectSelector> locators;

		public AspectBuildDefinition( 
			IParameterizedSource<ImmutableArray<ITypeDefinition>, ImmutableArray<IAspectSelector>> locatorSource,
			params ITypeDefinition[] definitions
		) : this( definitions.SelectTypes(), locatorSource.GetFixed( definitions ) ) {}

		// public AspectBuildDefinition( params IAspectInstanceLocator[] locators ) : this( locators.SelectTypes(), locators ) {}

		public AspectBuildDefinition( IEnumerable<Type> types, params IAspectSelector[] sources ) : 
			this( new ValidatingSpecification( sources.SelectTypes().Distinct().ToImmutableArray(), types.Distinct().ToArray() ).ToSpecificationDelegate(), sources ) {}

		[UsedImplicitly]
		public AspectBuildDefinition( Func<Type, bool> specification, params IAspectSelector[] sources ) : base( specification )
		{
			this.locators = sources.ToImmutableArray();
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
		}*/

	}

	public sealed class AspectLocatorFactory<TType, TMethod> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectSelector>
		where TType : IAspect 
		where TMethod : IAspect
	{
		public static AspectLocatorFactory<TType, TMethod> Default { get; } = new AspectLocatorFactory<TType, TMethod>();
		AspectLocatorFactory() {}

		public override IEnumerable<IAspectSelector> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			TypeAspectLocatorFactory<TType>.Default.Yield( parameter ).Concat( MethodAspectLocatorFactory<TMethod>.Default.Yield( parameter ) );
	}

	public sealed class TypeAspectLocatorFactory<T> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectSelector>
		where T : IAspect
	{
		public static TypeAspectLocatorFactory<T> Default { get; } = new TypeAspectLocatorFactory<T>();
		TypeAspectLocatorFactory() {}

		public override IEnumerable<IAspectSelector> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			parameter.Select( definition => new TypeAspectSelector<T>( definition ) );
	}

	public sealed class MethodAspectLocatorFactory<T> : ParameterizedItemSourceBase<ImmutableArray<ITypeDefinition>, IAspectSelector>
		where T : IAspect
	{
		public static MethodAspectLocatorFactory<T> Default { get; } = new MethodAspectLocatorFactory<T>();
		MethodAspectLocatorFactory() {}

		public override IEnumerable<IAspectSelector> Yield( ImmutableArray<ITypeDefinition> parameter ) => 
			parameter.AsEnumerable().Concat().Select( store => new MethodAspectSelector<T>( store ) );
	}
}