using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Relay
{
	public interface IAspectSelectors : IItemSource<IAspectSelector>, IParameterizedSource<object, IAspect> {}

	public class AspectSelectors<TInterface, TAspect> : ItemSource<IAspectSelector>, IAspectSelectors where TAspect : IAspect
	{
		readonly Func<object, TAspect> aspectSource;

		public AspectSelectors( Type supportedType, Type adapterType, params IAspectSelector[] selectors ) 
			: this( 
				new GenericAdapterFactory<object, TInterface>( supportedType, adapterType ).To( ParameterConstructor<TInterface, TAspect>.Default ).Get,
				selectors
			) {}

		AspectSelectors( Func<object, TAspect> aspectSource, params IAspectSelector[] selectors ) : base( selectors )
		{
			this.aspectSource = aspectSource;
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );
		
	}
}