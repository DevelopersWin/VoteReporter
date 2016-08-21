using DragonSpark.Diagnostics.Logging;
using SerilogTimings.Extensions;
using System;
using System.Reflection;

namespace DragonSpark.Sources.Parameterized
{
	public class DelegatedParameterizedSource<TParameter, TResult> : ParameterizedSourceBase<TParameter, TResult>
	{
		readonly Func<TParameter, TResult> source;

		public DelegatedParameterizedSource( Func<TParameter, TResult> source )
		{
			this.source = source;
		}

		public override TResult Get( TParameter parameter ) => source( parameter );
	}

	public sealed class TimedDelegatedSource<TParameter, TResult> : DelegatedParameterizedSource<TParameter, TResult>
	{
		readonly MethodBase method;
		readonly string template;

		public TimedDelegatedSource( Func<TParameter, TResult> get, string template ) : this( get, get.GetMethodInfo(), template ) {}

		public TimedDelegatedSource( Func<TParameter, TResult> get, MethodBase method, string template ) : base( get )
		{
			this.method = method;
			this.template = template;
		}

		public override TResult Get( TParameter parameter )
		{
			using ( Logger.Instance.Get( method ).TimeOperation( template, method, parameter ) )
			{
				return base.Get( parameter );
			}
		}
	}
}