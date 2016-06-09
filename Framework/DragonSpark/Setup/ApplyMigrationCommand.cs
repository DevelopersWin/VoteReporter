using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Setup
{
	public class ApplyMigrationCommand : CommandBase<MigrationParameter<IServiceProvider>>
	{
		public static ApplyMigrationCommand Instance { get; } = new ApplyMigrationCommand();

		public override void Execute( MigrationParameter<IServiceProvider> parameter )
		{
			var source = parameter.To.Get<IServiceProviderMigrationCommandSource>() ?? ServiceProviderMigrationCommandFactory.Instance;
			var command = source.Create( parameter.From );
			command.Execute( parameter );
		}
	}
}