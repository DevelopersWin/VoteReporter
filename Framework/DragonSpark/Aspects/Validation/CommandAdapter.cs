using System.Reflection;
using System.Windows.Input;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Validation
{
	public class CommandAdapter : ParameterValidationAdapterBase<object>
	{
		readonly static MethodInfo Method = typeof(ICommand).GetTypeInfo().GetDeclaredMethod( nameof(ICommand.Execute) );

		public CommandAdapter( ICommand inner ) : base( new DelegatedSpecification<object>( inner.CanExecute ), Method ) {}
	}
}