﻿using DragonSpark.Activation;
using DragonSpark.Runtime;
using System;

namespace DragonSpark.Diagnostics
{
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, string>
	{
		public static ISource<FormatterFactory> Instance { get; } = new ExecutionScope<FormatterFactory>( () => new FormatterFactory( ConstructFromKnownTypes<IFormattable>.Instance.Get().CreateUsing ) );

		readonly static Func<Parameter, object> Coerce = p => StringCoercer.Instance.Coerce( p.Instance );
		readonly static Coerce<Parameter> Coercer = ConstructCoercer<Parameter>.Instance.Coerce;

		readonly Func<object, IFormattable> factory;

		FormatterFactory( Func<object, IFormattable> factory ) : base( Coercer )
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