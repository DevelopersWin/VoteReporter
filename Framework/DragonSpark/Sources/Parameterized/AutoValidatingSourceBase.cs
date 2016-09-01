using DragonSpark.Aspects.Validation;
using System;

namespace DragonSpark.Sources.Parameterized
{
	abstract class AutoValidatingSourceBase<TParameter, TResult>
	{
		readonly IAutoValidationController controller;
		readonly Func<TParameter, TResult> source;
		readonly Func<TParameter, bool> specification;

		protected AutoValidatingSourceBase( IAutoValidationController controller, Func<TParameter, bool> specification, Func<TParameter, TResult> source )
		{
			this.controller = controller;
			this.specification = specification;
			this.source = source;
		}

		public bool IsSatisfiedBy( TParameter parameter ) => controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, specification( parameter ) );

		public TResult Get( TParameter parameter ) => (TResult)controller.Execute( parameter, () => source( parameter ) );
	}
}