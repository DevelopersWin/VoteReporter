using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using System;
using System.Composition;

namespace DragonSpark.Diagnostics
{
	[Persistent, Shared, Export]
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, object>
	{
		public static FormatterFactory Instance { get; } = new FormatterFactory( FromKnownFactory<IFormattable>.Instance );

		readonly Func<object, IFormattable> factory;

		[ImportingConstructor]
		public FormatterFactory( FromKnownFactory<IFormattable> factory ) : this( factory.CreateAs ) {}

		public FormatterFactory( Func<object, IFormattable> factory )
		{
			this.factory = factory;
		}

		protected override object CreateItem( Parameter parameter ) => 
			factory( parameter.Instance ).With( o => o.ToString( parameter.Format, parameter.Provider ), () => parameter.Instance );

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