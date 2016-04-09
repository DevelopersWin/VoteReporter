using DragonSpark.Composition;
using DragonSpark.Testing.Framework.Setup;
using DragonSpark.TypeSystem;
using DragonSpark.Windows.Runtime;
using System;
using System.Reflection;
using DragonSpark.Extensions;

namespace DragonSpark.Testing.Objects.IoC
{
	public class AutoDataAttribute : Framework.Setup.AutoDataAttribute
	{
		public AutoDataAttribute() : this( provider => new Application( provider ) ) {}

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( AssemblyProvider.Instance.Create, applicationSource ) {}

		protected AutoDataAttribute( Func<Assembly[]> assemblySource, Func<IServiceProvider, IApplication> applicationSource ) : this( data => new Activation.IoC.ServiceProviderFactory( new AssemblyBasedConfigurationContainerFactory( assemblySource() ).Create ).Create(), applicationSource ) {}

		protected AutoDataAttribute( Func<AutoData, IServiceProvider> providerSource, Func<IServiceProvider, IApplication> applicationSource ) : base( AttributeServices.From( providerSource, applicationSource ) ) {}
	}

	public class AssemblyProvider : AssemblyProviderBase
	{
		public static AssemblyProvider Instance { get; } = new AssemblyProvider();

		public AssemblyProvider( params Type[] others ) : base( others.Append( typeof(AssemblySourceBase) ).Fixed(), DomainApplicationAssemblyLocator.Instance.Create() ) {}
	}
}
