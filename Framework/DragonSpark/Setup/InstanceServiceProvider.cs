using DragonSpark.Extensions;
using DragonSpark.TypeSystem;

namespace DragonSpark.Setup
{
	public class InstanceServiceProvider : InstanceServiceProviderBase<object>, IServiceRepository
	{
		public InstanceServiceProvider() : this( Items<object>.Default ) {}
		public InstanceServiceProvider( params object[] instances ) : base( instances ) {}

		protected override T GetService<T>() => Query().FirstOrDefaultOfType<T>();
		public virtual void Add( InstanceRegistrationRequest request ) => Add( request.Instance );
	}
}