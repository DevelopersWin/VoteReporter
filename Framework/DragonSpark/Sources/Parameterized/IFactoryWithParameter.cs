namespace DragonSpark.Sources.Parameterized
{
	public interface IFactoryWithParameter : IParameterizedSource
	{
		bool CanCreate( object parameter );
	}

	/*public interface ICreator {}*/
}