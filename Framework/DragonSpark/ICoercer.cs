namespace DragonSpark
{
	public interface ICoercer<out TParameter>
	{
		TParameter Coerce( object parameter );
	}
}