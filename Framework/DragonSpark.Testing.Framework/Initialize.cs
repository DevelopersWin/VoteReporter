using System.Diagnostics;
using PostSharp.Aspects;
using ExecutionContext = DragonSpark.Testing.Framework.Setup.ExecutionContext;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Activation.Execution.Initialize( ExecutionContext.Instance );

		[ModuleInitializer( 1 )]
		public static void Tracing() => Trace.WriteLine( $"Initializing {typeof(Initialize)}" );
	}

	/*public class ApplicationDomainProxyFactory<T> : FactoryBase<object[], T>
	{
		readonly AppDomain domain;

		public ApplicationDomainProxyFactory( [Required] AppDomain domain )
		{
			this.domain = domain;
		}

		public T CreateUsing( params object[] arguments ) => Create( arguments );

		protected override T CreateItem( object[] parameter )
		{
			var assemblyPath = new Uri( typeof(T).Assembly.CodeBase).LocalPath;
			var result = (T)domain.CreateInstanceFromAndUnwrap(assemblyPath, typeof(T).FullName, false
				, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance
				, null, parameter, null, null );
			return result;
		}
	}

	[Serializable]
	public class AssemblyLoader : MarshalByRefObject, IDisposable
	{
		readonly string basePath;

		public AssemblyLoader( [NotEmpty]string basePath )
		{
			this.basePath = basePath;
		}

		public void Initialize() => AppDomain.CurrentDomain.AssemblyResolve += Resolve;

		Assembly Resolve( object sender, ResolveEventArgs args )
		{
			var assemblyName = new AssemblyName( args.Name ).Name;
			var result = Assembly.LoadFile( Path.Combine( basePath, $"{assemblyName}.dll" ) );
			return result;
		}

		public void Dispose() => AppDomain.CurrentDomain.AssemblyResolve -= Resolve;
	}*/
}