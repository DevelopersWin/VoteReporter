using DragonSpark.Activation;
using DragonSpark.Diagnostics.Logger;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
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
			var result = new SourceInstanceServiceProvider( new ClassFactory() ).Get<Class>();
			Assert.IsType<ClassFactory.ClassFromFactory>( result );
		}

		[Fact]
		public void Logger()
		{
			var result = DefaultServiceProvider.Instance.Get<ILogger>();
			Assert.NotNull( result );
			Assert.Same( Logging.Instance.Get( Execution.Current() ), result );
		}

		[Fact]
		public void SystemParts()
		{
			var system = DefaultServiceProvider.Instance.Get<SystemParts>();
			Assert.Empty( system.Assemblies );
			Assert.Empty( system.Types );

			var types = new[] { typeof(ClassFactory), typeof(Class) };
			new ApplySystemPartsConfiguration( types ).Run();

			var after = DefaultServiceProvider.Instance.Get<SystemParts>();
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