using DragonSpark.Application.Setup;
using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Specifications;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Application
{
	[ApplyAutoValidation]
	public abstract class ApplicationBase<T> : DeclarativeCompositeCommand<T>, IApplication<T>
	{
		protected ApplicationBase( params ICommand[] commands ) : this( new OnlyOnceSpecification<T>(), commands ) {}

		[UsedImplicitly]
		protected ApplicationBase( ISpecification<T> specification, params ICommand[] commands )
			: base( specification, new CommandCollection( commands.Distinct().Prioritize().Fixed() ) ) {}

		public void Dispose() => 
			Commands.Purge().OfType<IDisposable>().Reverse().Each( disposable => disposable.Dispose() );
	}
}