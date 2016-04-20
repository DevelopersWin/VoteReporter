using DragonSpark.TypeSystem;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;
using System;
using System.Composition;
using System.IO;
using System.Reflection;
using DragonSpark.Activation;

namespace DragonSpark.Windows.Runtime
{
	[Export, Shared]
	public class ApplicationAssemblyLocator : FirstFactory<Assembly>, IApplicationAssemblyLocator
	{
		[ImportingConstructor]
		public ApplicationAssemblyLocator( [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( DomainApplicationAssemblyLocator.Instance, system ) {}

		public ApplicationAssemblyLocator( [Required]DomainApplicationAssemblyLocator domain, [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( domain, system ) {}
	}

	public class DomainApplicationAssemblyLocator : FactoryBase<Assembly>
	{
		public static DomainApplicationAssemblyLocator Instance { get; } = new DomainApplicationAssemblyLocator();

		readonly AppDomain primary;

		// [InjectionConstructor]
		public DomainApplicationAssemblyLocator() : this( AppDomain.CurrentDomain ) {}

		public DomainApplicationAssemblyLocator( [Required]AppDomain primary ) 
		{
			this.primary = primary;
		}

		protected override Assembly CreateItem()
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