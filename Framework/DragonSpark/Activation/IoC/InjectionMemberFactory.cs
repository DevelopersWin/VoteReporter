using DragonSpark.Sources.Parameterized;
using Microsoft.Practices.Unity;

namespace DragonSpark.Activation.IoC
{
	public abstract class InjectionMemberFactory<TMember> : ValidatedParameterizedSourceBase<InjectionMemberParameter, TMember> where TMember : InjectionMember
	{}
}