using DragonSpark.Activation;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Testing.Framework.Setup.Location
{
	class FixtureRegistry : IServiceRegistry
	{
		readonly IFixture fixture;

		readonly ICollection<Type> registered = new List<Type>();

		public FixtureRegistry( [Required]IFixture fixture )
		{
			this.fixture = fixture;
		}

		public bool IsRegistered( Type type ) => registered.Contains( type );

		public void Register( [Required]MappingRegistrationParameter parameter )
		{
			fixture.Customizations.Insert( 0, new TypeRelay( parameter.RequestedType, parameter.MappedTo ) );
			new[] { parameter.RequestedType, parameter.MappedTo }.Distinct().Each( registered.Ensure );
		}

		public void Register( [Required]InstanceRegistrationParameter parameter ) => Invoke( parameter.RequestedType, nameof(RegisterInstance), parameter.Instance );

		void Invoke( Type type, string name, object parameter ) => GetType().Adapt().Invoke( this, name, type.ToItem(), parameter );

		void RegisterInstance<T>( [Required]T instance )
		{
			fixture.Inject( instance );
			registered.Ensure( typeof(T) );
		}

		public void RegisterFactory( [Required]FactoryRegistrationParameter parameter ) => Invoke( parameter.RequestedType, nameof(RegisterFactory), parameter.Factory );
		
		void RegisterFactory<T>( [Required]Func<object> factory )
		{
			var convert = factory.Convert<T>();
			fixture.Register( convert );
			registered.Ensure( typeof(T) );
		}
	}
}