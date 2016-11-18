using DragonSpark.Sources;
using System;
using System.Collections.Concurrent;

namespace DragonSpark.Testing.Framework.Runtime
{
	public sealed class ExecutionContext : SourceBase<TaskContext>, IDisposable
	{
		public static ExecutionContext Default { get; } = new ExecutionContext( Identification.Default );

		readonly ConcurrentDictionary<Identifier, TaskContext> entries = new ConcurrentDictionary<Identifier, TaskContext>();
		readonly ISource<Identifier> store;
		readonly Func<Identifier, TaskContext> create;
		readonly Action<Identifier> remove;

		ExecutionContext( ISource<Identifier> store )
		{
			this.store = store;
			create = Create;
			remove = Remove;
		}

		public override TaskContext Get() => entries.GetOrAdd( store.Get(), create );

		TaskContext Create( Identifier context ) => new TaskContext( context, remove );

		void Remove( Identifier obj )
		{
			TaskContext removed;
			entries.TryRemove( obj, out removed );
		}

		public void Dispose() => Get().Dispose();
	}
}