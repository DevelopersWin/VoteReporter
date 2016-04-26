using DragonSpark.ComponentModel;
using DragonSpark.Runtime;
using Microsoft.Practices.Unity;
using PostSharp.Patterns.Contracts;

namespace DragonSpark.Setup.Commands
{
	public abstract class UnityCommand : CommandBase<object>
	{
		[Locate, Required]
		public IUnityContainer Container { [return: Required]get; set; }
	}
}