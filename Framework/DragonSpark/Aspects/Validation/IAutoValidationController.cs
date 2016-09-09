using DragonSpark.Specifications;
using System;

namespace DragonSpark.Aspects.Validation
{
	public interface IAutoValidationController : ISpecification<object>
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, Func<object> proceed );
	}
}