using DragonSpark.Aspects;
using AutoValidationController = DragonSpark.Aspects.Validation.AutoValidationController;
using IAutoValidationController = DragonSpark.Aspects.Validation.IAutoValidationController;

namespace DragonSpark.Activation
{
	class AutoValidatingFactory : IFactoryWithParameter
	{
		public AutoValidatingFactory( IFactoryWithParameter inner ) : this( new AutoValidationController( new FactoryAdapter( inner ) ) ) {}

		protected AutoValidatingFactory( IAutoValidationController controller )
		{
			Controller = controller;
		}

		protected IAutoValidationController Controller { get; }

		public bool CanCreate( object parameter ) => Controller.IsValid( parameter );

		public object Create( object parameter ) => Controller.Execute( parameter );
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IFactory<TParameter, TResult>
	{
		public AutoValidatingFactory( IFactory<TParameter, TResult> inner ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner ) ) ) {}

		public bool CanCreate( TParameter parameter ) => Controller.IsValid( parameter );

		public TResult Create( TParameter parameter ) => (TResult)Controller.Execute( parameter );
	}
}