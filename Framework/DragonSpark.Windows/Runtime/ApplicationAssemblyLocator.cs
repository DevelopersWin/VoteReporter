using DragonSpark.Activation;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;
using System.IO;
using System.Reflection;

namespace DragonSpark.Windows.Runtime
{
	[Export, Shared]
	public class ApplicationAssemblyLocator : FirstFactory<Assembly>, IApplicationAssemblyLocator
	{
		[ImportingConstructor]
		public ApplicationAssemblyLocator( [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( DomainApplicationAssemblyLocator.Instance, system ) {}

		public ApplicationAssemblyLocator( [Required]DomainApplicationAssemblyLocator domain, [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( domain, system ) {}
	}

	public class DomainApplicationAssemblyLocator : CachedFactoryBase<Assembly>
	{
		public static DomainApplicationAssemblyLocator Instance { get; } = new DomainApplicationAssemblyLocator();

		readonly AppDomain primary;

		// [InjectionConstructor]
		public DomainApplicationAssemblyLocator() : this( AppDomain.CurrentDomain ) {}

		public DomainApplicationAssemblyLocator( [Required]AppDomain primary ) 
		{
			this.primary = primary;
		}

		protected override Assembly Cache()
		{
			try
			{
				return Assembly.Load( primary.FriendlyName );
			}
			catch ( FileNotFoundException )
			{
				var result = Assembly.GetEntryAssembly();
				return result;
			}
		}
	}
}