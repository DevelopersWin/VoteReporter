using System.Linq;
using DragonSpark.Activation;
using System.Reflection;
using DragonSpark.Extensions;

namespace DragonSpark.Testing.Framework.Setup
{
	public class CurrentExecution : AssignedLogical<MethodBase>, IExecutionContext
	{
		public static CurrentExecution Instance { get; } = new CurrentExecution();

		public override MethodBase Item => base.Item ?? MethodBase.GetCurrentMethod();

		// public override string ToString() => Item.ToString();
	}

	public static class Environment
	{
		static Environment()
		{
			AppDomainFactory.Instance.Create().FirstOrDefault( appDomain => appDomain.FriendlyName.Contains( "JetBrains.ReSharper.TaskRunner" ) ).With( domain =>
			{
				new InitializeTestRunnerEnvironmentCommand( domain ).ExecuteWith( new object() );
			});
		}
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