using DragonSpark.Activation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonSpark.Aspects.Adapters
{
	public class GenericAdapterConstructorFactory<TParameter, TResult> : ParameterizedSourceBase<Type, Func<TParameter, TResult>>
	{
		readonly static Func<Type, Type, Func<TParameter, TResult>> Factory = ParameterConstructor<TParameter, TResult>.Make;

		readonly Type parameterType;
		readonly Type implementedType;
		readonly Type adapterType;
		readonly Func<Type, Type, Func<TParameter, TResult>> factory;

		public GenericAdapterConstructorFactory( Type implementedType, Type adapterType ) : this( implementedType, implementedType, adapterType ) {}

		public GenericAdapterConstructorFactory( Type parameterType, Type implementedType, Type adapterType ) : this( parameterType, implementedType, adapterType, Factory ) {}

		[UsedImplicitly]
		protected GenericAdapterConstructorFactory( Type parameterType, Type implementedType, Type adapterType, Func<Type, Type, Func<TParameter, TResult>> factory )
		{
			this.parameterType = parameterType;
			this.implementedType = implementedType;
			this.adapterType = adapterType;
			this.factory = factory;
		}

		public override Func<TParameter, TResult> Get( Type parameter )
		{
			var adapter = parameter.Adapt();
			var instanceParameterType = adapter.GetImplementations( parameterType ).OrderByDescending( type => type, Comparer.Default ).FirstOrDefault() ?? adapter.ReferencedType;
			var instanceImplementedType = parameterType == implementedType ? instanceParameterType : adapter.GetImplementations( implementedType ).OrderByDescending( type => type, Comparer.Default ).First();
			var resultType = adapterType.MakeGenericType( instanceImplementedType.GenericTypeArguments );
			var result = factory( instanceParameterType, resultType );
			return result;
		}

		sealed class Comparer : IComparer<Type>
		{
			public static Comparer Default { get; } = new Comparer();
			Comparer() {}

			public int Compare( Type x, Type y ) => 
				x.GenericTypeArguments.Count( type => type != typeof(object) ).CompareTo( y.GenericTypeArguments.Count( type => type != typeof(object) ) );
		}
	}
}