using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using System;

namespace DragonSpark.Testing.Framework
{
	/*public class FactoryAttribute : CustomizeAttribute
	{
		readonly Type factoryType;

		public FactoryAttribute( Type factoryType = null )
		{
			this.factoryType = factoryType;
		}

		public override ICustomization GetCustomization( ParameterInfo parameter ) => 
			new RegistrationCustomization( new FactoryRegistration( factoryType ?? SourceTypeLocator.Default.Get( parameter.ParameterType ), parameter.ParameterType ) );
	}*/

	public static class FixtureExtensions
	{
		public static T Create<T>( this IFixture @this, Type type ) => (T)new SpecimenContext( @this ).Resolve( type );

		/*public static T TryCreate<T>( this IFixture @this, Type type )
		{
			try
			{
				var result = @this.Create<T>( type );
				return result;
			}
			catch ( ObjectCreationException )
			{
				return default(T);
			}
		}*/
	}

	/*public class RegisteredAttribute : CustomizeAttribute
	{
		class Customization : CustomizationBase
		{
			readonly Type serviceLocatorType;
			readonly Type registrationType;

			public Customization( [Required, OfType( typeof(IServiceLocator) )]Type serviceLocatorType, [Required]Type registrationType )
			{
				this.serviceLocatorType = serviceLocatorType;
				this.registrationType = registrationType;
			}

			protected override void OnCustomize( IFixture fixture )
			{
				new FreezingCustomization( registrationType ).Customize( fixture );

				var locator = fixture.Create<IServiceLocator>( serviceLocatorType );
				var instance = fixture.Create<object>( registrationType );
				var item = instance.AsTo<Mock, object>( mock => mock.Object ) ?? instance;
				var type = instance is Mock ? registrationType.Adapt().GetInnerType() : registrationType;
				var registry = locator.GetInstance<IServiceRegistry>();
				registry.Register( new InstanceRegistrationParameter( type, item ) );
			}
		}

		public override ICustomization GetCustomization( ParameterInfo parameter )
		{
			var serviceLocatorType = parameter.Member.AsTo<MethodInfo, Type>( x => x.GetParameterTypes().FirstOrDefault( typeof(IServiceLocator).IsAssignableFrom ) ) ?? typeof(IServiceLocator);
			var result = new Customization( serviceLocatorType, parameter.ParameterType );
			return result;
		}
	}*/
}