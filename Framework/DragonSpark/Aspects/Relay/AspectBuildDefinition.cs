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
		readonly ImmutableArray<IAspectSelector> sources;

		public AspectBuildDefinition( Type supportedType, Type adapterType, params IAspectSelector[] selectors ) 
			: this( 
				new GenericAdapterFactory<object, TInterface>( supportedType, adapterType ).To( ParameterConstructor<TInterface, TAspect>.Default ).Get,
				selectors
			) {}

		AspectBuildDefinition( /*Type supportedType,*/ Func<object, TAspect> aspectSource, params IAspectSelector[] selectors ) : base( /*supportedType.Yield(),*/ selectors )
		{
			// ReferencedType = supportedType;
			this.aspectSource = aspectSource;
			this.sources = selectors.ToImmutableArray();
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );

		// public Type ReferencedType { get; }

		// AspectInstance IParameterizedSource<Type, AspectInstance>.Get( Type parameter ) => locator.Get( parameter );
		public IEnumerator<IAspectSelector> GetEnumerator() => sources.AsEnumerable().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}