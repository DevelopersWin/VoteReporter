using DragonSpark.Extensions;
using DragonSpark.Runtime.Stores;
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

		public TParameter Coerce( object parameter ) => item;
	}

	/*public class ConstructFromParameterFactory : ConstructFromParameterFactory<object>
	{
		public ConstructFromParameterFactory( Type type ) : base( type ) {}
		public ConstructFromParameterFactory( IActivator activator, Type type ) : base( activator, type ) {}
	}*/

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

		public override T Create( object parameter )
		{
			var activate = activator.Create( new ConstructTypeRequest( type, parameter ) );
			var result = activate.AsTo<IStore<T>, T>( store => store.Value, () => activate.AsTo<IFactory<T>, T>( factory => factory.Create(), activate.As<T> ) );
			return result;
		}
	}

	public class Coercer<T> : CoercerBase<T>
	{
		public static Coercer<T> Instance { get; } = new Coercer<T>();

		protected override T PerformCoercion( object parameter ) => default(T);
	}

	public class ConstructCoercer<T> : CoercerBase<T>
	{
		public static ConstructCoercer<T> Instance { get; } = new ConstructCoercer<T>();

		readonly Func<object, T> projector;

		ConstructCoercer() : this( ConstructFromParameterFactory<T>.Instance.Create ) {}

		protected ConstructCoercer( Func<object, T> projector )
		{
			this.projector = projector;
		}

		protected override T PerformCoercion( object parameter ) => projector( parameter );
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
		public T Coerce( object parameter ) => parameter is T ? (T)parameter : parameter.With( PerformCoercion );

		protected abstract T PerformCoercion( object parameter );
	}
}