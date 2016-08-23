using System;
using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.ComponentModel
{
	public class SingletonDefaultValueProvider : IDefaultValueProvider
	{
		readonly static Func<Type, Type> Locator = ConventionTypes.Default.Get;

		readonly Func<Type, Type> locator;
		readonly Type hostType;
		readonly IParameterizedSource<Type, object> provider;
		
		public SingletonDefaultValueProvider( Type hostType, string propertyName ) : this( Locator, hostType, new SingletonLocator( new SingletonDelegates( new SingletonProperties( new SingletonSpecification( propertyName ) ) ).Get ) ) {}

		public SingletonDefaultValueProvider( Func<Type, Type> locator, Type hostType, IParameterizedSource<Type, object> provider )
		{
			this.locator = locator;
			this.hostType = hostType;
			this.provider = provider;
		}

		public object GetValue( DefaultValueParameter parameter )
		{
			var targetType = hostType ?? parameter.Metadata.PropertyType;
			var type = locator( targetType ) ?? targetType;
			var result = provider.Get( type );
			return result;
		}
	}
}