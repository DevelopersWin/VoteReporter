namespace DragonSpark.Activation
{
	public interface IParameterCoercer<out TParameter>
	{
		TParameter Coerce( object context );
	}
}