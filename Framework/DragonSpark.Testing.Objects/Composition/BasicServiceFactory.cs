using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Composition;
using DragonSpark.Runtime.Properties;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : FactoryBase<IBasicService>
	{
		public override IBasicService Create() => new BasicService().WithSelf( service => service.Get( Condition.Property ).Apply() );
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