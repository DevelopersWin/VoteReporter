using System.Collections.Immutable;
using System.Windows.Input;
using DragonSpark.Sources;
using DragonSpark.TypeSystem;

namespace DragonSpark.Application
{
	public sealed class ApplicationCommands : Scope<ImmutableArray<ICommand>>
	{
		public static IScope<ImmutableArray<ICommand>> Default { get; } = new ApplicationCommands();
		ApplicationCommands() : base( () => Items<ICommand>.Immutable ) {}
	}
}