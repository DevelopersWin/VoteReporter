using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Sources;
using DragonSpark.Runtime.Stores;
using DragonSpark.Setup;
using DragonSpark.TypeSystem;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DragonSpark.Configuration
{
	/*public interface IDelegateSource<in T> : IAssignable<Func<T>> {}

	public interface IConfiguration<T> : IAssignableSource<T>, IDelegateSource<T> {}

	public class Configuration<T> : Scope<T>, IConfiguration<T>//, IAssignable<Func<object, T>>
	{
		readonly IParameterizedConfiguration<T> source;
		// readonly IAssignable<Func<object, T>> assignable;

		public Configuration() : this( () => default(T) ) {}
		public Configuration( Func<T> defaultFactory ) : this( new AssignableDelegatedParameterizedScope<T>( defaultFactory ) ) {}

		public Configuration( IParameterizedConfiguration<T> source ) : base( source.ToCache() )
		{
			this.source = source;
		}

		public void Assign( Func<T> item ) => source.Assign( item.Wrap() );
	}*/

	public interface IConfigurationScope<T> : IScope<ImmutableArray<ITransformer<T>>> {}

	public class ConfigurationScope<T> : Scope<ImmutableArray<ITransformer<T>>>, IConfigurationScope<T>
	{
		public ConfigurationScope() : this( Items<ITransformer<T>>.Default ) {}
		public ConfigurationScope( params ITransformer<T>[] configurators ) : base( Factory.Scope( new ConfigurationSource<T>( configurators ).Get ) ) {}
		// public ConfigurationScope( Func<ImmutableArray<ITransformer<T>>> defaultFactory ) : base( defaultFactory ) {}
	}

	public class ConfigurationSource<T> : ItemsStoreBase<ITransformer<T>>
	{
		readonly string name;
		public ConfigurationSource() {}
		public ConfigurationSource( IEnumerable<ITransformer<T>> items ) : base( items ) {}

		public ConfigurationSource( string name = null, params ITransformer<T>[] items ) : base( items )
		{
			this.name = name;
		}

		protected override IEnumerable<ITransformer<T>> Yield() => base.Yield().Concat( Exports.Instance.Get().GetExports<ITransformer<T>>( name ).AsEnumerable() );
	}

	public abstract class ConfigurationSourceBase<TParameter, TConfiguration> : ParameterizedSourceBase<TParameter, ImmutableArray<ITransformer<TConfiguration>>>
	{
		public override ImmutableArray<ITransformer<TConfiguration>> Get( TParameter parameter ) => Yield( parameter ).ToImmutableArray();

		protected abstract IEnumerable<ITransformer<TConfiguration>> Yield( TParameter parameter );
	}

	/*public static class ConfigurationExtensions
	{
		/*public static T Apply<T>( this IConfiguration<T> @this, Func<T> factory )
		{
			@this.Assign( factory );
			return @this.Get();
		}#1#

		public static T Default<T>( this IParameterizedConfiguration<object, T> @this ) => @this.Get( Execution.Current() );

		/*public static IStore<T> ToStore<T>( this IConfiguration<object, T> @this ) => StoreCache<T>.Default.Get( @this );
		class StoreCache<T> : Cache<IConfiguration<object, T>, IStore<T>>
		{
			public static StoreCache<T> Default { get; } = new StoreCache<T>();
			StoreCache() : base( configuration => new ScopedStore<T>( configuration.Get ) ) {}
		}#1#
	}*/
}