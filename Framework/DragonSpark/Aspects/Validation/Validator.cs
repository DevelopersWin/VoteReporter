using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using System;
using System.Reflection;

namespace DragonSpark.Aspects.Validation
{
	public sealed class CommandAdapter<T> : ParameterValidationAdapterBase<T>
	{
		readonly static Func<MethodInfo, bool> Method = MethodEqualitySpecification.For( typeof(ICommand<T>).GetTypeInfo().GetDeclaredMethod( nameof(ICommand<T>.Execute) ) );

		readonly IParameterValidationAdapter general;

		public CommandAdapter( ICommand<T> inner ) : this( inner, new CommandAdapter( inner ) ) {}
		CommandAdapter( ISpecification<T> inner, IParameterValidationAdapter general ) : base( inner, Method )
		{
			this.general = general;
		}

		public override bool IsSatisfiedBy( object parameter ) => base.IsSatisfiedBy( parameter ) || general.IsSatisfiedBy( parameter );
	}
}