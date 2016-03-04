using DragonSpark.Composition;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Patterns.Contracts;
using System;
using System.Reflection;

namespace DragonSpark.Setup
{
	public class ApplicationHost : CompositeWritableValue<Assembly[]>
	{
		public ApplicationHost() : this( new AssemblyHost(), new CompositionHost() ) {}

		public ApplicationHost( [Required]AssemblyHost assemblyHost, [Required]CompositionHost compositionHost ) : base( assemblyHost, compositionHost ) {}
	}

	public class CompositionHost : FixedValue<Assembly[]>
	{
		readonly Func<Assembly[], System.Composition.Hosting.CompositionHost> factory;
		readonly CompositionHostContext context;

		public CompositionHost() : this( CompositionHostFactory.Instance.Create, new CompositionHostContext() ) {}

		public CompositionHost( [Required]Func<Assembly[], System.Composition.Hosting.CompositionHost> factory, [Required]CompositionHostContext context )
		{
			this.factory = factory;
			this.context = context;
		}

		public override void Assign( Assembly[] item )
		{
			var host =  factory( item );
			context.Assign( host );

			base.Assign( item );
		}
	}

	public abstract class Application<TArguments> : SetupContainer<TArguments>
	{
		protected Application( params ICommand<TArguments>[] commands ) : base( commands ) {}

		public virtual Assembly[] Assemblies { get; set; }

		protected override void OnExecute( TArguments parameter )
		{
			using ( new AssignValueCommand<Assembly[]>( new ApplicationHost() ).ExecuteWith( Assemblies ) )
			{
				using ( new AmbientContextCommand<ITaskMonitor>().ExecuteWith( new TaskMonitor() ) )
				{
					base.OnExecute( parameter );
				}
			}
		}
	}

	public class SetupApplicationCommand<TSetup> : DeferredCommand<TSetup, object> where TSetup : ISetup {}

	public abstract class SetupContainer<T> : CompositeCommand<T>
	{
		protected SetupContainer( params ICommand<T>[] commands ) : base( commands ) {}

		public Collection<object> Items { get; } = new Collection<object>();
	}

	public abstract class Setup : SetupContainer<object>, ISetup
	{
		protected override void OnExecute( object parameter )
		{
			using ( new AmbientContextCommand<ISetup>().ExecuteWith( this ) )
			{
				base.OnExecute( parameter );
			}
		}
	}
}
