namespace DragonSpark.Activation
{
	public interface IFactoryParameterCoercer<out TParameter>
	{
		TParameter Coerce( object context );
	}
}