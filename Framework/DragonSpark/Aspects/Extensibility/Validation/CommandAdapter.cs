using System;
using System.Reflection;
using System.Windows.Input;
using DragonSpark.Extensions;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class CommandAdapter : ParameterValidationAdapterBase<object>
	{
		readonly static Func<MethodInfo, bool> Method = MethodEqualitySpecification.For( typeof(ICommand).GetTypeInfo().GetDeclaredMethod( nameof(ICommand.Execute) ) );

		public CommandAdapter( ICommand inner ) : base( new DelegatedSpecification<object>( inner.CanExecute ), Method ) {}
	}
}