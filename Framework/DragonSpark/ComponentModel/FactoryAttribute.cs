using DragonSpark.Activation;
using System;

namespace DragonSpark.ComponentModel
{
	public sealed class ServiceAttribute : ServicesValueBase
	{
		public ServiceAttribute( Type serviceType = null ) : base( new ServicesValueProvider.Converter( serviceType ) ) {}
	}

	public sealed class FactoryAttribute : ServicesValueBase
	{
		// readonly static IParameterizedSource<MemberInfo, Type> Types = new ParameterizedSource<MemberInfo, Type>( new FactoryTypeLocator<MemberInfo>( member => member.GetMemberType(), member => member.DeclaringType ).Create );
		readonly static Func<Type, object> FactoryMethod = InstanceFromFactoryTypeFactory.Instance.Create;
		
		public FactoryAttribute( Type factoryType = null ) : base( new ServicesValueProvider.Converter( p => factoryType ?? FactoryTypeLocator.Instance.Get( p.GetMethod.ReturnType ) ), FactoryMethod ) {}
	}
}