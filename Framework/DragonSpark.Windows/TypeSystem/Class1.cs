using DragonSpark.Activation;
using DragonSpark.Runtime.Sources;
using DragonSpark.Windows.Io;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Windows.TypeSystem
{
	public sealed class AssemblyLocator : QueryableResourceLocator
	{
		public static ICache<string, ImmutableArray<string>> Instance { get; } = new AssemblyLocator().CachedForEquality();
		AssemblyLocator() : base( IsAssemblySpecification.Instance.IsSatisfiedBy ) {}
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
