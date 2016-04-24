using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System.Composition;
using DragonSpark.Activation;

namespace DragonSpark.Testing.Objects.Composition
{
	[Export]
	public class BasicServiceFactory : FactoryBase<IBasicService>
	{
		protected override IBasicService CreateItem() => new BasicService().WithSelf( service => new Checked( service ).Value.Apply() );
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

		protected override IParameterService CreateItem( Parameter parameter ) => new ParameterService( parameter );
	}
}