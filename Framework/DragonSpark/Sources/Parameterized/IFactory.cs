namespace DragonSpark.Sources.Parameterized
{
	public interface IFactory<in TParameter, out TResult> : IValidatedParameterizedSource
	{
		bool CanCreate( TParameter parameter );

		TResult Create( TParameter parameter );
	}
}