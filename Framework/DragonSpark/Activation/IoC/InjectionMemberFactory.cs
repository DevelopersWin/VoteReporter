using Microsoft.Practices.Unity;

namespace DragonSpark.Activation.IoC
{
	public abstract class InjectionMemberFactory<TMember> : FactoryWithSpecificationBase<InjectionMemberParameter, TMember> where TMember : InjectionMember
	{}
}