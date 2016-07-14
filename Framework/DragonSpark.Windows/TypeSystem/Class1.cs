using DragonSpark.Activation;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.TypeSystem
{
	/*public class LoadPartAssemblyCommand : DragonSpark.TypeSystem.LoadPartAssemblyCommand
	{
		public static LoadPartAssemblyCommand Instance { get; } = new LoadPartAssemblyCommand();
		LoadPartAssemblyCommand() : base( AssemblyLoader.Instance ) {}
	}*/

	public class AssemblyPathLoader : FactoryBase<string, ImmutableArray<Assembly>>
	{
		public static AssemblyPathLoader Instance { get; } = new AssemblyPathLoader();

		readonly static Func<string, Assembly> LoadFile = Assembly.LoadFile;

		public override ImmutableArray<Assembly> Create( string parameter ) => 
			new DirectoryInfo( "." ).GetFileSystemInfos( parameter ).Where( info => info.Extension == ".dll" ).Select( info => info.FullName ).Select( LoadFile ).ToImmutableArray();
	}

	public class AssemblyPartLocator : DragonSpark.TypeSystem.AssemblyPartLocator
	{
		public static AssemblyPartLocator Instance { get; } = new AssemblyPartLocator();
		AssemblyPartLocator() : base( AssemblyPathLoader.Instance.Create ) {}
	}

	/*public class AssemblyLoader : DragonSpark.TypeSystem.AssemblyLoader
	{
		public static AssemblyLoader Instance { get; } = new AssemblyLoader();
		AssemblyLoader() : base( AssemblyHintProvider.Instance.Create, AssemblyPathLoader.Instance.Create, Delegates<Assembly>.Empty ) {}
	}*/

	/*[Synchronized]
	[ApplyAutoValidation]
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
	}*/

	/**/
}
