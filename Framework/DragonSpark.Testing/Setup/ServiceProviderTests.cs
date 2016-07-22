using DragonSpark.Activation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Setup;
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
			var result = new InstanceContainerServiceProvider( new ClassFactory() ).Get<Class>();
			Assert.IsType<ClassFactory.ClassFromFactory>( result );
		}

		[Fact]
		public void Logger()
		{
			var result = DefaultServiceProvider.Instance.Value.Get<ILogger>();
			Assert.NotNull( result );
			Assert.Same( Logging.Instance.Get( Defaults.ExecutionContext() ), result );
		}

		[Fact]
		public void SystemParts()
		{
			var system = DefaultServiceProvider.Instance.Value.Get<SystemParts>();
			Assert.Empty( system.Assemblies );
			Assert.Empty( system.Types );

			var types = new[] { typeof(ClassFactory), typeof(Class) };
			new InitializeSetupCommand( types ).Run();

			var after = DefaultServiceProvider.Instance.Value.Get<SystemParts>();
			Assert.NotEmpty( after.Assemblies );
			Assert.NotEmpty( after.Types );
			Assert.Equal( types, after.Types.ToArray() );
		}

		class ClassFactory : FactoryBase<Class>
		{
			public override Class Create() => new ClassFromFactory();

			public class ClassFromFactory : Class {}
		}
	}
}