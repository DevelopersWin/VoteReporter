using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : SourceBase<IBasicService>
	{
		public override IBasicService Get() => new BasicService().WithSelf( service => Condition.Default.Get( service ).Apply() );
	}

	[Export]
	public class Parameter
	{
		public string Message { get; set; }
	}

	[Export]
	public class ParameterServiceFactory : FactoryBase<Parameter, IParameterService>
	{
		public ParameterServiceFactory() {}

		public override IParameterService Create( Parameter parameter ) => new ParameterService( parameter );
	}
}