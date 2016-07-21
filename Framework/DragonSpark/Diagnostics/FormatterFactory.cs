using DragonSpark.Activation;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Stores;
using System;

namespace DragonSpark.Diagnostics
{
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, string>
	{
		public static IStore<FormatterFactory> Instance { get; } = new ExecutionContextStore<FormatterFactory>( () => new FormatterFactory( FromKnownFactory<IFormattable>.Instance.Get().CreateUsing ) );

		readonly static Func<Parameter, object> Coerce = p => StringCoercer.Instance.Coerce( p.Instance );

		readonly Func<object, IFormattable> factory;

		FormatterFactory( Func<object, IFormattable> factory ) : base( ConstructCoercer<Parameter>.Instance.ToDelegate() )
		{
			this.factory = factory;
		}

		public override string Create( Parameter parameter ) => (string)CreateFrom( parameter, p => StringCoercer.Instance.Coerce( p.Instance ) );

		object CreateFrom( Parameter parameter, Func<Parameter, object> @default )
		{
			var formattable = factory( parameter.Instance );
			var result = formattable != null ? formattable.ToString( parameter.Format, parameter.Provider ) : @default( parameter );
			return result;
		}

		public object From( object item ) => CreateFrom( new Parameter( item ), Coerce );

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