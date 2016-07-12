namespace DragonSpark.Testing.Parts.Development
{
	public partial class Configure
	{
		public static Configure Instance { get; } = new Configure();

		/*[ModuleInitializer( 0 ), Aspects.Runtime, AssemblyInitialize]
		public static void Initialize() => new Configure().Run();*/

		Configure()
		{
			InitializeComponent();
		}
	}
}
