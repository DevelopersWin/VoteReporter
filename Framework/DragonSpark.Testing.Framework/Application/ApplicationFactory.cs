using System.Reflection;
using DragonSpark.Application;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Testing.Framework.Application.Setup;

namespace DragonSpark.Testing.Framework.Application
{
	public sealed class ApplicationFactory : ConfiguringFactory<MethodBase, IApplication>
	{
		public static ApplicationFactory Default { get; } = new ApplicationFactory();
		ApplicationFactory() : base( DefaultCreate, Initialize ) {}

		static void Initialize( MethodBase method ) => ApplicationInitializer.Default.Get().Execute( method );

		static IApplication DefaultCreate( MethodBase _ ) => 
			ApplicationFactory<Application>.Default.Create( MethodTypes.Default, ApplicationCommandSource.Default );
	}
}