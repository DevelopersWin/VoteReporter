using System.Windows.Input;
using DragonSpark.Aspects.Build;
using DragonSpark.Commands;

namespace DragonSpark.Aspects.Definitions
{
	public sealed class GenericCommandCoreTypeDefinition : TypeDefinitionWithPrimaryMethodBase
	{
		public static IMethods Execute { get; } = new Methods( typeof(ICommand<>), nameof(ICommand.Execute) );

		public static GenericCommandCoreTypeDefinition Default { get; } = new GenericCommandCoreTypeDefinition();
		GenericCommandCoreTypeDefinition() : base( Execute ) {}
	}
}