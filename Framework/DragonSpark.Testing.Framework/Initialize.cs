using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Testing.Framework.Setup;
using mscoree;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DragonSpark.Testing.Framework
{
	public static class Initialize
	{
		[ModuleInitializer( 0 )]
		public static void Execution() => Activation.Execution.Initialize( CurrentExecution.Instance );

		[ModuleInitializer( 1 )]
		public static void Environment() => InitializeJetBrainsTaskRunnerCommand.Instance.ExecuteWith( AppDomain.CurrentDomain.SetupInformation );
	}

	public class JetBrainsApplicationDomainFactory : FactoryBase<AppDomain>
	{
		public static JetBrainsApplicationDomainFactory Instance { get; } = new JetBrainsApplicationDomainFactory();

		readonly Func<ImmutableArray<AppDomain>> source;
		readonly string domainName;

		public JetBrainsApplicationDomainFactory() : this( AppDomainFactory.Instance.Create ) {}

		public JetBrainsApplicationDomainFactory( [Required] Func<ImmutableArray<AppDomain>> source, [NotEmpty] string domainName = "JetBrains.ReSharper.TaskRunner" )
		{
			this.source = source;
			this.domainName = domainName;
		}

		[Freeze]
		protected override AppDomain CreateItem() => source().Except( AppDomain.CurrentDomain.ToItem() ).FirstOrDefault( domain => domain.FriendlyName.Contains( domainName ) );
	}

	public class JetBrainsAssemblyLoaderFactory : FactoryBase<string, AssemblyLoader>
	{
		public static JetBrainsAssemblyLoaderFactory Instance { get; } = new JetBrainsAssemblyLoaderFactory();

		readonly Func<AppDomain> source;

		public JetBrainsAssemblyLoaderFactory() : this( JetBrainsApplicationDomainFactory.Instance.Create ) {}

		public JetBrainsAssemblyLoaderFactory( [Required] Func<AppDomain> source )
		{
			this.source = source;
		}

		protected override AssemblyLoader CreateItem( string parameter ) => source.Use( domain => new ApplicationDomainProxyFactory<AssemblyLoader>( domain ).CreateUsing( parameter ) );
	}

	public class InitializeJetBrainsTaskRunnerCommand : Command<AppDomainSetup>
	{
		public static InitializeJetBrainsTaskRunnerCommand Instance { get; } = new InitializeJetBrainsTaskRunnerCommand();

		readonly Func<string, AssemblyLoader> source;

		public InitializeJetBrainsTaskRunnerCommand() : this( JetBrainsAssemblyLoaderFactory.Instance.Create ) {}

		public InitializeJetBrainsTaskRunnerCommand( [Required] Func<string, AssemblyLoader> source )
		{
			this.source = source;
		}

		protected override void OnExecute( AppDomainSetup parameter ) => source( parameter.ApplicationBase ).With( loader => loader.Initialize() );
	}

	public class AppDomainFactory : FactoryBase<ImmutableArray<AppDomain>>
	{
		public static AppDomainFactory Instance { get; } = new AppDomainFactory();

		[Freeze]
		protected override ImmutableArray<AppDomain> CreateItem()
		{
			var enumHandle = IntPtr.Zero;
			var host = new CorRuntimeHostClass();

			var items = new List<AppDomain>();

			try
			{
				host.EnumDomains( out enumHandle );

				object domain;
				host.NextDomain( enumHandle, out domain );
				while ( domain != null )
				{
					items.Add( (AppDomain)domain );
					host.NextDomain( enumHandle, out domain );
				}
			}
			catch ( InvalidCastException ) {}
			finally
			{
				if ( enumHandle != IntPtr.Zero )
				{
					host.CloseEnum( enumHandle );
				}

				Marshal.ReleaseComObject( host );	
			}
			var result = items.ToImmutableArray();
			return result;
		}
	}

	public class ApplicationDomainProxyFactory<T> : FactoryBase<object[], T>
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
	}
}