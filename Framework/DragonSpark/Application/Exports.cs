using DragonSpark.Sources.Scopes;

namespace DragonSpark.Application
{
	public sealed class Exports : Scope<IExportProvider>
	{
		public static Exports Default { get; } = new Exports();
		Exports() : base( () => DefaultExportProvider.Default ) {}
	}
}