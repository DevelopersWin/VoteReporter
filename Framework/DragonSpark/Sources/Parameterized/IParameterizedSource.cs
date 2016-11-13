using System;

namespace DragonSpark.Sources.Parameterized
{
	public interface IParameterizedSource<out T> : IParameterizedSource<object, T> {}

	public interface IParameterizedSource<in TParameter, out TResult>
	{
		TResult Get( TParameter parameter );
	}

	public interface ICurry<in T1, in T2, out TResult> : IParameterizedSource<T1, Func<T2, TResult>> {}
}