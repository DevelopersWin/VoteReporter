using DragonSpark.Activation;
using DragonSpark.Aspects;
using System.Reflection;
using DragonSpark.Runtime.Values;

namespace DragonSpark.Testing.Framework.Setup
{
	public class CurrentExecution : AssignedLogical<string>, IExecutionContext
	{
		public static CurrentExecution Instance { get; } = new CurrentExecution();

		public override string Item => base.Item ?? MethodContext.Get( MethodBase.GetCurrentMethod() );

		public override string ToString() => Item;
	}

	//[Serializable]
	public class MethodContext
	{
		readonly static object Reference = new object();

		// readonly static ConcurrentDictionary<string, int> Cache = new ConcurrentDictionary<string, Tuple<string>>();

		public static string Get( MethodBase method )
		{
			var key = /*KeyFactory.Instance.CreateUsing( method )*/method.ToString();
			// var code = KeyFactory.Instance.CreateUsing( key ).ToString();
			var result = new Reference<string>( Reference, key ).Item;
			return result;
		}
	}
}