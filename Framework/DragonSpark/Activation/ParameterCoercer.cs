using DragonSpark.Extensions;
using System;
using System.Linq;

namespace DragonSpark.Activation
{
	public class FixedParameterCoercer<TParameter> : IParameterCoercer<TParameter>
	{
		public static FixedParameterCoercer<TParameter> Null { get; } = new FixedParameterCoercer<TParameter>();

		readonly TParameter item;

		public FixedParameterCoercer() : this( default(TParameter) ) {}

		public FixedParameterCoercer( TParameter item )
		{
			this.item = item;
		}

		public TParameter Coerce( object context ) => item;
	}

	public class ParameterCoercer<TParameter> : IParameterCoercer<TParameter>
	{
		public static ParameterCoercer<TParameter> Instance { get; } = new ParameterCoercer<TParameter>();

		protected ParameterCoercer() {}

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