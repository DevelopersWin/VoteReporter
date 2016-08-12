using DragonSpark.Sources.Parameterized;
using Microsoft.Practices.Unity;

namespace DragonSpark.Activation.IoC
{
	public abstract class InjectionMemberFactory<TMember> : ParameterizedSourceBase<InjectionMemberParameter, TMember> where TMember : InjectionMember {}
}