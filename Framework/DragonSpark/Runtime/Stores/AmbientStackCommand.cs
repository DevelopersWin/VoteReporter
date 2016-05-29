using DragonSpark.Extensions;
using DragonSpark.Runtime.Properties;
using PostSharp.Patterns.Contracts;
using System.Collections.Immutable;

namespace DragonSpark.Runtime.Stores
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
		public AmbientStackCommand() : this( AmbientStack<T>.Instance ) {}

		public AmbientStackCommand( AmbientStack<T> stack ) : base( stack.Value ) {}
	}

	public interface IStack<T>
	{
		bool Contains( T item );

		ImmutableArray<T> All();

		T Peek();

		void Push( T item );

		T Pop();
	}

	public class StackCommand<T> : DisposingCommand<T>
	{
		public StackCommand( [Required]IStack<T> stack )
		{
			Stack = stack;
		}

		protected IStack<T> Stack { get; }

		public override void Execute( T parameter ) => Stack.Push( parameter );

		protected override void OnDispose() => Stack.Pop().TryDispose();
	}
}