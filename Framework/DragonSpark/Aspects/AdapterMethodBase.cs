using DragonSpark.Aspects.Adapters;
using DragonSpark.Sources.Parameterized;
using PostSharp.Aspects;
using System;

namespace DragonSpark.Aspects
{
	public abstract class AdapterMethodBase : MethodInterceptionAspectBase
	{
		readonly Func<object, IAdapter> source;
		protected AdapterMethodBase( Func<object, IAdapter> source )
		{
			this.source = source;
		}

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var adapter = source( args.Instance );
			if ( adapter != null )
			{
				args.ReturnValue = adapter.Get( args.Arguments[0] );
			}
			else
			{
				args.Proceed();
			}
		}
	}

	/*public class AdapterInvocation<T> : DelegatedParameterizedSource<object, T>, IAdapterInvocation where T : IAdapter
	{
		public static AdapterInvocation<T> Default { get; } = new AdapterInvocation<T>();
		protected AdapterInvocation() : this( SourceCoercer<T>.Default.Get ) {}

		[UsedImplicitly]
		public AdapterInvocation( Func<object, T> source ) : base( source ) {}

		public object Apply( IAdapter adapter, object parameter = null ) => Apply( ValidatedCastCoercer<T>.Default.Get( adapter ), parameter );

		protected virtual object Apply( T adapter, object parameter = null ) => adapter.Get( parameter );

		IAdapter IParameterizedSource<object, IAdapter>.Get( object parameter ) => Get( parameter );
	}

	public interface IAdapterInvocation : IParameterizedSource<object, IAdapter>
	{
	}*/
}