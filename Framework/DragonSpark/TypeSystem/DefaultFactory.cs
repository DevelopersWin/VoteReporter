using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.TypeSystem
{
	// [Synchronized]
	public class DefaultFactory<T> : FactoryBase<T>
	{
		public static DefaultFactory<T> Instance { get; } = new DefaultFactory<T>();

		[Freeze]
		protected override T CreateItem()
		{
			var type = typeof(T).Adapt().GetEnumerableType();
			var result = type != null ? (T)typeof(Enumerable).InvokeGeneric( nameof(Enumerable.Empty), type.ToItem() ) : default(T);
			return result;
		}
	}

	public class DefaultItemProvider : FactoryBase<Type, object>
	{
		public static DefaultItemProvider Instance { get; } = new DefaultItemProvider();

		protected override object CreateItem( Type parameter )
		{
			var enumerableType = parameter.Adapt().GetEnumerableType();
			var items = enumerableType == null;
			var name = items ? nameof(Default<object>.Item) : nameof(Default<object>.Items);
			var targetType = enumerableType ?? parameter;
			var property = typeof(Default<>).MakeGenericType( targetType ).GetTypeInfo().GetDeclaredProperty( name );
			var result = property.GetValue( null );
			return result;
		}
	}
}