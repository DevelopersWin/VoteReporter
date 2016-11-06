using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System.Reflection;

namespace DragonSpark.Expressions
{
	sealed class InvokeMethodDelegate<T> : InvocationFactoryBase<MethodInfo, T> where T : class
	{
		public static IParameterizedSource<MethodInfo, T> Default { get; } = new Cache<MethodInfo, T>( new InvokeMethodDelegate<T>().Get );
		InvokeMethodDelegate() : base( InvokeMethodExpressionFactory.Default.Create ) {}
	}
}