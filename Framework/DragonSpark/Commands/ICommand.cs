using DragonSpark.Specifications;
using System.Windows.Input;

namespace DragonSpark.Commands
{
	public interface ICommand<in TParameter> : ICommand, ISpecification<TParameter>
	{
		void Execute( TParameter parameter );

		void Update();
	}
}