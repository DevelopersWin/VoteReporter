using PostSharp.Aspects;

namespace DragonSpark.Aspects
{
	public static class InterceptionArgsExtensions
	{
		public static object GetReturnValue( this MethodInterceptionArgs @this ) => @this.GetReturnValue<object>();

		public static T GetReturnValue<T>( this MethodInterceptionArgs @this )
		{
			@this.Proceed();
			var result = (T)@this.ReturnValue;
			return result;
		}
	}
}