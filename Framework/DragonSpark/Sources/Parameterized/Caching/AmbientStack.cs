using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem.Generics;
using System;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public static class AmbientStack
	{
		readonly static IGenericMethodContext<Invoke> Method = typeof(AmbientStack).Adapt().GenericFactoryMethods[nameof(GetCurrentItem)];

		public static object GetCurrentItem( Type type ) => Method.Make( type ).Invoke<object>();

		public static T GetCurrentItem<T>() => AmbientStack<T>.Default.GetCurrentItem();

		public static StackAssignment<T> Assignment<T>( this IStackSource<T> @this, T item )  => new StackAssignment<T>( @this, item );
	}
}