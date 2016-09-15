using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Linq;
using System.Windows.Input;
using DragonSpark.Aspects.Validation;

namespace DragonSpark.Application
{
	[ApplyAutoValidation]
	public abstract class Application<T> : CompositeCommand<T>, IApplication<T>
	{
		readonly ISpecification<T> specification;
		protected Application() : this( Items<ICommand>.Default ) {}

		protected Application( params ICommand[] commands ) : this( new OnlyOnceSpecification<T>(), commands ) {}

		protected Application( ISpecification<T> specification, params ICommand[] commands ) : base( commands.Distinct().Prioritize().Fixed() )
		{
			this.specification = specification;
		}

		public override bool IsSatisfiedBy( T parameter ) => specification.IsSatisfiedBy( parameter );
	}
}