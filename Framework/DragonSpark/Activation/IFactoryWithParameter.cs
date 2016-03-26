namespace DragonSpark.Activation
{
	public interface IFactoryWithParameter
	{
		bool CanCreate( object parameter );

		object Create( object parameter );

		/*Type ParameterType { get; }*/
	}
}