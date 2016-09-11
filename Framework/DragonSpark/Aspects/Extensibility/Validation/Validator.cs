using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Extensibility.Validation
{
	public sealed class CommandAdapter<T> : ParameterValidationAdapterBase<T>
	{
		readonly static Func<MethodInfo, bool> Method = MethodEqualitySpecification.For( typeof(ICommand<T>).GetTypeInfo().GetDeclaredMethod( nameof(ICommand<T>.Execute) ) );

		public CommandAdapter( ICommand<T> inner ) : this( inner, new CommandAdapter( inner ) ) {}
		CommandAdapter( ISpecification<T> inner, ISpecification<object> fallback ) : base( inner, fallback, Method ) {}
	}
}