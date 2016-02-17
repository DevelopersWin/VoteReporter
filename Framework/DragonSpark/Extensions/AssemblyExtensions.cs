using System.Linq;
using System.Reflection;

namespace DragonSpark.Extensions
{
	public static class AssemblyExtensions
	{
		public static string GetRootNamespace( this Assembly target )
		{
			var root = target.FullName.With( x => x.Split( ',' ).FirstOrDefault() );
			var result = target.ExportedTypes.Where( x => x.Namespace.StartsWith( root ) ).Select( x => x.Namespace ).OrderBy( x => x.Length ).FirstOrDefault();
			return result;
		}

		public static string GetAssemblyName( this Assembly assembly )
		{
			var result = assembly.FullName.With( x => x.Substring( 0, x.IndexOf( ',' ) ) );
			return result;
		}
	}
}
