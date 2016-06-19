using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Composition;

namespace DragonSpark.Diagnostics
{
	[Shared, Export]
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, string>
	{
		public static FormatterFactory Instance { get; } = new FormatterFactory( FromKnownFactory<IFormattable>.Instance );
		/*

		public static string Format( object item ) 
		{
			var formatter = Instance; // TODO: Make configurable: Services.Get
			var result = formatter.Create( new Parameter( item ) );
			return result;
		}*/

		readonly Func<object, IFormattable> factory;

		[ImportingConstructor]
		public FormatterFactory( FromKnownFactory<IFormattable> factory ) : this( factory.CreateUsing ) {}

		public FormatterFactory( Func<object, IFormattable> factory ) : base( ConstructCoercer<Parameter>.Instance.ToDelegate(), Specifications<Parameter>.Assigned )
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