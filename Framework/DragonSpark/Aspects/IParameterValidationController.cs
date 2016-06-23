using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Properties;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using PostSharp.Extensibility;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
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
				throw new InvalidOperationException( $"Controller not set for {args.Instance} - {args.Instance.GetHashCode()}" );
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

	public sealed class ValidatedCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = CommandParameterAdapterFactory.Instance.ToDelegate();

		public ValidatedCommand() : base( Factory ) {}

		public class Commands : ParameterWorkflowCommandsAspectBase
		{
			readonly static Profile Profile = new Profile( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );

			public Commands() : base( Profile ) {}
		}
	}

	public sealed class ValidatedGenericCommand : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = GenericCommandParameterAdapterFactory.Instance.ToDelegate();

		public ValidatedGenericCommand() : base( Factory ) {}

		public class Commands : ParameterWorkflowCommandsAspectBase
		{
			readonly static Profile Profile = new Profile( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );

			public Commands() : base( Profile ) {}
		}
	}

	public sealed class ValidatedFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = FactoryParameterAdapterFactory.Instance.ToDelegate();

		public ValidatedFactory() : base( Factory ) {}

		public class Commands : ParameterWorkflowCommandsAspectBase
		{
			readonly static Profile Profile = new Profile( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );

			public Commands() : base( Profile ) {}
		}
	}

	public sealed class ValidatedGenericFactory : ValidatedParameterAspectBase
	{
		readonly static Func<object, IParameterValidator> Factory = GenericFactoryParameterAdapterFactory.Instance.ToDelegate();
		
		public ValidatedGenericFactory() : base( Factory ) {}

		public class Commands : ParameterWorkflowCommandsAspectBase
		{
			readonly static Profile Profile = new Profile( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			public Commands() : base( Profile ) {}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( Inheritance = MulticastInheritance.Strict, TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public abstract class ParameterWorkflowCommandsAspectBase : TypeLevelAspect, IAspectProvider
	{
		readonly Profile profile;

		protected ParameterWorkflowCommandsAspectBase( Profile profile )
		{
			this.profile = profile;
		}

		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var type = targetElement as Type;
			if ( type != null )
			{
				var implementedType = profile.Type;
				var types = implementedType.Append( implementedType.GetTypeInfo().IsGenericTypeDefinition ? implementedType.GetTypeInfo().ImplementedInterfaces : Items<Type>.Default ).ToImmutableArray();

				foreach ( var check in types )
				{
					var mappedMethods = type.Adapt().GetMappedMethods( check );

					foreach ( var pair in mappedMethods )
					{
						if ( pair.Item2.DeclaringType == type && !pair.Item2.IsAbstract && ( pair.Item2.IsFinal || pair.Item2.IsVirtual ) )
						{
							var aspect = pair.Item1.Name == profile.IsValid ? ValidatorAspect.Instance :
										 pair.Item1.Name == profile.Execute ? ExecutionAspect.Instance
										 : default(IAspect);

							if ( aspect != null )
							{
								yield return new AspectInstance( pair.Item2, aspect );
							}
						}
					}
				}
			}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class ValidatedParameterAspectBase : InstanceLevelAspect
	{
		readonly Func<object, IParameterValidator> factory;
		
		protected ValidatedParameterAspectBase( Func<object, IParameterValidator> factory )
		{
			this.factory = factory;
		}

		public override void RuntimeInitializeInstance() => ParameterValidation.Controller.Set( Instance, new ParameterValidationController( factory( Instance ) ) );
	}
}