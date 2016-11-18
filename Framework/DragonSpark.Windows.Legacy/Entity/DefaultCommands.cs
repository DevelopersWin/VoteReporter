using DragonSpark.Commands;

namespace DragonSpark.Windows.Legacy.Entity
{
	public sealed class DefaultCommands : CompositeCommand<DbContextBuildingParameter>
	{
		public static DefaultCommands Default { get; } = new DefaultCommands();
		DefaultCommands() : base( EnableLocalStoragePropertyCommand.Default, new RegisterComplexTypesCommand() ) {}
	}
}