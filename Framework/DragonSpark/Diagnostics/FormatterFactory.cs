using DragonSpark.Activation;
using DragonSpark.Configuration;
using DragonSpark.Extensions;
using System;

namespace DragonSpark.Diagnostics
{
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, string>
	{
		public static IConfiguration<FormatterFactory> Instance { get; } = new Configuration<FormatterFactory>( () => new FormatterFactory( FromKnownFactory<IFormattable>.Instance.Get().CreateUsing ) );

		readonly Func<object, IFormattable> factory;

		FormatterFactory( Func<object, IFormattable> factory ) : base( ConstructCoercer<Parameter>.Instance.ToDelegate() )
		{
			this.factory = factory;
		}

		public override string Create( Parameter parameter ) => (string)CreateFrom( parameter, p => p.Instance.AsString() );

		object CreateFrom( Parameter parameter, Func<Parameter, object> @default )
		{
			var formattable = factory( parameter.Instance );
			var result = formattable != null ? formattable.ToString( parameter.Format, parameter.Provider ) : @default( parameter );
			return result;
		}

		public object From( object item ) => CreateFrom( new Parameter( item ), p => p.Instance.AsString() );

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