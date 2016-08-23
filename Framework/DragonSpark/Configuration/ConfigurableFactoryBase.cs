using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Linq;

namespace DragonSpark.Configuration
{
	public abstract class ConfigurableFactoryBase<T> : ConfigurableFactoryBase<T, T>/*, IConfigurableFactory<T>*/ where T : class
	{
		protected ConfigurableFactoryBase( Func<T> seed ) : this( seed, Items<ITransformer<T>>.Default ) {}
		protected ConfigurableFactoryBase( Func<T> seed, params ITransformer<T>[] configurations ) : this( seed, new ConfigurationScope<T>( configurations ), Delegates<T>.Self ) {}
		protected ConfigurableFactoryBase( Func<T> seed, IConfigurationScope<T> scope, Func<T, T> factory ) : base( seed, scope, factory ) {}
		protected ConfigurableFactoryBase( IScope<T> seed ) : this( seed, new ConfigurationScope<T>( Items<ITransformer<T>>.Default ), Delegates<T>.Self ) {}
		protected ConfigurableFactoryBase( IScope<T> seed, IConfigurationScope<T> scope, Func<T, T> factory ) : base( seed, scope, factory ) {}
	}

	public abstract class ConfigurableFactoryBase<TConfiguration, TResult> : SourceBase<TResult> where TConfiguration : class
	{
		readonly Func<TConfiguration, TResult> factory;
		
		protected ConfigurableFactoryBase( Func<TConfiguration> seed, IConfigurationScope<TConfiguration> scope, Func<TConfiguration, TResult> factory ) : 
			this( new Scope<TConfiguration>( seed ), scope, factory ) {}

		protected ConfigurableFactoryBase( IScope<TConfiguration> seed, IConfigurationScope<TConfiguration> scope, Func<TConfiguration, TResult> factory )
		{
			Seed = seed;
			Configurators = scope;
			this.factory = factory;
		}

		public IScope<TConfiguration> Seed { get; }

		public IConfigurationScope<TConfiguration> Configurators { get; }

		public override TResult Get()
		{
			var seed = Seed.Get();
			var configurations = Configurators.Get();
			var configured = configurations.Aggregate( seed, ( curent, transformer ) => transformer.Get( curent ) );
			var result = factory( configured );
			return result;
		}
	}
}