using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.TypeSystem;
using System;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Activation
{
	public abstract class FactoryTypeLocatorBase<T> : FactoryBase<T, Type>
	{
		readonly FactoryTypeLocator locator;
		readonly Func<T, Type> type;
		readonly Func<T, Type> context;

		protected FactoryTypeLocatorBase( FactoryTypeLocator locator, Func<T, Type> type, Func<T, Type> context )
		{
			this.locator = locator;
			this.type = type;
			this.context = context;
		}

		public override Type Create( T parameter )
		{
			var info = context( parameter ).GetTypeInfo();
			var nestedTypes = info.DeclaredNestedTypes.AsTypes().ToArray();
			var all = nestedTypes.Union( AssemblyTypes.All.Create( info.Assembly ) ).Where( Defaults.ApplicationType ).ToArray();
			var requests = FactoryTypeFactory.Instance.CreateMany( all );
			var candidates = new[] { new FactoryTypeLocator( requests ), locator };
			var mapped = new LocateTypeRequest( type( parameter ) );
			var result = candidates.Introduce( mapped, tuple => tuple.Item1.Get( tuple.Item2 ) ).FirstAssigned();
			return result;
		}
	}
}