namespace DragonSpark.Activation
{
	public interface IFactoryWithParameter : ICreator
	{
		bool CanCreate( object parameter );

		object Create( object parameter );

		/*Type ParameterType { get; }*/
	}

	public interface ICreator {}
}