using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using PostSharp.Aspects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DragonSpark.Aspects.Relay
{
	public class AspectBuildDefinition<TInterface, TAspect> : AspectBuildDefinition, IAspectBuildDefinition where TAspect : IAspect
	{
		readonly Func<object, TAspect> aspectSource;
		readonly ImmutableArray<IAspectSource> sources;

		public AspectBuildDefinition( Type destinationType, Type adapterType, params IAspectSource[] sources ) 
			: this( 
				new GenericAdapterFactory<object, TInterface>( destinationType, adapterType ).To( ParameterConstructor<TInterface, TAspect>.Default ).Get,
				sources
			) {}

		AspectBuildDefinition( /*Type supportedType,*/ Func<object, TAspect> aspectSource, params IAspectSource[] sources ) : base( /*supportedType.Yield(),*/ sources )
		{
			// ReferencedType = supportedType;
			this.aspectSource = aspectSource;
			this.sources = sources.ToImmutableArray();
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );

		// public Type ReferencedType { get; }

		// AspectInstance IParameterizedSource<Type, AspectInstance>.Get( Type parameter ) => locator.Get( parameter );
		public IEnumerator<IAspectSource> GetEnumerator() => sources.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}