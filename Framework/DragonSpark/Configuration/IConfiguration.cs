using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Configuration
{
	public abstract class ConfigurableParameterizedFactoryBase<TConfiguration, TResult> : ConfigurableParameterizedFactoryBase<TConfiguration, object, TResult>
	{
		protected ConfigurableParameterizedFactoryBase( Func<object, TConfiguration> seed, Func<object, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
		protected ConfigurableParameterizedFactoryBase( IParameterizedScope<TConfiguration> seed, IParameterizedScope<ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, object, TResult> factory ) : base( seed, configurators, factory ) {}
	}

	public abstract class ConfigurableParameterizedFactoryBase<TConfiguration, TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TConfiguration, TParameter, TResult> factory;

		protected ConfigurableParameterizedFactoryBase( Func<TParameter, TConfiguration> seed, Func<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory ) : 
			this( new ParameterizedScope<TParameter, TConfiguration>( seed ), new ParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>>( configurators ), factory ) {}

		protected ConfigurableParameterizedFactoryBase( IParameterizedScope<TParameter, TConfiguration> seed, IParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>> configurators, Func<TConfiguration, TParameter, TResult> factory )
		{
			Seed = seed;
			Configurators = configurators;
			this.factory = factory;
		}

		public IParameterizedScope<TParameter, TConfiguration> Seed { get; }

		public IParameterizedScope<TParameter, ImmutableArray<ITransformer<TConfiguration>>> Configurators { get; }

		public override TResult Get( TParameter parameter )
		{
			var configured = Configurators.Get( parameter ).Aggregate( Seed.Get( parameter ), ( configuration, transformer ) => transformer.Get( configuration ) );
			var result = factory( configured, parameter );
			return result;
		}
	}

	/*public interface IConfigurableFactory<T> : IConfigurableFactory<T, T> {}

	public interface IConfigurableFactory<TConfiguration, out TResult> : IFactory<TResult>
	{
		IScope<TConfiguration> Seed { get; }

		IConfigurationScope<TConfiguration> ConfigurationScope { get; }
	}*/

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
			ConfigurationScope = scope;
			this.factory = factory;
		}

		public IScope<TConfiguration> Seed { get; }

		public IConfigurationScope<TConfiguration> ConfigurationScope { get; }

		public override TResult Get()
		{
			var seed = Seed.Get();
			var configurations = ConfigurationScope.Get();
			var configured = configurations.Aggregate( seed, ( curent, transformer ) => transformer.Get( curent ) );
			var result = factory( configured );
			return result;
		}
	}

	public interface IConfigurationScope<T> : IScope<ImmutableArray<ITransformer<T>>> {}

	public class ConfigurationScope<T> : Scope<ImmutableArray<ITransformer<T>>>, IConfigurationScope<T>
	{
		public ConfigurationScope() : this( Items<ITransformer<T>>.Default ) {}
		public ConfigurationScope( params ITransformer<T>[] configurators ) : base( new ConfigurationSource<T>( configurators ).Global() ) {}
	}

	public class ConfigurationSource<T> : CompositeItemSource<ITransformer<T>>
	{
		public ConfigurationSource( params ITransformer<T>[] configurators ) : base( new ItemSource<ITransformer<T>>( configurators ), ExportSource<ITransformer<T>>.Instance ) {}
	}

	/*public abstract class ConfigurationSourceBase<TParameter, TConfiguration> : ParameterizedSourceBase<TParameter, ImmutableArray<ITransformer<TConfiguration>>>
	{
		public override ImmutableArray<ITransformer<TConfiguration>> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		protected abstract IEnumerable<ITransformer<TConfiguration>> Yield( TParameter parameter );
	}*/

	/*public static class ConfigurationExtensions
	{
		/*public static T Apply<T>( this IConfiguration<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}#1#

		public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Execution.Current() );

		/*public static IStore<T> ToStore<T>( this IConfiguration<object, T> @this ) => SourceCache<T>.Default.Get( @this );
		class SourceCache<T> : Cache<IConfiguration<object, T>, IStore<T>>
		{
			public static SourceCache<T> Default { get; } = new SourceCache<T>();
			SourceCache() : base( configuration => new ScopedStore<T>( configuration.Get ) ) {}
		}#1#
	}*/
}