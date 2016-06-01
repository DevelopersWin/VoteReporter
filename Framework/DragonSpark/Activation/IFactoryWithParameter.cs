using DragonSpark.Aspects;

namespace DragonSpark.Activation
{
	public interface IFactoryWithParameter : ICreator, IValidationAware
	{
		bool CanCreate( object parameter );

		object Create( object parameter );

		/*Type ParameterType { get; }*/
	}

	public interface ICreator {}
}