using DragonSpark.Runtime.Values;

namespace DragonSpark.Activation
{
	public interface IExecutionContext : IStore
	{}

	public static class Execution
	{
		static Execution()
		{
			Initialize( ExecutionContext.Instance );
		}

		public static void Initialize( IExecutionContext current )
		{
			Context = current;
		}	static IExecutionContext Context { get; set; }

		public static object Current => Context.Value;
	}

	public class ExecutionContext : FixedStore<object>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		const string Default = "DefaultExecutionContext";

		protected override object Get() => base.Get() ?? Default;
	}
}