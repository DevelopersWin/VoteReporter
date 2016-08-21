using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Testing.Objects;
using Serilog;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Setup
{
	public class ServiceProviderTests
	{
		[Fact]
		public void BasicTest()
		{
			var target = new Class();
			var composite = new CompositeServiceProvider( new InstanceServiceProvider( target ) );
			var result = composite.Get<Class>();
			Assert.Same( target, result );
		}

		[Fact]
		public void Factory()
		{
			var result = new SourceServiceProvider( new ClassFactory() ).Get<Class>();
			Assert.IsType<ClassFactory.ClassFromFactory>( result );
		}

		[Fact]
		public void Logger()
		{
			var result = DefaultServiceProvider.Default.Get<ILogger>();
			Assert.NotNull( result );
			Assert.Same( DragonSpark.Diagnostics.Logging.Logger.Default.Get( Execution.Current() ), result );
		}

		[Fact]
		public void SystemParts()
		{
			var system = DefaultServiceProvider.Default.Get<SystemParts>();
			Assert.Empty( system.Assemblies );
			Assert.Empty( system.Types );

			var types = new[] { typeof(ClassFactory), typeof(Class) };
			new AssignSystemPartsCommand( types ).Run();

			var after = DefaultServiceProvider.Default.Get<SystemParts>();
			Assert.NotEmpty( after.Assemblies );
			Assert.NotEmpty( after.Types );
			Assert.Equal( types, after.Types.ToArray() );

		}

		class ClassFactory : SourceBase<Class>
		{
			public override Class Get() => new ClassFromFactory();

			public class ClassFromFactory : Class {}
		}
	}
}