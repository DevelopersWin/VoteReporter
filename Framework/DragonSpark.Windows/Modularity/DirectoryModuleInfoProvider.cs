using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Modularity;
using DragonSpark.Windows.Io;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Windows.Modularity
{
	sealed class DirectoryModuleInfoProvider : MarshalByRefObject, IModuleInfoProvider
	{
		readonly IModuleInfoBuilder builder;
		readonly DirectoryInfo directory;
		readonly ResolveEventHandler resolve;
		readonly static Func<string, Assembly> LoadFactory = Load;

		public DirectoryModuleInfoProvider( IModuleInfoBuilder builder, IEnumerable<string> assemblies, string directoryPath )
		{
			this.builder = builder;
			directory = new DirectoryInfo( directoryPath );

			assemblies.Each( LoadFactory );

			resolve = CurrentDomainOnReflectionOnlyAssemblyResolve;
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += resolve;
		}

		public IEnumerable<ModuleInfo> GetModuleInfos()
		{
			Assembly assembly = FromLoaded( typeof(IModule).Assembly.FullName );
			Type moduleType = assembly.GetType(typeof(IModule).FullName);

			var result = GetModuleInfos(moduleType).ToArray();
			return result;
		}

		static Assembly FromLoaded( string name, Func<Assembly, string> resolve = null  )
		{
			var resolver = resolve ?? ( assembly => assembly.FullName );
			var assemblies = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();
			var result = assemblies.Introduce( ValueTuple.Create( name, resolver ), asm => string.Equals( asm.Item2.Item2( asm.Item1 ), asm.Item2.Item1, StringComparison.OrdinalIgnoreCase) ).FirstOrDefault();
			return result;
		}

		Assembly CurrentDomainOnReflectionOnlyAssemblyResolve( object sender, ResolveEventArgs args )
		{
			var result = FromLoaded( args.Name ) ?? Load( args, directory );
			return result;
		}

		static Assembly Load( ResolveEventArgs args, DirectoryInfo directory )
		{
			var assemblyName = new AssemblyName( args.Name );
			var filename = Path.Combine( directory.FullName, $"{assemblyName.Name}{FileSystem.AssemblyExtension}" );
			var result = File.Exists( filename ) ? Assembly.ReflectionOnlyLoadFrom( filename ) : ReflectionOnlyLoad( args.Name );
			return result;
		}

		static Assembly ReflectionOnlyLoad( string assemblyName )
		{
			try
			{
				return Assembly.ReflectionOnlyLoad( assemblyName );
			}
			catch ( FileNotFoundException )
			{
				var dllName = assemblyName.Contains(',') ? assemblyName.Substring(0, assemblyName.IndexOf(',')) : assemblyName;
				var name = dllName.Replace( FileSystem.AssemblyExtension, string.Empty );
				return FromLoaded( name, assembly => assembly.GetName().Name );
			}
		}

		static Assembly Load(string assemblyPath)
		{
			try
			{
				var result = Assembly.ReflectionOnlyLoadFrom( assemblyPath ).WithSelf( assembly => assembly.GetExportedTypes() );
				return result;
			}
			catch ( FileLoadException ) // Couldn't load a dependency.
			{}
			catch (BadImageFormatException ) // skip non-.NET Dlls
			{}
			catch (FileNotFoundException ) // Continue loading assemblies even if an assembly can not be loaded in the new AppDomain
			{ }
			return null;
		}

		IEnumerable<DynamicModuleInfo> GetModuleInfos(Type moduleType)
		{
			var loaded = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select( assembly => Path.GetFileName(assembly.Location) ).ToArray();

			var names = directory.GetFiles($"*{FileSystem.AssemblyExtension}")
				.Introduce( loaded, tuple => !tuple.Item2.Introduce( tuple.Item1.Name ).Any( t => string.Equals( t.Item1, t.Item2, StringComparison.OrdinalIgnoreCase) ) )
				.Select( info => info.FullName );

			var valid = names.Select( LoadFactory ).WhereAssigned();

			var result = valid.SelectMany( assembly => assembly.ExportedTypes ).Introduce( ValueTuple.Create( moduleType, builder ), 
				tuple => !tuple.Item1.IsAbstract && tuple.Item1 != tuple.Item2.Item1 && tuple.Item2.Item1.IsAssignableFrom( tuple.Item1 ), tuple => tuple.Item2.Item2.CreateModuleInfo( tuple.Item1 ) )
				.OfType<DynamicModuleInfo>();
			return result;
		}

		/// <summary>
		/// Disposes the associated <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="disposing">When <see langword="true"/>, disposes the associated <see cref="TextWriter"/>.</param>
		void Dispose(bool disposing)
		{
			if (disposing)
			{
				AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= resolve;
			}
		}

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		/// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}