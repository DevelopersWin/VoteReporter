using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		readonly Action<T> inner;

		protected Application() : this( Items<ICommand>.Default ) {}

		protected Application( params ICommand[] commands ) : this( new OnlyOnceSpecification<T>(), commands ) {}

		protected Application( ISpecification<T> specification, params ICommand[] commands ) : base( commands.Distinct().Prioritize().Fixed() )
		{
			inner = new SpecificationCommand<T>( specification, base.Execute ).Execute;
		}

		public sealed override void Execute( T parameter = default(T) ) => inner( parameter );
	}
}