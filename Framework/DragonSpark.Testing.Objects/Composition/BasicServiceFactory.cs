using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System.Composition;

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
	public class ParameterServiceFactory : ParameterizedSourceBase<Parameter, IParameterService>
	{
		public override IParameterService Get( Parameter parameter ) => new ParameterService( parameter );
	}
}