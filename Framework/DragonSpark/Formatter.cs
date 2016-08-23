using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;

namespace DragonSpark
{
	public sealed class Formatter : ParameterizedSourceBase<Formatter.Parameter, string>
	{
		readonly static Func<Parameter, string> Coerce = p => StringCoercer.Default.Coerce( p.Instance );
		readonly static Coerce<Parameter> Coercer = ConstructCoercer<Parameter>.Default.Coerce;

		[Export]
		public static Formatter Default { get; } = new Formatter();
		Formatter() : this( ConstructFromKnownTypes<IFormattable>.Default.Delegate() ) {}

		readonly Func<object, IFormattable> factory;

		Formatter( Func<object, IFormattable> factory ) : base( Coercer )
		{
			this.factory = factory;
		}

		public override string Get( [Assigned]Parameter parameter ) => factory( parameter.Instance )?.ToString( parameter.Format, parameter.Provider ) ?? Coerce( parameter );

		public object Format( object item ) => Get( new Parameter( item ) );

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