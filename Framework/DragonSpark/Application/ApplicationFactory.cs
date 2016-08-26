using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	public class ApplicationFactory<T> : ConfiguringFactory<ImmutableArray<ICommand>, IApplication> where T : IApplication, new()
	{
		public static ApplicationFactory<T> Default { get; } = new ApplicationFactory<T>();
		ApplicationFactory() : this( array => new T() ) {}

		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory ) : this( factory, ApplicationCommands.Default.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize ) : this( factory, initialize, ApplicationServices.Default.Assign ) {}
		public ApplicationFactory( Func<ImmutableArray<ICommand>, IApplication> factory, Action<ImmutableArray<ICommand>> initialize, Action<IApplication> configure ) : base( factory, initialize, configure ) {}

		public T Create() => (T)Get( Items<ICommand>.Immutable );

		public T Create( ITypeSource types ) => Create( types, Items<ICommand>.Default );

		// public T Create( ITypeSource types, params ICommandSource[] sources ) => Create( types, sources.Select( source => source.Get() ).Concat() );

		public T Create( ITypeSource types, params ICommand[] commands ) => Create( types, commands.AsEnumerable() );
		
		public T Create( ITypeSource types, IEnumerable<ICommand> commands ) => (T)Get( commands.StartWith( ApplicationParts.Default.Configured( SystemPartsFactory.Default.Get( types ) ) ).Append( new DisposeDisposableCommand( Disposables.Default.Get() ) ).Distinct().Prioritize().ToImmutableArray() );
	}
}