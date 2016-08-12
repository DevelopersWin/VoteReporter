using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using System;

namespace DragonSpark.Sources.Parameterized
{
	class AutoValidatingFactory : IValidatedParameterizedSource
	{
		readonly IValidatedParameterizedSource inner;
		public AutoValidatingFactory( IValidatedParameterizedSource inner ) : this( new AutoValidationController( new FactoryAdapter( inner ) ), inner ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IValidatedParameterizedSource inner ) : this( controller, inner, inner.IsValid ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IValidatedParameterizedSource inner, Func<object, bool> specification )
		{
			Controller = controller;
			this.inner = inner;
		}

		protected IAutoValidationController Controller { get; }

		public bool IsValid( object parameter )
		{
			var result = Controller.IsValid( parameter ) || inner.IsValid( parameter );
			Controller.MarkValid( parameter, result );
			return result;
		}

		public object Get( object parameter ) => Controller.Execute( parameter, () => inner.Get( parameter ) );
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IValidatedParameterizedSource<TParameter, TResult>
	{
		readonly IValidatedParameterizedSource<TParameter, TResult> inner;
		readonly Func<TParameter, bool> specification;

		public AutoValidatingFactory( IValidatedParameterizedSource<TParameter, TResult> inner ) : this( inner, inner.IsValid ) {}

		public AutoValidatingFactory( IValidatedParameterizedSource<TParameter, TResult> inner, Func<TParameter, bool> specification ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ), inner )
		{
			this.inner = inner;
			this.specification = specification;
		}

		public bool IsValid( TParameter parameter )
		{
			var result = Controller.IsValid( parameter ) || specification( parameter );
			Controller.MarkValid( parameter, result );
			return result;
		}

		public TResult Get( TParameter parameter ) => (TResult)Controller.Execute( parameter, () => inner.Get( parameter ) );
	}
}