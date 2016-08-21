using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;

namespace DragonSpark.ComponentModel
{
	public class CollectionAttribute : ServicesValueBase
	{
		public CollectionAttribute( Type elementType = null ) : base( t => Create( elementType ) ) {}

		static ServicesValueProvider Create( Type type = null ) => new ServicesValueProvider( p =>
		{
			var elementType = type ?? p.PropertyType.Adapt().GetEnumerableType();
			var result = elementType.With( Transformer.Default.Get );
			return result;
		} );

		public class Collection<T> : System.Collections.ObjectModel.Collection<T> {}

		class Transformer : ParameterizedSourceBase<Type, Type>
		{
			public static Transformer Default { get; } = new Transformer();

			public override Type Get( Type parameter ) => typeof(Collection<>).MakeGenericType( parameter );
		}
	}
}