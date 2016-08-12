using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Sources.Caching
{
	public static class AmbientStack
	{
		readonly static IGenericMethodContext<Invoke> Method = typeof(AmbientStack).Adapt().GenericFactoryMethods[nameof(GetCurrentItem)];

		public static object GetCurrentItem( [Required]Type type ) => Method.Make( type ).Invoke<object>();

		public static T GetCurrentItem<T>() => AmbientStack<T>.Default.GetCurrentItem();

		public static StackAssignment<T> Assignment<T>( this IStackSource<T> @this, T item )  => new StackAssignment<T>( @this, item );

		public struct StackAssignment<T> : IDisposable
		{
			readonly IStackSource<T> source;

			// public StackAssignment( T item ) : this( AmbientStack<T>.Default, item ) {}

			public StackAssignment( IStackSource<T> source, T item )
			{
				this.source = source;
				source.Get().Push( item );
			}

			public void Dispose() => source.Get().Pop();
		}
	}
}