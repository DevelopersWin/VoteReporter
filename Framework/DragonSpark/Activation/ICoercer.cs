namespace DragonSpark.Activation
{
	public interface ICoercer<out TParameter>
	{
		TParameter Coerce( object context );
	}
}