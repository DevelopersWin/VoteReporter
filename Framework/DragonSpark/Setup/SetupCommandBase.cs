using DragonSpark.ComponentModel;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;

namespace DragonSpark.Setup
{
	public abstract class SetupCommandBase<T> : Command<T> // where T : ISetupParameter
	{
		protected SetupCommandBase() {}

		protected SetupCommandBase( ISpecification<T> specification ) : base( specification ) {}

		[Default( true )]
		public bool Enabled { get; set; }

		public override bool CanExecute( T parameter ) => Enabled && base.CanExecute( parameter );
	}

	public abstract class SetupCommandBase : SetupCommandBase<object>
	{
		protected SetupCommandBase() {}

		protected SetupCommandBase( ISpecification<object> specification ) : base( specification ) {}
	}

	// public abstract class SetupCommand<TArgument> : SetupCommandBase<ISetupParameter<TArgument>> {}
}