using DragonSpark.Activation;
using DragonSpark.Runtime.Stores;
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
		readonly static IFactory<Assembly> Default = DomainApplicationAssemblyLocator.Instance.Value.ToFactory();

		[ImportingConstructor]
		public ApplicationAssemblyLocator( [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( Default, system ) {}

		public ApplicationAssemblyLocator( [Required]DomainApplicationAssemblyLocator domain, [Required]DragonSpark.TypeSystem.ApplicationAssemblyLocator system ) : base( domain.ToDelegate(), system.ToDelegate() ) {}
	}

	public class DomainApplicationAssemblyLocator : Store<Assembly>
	{
		public static DomainApplicationAssemblyLocator Instance { get; } = new DomainApplicationAssemblyLocator();
		DomainApplicationAssemblyLocator() : this( AppDomain.CurrentDomain ) {}
		public DomainApplicationAssemblyLocator( AppDomain domain ) : base( Factory.Instance.Create( domain ) ) {}

		class Factory : FactoryBase<AppDomain, Assembly>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() {}

			public override Assembly Create( AppDomain parameter )
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
}