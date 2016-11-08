using DragonSpark.Activation;
using DragonSpark.Aspects.Build;
using DragonSpark.Extensions;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Relay
{
	public class RelayMethodAspectBuildDefinition<TInterface, TAspect> : AspectBuildDefinition, IRelayMethodAspectBuildDefinition, IRelayAspectSource where TAspect : IAspect
	{
		readonly Func<object, TAspect> aspectSource;
		readonly TypeBasedAspectInstanceLocator<TAspect> locator;

		public RelayMethodAspectBuildDefinition( Type supportedType, Type destinationType, Type adapterType, params IAspectInstanceLocator[] locators ) 
			: this( supportedType,
					new AspectFactory<TInterface, TAspect>( destinationType, adapterType ).Get,
					locators
			) {}

		RelayMethodAspectBuildDefinition( Type supportedType, Func<object, TAspect> aspectSource, params IAspectInstanceLocator[] locators ) : base( supportedType.Yield(), locators )
		{
			ReferencedType = supportedType;
			this.aspectSource = aspectSource;
			locator = new TypeBasedAspectInstanceLocator<TAspect>( ReferencedType );
		}

		public IAspect Get( object parameter ) => aspectSource( parameter );

		public Type ReferencedType { get; }

		AspectInstance IParameterizedSource<Type, AspectInstance>.Get( Type parameter ) => locator.Get( parameter );
	}

	public class AspectFactory<TInterface, TAspect> : DelegatedParameterizedSource<object, TAspect> where TAspect : IAspect
	{
		public AspectFactory( Type constructorParameterType, Type resultType ) : base( 
			new AdapterFactory<object, TInterface>( constructorParameterType, resultType )
				.To( ParameterConstructor<TInterface, TAspect>.Default ).Get ) {}
	}
}