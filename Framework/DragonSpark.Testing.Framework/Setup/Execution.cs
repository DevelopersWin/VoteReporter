using DragonSpark.Activation;
using DragonSpark.Aspects;
using System.Reflection;

namespace DragonSpark.Testing.Framework.Setup
{
	public class CurrentExecution : AssignedLogical<int?>, IExecutionContext
	{
		public static CurrentExecution Instance { get; } = new CurrentExecution();

		public override int? Item => base.Item ?? MethodContext.Get( MethodBase.GetCurrentMethod() );
	}

	//[Serializable]
	public class MethodContext
	{
		// readonly static ConcurrentDictionary<string, int> Cache = new ConcurrentDictionary<string, Tuple<string>>();

		public static int? Get( MethodBase method ) => KeyFactory.Instance.CreateUsing( method );
	}
}