using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public static class InterceptionArgsExtensions
	{
		public static object GetReturnValue( this MethodInterceptionArgs @this ) => @this.GetReturnValue<object>();

		public static T GetReturnValue<T>( this MethodInterceptionArgs @this ) => (T)@this.With( x => x.Proceed() ).ReturnValue;
		
		public static void ApplyReturnValue( this MethodInterceptionArgs @this, object result = null ) =>
			@this.Method.As<MethodInfo>( info =>
											{
												@this.ReturnValue = info.ReturnType == typeof(void) ? result ?? @this.ReturnValue : DefaultValueFactory.Instance.Create( info.ReturnType );
											} );

	}
}