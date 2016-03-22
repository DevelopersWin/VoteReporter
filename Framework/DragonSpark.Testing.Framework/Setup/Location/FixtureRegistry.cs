using DragonSpark.Activation;
using DragonSpark.Extensions;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.Kernel;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using DragonSpark.Activation.FactoryModel;
using DragonSpark.Runtime.Values;

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
			fixture.Customizations.Insert( 0, new TypeRelay( parameter.Type, parameter.MappedTo ) );
			/*if ( new ApplyRegistration( parameter ).Item )
			{}*/
			new[] { parameter.Type, parameter.MappedTo }.Distinct().Each( registered.Ensure );
		}

		public void Register( [Required]InstanceRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterInstance), new[] { parameter.Type }, parameter.Instance/*, new ApplyRegistration( parameter ).Item*/ );

		void RegisterInstance<T>( [Required]T instance/*, bool applyRegistration*/ )
		{
			fixture.Inject( instance );
			registered.Ensure( typeof(T) );
		}

		public void RegisterFactory( [Required]FactoryRegistrationParameter parameter ) => this.InvokeGenericAction( nameof(RegisterFactory), parameter.Type.ToItem(), parameter.Factory/*, new ApplyRegistration( parameter ).Item*/ );

		void RegisterFactory<T>( [Required]Func<object> factory/*, bool applyRegistration*/ )
		{
			var convert = factory.Convert<T>();
			fixture.Register( convert );
			registered.Ensure( typeof(T) );
		}

		/*public class ApplyRegistration : AssociatedValue<RegistrationParameter, bool>
		{
			public ApplyRegistration( RegistrationParameter instance ) : base( instance, typeof(ApplyRegistration), () => true ) {}
		}*/
	}
}