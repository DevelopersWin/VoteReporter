using DragonSpark.Activation;
using DragonSpark.Aspects.Build;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects.Relay
{
	public class SupportedMethodsDefinition<T> : SupportDefinitionBase, ISupportDefinition, IRelayAspectSource where T : ApplyRelayAspectBase
	{
		readonly Func<object, object> adapterSource;
		readonly Func<object, T> aspectSource;
		readonly TypeBasedAspectInstanceLocator<T> locator;

		public SupportedMethodsDefinition( Type supportedType, Type destinationType, Type adapterType, Type introducedInterface, params IAspectInstanceLocator[] locators ) 
			: this( supportedType,
					new AdapterFactorySource( destinationType, adapterType ).Get, 
					ParameterConstructor<object, T>.Make( introducedInterface, typeof(T) ), 
					locators
			) {}

		SupportedMethodsDefinition( Type supportedType, Func<object, object> adapterSource, Func<object, T> aspectSource, params IAspectInstanceLocator[] locators ) : base( new Build.Specification( supportedType ).IsSatisfiedBy, locators )
		{
			DeclaringType = supportedType;
			this.adapterSource = adapterSource;
			this.aspectSource = aspectSource;
			locator = new TypeBasedAspectInstanceLocator<T>( DeclaringType );
		}

		public IAspect Get( object parameter ) => aspectSource( adapterSource( parameter ) );

		/*IEnumerable<AspectInstance> GetMappings( Type parameter )
		{
			foreach ( var locator in locators )
			{
				var instance = locator.Get( parameter );
				if ( instance != null )
				{
					yield return instance;
				}
			}
		}*/

		public Type DeclaringType { get; }

		AspectInstance IParameterizedSource<Type, AspectInstance>.Get( Type parameter ) => locator.Get( parameter );
	}
}