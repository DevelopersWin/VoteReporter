using DragonSpark.Windows.Modularity;
using System;
using System.Collections.Generic;
using System.Reflection;
using IApplication = DragonSpark.Testing.Framework.Setup.IApplication;

namespace DragonSpark.Windows.Testing.Setup
{
	public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
	{
		public static IEnumerable<Assembly> Assemblies { get; } = new DragonSpark.Testing.Objects.IoC.AssemblyProvider( typeof(AssemblyModuleCatalog) ).Create();

		protected AutoDataAttribute( Func<IServiceProvider, IApplication> applicationSource ) : this( Assemblies, applicationSource ) {}

		protected AutoDataAttribute( IEnumerable<Assembly> assemblies, Func<IServiceProvider, IApplication> applicationSource ) : base( assemblies, applicationSource ) {}
	}
}