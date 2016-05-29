using DragonSpark.Runtime.Stores;

namespace DragonSpark.Activation
{
	public interface IExecutionContext : IWritableStore
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

		public static void Assign( object current ) => Context.Assign( current );

		public static object GetCurrent() => Context.Value;
	}

	public class ExecutionContext : FixedStore<object>, IExecutionContext
	{
		const string Default = "DefaultExecutionContext";

		public static ExecutionContext Instance { get; } = new ExecutionContext();

		protected override object Get() => base.Get() ?? Default;
	}
}