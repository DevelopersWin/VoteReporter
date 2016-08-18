using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using System;
using System.Composition;

namespace DragonSpark.Diagnostics
{
	public class Formatter : ParameterizedSourceBase<Formatter.Parameter, string>
	{
		readonly static Func<Parameter, string> Coerce = p => StringCoercer.Instance.Coerce( p.Instance );
		readonly static Coerce<Parameter> Coercer = ConstructCoercer<Parameter>.Instance.Coerce;

		[Export]
		public static Formatter Instance { get; } = new Formatter();
		Formatter() : this( ConstructFromKnownTypes<IFormattable>.Instance.Delegate() ) {}

		readonly Func<object, IFormattable> factory;

		Formatter( Func<object, IFormattable> factory ) : base( Coercer )
		{
			this.factory = factory;
		}

		public override string Get( Parameter parameter )
		{
			var formattable = factory( parameter.Instance );
			var result = formattable != null ? formattable.ToString( parameter.Format, parameter.Provider ) : Coerce( parameter );
			return result;
		}

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