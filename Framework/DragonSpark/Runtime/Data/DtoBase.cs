using DragonSpark.Sources;
using System;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime.Data
{
	[DataContract]
	public abstract class DtoBase<T> : DtoBase, ISource<T> where T : class, new()
	{
		public override object Get() => new T();

		T ISource<T>.Get() => new T();

		public override Type SourceType => typeof(T);
	}

	[DataContract]
	public abstract class DtoBase : ISourceAware
	{
		public abstract object Get();

		public abstract Type SourceType { get; }
	}
}