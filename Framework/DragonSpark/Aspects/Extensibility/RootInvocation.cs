using DragonSpark.Sources;

namespace DragonSpark.Aspects.Extensibility
{
	sealed class RootInvocation : SuppliedSource<IInvocation>, IRootInvocation
	{
		readonly IOriginInvocation origin;

		public RootInvocation() : this( new OriginInvocation() ) {}

		public RootInvocation( IOriginInvocation origin ) : base( origin )
		{
			this.origin = origin;
		}

		protected override void OnAssign( IInvocation item )
		{
			Enabled = origin != null;
			base.OnAssign( item );
		}

		public void Assign( AspectInvocation item ) => origin.Assign( item );

		public object Invoke( object parameter ) => Get().Invoke( parameter );

		public bool Enabled { get; private set; }
	}
}