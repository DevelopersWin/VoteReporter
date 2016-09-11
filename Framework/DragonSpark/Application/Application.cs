using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	public abstract class Application<T> : Aspects.Extensibility.CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( Items<ICommand>.Default ) {}

		protected Application( params ICommand[] commands ) : this( new OnlyOnceSpecification<T>(), commands ) {}

		protected Application( ISpecification<T> specification, params ICommand[] commands ) : base( commands.Distinct().Prioritize().Fixed() )
		{
			this.ExtendUsing( specification );
		}
	}
}