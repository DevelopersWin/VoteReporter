namespace DragonSpark.Testing.Framework.IoC
{
	public class AutoDataAttribute : Setup.AutoDataAttribute
	{
		// public AutoDataAttribute() {}

		/*readonly static Func<MethodBase, IApplication> Source = new ApplicationFactory( ServiceProviderFactory.Instance ).Create;

		public AutoDataAttribute() : base( Source ) {}

		protected AutoDataAttribute( Func<MethodBase, IApplication> applicationSource ) : base( applicationSource ) {}*/

		/*public sealed class ApplicationFactory<T> : ApplicationFactory where T : class, ICommand
		{
			public new static ApplicationFactory<T> Instance { get; } = new ApplicationFactory<T>();
			ApplicationFactory() : base( new ApplicationCommandFactory( new ApplyExportedCommandsCommand<T>() ).Create, ServiceProviderFactory.Instance ) {}
		}*/
	}
}
