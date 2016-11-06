using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem.Generics;
using System;
using System.Linq;
using System.Reflection;
using Activator = System.Activator;

namespace DragonSpark.TypeSystem
{
	public sealed class DefaultValues : Cache<Type, object>
	{
		public static IParameterizedSource<Type, object> Default { get; } = new DefaultValues();
		DefaultValues() : base( DefaultImplementation.Implementation.Get ) {}

		sealed class DefaultImplementation : ParameterizedSourceBase<Type, object>
		{
			public static DefaultImplementation Implementation { get; } = new DefaultImplementation();
			DefaultImplementation() {}

			public override object Get( Type parameter ) => 
				parameter.GetTypeInfo().IsValueType ? Activator.CreateInstance( parameter ) : Empty( parameter );

			static object Empty( Type parameter )
			{
				var type = parameter.Adapt().GetEnumerableType();
				var result = type != null ? Support.Method.Make( type.ToItem() ).Invoke<Array>() : null;
				return result;
			}

			static class Support
			{
				public static IGenericMethodContext<Invoke> Method { get; } = typeof(Enumerable).Adapt().GenericFactoryMethods[nameof(Enumerable.Empty)];
			}
		}
	}
}