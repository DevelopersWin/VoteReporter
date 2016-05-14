using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DragonSpark.TypeSystem
{
	public class DefaultValueFactory<T> : FactoryBase<T>
	{
		readonly Func<Type, object> source;

		public static DefaultValueFactory<T> Instance { get; } = new DefaultValueFactory<T>( DefaultValueFactory.Instance.Create );

		DefaultValueFactory( Func<Type, object> source )
		{
			this.source = source;
		}

		protected override T CreateItem() => (T)source( typeof(T) );
	}

	[Validation( false )]
	public class DefaultValueFactory : FactoryBase<Type, object>
	{
		public static DefaultValueFactory Instance { get; } = new DefaultValueFactory();

		DefaultValueFactory() {}

		[Freeze]
		public override object Create( Type parameter )
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

		static object Default( Type parameter ) => Expression.Lambda( Expression.Default( parameter ) ).Compile().DynamicInvoke();
	}
}