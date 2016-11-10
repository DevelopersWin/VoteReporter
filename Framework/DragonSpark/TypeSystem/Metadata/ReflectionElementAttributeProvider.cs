using DragonSpark.Sources.Parameterized;

namespace DragonSpark.TypeSystem.Metadata
{
	sealed class ReflectionElementAttributeProvider : FirstParameterConstructedSelector<IAttributeProvider>
	{
		public static IParameterizedSource<IAttributeProvider> Default { get; } = new ReflectionElementAttributeProvider().ToCache();
		ReflectionElementAttributeProvider() : base( typeof(TypeInfoAttributeProvider), typeof(PropertyInfoAttributeProvider), typeof(MethodInfoAttributeProvider), typeof(MemberInfoAttributeProvider) ) {}
	}
}