using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DragonSpark.TypeSystem
{
	public class DefaultFactory<T> : FactoryBase<T>
	{
		readonly Func<Type, object> source;

		public static DefaultFactory<T> Instance { get; } = new DefaultFactory<T>( DefaultItemProvider.Instance.Create );

		DefaultFactory( Func<Type, object> source )
		{
			this.source = source;
		}

		protected override T CreateItem() => (T)source( typeof(T) );
	}

	public class DefaultItemProvider : FactoryBase<Type, object>
	{
		public static DefaultItemProvider Instance { get; } = new DefaultItemProvider();

		DefaultItemProvider() {}

		[Freeze]
		protected override object CreateItem( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? typeof(Enumerable).InvokeGeneric( nameof(Enumerable.Empty), type.ToItem() ) : Default( parameter );
			return result;

			/*var enumerableType = parameter.Adapt().GetEnumerableType();
			var items = enumerableType == null;
			var name = items ? nameof(Default<object>.Item) : nameof(Default<object>.Items);
			var targetType = enumerableType ?? parameter;
			var property = typeof(Default<>).MakeGenericType( targetType ).GetTypeInfo().GetDeclaredProperty( name );
			var result = property.GetValue( null );
			return result;*/
		}

		static object Default( Type parameter ) => Expression.Lambda<Func<object>>( Expression.Default( parameter ) ).Compile()();
	}
}