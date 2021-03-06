using DragonSpark.Runtime;

namespace DragonSpark.Windows.Runtime.Data
{
	public sealed class DataSerializer : DataSerializer<object>
	{
		public new static DataSerializer Default { get; } = new DataSerializer();
		DataSerializer() {}
	}

	public class DataSerializer<T> : DataTransformer<T>
	{
		public static DataSerializer<T> Default { get; } = new DataSerializer<T>();
		protected DataSerializer() : this( Serializer.Default ) {}

		public DataSerializer( ISerializer serializer ) : base( serializer.Load<T> ) {}
	}
}