using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DragonSpark
{
	public interface IPriorityAware
	{
		[DefaultValue( Priority.Normal )]
		Priority Priority { get; }
	}

	public static class PriorityExtensions
	{
		public static T WithPriority<T>( this T @this, Priority priority ) => WithPriority( @this, new PriorityAware( priority ) );

		public static T WithPriority<T>( this T @this, IPriorityAware aware )
		{
			AssociatedPriority.Default.Set( @this, aware );
			return @this;
		}
	}

	class PriorityAware : IPriorityAware
	{
		public static PriorityAware Default { get; } = new PriorityAware();
		PriorityAware() : this( Priority.Normal ) {}

		public PriorityAware( Priority priority )
		{
			Priority = priority;
		}

		public Priority Priority { get; }
	}

	public class AssociatedPriority : DecoratedSourceCache<IPriorityAware>
	{
		public static AssociatedPriority Default { get; } = new AssociatedPriority();
		AssociatedPriority() {}
	}

	/*public sealed class PriorityAwareLocator<T> : PriorityAwareLocator<T, PriorityAttribute>
	{
		PriorityAwareLocator() {}
	}*/

	/*public class PriorityAwareLocator<T, TAttribute> : PriorityAwareLocatorBase<T> where TAttribute : PriorityAttribute
	{
		public static PriorityAwareLocator<T, TAttribute> Default { get; } = new PriorityAwareLocator<T, TAttribute>();
		protected PriorityAwareLocator() : base(  ) {}
	}*/

	public class AssemblyPriorityLocator : PriorityAwareLocator<Assembly>
	{
		public new static AssemblyPriorityLocator Default { get; } = new AssemblyPriorityLocator();
		AssemblyPriorityLocator() : base( assembly => assembly.GetAttribute<PriorityAttribute>() ) {}
	}

	public class PriorityAwareLocator<T> : ParameterizedSourceBase<T, IPriorityAware>
	{
		public static PriorityAwareLocator<T> Default { get; } = new PriorityAwareLocator<T>();
		PriorityAwareLocator() : this( o => AttributeSupport<PriorityAttribute>.Local.Get( o.GetType() ) ) {}

		readonly Func<T, IPriorityAware> get;

		protected PriorityAwareLocator( Func<T, IPriorityAware> get )
		{
			this.get = get;
		}

		public override IPriorityAware Get( T parameter ) => parameter as IPriorityAware ?? get( parameter ) ?? AssociatedPriority.Default.Get( parameter ) ?? PriorityAware.Default;
	}

	public class PriorityComparer : IComparer<IPriorityAware>
	{
		public static PriorityComparer Default { get; } = new PriorityComparer();

		public int Compare( IPriorityAware x, IPriorityAware y ) => Comparer<Priority>.Default.Compare( x.Priority, y.Priority );
	}

	public class PrioritizedCollection<T> : CollectionBase<T>
	{
		public PrioritizedCollection() {}
		public PrioritizedCollection( IEnumerable<T> items ) : base( items ) {}
		public PrioritizedCollection( ICollection<T> source ) : base( source ) {}

		protected override IEnumerable<T> Query => base.Query.Prioritize();
	}
		

	public class PriorityAwareCollection<T> : CollectionBase<T> where T : IPriorityAware
	{
		public PriorityAwareCollection() {}
		public PriorityAwareCollection( IEnumerable<T> items ) : base( items ) {}
		public PriorityAwareCollection( ICollection<T> source ) : base( source ) {}

		protected override IEnumerable<T> Query => base.Query.OrderBy( arg => arg, PriorityComparer.Default );
	}
}