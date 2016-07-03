using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System;
using System.ComponentModel;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public class DefaultPropertyValueFactory : FactoryBase<DefaultValueParameter, object>
	{
		public static DefaultPropertyValueFactory Instance { get; } = new DefaultPropertyValueFactory();

		readonly Func<MemberInfo, IDefaultValueProvider[]> factory;

		DefaultPropertyValueFactory() : this( HostedValueLocator<IDefaultValueProvider>.Instance.ToDelegate() ) {}

		public DefaultPropertyValueFactory( [Required]Func<MemberInfo, IDefaultValueProvider[]> factory )
		{
			this.factory = factory;
		}

		public override object Create( DefaultValueParameter parameter ) => factory( parameter.Metadata ).Introduce( parameter, tuple => tuple.Item1.GetValue( tuple.Item2 ) ).FirstAssigned() ?? parameter.Metadata.From<DefaultValueAttribute, object>( attribute => attribute.Value );
	}

	public struct DefaultValueParameter
	{
		public DefaultValueParameter( [Required]object instance, [Required]PropertyInfo metadata )
		{
			Instance = instance;
			Metadata = metadata;
		}

		public object Instance { get; }

		public PropertyInfo Metadata { get; }

		/*public DefaultValueParameter Assign( object value )
		{
			Metadata.SetValue( Instance, value );
			return this;
		}*/
	}

}