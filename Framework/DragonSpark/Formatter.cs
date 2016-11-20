using DragonSpark.Activation;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using JetBrains.Annotations;
using System;
using System.Composition;

namespace DragonSpark
{
	public sealed class Formatter : DecoratedParameterizedSource<FormatterParameter, string>, IFormatter
	{
		[Export]
		public static IFormatter Default { get; } = new Formatter();
		Formatter() : this( Implementation.Instance.Allow( ConstructCoercer<FormatterParameter>.Default ).ToCache(), Implementation.Instance ) {}

		readonly IParameterizedSource<object, string> general;

		[UsedImplicitly]
		public Formatter( IParameterizedSource<object, string> general, IParameterizedSource<FormatterParameter, string> source ) : base( source )
		{
			this.general = general;
		}

		public string Get( object parameter ) => general.Get( parameter );

		sealed class Implementation : ParameterizedSourceBase<FormatterParameter, string>
		{
			readonly static Func<FormatterParameter, string> Coerce = p => StringCoercer.Default.Get( p.Instance );

			public static Implementation Instance { get; } = new Implementation();
			Implementation() : this( Formatters.Default.Get ) {}

			readonly Func<object, IFormattable> factory;

			Implementation( Func<object, IFormattable> factory )
			{
				this.factory = factory;
			}

			public override string Get( FormatterParameter parameter ) => factory( parameter.Instance )?.ToString( parameter.Format, parameter.Provider ) ?? Coerce( parameter );
		}
	}
}