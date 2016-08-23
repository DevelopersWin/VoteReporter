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

		public bool IsSatisfiedBy( TParameter parameter ) => controller.Marked( parameter, controller.IsSatisfiedBy( parameter ) || specification( parameter ) );

		public TResult Get( TParameter parameter ) => (TResult)controller.Execute( parameter, () => source( parameter ) );
	}

	class AutoValidatingSource : AutoValidatingSourceBase<object, object>, IValidatedParameterizedSource
	{
		public AutoValidatingSource( IValidatedParameterizedSource inner ) : base( new AutoValidationController( new FactoryAdapter( inner ) ), inner.IsSatisfiedBy, inner.Get ) {}}

	class AutoValidatingSource<TParameter, TResult> : AutoValidatingSourceBase<TParameter, TResult>, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly IValidatedParameterizedSource inner;

		public AutoValidatingSource( IValidatedParameterizedSource<TParameter, TResult> inner ) : 
			base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ), inner.IsSatisfiedBy, inner.Get )
		{
			this.inner = inner;
		}

		public object Get( object parameter ) => inner.Get( parameter );
		
		public bool IsSatisfiedBy( object parameter ) => inner.IsSatisfiedBy( parameter );
	}
}