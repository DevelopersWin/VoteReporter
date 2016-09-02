namespace DragonSpark
{
	public interface ICoercer<out T> : ICoercer<object, T> {}

	public interface ICoercer<in TParameter, out TResult>
	{
		TResult Coerce( TParameter parameter );
	}
}