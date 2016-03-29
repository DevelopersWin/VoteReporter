using System;
using System.Composition.Hosting;
using System.Reflection;
using DragonSpark.Activation;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Composition
{
	public static class Create
	{
		public static CompositionHost From( [Required] Assembly[] assemblies ) => Factory( assemblies )();

		public static CompositionHost From( [Required] Type[] types ) => Factory( types )();

		public static CompositionHost From( [Required] IFactory<ContainerConfiguration> configuration ) => Factory( configuration.Create )();

		public static Func<CompositionHost> Factory( [Required] Assembly[] assemblies ) => Factory( new AssemblyBasedConfigurationContainerFactory( assemblies ) );

		public static Func<CompositionHost> Factory( [Required] Type[] types ) => Factory( new TypeBasedConfigurationContainerFactory( types ) );

		public static Func<CompositionHost> Factory( [Required] IFactory<ContainerConfiguration> configuration ) => Factory( configuration.Create );

		public static Func<CompositionHost> Factory( [Required] Func<ContainerConfiguration> configuration ) => new CompositionFactory( configuration ).Create;
	}
}