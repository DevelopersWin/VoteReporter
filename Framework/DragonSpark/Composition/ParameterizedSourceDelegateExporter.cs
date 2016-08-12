using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Setup.Registration;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Composition.Hosting.Core;

namespace DragonSpark.Composition
{
	public sealed class ParameterizedSourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.InstanceWithParameter.ToDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.Instance.Get;

		public ParameterizedSourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : ParameterizedSourceBase<ActivatorParameter, object>
		{
			public static Factory Instance { get; } = new Factory();
			Factory() : this( ParameterTypes.Instance.ToDelegate(), ResultTypes.Instance.ToDelegate() ) {}

			readonly Func<Type, Type> parameterLocator;
			readonly Func<Type, Type> resultLocator;

			Factory( Func<Type, Type> parameterLocator, Func<Type, Type> resultLocator )
			{
				this.parameterLocator = parameterLocator;
				this.resultLocator = resultLocator;
			}

			public override object Get( ActivatorParameter parameter )
			{
				var factory = new ParameterizedSourceDelegates( parameter.Services.Self ).Get( parameter.FactoryType );
				var result = factory.Convert( parameterLocator( parameter.FactoryType ), resultLocator( parameter.FactoryType ) );
				return result;
			}
		}
	}
}