using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using Ploeh.AutoFixture;

namespace DragonSpark.Testing.Framework.Setup.Location
{
	sealed class FixtureRegistry : InstanceServiceProvider
	{
		readonly IFixture fixture;

		readonly GenericMethodCommands commands;

		public FixtureRegistry( IFixture fixture )
		{
			this.fixture = fixture;
			commands = new GenericMethodCommands( this );
		}

		public override void Add( InstanceRegistrationRequest request )
		{
			commands[nameof(RegisterInstance)].Make( request.RequestedType ).Invoke( request.Instance );
			base.Add( request );
		}

		void RegisterInstance<T>( T instance ) => fixture.Inject( instance );
	}
}