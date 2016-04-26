using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using System;

namespace DragonSpark.Activation
{
	public class FixedCoercer<TParameter> : ICoercer<TParameter>
	{
		public static FixedCoercer<TParameter> Null { get; } = new FixedCoercer<TParameter>();

		readonly TParameter item;

		public FixedCoercer() : this( default(TParameter) ) {}

		public FixedCoercer( TParameter item )
		{
			this.item = item;
		}

		public TParameter Coerce( object context ) => item;
	}

	public class ConstructFromParameterFactory : ConstructFromParameterFactory<object>
	{
		public ConstructFromParameterFactory( Type type ) : base( type ) {}
		public ConstructFromParameterFactory( IActivator activator, Type type ) : base( activator, type ) {}
	}

	public class ConstructFromParameterFactory<T> : FactoryBase<object, T>
	{
		public static ConstructFromParameterFactory<T> Instance { get; } = new ConstructFromParameterFactory<T>( typeof(T) );

		readonly IActivator activator;
		readonly Type type;

		public ConstructFromParameterFactory( Type type ) : this( Constructor.Instance, type ) {}

		public ConstructFromParameterFactory( IActivator activator, Type type )
		{
			this.activator = activator;
			this.type = type;
		}

		protected override T CreateItem( object parameter )
		{
			var activate = activator.Create( new ConstructTypeRequest( type, parameter ) );
			var result = activate.AsTo<IStore<T>, T>( store => store.Value, () => activate.AsTo<IFactory<T>, T>( factory => factory.Create(), activate.As<T> ) );
			return result;
		}
	}

	public class Coercer<T> : CoercerBase<T>
	{
		public static Coercer<T> Instance { get; } = new Coercer<T>();

		public Coercer() {}

		protected override T PerformCoercion( object parameter ) => /*(T)Constructor.Instance.Create( new ConstructTypeRequest( typeof(T), parameter ) )*/ ConstructFromParameterFactory<T>.Instance.Create( parameter );

		/*var constructor = typeof(T).Adapt().FindConstructor( parameter.GetType() );
			var result = (T)constructor.With( info =>
			{
				var parameters = info.GetParameters().First().ParameterType.Adapt().Qualify( parameter ).Append( Enumerable.Repeat( Type.Missing, Math.Max( 0, constructor.GetParameters().Length - 1 ) ) ).ToArray();
				return info.Invoke( parameters );
			} );
			return result;*/
	}

	public class Projector<TFrom, TTo> : CoercerBase<TTo>
	{
		readonly Func<TFrom, TTo> projection;
		public Projector( Func<TFrom, TTo> projection )
		{
			this.projection = projection;
		}

		protected override TTo PerformCoercion( object parameter ) => parameter.AsTo( projection );
	}

	public abstract class CoercerBase<T> : ICoercer<T>
	{
		public T Coerce( object context ) => context is T ? (T)context : context.With( PerformCoercion );

		protected abstract T PerformCoercion( object parameter );
	}
}