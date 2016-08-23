using System;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public interface IAutoValidationController : ISpecification
	{
		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, Func<object> proceed );
	}
}