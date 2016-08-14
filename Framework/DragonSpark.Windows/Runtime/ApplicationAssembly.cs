using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Reflection;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Windows.Runtime
{
	public sealed class ApplicationAssembly : FixedFactory<IEnumerable<Assembly>, Assembly>
	{
		[Export]
		public static ISource<Assembly> Instance { get; } = new Scope<Assembly>( Factory.ForGlobalScope( new ApplicationAssembly().Get ) );
		ApplicationAssembly() : base( ApplicationAssemblyLocator.Instance.Get, ApplicationAssemblies.Instance.Get().AsEnumerable() ) {}
	}

	public sealed class ApplicationAssemblyLocator : ParameterizedSourceBase<IEnumerable<Assembly>, Assembly>
	{
		readonly Func<Assembly> defaultSource;

		public static ApplicationAssemblyLocator Instance { get; } = new ApplicationAssemblyLocator();
		ApplicationAssemblyLocator() : this( AppDomain.CurrentDomain ) {}

		public ApplicationAssemblyLocator( AppDomain domain ) : this( new FixedFactory<AppDomain, Assembly>( DomainApplicationAssemblies.Instance.Get, domain ).Get ) {}

		public ApplicationAssemblyLocator( Func<Assembly> defaultSource )
		{
			this.defaultSource = defaultSource;
		}

		public override Assembly Get( IEnumerable<Assembly> parameter ) => DragonSpark.TypeSystem.ApplicationAssemblyLocator.Instance.Get( parameter ) ?? defaultSource();
	}

	public sealed class DomainApplicationAssemblies : FactoryCache<AppDomain, Assembly>
	{
		public static DomainApplicationAssemblies Instance { get; } = new DomainApplicationAssemblies();
		DomainApplicationAssemblies() {}

		protected override Assembly Create( AppDomain parameter )
		{
			try
			{
				return Assembly.Load( parameter.FriendlyName );
			}
			catch ( FileNotFoundException )
			{
				var result = Assembly.GetEntryAssembly();
				return result;
			}
		}
	}
}