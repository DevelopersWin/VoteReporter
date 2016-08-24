using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;
using System.Linq;
using System.Reflection;
using Activator = System.Activator;

namespace DragonSpark.TypeSystem
{
	sealed class DefaultValues : Cache<Type, object>
	{
		readonly static IGenericMethodContext<Invoke> Method = typeof(Enumerable).Adapt().GenericFactoryMethods[nameof(Enumerable.Empty)];

		public static ICache<Type, object> Default { get; } = new DefaultValues();
		DefaultValues() : base( Create ) {}

		static object Create( Type parameter ) => parameter.GetTypeInfo().IsValueType ? Activator.CreateInstance( parameter ) : Empty( parameter );

		static object Empty( Type parameter )
		{
			var type = parameter.Adapt().GetEnumerableType();
			var result = type != null ? Method.Make( type.ToItem() ).Invoke<Array>() : null;
			return result;
		}
	}
}