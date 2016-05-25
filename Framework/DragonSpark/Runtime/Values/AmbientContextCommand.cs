using DragonSpark.Activation;
using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Runtime.Values
{
	public class AmbientContextCommand<T> : StackCommand<T>
	{
		readonly object instance;
		readonly IAttachedProperty<object, Stack<T>> property;

		public AmbientContextCommand() : this( ThreadLocalStack<T>.Property ) {}

		public AmbientContextCommand( IAttachedProperty<object, Stack<T>> property ) : this( Execution.Current, property ) {}

		public AmbientContextCommand( object instance, IAttachedProperty<object, Stack<T>> property  ) : base( instance.Get( property ) )
		{
			this.instance = instance;
			this.property = property;
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			var chain = instance.Get( property );

			if ( !chain.Any() )
			{
				property.Clear( instance );
				
			}
		}
	}

	public class StackCommand<T> : DisposingCommand<T>
	{
		readonly Stack<T> stack;

		public StackCommand( [Required]Stack<T> stack )
		{
			this.stack = stack;
		}

		public override void Execute( T parameter ) => stack.Push( parameter );

		protected override void OnDispose() => stack.Pop().TryDispose();
	}
}