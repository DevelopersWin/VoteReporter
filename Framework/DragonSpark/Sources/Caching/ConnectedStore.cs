namespace DragonSpark.Sources.Caching
{
	public class AmbientStack<T> : Scope<IStack<T>>, IStackSource<T>
	{
		public static AmbientStack<T> Default { get; } = new AmbientStack<T>();
		/*readonly static Func<IStack<T>> Store = Default.Get;*/

		public AmbientStack() : this( Stacks<T>.Default ) {}
		public AmbientStack( IParameterizedSource<object, IStack<T>> source ) : base( source.Get ) {}

		public T GetCurrentItem() => Get().Peek();
	}

	public class Condition<T> : Cache<T, ConditionMonitor> where T : class
	{
		public static Condition<T> Default { get; } = new Condition<T>();

		public Condition() : base( key => new ConditionMonitor() ) {}
	}
}