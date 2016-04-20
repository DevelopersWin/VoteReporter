using System;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DragonSpark.Activation;
using DragonSpark.TypeSystem;

namespace DragonSpark.ComponentModel
{
	public class DefaultPropertyValueFactory : FactoryBase<DefaultValueParameter, object>
	{
		public static DefaultPropertyValueFactory Instance { get; } = new DefaultPropertyValueFactory();

		readonly Func<MemberInfo, IDefaultValueProvider[]> factory;

		public DefaultPropertyValueFactory() : this( HostedValueLocator<IDefaultValueProvider>.Instance.Create ) {}

		public DefaultPropertyValueFactory( [Required]Func<MemberInfo, IDefaultValueProvider[]> factory )
		{
			this.factory = factory;
		}

		protected override object CreateItem( DefaultValueParameter parameter )
		{
			var result = factory( parameter.Metadata ).Select( p => p.GetValue( parameter ) ).NotNull().FirstOrDefault()
						 ??
						 Attributes.Get( parameter.Metadata ).From<DefaultValueAttribute, object>( attribute => attribute.Value );
			return result;
		}
	}

	public class DefaultValueParameter
	{
		public DefaultValueParameter( [Required]object instance, [Required]PropertyInfo metadata )
		{
			Instance = instance;
			Metadata = metadata;
		}

		public object Instance { get; }

		public PropertyInfo Metadata { get; }

		public DefaultValueParameter Assign( object value )
		{
			Metadata.SetValue( Instance, value );
			return this;
		}
	}

}