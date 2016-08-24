using System;
using System.Reflection;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem.Metadata
{
	static class AttributeSupport<T> where T : Attribute
	{
		public static IParameterizedSource<Type, T> Local { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>() );
		public static IParameterizedSource<Type, T> All { get; } = new Cache<Type, T>( type => type.GetTypeInfo().GetCustomAttribute<T>( true ) );
	}
}