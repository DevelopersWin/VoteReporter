using DragonSpark.Commands;
using JetBrains.Annotations;

namespace DragonSpark.Application.Setup
{
	public sealed class RegisterInstanceCommand : CommandBase<object>
	{
		public static RegisterInstanceCommand Default { get; } = new RegisterInstanceCommand();
		RegisterInstanceCommand() : this( Instances.Default ) {}

		readonly IServiceRepository repository;

		[UsedImplicitly]
		public RegisterInstanceCommand( IServiceRepository repository )
		{
			this.repository = repository;
		}

		public override void Execute( object parameter ) => repository.Add( parameter );
	}
}