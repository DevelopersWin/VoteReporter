using JetBrains.Annotations;
using PostSharp.Aspects.Advices;

namespace DragonSpark.Aspects.Relay
{
	[IntroduceInterface( typeof(ICommandRelay) )]
	public sealed class ApplyCommandRelay : SpecificationRelayAspectBase, ICommandRelay
	{
		readonly ICommandRelay relay;

		public ApplyCommandRelay() : base( CommandDescriptor.Default ) {}

		[UsedImplicitly]
		public ApplyCommandRelay( ICommandRelay relay ) : base( relay, CommandDescriptor.Default )
		{
			this.relay = relay;
		}

		public void Execute( object parameter ) => relay.Execute( parameter );

		/*sealed class Support : Relay.Support
		{
			public static Support Default { get; } = new Support();
			Support() : base( typeof(ICommand),
				new MethodBasedAspectInstanceLocator<Specification>( CommandTypeDefinition.Default.Validation ),
				new MethodBasedAspectInstanceLocator<Command>( CommandTypeDefinition.Default.Execution ) 
				) {}
		}*/
	}
}