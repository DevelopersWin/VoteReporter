using System;
using System.Composition.Hosting.Core;
using DragonSpark.Activation;
using DragonSpark.Activation.Sources;
using DragonSpark.Activation.Sources.Caching;
using DragonSpark.Runtime;
using DragonSpark.Setup.Registration;

namespace DragonSpark.Composition
{
	public sealed class ParameterizedSourceDelegateExporter : SourceDelegateExporterBase
	{
		readonly static Func<CompositionContract, CompositionContract> Default = SourceDelegateContractResolver.InstanceWithParameter.ToDelegate();
		readonly static Func<ActivatorParameter, object> DelegateSource = Factory.Instance.Create;

		public ParameterizedSourceDelegateExporter() : base( DelegateSource, Default ) {}

		sealed class Factory : FactoryBase<ActivatorParameter, object>
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

			public override object Create( ActivatorParameter parameter )
			{
				var factory = new ParameterizedSourceDelegates( parameter.Services.Self ).Get( parameter.FactoryType );
				var result = factory.Convert( parameterLocator( parameter.FactoryType ), resultLocator( parameter.FactoryType ) );
				return result;
			}
		}
	}
}