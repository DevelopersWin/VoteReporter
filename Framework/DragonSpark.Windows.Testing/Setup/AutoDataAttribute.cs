using DragonSpark.Windows.Modularity;
using System;
using System.Collections.Immutable;
using System.Reflection;
using IApplication = DragonSpark.Testing.Framework.Setup.IApplication;

namespace DragonSpark.Windows.Testing.Setup
{
	public class AutoDataAttribute : DragonSpark.Testing.Objects.IoC.AutoDataAttribute
	{
		static ImmutableArray<Assembly> Assemblies { get; } = new DragonSpark.Testing.Objects.IoC.AssemblyProvider( typeof(AssemblyModuleCatalog) ).Create();

		protected AutoDataAttribute( Func<IApplication> applicationSource ) : this( Assemblies, applicationSource ) {}

		protected AutoDataAttribute( ImmutableArray<Assembly> assemblies, Func<IApplication> applicationSource ) : base( assemblies, applicationSource ) {}
	}
}