using DragonSpark.Extensions;
using System;
using DragonSpark.Activation;

namespace DragonSpark.ComponentModel
{
	public class CollectionAttribute : ServicesValueBase
	{
		public CollectionAttribute( Type elementType = null, string name = null ) : base( t => Create( elementType, name ) ) {}

		static ServicesValueProvider Create( Type type, string name ) => new ServicesValueProvider( p =>
		{
			var elementType = type ?? p.PropertyType.Adapt().GetEnumerableType();
			var result = elementType.With( Transformer.Instance.Create );
			return result;
		} );

		public class Collection<T> : System.Collections.ObjectModel.Collection<T> {}

		class Transformer : TransformerBase<Type>
		{
			public static Transformer Instance { get; } = new Transformer();

			protected override Type CreateItem( Type parameter ) => typeof(Collection<>).MakeGenericType( parameter );
		}
	}
}