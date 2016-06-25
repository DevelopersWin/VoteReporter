using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Windows.Input;

namespace DragonSpark.Aspects
{
	[PSerializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ParameterValidatorAspectBase : MethodInterceptionAspect
	{
		readonly static Func<object, IParameterValidationController> Get = ParameterValidation.Controller.Get;

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = Get( args.Instance );
			if ( controller != null )
			{
				var parameter = new RelayParameter( args, args.Arguments.GetArgument( 0 ) );
				args.ReturnValue = Execute( controller, parameter ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		protected abstract object Execute( IParameterValidationController controller, RelayParameter parameter );
	}

	[PSerializable]
	public class ExecutionAspect : ParameterValidatorAspectBase
	{
		public static ExecutionAspect Instance { get; } = new ExecutionAspect();

		protected override object Execute( IParameterValidationController controller, RelayParameter parameter ) => controller.Execute( parameter );
	}

	[PSerializable]
	public class ValidatorAspect : ParameterValidatorAspectBase
	{
		public static ValidatorAspect Instance { get; } = new ValidatorAspect();

		protected override object Execute( IParameterValidationController controller, RelayParameter parameter )
		{
			var result = parameter.Proceed<bool>();
			controller.MarkValid( parameter.Parameter, result );
			return null;
		}
	}

	public static class ParameterValidation
	{
		public static ICache<IParameterValidationController> Controller { get; } = new Cache<IParameterValidationController>();
	}

	/*public sealed class ValidatedCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = CommandAdapterFactory.Instance.ToDelegate();

		public ValidatedCommand() : base( Factory ) {}

		public class Commands : AutoValidationAttributeBase
		{
			readonly static ProfileTypeDescriptor Profile = new ProfileTypeDescriptor( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );

			public Commands() : base( Profile ) {}
		}
	}

	public sealed class ValidatedGenericCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = GenericCommandAdapterFactory.Instance.ToDelegate();

		public ValidatedGenericCommand() : base( Factory ) {}

		public class Commands : AutoValidationAttributeBase
		{
			readonly static ProfileTypeDescriptor Profile = new ProfileTypeDescriptor( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );

			public Commands() : base( Profile ) {}
		}
	}*/

	/*public sealed class ValidatedFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = FactoryAdapterFactory.Instance.ToDelegate();

		public ValidatedFactory() : base( Factory ) {}

		public class Commands : AutoValidationAttributeBase
		{
			readonly static ProfileTypeDescriptor Profile = new ProfileTypeDescriptor( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );

			public Commands() : base( Profile ) {}
		}
	}*/

	/*public sealed class ValidatedGenericFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = GenericFactoryAdapterFactory.Instance.ToDelegate();
		
		public ValidatedGenericFactory() : base( Factory ) {}

		public class Commands : AutoValidationAttributeBase
		{
			readonly static ProfileTypeDescriptor Profile = new ProfileTypeDescriptor( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			public Commands() : base( Profile ) {}
		}
	}*/

	public static class AutoValidation
	{
		public sealed class Factory : AutoValidationAttributeBase
		{
			public static ProfileTypeDescriptor Descriptor { get; } = new ProfileTypeDescriptor( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( FactoryAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor ) );

			public Factory() : base( Profile ) {}
		}

		public sealed class GenericFactory : AutoValidationAttributeBase
		{
			public static ProfileTypeDescriptor Descriptor { get; } = new ProfileTypeDescriptor( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( GenericFactoryAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor, Factory.Descriptor ) );
			public GenericFactory() : base( Profile ) {}
		}

		public sealed class Command : AutoValidationAttributeBase
		{
			public static ProfileTypeDescriptor Descriptor { get; } = new ProfileTypeDescriptor( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( CommandAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor ) );

			public Command() : base( Profile ) {}
		}

		public sealed class GenericCommand : AutoValidationAttributeBase
		{
			public static ProfileTypeDescriptor Descriptor { get; } = new ProfileTypeDescriptor( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( GenericCommandAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor, Command.Descriptor ) );
			public GenericCommand() : base( Profile ) {}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public abstract class AutoValidationAttributeBase : InstanceLevelAspect, IAspectProvider
	{
		readonly AutoValidationProfile profile;

		protected AutoValidationAttributeBase( AutoValidationProfile profile )
		{
			this.profile = profile;
		}

		public override void RuntimeInitializeInstance() => ParameterValidation.Controller.Set( Instance, new ParameterValidationController( profile.Factory( Instance ) ) );

		IEnumerable<AspectInstance> IAspectProvider.ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			if ( type != null )
			{
				var repository = PostSharpEnvironment.CurrentProject.GetService<IAspectRepositoryService>();
		
				foreach ( var descriptor in profile.Descriptors )
				{
					var mappedMethods = type.Adapt().GetMappedMethods( descriptor.Type );

					foreach ( var pair in mappedMethods )
					{
						if ( !pair.Item2.IsAbstract && ( pair.Item2.IsFinal || pair.Item2.IsVirtual ) )
						{
							var aspect = pair.Item1.Name == descriptor.IsValid ? ValidatorAspect.Instance :
											pair.Item1.Name == descriptor.Execute ? ExecutionAspect.Instance
											: default(IAspect);
							var method = pair.Item2.FromGenericDefinition();
							if ( aspect != null && !repository.HasAspect( method, aspect.GetType() ) )
							{
								yield return new AspectInstance( method, aspect );
							}
						}
					}
				}
			}
		}
	}

	/*[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ValidatedParameterAspectBase : InstanceLevelAspect
	{
		readonly Func<object, IParameterValidator> factory;
		
		protected ValidatedParameterAspectBase( Func<object, IParameterValidator> factory )
		{
			this.factory = factory;
		}

		
	}*/
}