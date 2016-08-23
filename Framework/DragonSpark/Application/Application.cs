using DragonSpark.Commands;
using DragonSpark.Specifications;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( ApplicationCommands.Default.Get().ToArray() ) {}

		protected Application( params ICommand[] commands ) : base( new OnlyOnceSpecification<T>(), commands ) {}
	}
}