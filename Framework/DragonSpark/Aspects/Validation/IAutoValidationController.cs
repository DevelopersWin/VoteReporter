namespace DragonSpark.Aspects.Validation
{
	public interface IAutoValidationController // : ISpecification<object>
	{
		bool Handles( object parameter );

		void MarkValid( object parameter, bool valid );

		object Execute( object parameter, IInvocation proceed );
	}
}