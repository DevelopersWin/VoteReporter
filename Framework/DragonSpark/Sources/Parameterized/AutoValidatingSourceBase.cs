using DragonSpark.Aspects.Adapters;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using System;

namespace DragonSpark.Sources.Parameterized
{
	abstract class AutoValidatingSourceBase<TParameter, TResult>
	{
		readonly IAutoValidationController controller;
		readonly Func<TParameter, bool> specification;
		readonly IAdapter adapter;

		protected AutoValidatingSourceBase( IAutoValidationController controller, Func<TParameter, bool> specification, Func<TParameter, TResult> source ) : this( controller, specification, new DelegatedAdapter<TParameter, TResult>( source ) ) {}

		AutoValidatingSourceBase( IAutoValidationController controller, Func<TParameter, bool> specification, IAdapter adapter )
		{
			this.controller = controller;
			this.specification = specification;
			this.adapter = adapter;
		}

		public bool IsSatisfiedBy( TParameter parameter ) => controller.Handles( parameter ) || controller.Marked( parameter, specification( parameter ) );

		public TResult Get( TParameter parameter ) => controller.Execute( parameter, adapter ).As<TResult>();
	}
}