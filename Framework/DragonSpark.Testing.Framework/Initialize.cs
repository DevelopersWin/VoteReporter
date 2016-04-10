using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework.Setup;
using mscoree;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Activation.Execution.Initialize( CurrentExecution.Instance );
	}

	public class AppDomainFactory : FactoryBase<IEnumerable<AppDomain>>
	{
		public static AppDomainFactory Instance { get; } = new AppDomainFactory();

		protected override IEnumerable<AppDomain> CreateItem()
		{
			var enumHandle = IntPtr.Zero;
			var host = new CorRuntimeHostClass();

			var result = new List<AppDomain>();

			try
			{
				host.EnumDomains( out enumHandle );

				object domain;
				host.NextDomain( enumHandle, out domain );
				while ( domain != null )
				{
					result.Add( (AppDomain)domain );
					host.NextDomain( enumHandle, out domain );
				}
			}
			catch ( InvalidCastException )
			{
				
			}
			finally
			{
				if ( enumHandle != IntPtr.Zero )
				{
					host.CloseEnum( enumHandle );
				}

				Marshal.ReleaseComObject( host );	
			}
			return result;
		}
	}

	public class InitializeTestRunnerEnvironmentCommand : DisposingCommand<object>
	{
		readonly AssemblyLoader loader;

		public InitializeTestRunnerEnvironmentCommand( [Required] AppDomain domain ) : this( domain, AppDomain.CurrentDomain.SetupInformation.ApplicationBase ) {}

		public InitializeTestRunnerEnvironmentCommand( [Required] AppDomain domain, [Required] string basePath ) : this( CreateFrom<AssemblyLoader>( domain, basePath ) ) {}

		static T CreateFrom<T>( AppDomain domain, params object[] arguments )
		{
			var assemblyPath = new Uri( typeof(T).Assembly.CodeBase).LocalPath;
			var result = (T)domain.CreateInstanceFromAndUnwrap(assemblyPath, typeof(T).FullName, false
				, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance
				, null, arguments, null, null );
			return result;
		}

		public InitializeTestRunnerEnvironmentCommand( [Required] AssemblyLoader loader )
		{
			this.loader = loader;
		}

		protected override void OnExecute( object parameter ) => loader.Initialize();

		protected override void OnDispose()
		{
			base.OnDispose();
			loader.Dispose();
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
	}
}