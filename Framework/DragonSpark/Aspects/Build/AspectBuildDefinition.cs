using DragonSpark.Aspects.Definitions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
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
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IEnumerable<IAspectSelector>> selectors ) : base( new AspectSelection( selectors.TryGet ), selectors.Keys.Fixed() ) {}
		public PairedAspectBuildDefinition( IDictionary<ITypeDefinition, IAspectSelector> selectors ) : base( new AspectSelection( selectors.Yield ), selectors.Keys.Fixed() ) {}
	}

	public class AspectBuildDefinition : ParameterizedItemSourceBase<TypeInfo, AspectInstance>, IAspectBuildDefinition
	{
		readonly IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectSelector>> selector;
		readonly IEnumerable<Type> types;
		readonly ISpecification<TypeInfo> specification;
		readonly IParameterizedSource<TypeInfo, ITypeDefinition> definitionSource;

		public AspectBuildDefinition( 
			IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectSelector>> selector, 
			params ITypeDefinition[] candidates ) 
			: 
			this( selector, candidates.SelectTypes(), 
				new FirstSelector<TypeInfo, ITypeDefinition>( 
					info => new Specification( info ).Get,
					candidates.Select( candidate => candidate.Wrap() ).Fixed() ).ToCache() 
				)
		{}

		public AspectBuildDefinition( 
			IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectSelector>> selector,
			IEnumerable<Type> types,
			IParameterizedSource<TypeInfo, ITypeDefinition> definitionSource ) : this( selector, types, new DelegatedAssignedSpecification<TypeInfo, ITypeDefinition>( definitionSource.ToDelegate() ), definitionSource ) {}

		[UsedImplicitly]
		public AspectBuildDefinition( 
			IParameterizedSource<ITypeDefinition, ImmutableArray<IAspectSelector>> selector, 
			IEnumerable<Type> types,
			ISpecification<TypeInfo> specification,
			IParameterizedSource<TypeInfo, ITypeDefinition> definitionSource )
		{
			this.selector = selector;
			this.types = types;
			this.specification = specification;
			this.definitionSource = definitionSource;
		}

		public bool IsSatisfiedBy( TypeInfo parameter ) => specification.IsSatisfiedBy( parameter );

		public override IEnumerable<AspectInstance> Yield( TypeInfo parameter )
		{
			var definition = definitionSource.Get( parameter );
			var selectors = selector.GetFixed( definition );
			var result = new CompositeFactory<TypeInfo, AspectInstance>( selectors ).GetEnumerable( parameter );
			return result;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement ) => this.GetEnumerable( (TypeInfo)targetElement );

		public IEnumerator<Type> GetEnumerator() => types.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		sealed class Specification : ParameterizedSourceBase<ITypeDefinition, bool>
		{
			readonly TypeInfo info;
			public Specification( TypeInfo info )
			{
				this.info = info;
			}

			public override bool Get( ITypeDefinition parameter ) => parameter.IsSatisfiedBy( info );
		}
	}

	public sealed class AspectSelection<TType, TMethod> : ParameterizedItemSourceBase<ITypeDefinition, IAspectSelector>
		where TType : IAspect 
		where TMethod : IAspect
	{
		public static AspectSelection<TType, TMethod> Default { get; } = new AspectSelection<TType, TMethod>();
		AspectSelection() {}

		public override IEnumerable<IAspectSelector> Yield( ITypeDefinition parameter ) => 
			TypeAspectSelection<TType>.Default.Yield( parameter ).Concat( MethodAspectSelection<TMethod>.Default.Yield( parameter ) );
	}

	public sealed class TypeAspectSelection<T> : ParameterizedItemSourceBase<ITypeDefinition, IAspectSelector>
		where T : IAspect
	{
		public static TypeAspectSelection<T> Default { get; } = new TypeAspectSelection<T>();
		TypeAspectSelection() {}

		public override IEnumerable<IAspectSelector> Yield( ITypeDefinition parameter )
		{
			yield return new TypeAspectSelector<T>( parameter );
		}
			
	}

	public sealed class MethodAspectSelection<T> : ParameterizedItemSourceBase<ITypeDefinition, IAspectSelector>
		where T : IAspect
	{
		public static MethodAspectSelection<T> Default { get; } = new MethodAspectSelection<T>();
		MethodAspectSelection() {}

		public override IEnumerable<IAspectSelector> Yield( ITypeDefinition parameter ) => 
			parameter.Select( store => new MethodAspectSelector<T>( store ) );
	}
}