using DragonSpark.Aspects.Invocation;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public interface IAutoValidationController : ISpecification<object>
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, IInvocation proceed );
	}
}