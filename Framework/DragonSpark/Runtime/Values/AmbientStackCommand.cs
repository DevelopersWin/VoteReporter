using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Immutable;

namespace DragonSpark.Runtime.Values
{
	public class AmbientStackCommand<T> : StackCommand<T>
	{
		public AmbientStackCommand() : this( AmbientStackProperty<T>.Default ) {}

		public AmbientStackCommand( IAttachedProperty<object, IStack<T>> property ) : this( property, Execution.Current ) {}

		public AmbientStackCommand( IAttachedProperty<object, IStack<T>> property, object instance  ) : base( property.Get( instance ) ) {}
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
		readonly IStack<T> stack;

		public StackCommand( [Required]IStack<T> stack )
		{
			this.stack = stack;
		}

		public override void Execute( T parameter ) => stack.Push( parameter );

		protected override void OnDispose() => stack.Pop().TryDispose();
	}
}