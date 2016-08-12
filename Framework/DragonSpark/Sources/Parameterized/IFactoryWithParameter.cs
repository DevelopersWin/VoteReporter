namespace DragonSpark.Sources.Parameterized
{
	public interface IFactoryWithParameter /*: ICreator*/
	{
		bool CanCreate( object parameter );

		object Create( object parameter );
	}

	/*public interface ICreator {}*/
}