using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using PostSharp.Aspects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Build
{
	public class PairedAspectBuildDefinition : AspectBuildDefinition
	{
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IEnumerable<IAspectDefinition>> selectors ) : base( new AspectDefinitionSelector( selectors.TryGet ), selectors.Keys.Fixed() ) {}
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IAspectDefinition> selectors ) : base( new AspectDefinitionSelector( selectors.Yield ), selectors.Keys.Fixed() ) {}
	}

	public class AspectBuildDefinition : ParameterizedItemSourceBase<TypeInfo, AspectInstance>, IAspectBuildDefinition
	{
		readonly ImmutableArray<Type> types;
		readonly ISpecification<TypeInfo> specification;
		readonly IParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?> instanceSource;

		public AspectBuildDefinition( IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector, params ITypeDefinition[] candidates ) 
			: this( selector, candidates.SelectTypes().ToImmutableArray(), candidates ) {}

		AspectBuildDefinition( IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector, ImmutableArray<Type> types, params ITypeDefinition[] candidates ) 
			: this( types, new Cache( selector, types, candidates ) ) {}

		public AspectBuildDefinition( ImmutableArray<Type> types, IParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?> instanceSource ) 
			: this( types, 
				  new DelegatedAssignedSpecification<TypeInfo, ImmutableArray<AspectInstance>?>( instanceSource.ToDelegate() )
				  /*.Or(  )*/, instanceSource ) {}

		[UsedImplicitly]
		public AspectBuildDefinition( ImmutableArray<Type> types, ISpecification<TypeInfo> specification, IParameterizedSource<TypeInfo, ImmutableArray<AspectInstance>?> instanceSource )
		{
			this.types = types;
			this.specification = specification;
			this.instanceSource = instanceSource;
		}

		public bool IsSatisfiedBy( TypeInfo parameter ) => specification.IsSatisfiedBy( parameter );

		public override IEnumerable<AspectInstance> Yield( TypeInfo parameter ) => instanceSource.Get( parameter )?.ToArray();

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (TypeInfo)targetElement );

		public IEnumerator<Type> GetEnumerator() => types.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		sealed class Cache : CacheWithImplementedFactoryBase<TypeInfo, ImmutableArray<AspectInstance>?>
		{
			readonly ISpecification<TypeInfo> specification;
			readonly IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector;
			readonly ImmutableArray<ITypeDefinition> candidates;

			public Cache( IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector, ImmutableArray<Type> types, params ITypeDefinition[] candidates ) : this( new AdapterAssignableSpecification( types.ToArray() ), selector, candidates ) {}

			Cache( ISpecification<TypeInfo> specification, IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectDefinition>> selector, params ITypeDefinition[] candidates )
			{
				this.specification = specification;
				this.selector = selector;
				this.candidates = candidates.ToImmutableArray();
			}

			protected override ImmutableArray<AspectInstance>? Create( TypeInfo parameter )
			{
				foreach ( var candidate in candidates )
				{
					var builder = ImmutableArray.CreateBuilder<AspectInstance>();
					var selectors = selector.GetFixed( candidate );
					foreach ( var item in selectors )
					{
						if ( item.IsSatisfiedBy( parameter ) )
						{
							builder.Add( item.Get( parameter ) );
						}
					}

					if ( builder.Any() )
					{
						return builder.ToImmutable();
					}
				}

				var result = specification.IsSatisfiedBy( parameter ) ? Items<AspectInstance>.Immutable : (ImmutableArray<AspectInstance>?)null;
				return result;
			}
		}
	}

	public class IntroducedAspectSelector<TType, TMethod> : AspectSelector<TType, TMethod>
		where TType : CompositionAspect
		where TMethod : IAspect
	{
		public new static IntroducedAspectSelector<TType, TMethod> Default { get; } = new IntroducedAspectSelector<TType, TMethod>();
		IntroducedAspectSelector() : base( definition => new IntroducedTypeAspectDefinition<TType>( definition ).Yield() ) {}
	}

	public class AspectSelector<TType, TMethod> : CompositeAspectSelector
		where TType : IAspect 
		where TMethod : IAspect
	{
		public static AspectSelector<TType, TMethod> Default { get; } = new AspectSelector<TType, TMethod>();
		AspectSelector() : this( definition => new TypeAspectDefinition<TType>( definition ).Yield() ) {}

		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspectDefinition>> typeSource ) : this( typeSource, MethodAspectSelector<TMethod>.Default.Yield ) {}

		[UsedImplicitly]
		public AspectSelector( Func<ITypeDefinition, IEnumerable<IAspectDefinition>> typeSource, Func<ITypeDefinition, IEnumerable<IAspectDefinition>> methodSource ) 
			: base( typeSource, methodSource ) {}
	}

	public class CompositeAspectSelector : ParameterizedItemSourceBase<ITypeDefinition, IAspectDefinition>
	{
		readonly ImmutableArray<Func<ITypeDefinition, IEnumerable<IAspectDefinition>>> sources;

		public CompositeAspectSelector( params Func<ITypeDefinition, IEnumerable<IAspectDefinition>>[] sources )
		{
			this.sources = sources.ToImmutableArray();
		}

		public override IEnumerable<IAspectDefinition> Yield( ITypeDefinition parameter )
		{
			foreach ( var source in sources )
			{
				foreach ( var aspectDefinition in source( parameter ) )
				{
					yield return aspectDefinition;
				}
			}
		}
	}

	public sealed class MethodAspectSelector<T> : ParameterizedItemSourceBase<ITypeDefinition, IAspectDefinition>
		where T : IAspect
	{
		public static MethodAspectSelector<T> Default { get; } = new MethodAspectSelector<T>();
		MethodAspectSelector() {}

		public override IEnumerable<IAspectDefinition> Yield( ITypeDefinition parameter ) => 
			parameter.Select( store => new MethodAspectDefinition<T>( store ) );
	}
}