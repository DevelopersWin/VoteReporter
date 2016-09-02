using DragonSpark.Activation;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;

namespace DragonSpark
{
	public interface IFormatter : IParameterizedSource<Formatter.Parameter, string>, IParameterizedSource<object, string> {}

	public sealed class Formatter : DecoratedParameterizedSource<Formatter.Parameter, string>, IFormatter
	{
		[Export]
		public static IFormatter Default { get; } = new Formatter();
		Formatter() : this( Inner.DefaultNested.With( ConstructCoercer<Parameter>.Default ), Inner.DefaultNested ) {}

		readonly IParameterizedSource<object, string> general;
		
		Formatter( IParameterizedSource<object, string> general, IParameterizedSource<Parameter, string> source ) : base( source )
		{
			this.general = general;
		}

		public string Get( object item ) => general.Get( item );

		sealed class Inner : ParameterizedSourceBase<Parameter, string>
		{
			readonly static Func<Parameter, string> Coerce = p => StringCoercer.Default.Coerce( p.Instance );

			public static Inner DefaultNested { get; } = new Inner();
			Inner() : this( ConstructFromKnownTypes<IFormattable>.Default.Delegate() ) {}


			readonly Func<object, IFormattable> factory;

			Inner( Func<object, IFormattable> factory )
			{
				this.factory = factory;
			}

			public override string Get( Parameter parameter ) => factory( parameter.Instance )?.ToString( parameter.Format, parameter.Provider ) ?? Coerce( parameter );
		}

		public struct Parameter
		{
			public Parameter( object instance, string format = null, IFormatProvider provider = null )
			{
				Instance = instance;
				Format = format;
				Provider = provider;
			}

			public object Instance { get; }
			public string Format { get; }
			public IFormatProvider Provider { get; }
		}
	}
}