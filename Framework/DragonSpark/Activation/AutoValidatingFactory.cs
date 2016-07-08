using DragonSpark.Aspects;
using AutoValidationController = DragonSpark.Aspects.Validation.AutoValidationController;
using IAutoValidationController = DragonSpark.Aspects.Validation.IAutoValidationController;

namespace DragonSpark.Activation
{
	class AutoValidatingFactory : IFactoryWithParameter
	{
		readonly IFactoryWithParameter inner;
		public AutoValidatingFactory( IFactoryWithParameter inner ) : this( new AutoValidationController( new FactoryAdapter( inner )//, 
			
			// new FixedFactory<InstanceMethod, IParameterAwareHandler>( ParameterHandlerRegistry.Instance.For, new InstanceMethod( inner, FactoryProfileFactory.Method ) ).Create
			//ParameterHandlerRegistry.Instance.For( new InstanceMethod( inner, FactoryProfileFactory.Method ) )
			
			), inner ) {}

		protected AutoValidatingFactory( IAutoValidationController controller, IFactoryWithParameter inner )
		{
			Controller = controller;
			this.inner = inner;
		}

		protected IAutoValidationController Controller { get; }

		public bool CanCreate( object parameter ) => Controller.IsValid( parameter, () => inner.CanCreate( parameter ) ).GetValueOrDefault();

		public object Create( object parameter ) => Controller.Execute( parameter, () => inner.Create( parameter ) );
	}

	class AutoValidatingFactory<TParameter, TResult> : AutoValidatingFactory, IFactory<TParameter, TResult>
	{
		readonly IFactory<TParameter, TResult> inner;
		public AutoValidatingFactory( IFactory<TParameter, TResult> inner ) : base( new AutoValidationController( new FactoryAdapter<TParameter, TResult>( inner )//, 
			
			// new FixedFactory<InstanceMethod, IParameterAwareHandler>( ParameterHandlerRegistry.Instance.For, new InstanceMethod( inner, GenericFactoryProfileFactory.Method<TParameter, TResult>.Default ) ).Create
			// ParameterHandlerRegistry.Instance.For( new InstanceMethod( inner, GenericFactoryProfileFactory.Method<TParameter, TResult>.Default ) ) 
			
			), inner )
		{
			this.inner = inner;
		}

		public bool CanCreate( TParameter parameter ) => Controller.IsValid( parameter, () => inner.CanCreate( parameter ) ).GetValueOrDefault();

		public TResult Create( TParameter parameter ) => (TResult)Controller.Execute( parameter, () => inner.Create( parameter ) );
	}
}