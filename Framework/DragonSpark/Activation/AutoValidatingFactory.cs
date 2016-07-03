using DragonSpark.Aspects;
using AutoValidationController = DragonSpark.Aspects.Validation.AutoValidationController;
using IAutoValidationController = DragonSpark.Aspects.Validation.IAutoValidationController;

namespace DragonSpark.Activation
{
	class AutoValidatingFactory : IFactoryWithParameter
	{
		readonly IFactoryWithParameter inner;
		public AutoValidatingFactory( IFactoryWithParameter inner ) : this( new AutoValidationController( new FactoryAdapter( inner ) ), inner ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IFactoryWithParameter inner )
		{
			this.inner = inner;
			Controller = controller;
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

		public object Create( object parameter ) => Controller.Execute( parameter ) ? inner.Create( parameter ) : null;
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IFactory<TParameter, TResult>
	{
		readonly IFactory<TParameter, TResult> inner;
		public AutoValidatingFactory( IFactory<TParameter, TResult> inner ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ), inner )
		{
			this.inner = inner;
		}

		public bool CanCreate( TParameter parameter )
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

		public TResult Create( TParameter parameter ) => Controller.Execute( parameter ) ? inner.Create( parameter ) : default(TResult);
	}
}