using DragonSpark.Extensions;
using System.Collections.ObjectModel;

namespace DragonSpark.Configuration
{
	public class ValueStore : KeyedCollection<string, Registration>, IValueStore
	{
		public object Get( string key ) => this.WithFirst( registration => registration.Equals( key ), registration => registration.Value );

		protected override string GetKeyForItem( Registration item ) => item.Key;
	}
}