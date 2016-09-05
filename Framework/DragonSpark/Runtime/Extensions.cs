using DragonSpark.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public static class Extensions
	{
		// public static Delegate GetReference( this Delegate @this ) => Delegates.Default.Get( @this.Target ).Get( @this.GetMethodInfo() );

		// public static Delegate GetDelegate<T>( this object @this, string methodName ) => @this.GetDelegate( typeof(T), methodName );

		public static MethodInfo GetMethod( this Type @this, MethodDescriptor descriptor ) => @this.GetMethod( descriptor.DeclaringType, descriptor.MethodName );
		public static MethodInfo GetMethod( this Type @this, Type interfaceType, string methodName )
		{
			var methodMapping = @this.Adapt().GetMappedMethods( interfaceType ).Introduce( methodName, tuple => tuple.Item1.InterfaceMethod.Name == tuple.Item2 ).Only();
			var result = methodMapping.MappedMethod;
			return result;
		}
	}
}