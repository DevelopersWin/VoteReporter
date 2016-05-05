namespace DragonSpark.Testing.Framework
{
	/*public sealed class Runtime : SpecificationBasedAspect
	{
		readonly static ISpecification Specification = JetBrainsAppDomainSpecification.Instance.Inverse().And( RuntimeSpecification.Instance );

		public Runtime() : base( Specification ) {}
	}*/

	/*class JetBrainsAppDomainSpecification : SpecificationBase<object>
	{
		readonly AppDomain domain;
		public static JetBrainsAppDomainSpecification Instance { get; } = new JetBrainsAppDomainSpecification();

		public JetBrainsAppDomainSpecification() : this( AppDomain.CurrentDomain ) {}

		public JetBrainsAppDomainSpecification( AppDomain domain )
		{
			this.domain = domain;
		}

		public override bool IsSatisfiedBy( object parameter ) => domain.FriendlyName.Contains( JetBrainsEnvironment.JetbrainsResharperTaskrunner );
	}

	public static class JetBrainsEnvironment
	{
		public const string JetbrainsResharperTaskrunner = "JetBrains.ReSharper.TaskRunner";
	}

	public class JetBrainsApplicationDomainFactory : FactoryBase<AppDomain>
	{
		public static JetBrainsApplicationDomainFactory Instance { get; } = new JetBrainsApplicationDomainFactory();

		readonly Func<ImmutableArray<AppDomain>> source;

		JetBrainsApplicationDomainFactory() : this( AppDomainFactory.Instance.Create ) {}

		public JetBrainsApplicationDomainFactory( [Required] Func<ImmutableArray<AppDomain>> source )
		{
			this.source = source;
		}

		[Freeze]
		protected override AppDomain CreateItem() => source().Except( AppDomain.CurrentDomain.ToItem() ).FirstOrDefault( JetBrainsAppDomainSpecification.Instance.IsSatisfiedBy );
	}

	public class AppDomainFactory : FactoryBase<ImmutableArray<AppDomain>>
	{
		public static AppDomainFactory Instance { get; } = new AppDomainFactory();

		// #pragma warning disable 3305
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

	public class InitializeJetBrainsTaskRunnerCommand : CommandBase<AppDomainSetup>
	{
		public static InitializeJetBrainsTaskRunnerCommand Instance { get; } = new InitializeJetBrainsTaskRunnerCommand();

		readonly Func<string, AssemblyLoader> source;

		public InitializeJetBrainsTaskRunnerCommand() : this( JetBrainsAssemblyLoaderFactory.Instance.Create ) {}

		public InitializeJetBrainsTaskRunnerCommand( [Required] Func<string, AssemblyLoader> source )
		{
			this.source = source;
		}

		protected override void OnExecute( AppDomainSetup parameter ) => source( parameter.ApplicationBase ).With( loader => loader.Initialize() );
	}*/


}