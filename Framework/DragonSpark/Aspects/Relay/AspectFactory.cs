using DragonSpark.Activation;
using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Relay
{
	public class AspectFactory<TInterface, TAspect> : DelegatedParameterizedSource<object, IAspect>, IAspectFactory where TAspect : IAspect
	{
		public AspectFactory( Type supportedType, Type adapterType ) 
			: base( 
				  new GenericAdapterFactory<object, TInterface>( supportedType, adapterType )
					.To( ParameterConstructor<TInterface, TAspect>.Default )
					.To( CastCoercer<TAspect, IAspect>.Default ).Get ) {}

		/*public ImmutableArray<IAspectSource> Get( ITypeDefinition parameter )
		{
			return Yield().Where( definition => definition.IsSatisfiedBy( parameter ) );
		}*/
	}
}