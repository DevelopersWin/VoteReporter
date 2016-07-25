using DragonSpark.Activation;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.TypeSystem
{
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

		public static ICache<Assembly, ImmutableArray<Type>> All { get; } = new StoreCache<Assembly, ImmutableArray<Type>>( assembly => Instance.Create( assembly ).Select( AssemblyTypes.All.Get ).Concat().ToImmutableArray() );
		public static ICache<Assembly, ImmutableArray<Type>> Public { get; } = new StoreCache<Assembly, ImmutableArray<Type>>( assembly => Instance.Create( assembly ).Select( AssemblyTypes.Public.Get ).Concat().ToImmutableArray() );
	}
}
