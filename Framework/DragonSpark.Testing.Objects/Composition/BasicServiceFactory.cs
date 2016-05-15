using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System.Composition;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : FactoryBase<IBasicService>
	{
		public override IBasicService Create() => new BasicService().WithSelf( service => new Checked( service ).Value.Apply() );
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