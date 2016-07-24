using DragonSpark.Activation;
using DragonSpark.Activation.IoC;
using System;

namespace DragonSpark.ComponentModel
{
	public class SingletonAttribute : DefaultValueBase
	{
		public SingletonAttribute() : this( null ) {}

		public SingletonAttribute( Type hostType, string propertyName = nameof(SingletonLocator.Instance) ) : base( t => new SingletonDefaultValueProvider( hostType, propertyName ) ) {}
	}

	public class SingletonDefaultValueProvider : IDefaultValueProvider
	{
		readonly Func<Type, Type> locator;
		readonly Type hostType;
		readonly string propertyName;

		public SingletonDefaultValueProvider( Type hostType, string propertyName ) : this( BuildableTypeFromConventionLocator.Instance.Get(), hostType, propertyName ) {}

		public SingletonDefaultValueProvider( Func<Type, Type> locator, Type hostType, string propertyName )
		{
			this.locator = locator;
			this.hostType = hostType;
			this.propertyName = propertyName;
		}

		public object GetValue( DefaultValueParameter parameter )
		{
			var targetType = hostType ?? parameter.Metadata.PropertyType;
			var type = locator( targetType ) ?? targetType;
			var result = new SingletonLocator( propertyName ).Locate( type );
			return result;
		}
	}
}