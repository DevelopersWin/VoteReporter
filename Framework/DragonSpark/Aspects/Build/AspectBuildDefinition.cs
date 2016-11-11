using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class AspectBuildDefinition : SpecificationParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>>, IAspectBuildDefinition
	{
		public AspectBuildDefinition( params IAspectSelector[] selectors ) : this( new AnySpecification<TypeInfo>( selectors ), selectors ) {}

		[UsedImplicitly]
		public AspectBuildDefinition( ISpecification<TypeInfo> specification, params IAspectSelector[] selectors ) : base( specification, new SourcedAspectProvider<TypeInfo>( selectors ).Get ) {}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (TypeInfo)targetElement );
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