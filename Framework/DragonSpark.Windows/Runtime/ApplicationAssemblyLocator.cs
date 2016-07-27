using DragonSpark.Runtime.Properties;
using DragonSpark.Runtime.Stores;
using System;
using System.Composition;
using System.IO;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	[Export, Shared]
	public class ApplicationAssemblyLocator : FixedStore<Assembly>
	{
		readonly static Assembly Default = DomainApplicationAssemblyLocator.Instance.Get( AppDomain.CurrentDomain );

		[ImportingConstructor]
		public ApplicationAssemblyLocator( Assembly[] assemblies ) : this( assemblies, AppDomain.CurrentDomain ) {}

		public ApplicationAssemblyLocator( Assembly[] assemblies, AppDomain domain ) : base( DragonSpark.TypeSystem.ApplicationAssemblyLocator.Instance.Create( assemblies ) ?? DomainApplicationAssemblyLocator.Instance.Get( domain ) ) {}
	}

	public class DomainApplicationAssemblyLocator : Cache<AppDomain, Assembly>
	{
		public static DomainApplicationAssemblyLocator Instance { get; } = new DomainApplicationAssemblyLocator();
		DomainApplicationAssemblyLocator() : base( Create ) {}

		static Assembly Create( AppDomain parameter )
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