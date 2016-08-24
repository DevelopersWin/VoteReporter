using System.Windows.Input;
using DragonSpark.Commands;

namespace DragonSpark.Windows.Entity
{
	public class DefaultCommands : CompositeCommand<DbContextBuildingParameter>
	{
		public static DefaultCommands Default { get; } = new DefaultCommands();

		public DefaultCommands() : base( new ICommand[] { new EnableLocalStoragePropertyCommand(), new RegisterComplexTypesCommand() } ) {}
	}
}