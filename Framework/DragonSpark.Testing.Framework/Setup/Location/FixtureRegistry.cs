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
			fixture.Customizations.Add( new TypeRelay( parameter.Type, parameter.MappedTo ) );
			new[] { parameter.Type, parameter.MappedTo }.Distinct().Each( registered.Ensure );
		}

		public void Register( [Required]InstanceRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterInstance), new[] { parameter.Type }, parameter.Instance );

		void RegisterInstance<T>( [Required]T instance )
		{
			fixture.Inject( instance );
			registered.Ensure( typeof(T) );
		}

		public void RegisterFactory( [Required]FactoryRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterFactory), parameter.Type.ToItem(), parameter.Factory );

		void RegisterFactory<T>( [Required]Func<object> factory )
		{
			fixture.Customize<T>( c => c.FromFactory( () => (T)factory() ).OmitAutoProperties() );
			registered.Ensure( typeof(T) );
		}
	}
}