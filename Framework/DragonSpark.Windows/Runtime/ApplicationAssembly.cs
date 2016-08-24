using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	public sealed class ApplicationAssemblyLocator : ParameterizedSourceBase<IEnumerable<Assembly>, Assembly>
	{
		readonly Func<Assembly> defaultSource;

		public static ApplicationAssemblyLocator Default { get; } = new ApplicationAssemblyLocator();
		ApplicationAssemblyLocator() : this( AppDomain.CurrentDomain ) {}

		public ApplicationAssemblyLocator( AppDomain domain ) : this( new FixedFactory<AppDomain, Assembly>( DomainApplicationAssemblies.Default.Get, domain ).Get ) {}

		public ApplicationAssemblyLocator( Func<Assembly> defaultSource )
		{
			this.defaultSource = defaultSource;
		}

		public override Assembly Get( IEnumerable<Assembly> parameter ) => Application.ApplicationAssemblyLocator.Default.Get( parameter ) ?? defaultSource();
	}

	public sealed class DomainApplicationAssemblies : FactoryCache<AppDomain, Assembly>
	{
		public static DomainApplicationAssemblies Default { get; } = new DomainApplicationAssemblies();
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