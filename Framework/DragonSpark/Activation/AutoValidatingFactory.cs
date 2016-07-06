using DragonSpark.Aspects;
using DragonSpark.Aspects.Validation;
using AutoValidationController = DragonSpark.Aspects.Validation.AutoValidationController;
using IAutoValidationController = DragonSpark.Aspects.Validation.IAutoValidationController;

namespace DragonSpark.Activation
{
	class AutoValidatingFactory : IFactoryWithParameter
	{
		readonly IFactoryWithParameter inner;
		public AutoValidatingFactory( IFactoryWithParameter inner ) : this( new AutoValidationController( new FactoryAdapter( inner )/*, ParameterHandlerLocator.Instance.Create( new Func<object, object>( inner.Create ) )*/ ), inner ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IFactoryWithParameter inner )
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

		public object Create( object parameter )
		{
			object result;
			switch ( Controller.Execute( parameter, out result ) )
			{
				case AutoValidationControllerResult.ResultFound:
					return result;
				case AutoValidationControllerResult.Proceed:
					return inner.Create( parameter );
			}
			return null;
		}
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IFactory<TParameter, TResult>
	{
		readonly IFactory<TParameter, TResult> inner;
		public AutoValidatingFactory( IFactory<TParameter, TResult> inner ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner )/*, ParameterHandlerLocator.Instance.Create( new Func<TParameter, TResult>( inner.Create ) )*/ ), inner )
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

		public TResult Create( TParameter parameter )
		{
			object result;
			switch ( Controller.Execute( parameter, out result ) )
			{
				case AutoValidationControllerResult.ResultFound:
					return (TResult)result;
				case AutoValidationControllerResult.Proceed:
					return inner.Create( parameter );
			}
			return default(TResult);
		}
	}
}