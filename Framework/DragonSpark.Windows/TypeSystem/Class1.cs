using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Threading;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

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

		public override Assembly[] Create( string parameter )
		{
			var directoryInfo = new DirectoryInfo( "." );
			var result = directoryInfo.GetFileSystemInfos( parameter ).Where( info => info.Extension == ".dll" ).Select( info => info.FullName ).Select( Assembly.LoadFile ).Fixed();
			return result;
		}
	}

	[Synchronized]
	[AutoValidation.GenericCommand]
	public class AssemblyInitializer : CommandBase<Assembly>
	{
		readonly static Action<ModuleHandle> Initialize = System.Runtime.CompilerServices.RuntimeHelpers.RunModuleConstructor;
		public static AssemblyInitializer Instance { get; } = new AssemblyInitializer();

		AssemblyInitializer() : base( Specification.Instance ) {}

		public override void Execute( Assembly parameter )
		{
			parameter.GetModules().Select( module => module.ModuleHandle ).ForEach( Initialize );
			if ( !Activated( parameter ) )
			{
				DragonSpark.TypeSystem.Activated.Property.Set( parameter, true );
			}
		}

		static bool Activated( Assembly parameter ) => DragonSpark.TypeSystem.Activated.Property.Get( parameter );

		class Specification : OncePerParameterSpecification<Assembly>
		{
			public static Specification Instance { get; } = new Specification();

			public override bool IsSatisfiedBy( Assembly parameter ) => !Activated( parameter ) && base.IsSatisfiedBy( parameter );
		}
	}

	public class AssemblyLoader : DragonSpark.TypeSystem.AssemblyLoader
	{
		public static AssemblyLoader Instance { get; } = new AssemblyLoader();

		AssemblyLoader() : this( AssemblyHintProvider.Instance.Create ) {}

		public AssemblyLoader( Func<Assembly, string> hintSource ) : base( hintSource, AssemblyPathLoader.Instance.Create, AssemblyInitializer.Instance.Execute ) {}
	}
}
