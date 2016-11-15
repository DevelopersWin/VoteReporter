using System;
using System.Windows.Input;

namespace DragonSpark.Sources.Parameterized.Caching
{
	public class CommandCache<TConstructor, TTemplate> : Cache<TConstructor, TTemplate> where TConstructor : class where TTemplate : class, ICommand
	{
		public CommandCache( Func<TConstructor, TTemplate> create ) : base( create ) {}
	}
}