using System;
using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;

namespace DragonSpark.Aspects
{
	public abstract class AdapterConstructorBase<T> : ParameterizedSourceBase<Type, Func<object, T>>
	{
		readonly Type primaryType;
		readonly Type adapterType;

		protected AdapterConstructorBase( Type primaryType, Type adapterType )
		{
			this.primaryType = primaryType;
			this.adapterType = adapterType;
		}

		public override Func<object, T> Get( Type parameter )
		{
			var inner = parameter.Adapt().GetImplementations( primaryType ).Only().Adapt().GetInnerType();
			var type = inner.ToItem();
			var result = ParameterConstructor<object, T>.Make( primaryType.MakeGenericType( type ), adapterType.MakeGenericType( type ) );
			return result;
		}
	}
}