using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.TypeSystem;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class ExtendedDictionaryCache<TKey, TValue> : DictionaryCache<TKey, TValue>
	{
		static IEqualityComparer<TKey> EqualityComparer { get; } = 
			TypeAssignableSpecification<IStructuralEquatable>.Default.IsSatisfiedBy( typeof(TKey) )
			? StructuralEqualityComparer<TKey>.Default : EqualityComparer<TKey>.Default;

		public ExtendedDictionaryCache() : this( key => default(TValue) ) {}
		public ExtendedDictionaryCache( Func<TKey, TValue> factory ) : this( factory, EqualityComparer ) {}

		public ExtendedDictionaryCache( Func<TKey, TValue> factory, IEqualityComparer<TKey> comparer )
			: this( factory, new ExtendedDictionary<TKey, TValue>( comparer ) ) {}

		public ExtendedDictionaryCache( Func<TKey, TValue> factory, IExtendedDictionary<TKey, TValue> dictionary )
			: base( dictionary, new Context( dictionary.GetOrAdd, factory ).Get ) {}

		sealed class Context : ParameterizedSourceBase<TKey, TValue>
		{
			readonly Func<TKey, Func<TKey, TValue>, TValue> get;
			readonly Func<TKey, TValue> defaultFactory;

			public Context( Func<TKey, Func<TKey, TValue>, TValue> get, Func<TKey, TValue> defaultFactory )
			{
				this.get = get;
				this.defaultFactory = defaultFactory;
			}

			public override TValue Get( TKey parameter ) => get( parameter, defaultFactory );
		}
	}

	public class DictionaryCache<TKey, TValue> : DelegatedCache<TKey, TValue>
	{
		public DictionaryCache( IDictionary<TKey, TValue> dictionary ) : this( dictionary, dictionary.TryGet ) {}

		public DictionaryCache( IDictionary<TKey, TValue> dictionary, Func<TKey, TValue> get )
			: base( dictionary.ContainsKey, get, dictionary.Set, dictionary.Remove ) {}
	}

	public class ExtendedDictionary<TKey, TValue> : ConcurrentDictionary<TKey, TValue>, IExtendedDictionary<TKey, TValue>
	{
		public ExtendedDictionary( IEqualityComparer<TKey> comparer ) : base( comparer ) {}
	}

	public interface IExtendedDictionary<TArgument, TValue> : IDictionary<TArgument, TValue>
	{
		/*TValue GetOrAdd( TArgument key, TValue value );*/
		TValue GetOrAdd( TArgument key, Func<TArgument, TValue> valueFactory );
	}
}
