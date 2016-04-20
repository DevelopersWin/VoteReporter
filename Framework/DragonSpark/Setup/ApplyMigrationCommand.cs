using DragonSpark.Extensions;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Setup
{
	public class ApplyMigrationCommand : Command<MigrationParameter<IServiceProvider>>
	{
		public static ApplyMigrationCommand Instance { get; } = new ApplyMigrationCommand();

		protected override void OnExecute( MigrationParameter<IServiceProvider> parameter )
		{
			var source = parameter.To.Get<IServiceProviderMigrationCommandSource>() ?? ServiceProviderMigrationCommandFactory.Instance;
			var command = source.Create( parameter.From );
			command.Run( parameter );
		}
	}
}