using System;
using DragonSpark.Expressions;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;

namespace DragonSpark.TypeSystem.Generics
{
	public class GenericInvocationFactory<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult> where TParameter : class
	{
		readonly private Func<Type, Func<TParameter, TResult>> get;

		public GenericInvocationFactory( Type genericTypeDefinition, Type owningType, string methodName ) : this( new DelegateCache( owningType.Adapt().GenericFactoryMethods[ methodName ], genericTypeDefinition ).Get ) {}

		GenericInvocationFactory( Func<Type, Func<TParameter, TResult>> get )
		{
			this.get = get;
		}

		sealed class DelegateCache : Cache<Type, Func<TParameter, TResult>>
		{
			public DelegateCache( IGenericMethodContext<Invoke> context, Type genericType ) : base( new Factory( context, genericType ).Get ) {}

			sealed class Factory : ParameterizedSourceBase<Type, Func<TParameter, TResult>>
			{
				readonly IGenericMethodContext<Invoke> context;
				readonly Type genericType;

				public Factory( IGenericMethodContext<Invoke> context, Type genericType )
				{
					this.context = context;
					this.genericType = genericType;
				}

				public override Func<TParameter, TResult> Get( Type parameter ) => context.Make( parameter.Adapt().GetTypeArgumentsFor( genericType ) ).Get( new[] { parameter } ).Invoke<TParameter, TResult>;
			}
		}

		public override TResult Get( TParameter parameter ) => get( parameter.GetType() )( parameter );
	}
}