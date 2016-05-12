using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using PostSharp.Patterns.Threading;

namespace DragonSpark.Windows.TypeSystem
{
	public class LoadPartAssemblyCommand : DragonSpark.TypeSystem.LoadPartAssemblyCommand
	{
		public static LoadPartAssemblyCommand Instance { get; } = new LoadPartAssemblyCommand();

		LoadPartAssemblyCommand() : base( AssemblyLoader.Instance ) {}
	}

	public class AssemblyPathLoader : FactoryBase<string, Assembly[]>
	{
		public static AssemblyPathLoader Instance { get; } = new AssemblyPathLoader();

		protected override Assembly[] CreateItem( string parameter )
		{
			var directoryInfo = new DirectoryInfo( "." );
			var result = directoryInfo.GetFileSystemInfos( parameter ).Where( info => info.Extension == ".dll" ).Select( info => info.FullName ).Select( Assembly.LoadFile ).Fixed();
			return result;
		}
	}

	[Synchronized]
	public class AssemblyInitializer : CommandBase<Assembly>
	{
		public static AssemblyInitializer Instance { get; } = new AssemblyInitializer();

		AssemblyInitializer() : base( new OnlyOnceSpecification() ) {}

		public override void Execute( Assembly parameter ) => parameter.GetModules().Select( module => module.ModuleHandle ).Each( System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor );
	}

	public class AssemblyLoader : DragonSpark.TypeSystem.AssemblyLoader
	{
		public static AssemblyLoader Instance { get; } = new AssemblyLoader();

		AssemblyLoader() : this( AssemblyHintProvider.Instance.Create ) {}

		public AssemblyLoader( Func<Assembly, string> hintSource ) : base( hintSource, AssemblyPathLoader.Instance.Create, AssemblyInitializer.Instance.Run ) {}
	}
}
