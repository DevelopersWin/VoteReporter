﻿using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Sources.Caching;
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
	public class ParameterServiceFactory : FactoryBase<Parameter, IParameterService>
	{
		public ParameterServiceFactory() {}

		public override IParameterService Create( Parameter parameter ) => new ParameterService( parameter );
	}
}