using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : FactoryBase<IBasicService>
	{
		public override IBasicService Create() => new BasicService().WithSelf( service => Condition.Default.Get( service ).Apply() );
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