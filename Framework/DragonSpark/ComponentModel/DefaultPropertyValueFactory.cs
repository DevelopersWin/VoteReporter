using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public sealed class DefaultPropertyValueFactory : ParameterizedSourceBase<DefaultValueParameter, object>
	{
		public static DefaultPropertyValueFactory Instance { get; } = new DefaultPropertyValueFactory();
		DefaultPropertyValueFactory() : this( HostedValueLocator<IDefaultValueProvider>.Instance.ToDelegate() ) {}

		readonly Func<MemberInfo, ImmutableArray<IDefaultValueProvider>> factory;

		public DefaultPropertyValueFactory( Func<MemberInfo, ImmutableArray<IDefaultValueProvider>> factory )
		{
			this.factory = factory;
		}

		public override object Get( DefaultValueParameter parameter ) => factory( parameter.Metadata ).Introduce( parameter, tuple => tuple.Item1.GetValue( tuple.Item2 ) ).FirstAssigned() ?? parameter.Metadata.From<DefaultValueAttribute, object>( attribute => attribute.Value );
	}

	public struct DefaultValueParameter
	{
		public DefaultValueParameter( object instance, PropertyInfo metadata )
		{
			Instance = instance;
			Metadata = metadata;
		}

		public object Instance { get; }

		public PropertyInfo Metadata { get; }
	}
}