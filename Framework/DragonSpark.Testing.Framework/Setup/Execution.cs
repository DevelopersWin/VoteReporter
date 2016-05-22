using DragonSpark.Activation;
using System.Linq;
using System.Reflection;
using DragonSpark.Windows.Runtime;

namespace DragonSpark.Testing.Framework.Setup
{
	public class ExecutionContext : TaskLocalStore<MethodBase>, IExecutionContext
	{
		public MethodInfo Default { get; } = typeof(Services).GetMethods().First( info => info.Name == nameof(Services.Get) );

		public static ExecutionContext Instance { get; } = new ExecutionContext();

		protected override MethodBase Get() => base.Get() ?? Default;
	}
}