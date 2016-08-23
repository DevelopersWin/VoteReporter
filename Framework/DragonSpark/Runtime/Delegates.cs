using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Reflection;

namespace DragonSpark.Runtime
{
	public class Delegates : Cache<ICache<MethodInfo, Delegate>>
	{
		public static Delegates Default { get; } = new Delegates();
		Delegates() : base( o => new Factory( o ).ToCache() ) {}

		sealed class Factory : ParameterizedSourceBase<MethodInfo, Delegate>
		{
			readonly object instance;

			public Factory( object instance )
			{
				this.instance = instance;
			}

			public override Delegate Get( MethodInfo parameter )
			{
				var info = parameter.AccountForClosedDefinition( instance.GetType() );
				var delegateType = DelegateType.Default.Get( info );
				var result = info.CreateDelegate( delegateType, parameter.IsStatic ? null : instance );
				return result;
			}
		}
	}
}