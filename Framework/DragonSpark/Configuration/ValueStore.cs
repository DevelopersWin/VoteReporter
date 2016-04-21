using DragonSpark.Extensions;
using PostSharp.Patterns.Contracts;
using System.Collections.ObjectModel;

namespace DragonSpark.Configuration
{
	public interface IValueStore
	{
		object Get( string key );
	}

	public class ValueStore : KeyedCollection<string, Registration>, IValueStore
	{
		public object Get( [Required]string key ) => this.WithFirst( registration => registration.Equals( key ), registration => registration.Value );

		protected override string GetKeyForItem( Registration item ) => item.Key;
	}
}