using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using System;
using System.Collections.ObjectModel;
using Activator = DragonSpark.Activation.Activator;

namespace DragonSpark.ComponentModel
{
	public sealed class CollectionAttribute : ServicesValueBase
	{
		readonly static Func<Type, Type> Transform = Transformer.Default.Get;

		public CollectionAttribute( Type elementType = null ) : base( t => Create( elementType ) ) {}

		static ServicesValueProvider Create( Type type = null ) => new ServicesValueProvider( p =>
		{
			var elementType = type ?? p.PropertyType.Adapt().GetEnumerableType();
			var result = elementType.With( Transform );
			return result;
		}, Activator.Activate<object> );

		/*class Collection<T> : System.Collections.ObjectModel.Collection<T> {}*/

		sealed class Transformer : TransformerBase<Type>
		{
			public static Transformer Default { get; } = new Transformer();
			Transformer() {}

			public override Type Get( Type parameter ) => typeof(Collection<>).MakeGenericType( parameter );
		}
	}
}