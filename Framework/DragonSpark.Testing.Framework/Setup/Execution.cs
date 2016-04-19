using System.Linq;
using DragonSpark.Activation;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ExecutionContext : AssignedLogical<MethodBase>, IExecutionContext
	{
		readonly MethodInfo Default = typeof(Services).GetMethods().First( info => info.Name == nameof(Services.Get) );

		public static ExecutionContext Instance { get; } = new ExecutionContext();

		public override MethodBase Item => base.Item ?? Default;

		// public override string ToString() => Item.ToString();
	}

	/*//[Serializable]
	public class MethodContext
	{
		// readonly static object Reference = new object();

		public static MethodBase Get( MethodBase method ) => method;
		/*{
			var key = /*KeyFactory.Instance.CreateUsing( method )#2#method.ToString();
			// var code = KeyFactory.Instance.CreateUsing( key ).ToString();
			var result = new Reference<string>( Reference, key ).Item;
			return result;
		}#1#
	}*/
}