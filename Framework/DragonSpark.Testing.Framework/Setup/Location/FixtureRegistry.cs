using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Testing.Framework.Setup.Location
{
	sealed class FixtureRegistry : InstanceServiceProvider
	{
		readonly IFixture fixture;

		readonly GenericMethodCommands commands;

		public FixtureRegistry( [Required]IFixture fixture )
		{
			this.fixture = fixture;
			commands = new GenericMethodCommands( this );
		}

		/*public bool IsRegistered( Type type ) => registered.Contains( type );

		public void Register( [Required]MappingRegistrationParameter parameter )
		{
			fixture.Customizations.Insert( 0, new TypeRelay( parameter.RequestedType, parameter.MappedTo ) );
			new[] { parameter.RequestedType, parameter.MappedTo }.Distinct().Each( registered.Ensure );
		}*/

		//public void Register( [Required]InstanceRegistrationParameter parameter ) => Invoke( parameter.RequestedType, nameof(RegisterInstance), parameter.Instance );

		/*protected override void OnAdd( object entry )
		{
			
			base.OnAdd( entry );
		}*/

		public override void Add( InstanceRegistrationRequest request )
		{
			commands[nameof(RegisterInstance)].Make( request.RequestedType ).Invoke( request.Instance );
			base.Add( request );
		}

		void RegisterInstance<T>( [Required]T instance ) => fixture.Inject( instance );

		// public void RegisterFactory( [Required]FactoryRegistrationParameter parameter ) => Invoke( parameter.RequestedType, nameof(RegisterFactory), parameter.Factory );
		
		/*void RegisterFactory<T>( [Required]Func<object> factory )
		{
			var convert = factory.Convert<T>();
			fixture.Register( convert );
			registered.Ensure( typeof(T) );
		}*/
	}
}