using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Attributes
	{
		//readonly static Func<object, IAttributeProvider> Source = AttributeProviders.Instance.Delegate();
		public static IAttributeProvider Get( object target ) => target as IAttributeProvider ?? AttributeProviders.Instance.Get( target );
	}

	static class AttributeSupport<T> where T : Attribute
	{
		public static IParameterizedSource<Type, T> Local { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>() );
		public static IParameterizedSource<Type, T> All { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>( true ) );
	}
}