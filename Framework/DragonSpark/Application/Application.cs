using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		protected Application() : this( Items<ICommand>.Default ) {}
		protected Application( params ICommand[] commands ) : base( new OnlyOnceSpecification<T>(), commands.Distinct().Prioritize().Fixed() ) {}
	}
}