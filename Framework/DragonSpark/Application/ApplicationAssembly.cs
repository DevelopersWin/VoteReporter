using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;

namespace DragonSpark.Application
{
	public sealed class ApplicationAssembly : FixedFactory<IEnumerable<Assembly>, Assembly>
	{
		[Export]
		public static ISource<Assembly> Default { get; } = new Scope<Assembly>( Factory.Global( new ApplicationAssembly().Get ) );
		ApplicationAssembly() : base( TypeSystem.Configuration.ApplicationAssemblyLocator.Get, ApplicationAssemblies.Default.GetEnumerable ) {}
	}
}