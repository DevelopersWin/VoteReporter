using DragonSpark.Aspects;
using System;
using AutoValidationController = DragonSpark.Aspects.Validation.AutoValidationController;
using IAutoValidationController = DragonSpark.Aspects.Validation.IAutoValidationController;

namespace DragonSpark.Activation
{
	class AutoValidatingFactory : IFactoryWithParameter
	{
		readonly IFactoryWithParameter inner;
		public AutoValidatingFactory( IFactoryWithParameter inner ) : this( new AutoValidationController( new FactoryAdapter( inner ) ), inner ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IFactoryWithParameter inner ) : this( controller, inner, inner.CanCreate ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IFactoryWithParameter inner, Func<object, bool> specification )
		{
			Controller = controller;
			this.inner = inner;
		}

		protected IAutoValidationController Controller { get; }

		public bool CanCreate( object parameter )
		{
			var valid = Controller.IsValid( parameter );
			if ( !valid.HasValue )
			{
				var result = inner.CanCreate( parameter );
				Controller.MarkValid( parameter, result );
				return result;
			}
			return valid.Value;
		}

		public object Create( object parameter ) => Controller.Execute( parameter, () => inner.Create( parameter ) );
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IFactory<TParameter, TResult>
	{
		readonly IFactory<TParameter, TResult> inner;
		readonly Func<TParameter, bool> specification;

		public AutoValidatingFactory( IFactory<TParameter, TResult> inner ) : this( inner, inner.CanCreate ) {}

		public AutoValidatingFactory( IFactory<TParameter, TResult> inner, Func<TParameter, bool> specification ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ), inner )
		{
			this.inner = inner;
			this.specification = specification;
		}

		public bool CanCreate( TParameter parameter )
		{
			var valid = Controller.IsValid( parameter );
			if ( !valid.HasValue )
			{
				var result = specification( parameter );
				Controller.MarkValid( parameter, result );
				return result;
			}
			return valid.Value;
		}

		public TResult Create( TParameter parameter ) => (TResult)Controller.Execute( parameter, () => inner.Create( parameter ) );
	}
}