using DragonSpark.Activation;
using DragonSpark.Windows.Io;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Windows.TypeSystem
{
	public sealed class AssemblyLocator : QueryableResourceLocator
	{
		public static ICache<string, ImmutableArray<string>> Default { get; } = new AssemblyLocator().ToEqualityCache();
		AssemblyLocator() : base( IsAssemblySpecification.Default.IsSatisfiedBy ) {}
	}

	public class QueryableResourceLocator : ParameterizedSourceBase<string, ImmutableArray<string>>
	{
		readonly Func<FileSystemInfo, bool> specification;
		public QueryableResourceLocator( Func<FileSystemInfo, bool> specification )
		{
			this.specification = specification;
		}

		public override ImmutableArray<string> Get( string parameter ) => 
			new DirectoryInfo( "." ).GetFileSystemInfos( parameter ).Where( specification ).Select( info => info.FullName ).ToImmutableArray();
	}
}
