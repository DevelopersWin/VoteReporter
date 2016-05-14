using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Specifications;
using System;
using System.Composition;

namespace DragonSpark.Diagnostics
{
	[Shared, Export]
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, object>
	{
		public static FormatterFactory Instance { get; } = new FormatterFactory( FromKnownFactory<IFormattable>.Instance );

		readonly Func<object, IFormattable> factory;

		[ImportingConstructor]
		public FormatterFactory( FromKnownFactory<IFormattable> factory ) : this( factory.CreateUsing ) {}

		public FormatterFactory( Func<object, IFormattable> factory ) : base( ConstructCoercer<Parameter>.Instance, Specifications.NotNull )
		{
			this.factory = factory;
		}

		public override object Create( Parameter parameter ) => 
			factory( parameter.Instance ).With( o => o.ToString( parameter.Format, parameter.Provider ), parameter.Instance.Self );

		public class Parameter
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