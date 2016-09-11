using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public interface IAutoValidationController : ISpecification<object>
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, IInvocation proceed );
	}
}