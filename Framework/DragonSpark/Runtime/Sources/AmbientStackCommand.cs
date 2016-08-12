using DragonSpark.Runtime.Sources.Caching;

namespace DragonSpark.Runtime.Sources
{
	/*public class PropertyContext<T>
	{
		readonly Func<object> host;
		readonly IAttachedProperty<T> property;

		public PropertyContext( object host, IAttachedProperty<T> property ) : this( host.Self, property ) {}

		public PropertyContext( Func<object> host, IAttachedProperty<T> property )
		{
			this.host = host;
			this.property = property;
		}

		public T Get() => property.Get( host() );
	}*/

	public class AmbientStackCommand<T> : StackCommand<T>
	{
		public AmbientStackCommand() : this( AmbientStack<T>.Default ) {}

		public AmbientStackCommand( AmbientStack<T> stack ) : base( stack.Get() ) {}
	}
}