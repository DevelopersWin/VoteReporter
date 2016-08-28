using DragonSpark.Activation.Location;
using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.ComponentModel
{
	public abstract class DefaultValueProviderBase : ParameterizedSourceBase<DefaultValueParameter, object>, IDefaultValueProvider {}

	public sealed class SingletonDefaultValueProvider : DefaultValueProviderBase
	{
		readonly static Func<Type, Type> Locator = ConventionTypes.Default.Get;

		readonly Func<Type, Type> locator;
		readonly Type hostType;
		readonly IParameterizedSource<Type, object> provider;
		
		public SingletonDefaultValueProvider( Type hostType, string propertyName ) : this( hostType, Locator, new Singletons( new SingletonDelegates( new SingletonProperties( new SpecifiedSingletonHostSpecification( hostType, propertyName.ToItem() ).Project<SingletonRequest, PropertyInfo>( request => request.Candidate ) ) ).Get ) ) {}

		SingletonDefaultValueProvider( Type hostType, Func<Type, Type> locator, IParameterizedSource<Type, object> provider )
		{
			this.locator = locator;
			this.hostType = hostType;
			this.provider = provider;
		}

		public override object Get( DefaultValueParameter parameter )
		{
			var targetType = hostType ?? parameter.Metadata.PropertyType;
			var type = locator( targetType ) ?? targetType;
			var result = provider.Get( type );
			return result;
		}
	}
}