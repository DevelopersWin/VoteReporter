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
	public interface IAspectSelectors : IItemSource<IAspectDefinition>, IParameterizedSource<object, IAspect> {}

	public class AspectSelectors<TInterface, TAspect> : ItemSource<IAspectDefinition>, IAspectSelectors where TAspect : IAspect
	{
		readonly Func<object, TAspect> aspectSource;

		public AspectSelectors( Type supportedType, Type adapterType, params IAspectDefinition[] definitions ) 
			: this( 
				new GenericAdapterFactory<object, TInterface>( supportedType, adapterType ).To( ParameterConstructor<TInterface, TAspect>.Default ).Get,
				definitions
			) {}

		AspectSelectors( Func<object, TAspect> aspectSource, params IAspectDefinition[] definitions ) : base( definitions )
		{
			this.aspectSource = aspectSource;
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );
		
	}
}