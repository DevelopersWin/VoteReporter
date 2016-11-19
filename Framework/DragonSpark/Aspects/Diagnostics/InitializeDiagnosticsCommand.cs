using DragonSpark.Commands;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Diagnostics
{
	public sealed class InitializeDiagnosticsCommand : SpecificationCommand<object>
	{
		public static InitializeDiagnosticsCommand Default { get; } = new InitializeDiagnosticsCommand();
		InitializeDiagnosticsCommand() : base( new OnlyOnceSpecification(), new CompositeCommand( ConfigurationCommands.Default ).Execute ) {}

		public override void Execute( object parameter = null )
		{
			base.Execute( parameter );

			/*var log = Logger.Default.Get( this );
			for ( int i = 0; i < 1000; i++ )
			{
				log.Information( $"TESTING THIS: {i}" );
			}*/
		}
	}
}