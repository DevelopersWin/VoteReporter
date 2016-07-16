using DragonSpark.Runtime.Properties;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	public static class Attributes
	{
		public static IAttributeProvider Get( [Required]object target ) => target as IAttributeProvider ?? AttributeProviderHost.Instance.Get( target );
	}

	static class AttributeSupport<T> where T : Attribute
	{
		public static ICache<Type, T> Local { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>() );
		public static ICache<Type, T> All { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>( true ) );
	}
}