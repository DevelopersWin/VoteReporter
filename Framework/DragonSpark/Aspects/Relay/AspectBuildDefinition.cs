using DragonSpark.Activation;
using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using PostSharp.Aspects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using DragonSpark.Aspects.Adapters;

namespace DragonSpark.Aspects.Relay
{
	public class AspectBuildDefinition<TInterface, TAspect> : AspectBuildDefinition, IAspectBuildDefinition where TAspect : IAspect
	{
		readonly Func<object, TAspect> aspectSource;
		readonly ImmutableArray<IAspectSelector> sources;

		public AspectBuildDefinition( Type supportedType, Type destinationType, Type adapterType, params IAspectSelector[] sources ) 
			: this( 
				  /*supportedType,*/
				new AdapterFactory<object, TInterface>( destinationType, adapterType ).To( ParameterConstructor<TInterface, TAspect>.Default ).Get,
				sources.Append( new TypeAspectSelector<TAspect>( supportedType ) ).Fixed()
			) {}

		AspectBuildDefinition( /*Type supportedType,*/ Func<object, TAspect> aspectSource, params IAspectSelector[] sources ) : base( /*supportedType.Yield(),*/ sources )
		{
			// ReferencedType = supportedType;
			this.aspectSource = aspectSource;
			this.sources = sources.ToImmutableArray();
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );

		// public Type ReferencedType { get; }

		// AspectInstance IParameterizedSource<Type, AspectInstance>.Get( Type parameter ) => locator.Get( parameter );
		public IEnumerator<IAspectSelector> GetEnumerator() => sources.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}