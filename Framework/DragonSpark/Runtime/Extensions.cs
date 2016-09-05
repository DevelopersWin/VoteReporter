using DragonSpark.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public static class Extensions
	{
		public static Delegate GetReference( this Delegate @this ) => Delegates.Default.Get( @this.Target ).Get( @this.GetMethodInfo() );

		public static Delegate GetDelegate<T>( this object @this, string methodName ) => @this.GetDelegate( typeof(T), methodName );

		public static Delegate GetDelegate( this object @this, MethodDescriptor descriptor ) => @this.GetDelegate( descriptor.DeclaringType, descriptor.MethodName );
		public static Delegate GetDelegate( this object @this, Type interfaceType, string methodName )
		{
			var methodMapping = @this.GetType().Adapt().GetMappedMethods( interfaceType ).Introduce( methodName, tuple => tuple.Item1.InterfaceMethod.Name == tuple.Item2 ).Only();
			var method = methodMapping.MappedMethod;
			var result = Delegates.Default.Get( @this ).Get( method );
			return result;
		}
	}
}