using DragonSpark.Activation;
using DragonSpark.Extensions;
using System.Collections.Generic;
using System.ComponentModel;

namespace DragonSpark
{
	public interface IPriorityAware
	{
		[DefaultValue( Priority.Normal )]
		Priority Priority { get; }
	}

	class PriorityAware : IPriorityAware
	{
		public static PriorityAware Default { get; } = new PriorityAware();

		public Priority Priority { get; } = Priority.Normal;
	}

	public class PriorityAwareLocator<T> : FactoryWithSpecificationBase<T, IPriorityAware>
	{
		public static PriorityAwareLocator<T> Instance { get; } = new PriorityAwareLocator<T>();

		public override IPriorityAware Create( T parameter ) => parameter as IPriorityAware ?? (IPriorityAware)parameter.GetAttribute<PriorityAttribute>() ?? PriorityAware.Default;
	}

	public class PriorityComparer : IComparer<IPriorityAware>
	{
		public static PriorityComparer Instance { get; } = new PriorityComparer();

		public int Compare( IPriorityAware x, IPriorityAware y ) => Comparer<Priority>.Default.Compare( x.Priority, y.Priority );
	}
}