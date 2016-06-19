namespace DragonSpark.Activation
{
	public interface ICoercer<out TParameter>
	{
		TParameter Coerce( object parameter );
	}

	public delegate T Coerce<out T>( object parameter );
}