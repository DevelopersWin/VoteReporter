using System;
using System.Linq;
using DragonSpark.Extensions;

namespace DragonSpark.Activation
{
	public class FixedFactoryParameterCoercer<TParameter> : IFactoryParameterCoercer<TParameter>
	{
		public static FixedFactoryParameterCoercer<TParameter> Null { get; } = new FixedFactoryParameterCoercer<TParameter>();

		readonly TParameter item;

		public FixedFactoryParameterCoercer() : this( default(TParameter) )
		{}

		public FixedFactoryParameterCoercer( TParameter item )
		{
			this.item = item;
		}

		public TParameter Coerce( object context ) => item;
	}

	public class FactoryParameterCoercer<TParameter> : IFactoryParameterCoercer<TParameter>
	{
		public static FactoryParameterCoercer<TParameter> Instance { get; } = new FactoryParameterCoercer<TParameter>();

		protected FactoryParameterCoercer() {}

		public TParameter Coerce( object context ) => context is TParameter ? (TParameter)context : PerformCoercion( context );

		protected virtual TParameter PerformCoercion( object context ) => context.With( Construct );

		static TParameter Construct( object parameter )
		{
			var constructor = typeof(TParameter).Adapt().FindConstructor( parameter.GetType() );
			var result = (TParameter)constructor.With( info =>
			{
				var parameters = info.GetParameters().First().ParameterType.Adapt().Qualify( parameter ).Append( Enumerable.Repeat( Type.Missing, Math.Max( 0, constructor.GetParameters().Length - 1 ) ) ).ToArray();
				return info.Invoke( parameters );
			} );
			return result;
		}
	}
}