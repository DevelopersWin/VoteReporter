using DragonSpark.Sources;
using DragonSpark.Specifications;

namespace DragonSpark.Aspects.Extensions
{
	sealed class RootInvocation : SuppliedSource<IInvocation>, IRootInvocation
	{
		readonly IOriginInvocation origin;

		public RootInvocation() : this( new OriginInvocation() ) {}

		public RootInvocation( IOriginInvocation origin ) : base( origin )
		{
			this.origin = origin;
		}

		public void Assign( AspectInvocation item ) => origin.Assign( item );

		public object Invoke( object parameter ) => Get().Invoke( parameter );

		public void Add( ISpecification<object> instance ) => Specification = Specification != null ? Specification.And( instance ) : instance;

		ISpecification<object> Specification { get; set; }

		public bool IsSatisfiedBy( object parameter ) => Specification?.IsSatisfiedBy( parameter ) ?? true;
	}
}