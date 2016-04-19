using DragonSpark.Runtime.Values;

namespace DragonSpark.Activation
{
	public interface IExecutionContext : IValue
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

		public static object Current => Context.Item;
	}

	public class ExecutionContext : FixedValue<object>, IExecutionContext
	{
		public static ExecutionContext Instance { get; } = new ExecutionContext();

		const string Default = "DefaultExecutionContext";

		public override object Item => base.Item ?? Default;
	}
}