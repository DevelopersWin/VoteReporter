using DragonSpark.Extensions;

namespace DragonSpark.Sources.Coercion
{
	public sealed class ValidatedCastCoercer<T> : DelegatedCoercer<object, T>
	{
		public static ValidatedCastCoercer<T> Default { get; } = new ValidatedCastCoercer<T>();
		ValidatedCastCoercer() : base( ValidatedCastCoercer<object, T>.Default.Get ) {}
	}

	public sealed class ValidatedCastCoercer<TFrom, TTo> : CoercerBase<TFrom, TTo>
	{
		public static ValidatedCastCoercer<TFrom, TTo> Default { get; } = new ValidatedCastCoercer<TFrom, TTo>();
		ValidatedCastCoercer() {}

		protected override TTo Coerce( TFrom parameter ) => parameter.AsValid<TTo>();
	}

	public sealed class CastCoercer<T> : DelegatedCoercer<object, T>
	{
		public static CastCoercer<T> Default { get; } = new CastCoercer<T>();
		CastCoercer() : base( CastCoercer<object, T>.Default.Get ) {}
	}

	public sealed class CastCoercer<TFrom, TTo> : CoercerBase<TFrom, TTo>
	{
		public static CastCoercer<TFrom, TTo> Default { get; } = new CastCoercer<TFrom, TTo>();
		CastCoercer() {}

		protected override TTo Coerce( TFrom parameter ) => default(TTo);
	}
}