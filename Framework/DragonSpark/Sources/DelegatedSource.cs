using DragonSpark.Diagnostics.Logging;
using SerilogTimings.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Sources
{
	public class DelegatedSource<T> : SourceBase<T>
	{
		readonly Func<T> get;

		public DelegatedSource( Func<T> get )
		{
			this.get = get;
		}

		public override T Get() => get();
	}

	public sealed class TimedDelegatedSource<T> : DelegatedSource<T>
	{
		readonly MethodBase method;
		readonly string template;

		public TimedDelegatedSource( Func<T> get, string template ) : this( get, get.GetMethodInfo(), template ) {}

		public TimedDelegatedSource( Func<T> get, MethodBase method, string template ) : base( get )
		{
			this.method = method;
			this.template = template;
		}

		public override T Get()
		{
			using ( Logger.Instance.Get( method ).TimeOperation( template, method ) )
			{
				return base.Get();
			}
		}
	}
}