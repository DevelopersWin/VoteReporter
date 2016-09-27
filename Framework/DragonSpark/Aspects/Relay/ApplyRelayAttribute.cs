using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;

namespace DragonSpark.Aspects.Relay
{
	[ProvideAspectRole( KnownRoles.InvocationWorkflow ), LinesOfCodeAvoided( 1 ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Validation )]
	public sealed class ApplyRelayAttribute : ApplyAspectBase
	{
		public ApplyRelayAttribute() : base( Support.Default ) {}
	}

	/*public sealed class CommandValidationDescriptor : Descriptor<SpecificationRelayAspect, >
	{
		public static CommandValidationDescriptor Default { get; } = new CommandValidationDescriptor();
		CommandValidationDescriptor() : base( CommandTypeDefinition.Default.Validation, GenericCommandTypeDefinition.Default.Validation, typeof(SpecificationRelay<>), typeof(ISpecificationRelay) ) {}
	}*/

	/*sealed class InvocationLocator<T> : IParameterizedSource<object, T> where T : class, IInvocation
	{
		public static InvocationLocator<T> Default { get; } = new InvocationLocator<T>();
		InvocationLocator() {}

		public T Get( object parameter ) => parameter as T;
	}*/
}
