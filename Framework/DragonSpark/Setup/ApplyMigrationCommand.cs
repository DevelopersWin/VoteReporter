namespace DragonSpark.Setup
{
	/*public class ApplyMigrationCommand : CommandBase<MigrationParameter<IServiceProvider>>
	{
		public static ApplyMigrationCommand Default { get; } = new ApplyMigrationCommand();

		public override void Execute( MigrationParameter<IServiceProvider> parameter )
		{
			var source = parameter.To.Get<IServiceProviderMigrationCommandSource>() ?? ServiceProviderMigrationCommandFactory.Default;
			var command = source.Create( parameter.From );
			command.Execute( parameter );
		}
	}*/
}