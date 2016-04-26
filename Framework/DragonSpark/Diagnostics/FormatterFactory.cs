using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Setup.Registration;
using DragonSpark.TypeSystem;
using System;
using System.Composition;
using System.Linq;

namespace DragonSpark.Diagnostics
{
	public class FromKnownFactory<T> : FirstFromParameterFactory<object, object>
	{
		public FromKnownFactory( KnownTypeFactory factory ) : base( factory.Create( typeof(T) ).Select( type => new ConstructFromParameterFactory( type ) ).Fixed() ) {}

		public T CreateAs( object parameter ) => (T)Create(	parameter );
	}

	[Persistent, Shared]
	public class FormatterFactory : FactoryBase<FormatterFactory.Parameter, object>
	{
		readonly Func<object, IFormattable> factory;

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