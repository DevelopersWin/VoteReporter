using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using System.Reflection;

namespace DragonSpark.Aspects
{
	public static class InterceptionArgsExtensions
	{
		public static object GetReturnValue( this MethodInterceptionArgs @this ) => @this.With( x => x.Proceed() ).ReturnValue;
		
		public static void ApplyReturnValue( this MethodInterceptionArgs @this ) =>
			@this.Method.As<MethodInfo>( info =>
											{
												@this.ReturnValue = info.ReturnType == typeof(void) ? @this.ReturnValue : DefaultValueFactory.Instance.Create( info.ReturnType );
											} );

	}
}