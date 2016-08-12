namespace DragonSpark.Sources.Parameterized
{
	public interface IValidatedParameterizedSource : IParameterizedSource
	{
		bool IsValid( object parameter );
	}
}