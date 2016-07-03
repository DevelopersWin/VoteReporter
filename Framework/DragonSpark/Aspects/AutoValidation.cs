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
using System.Linq;
using System.Windows.Input;

namespace DragonSpark.Aspects
{
	[PSerializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	public abstract class AutoValidationCommandBase : MethodInterceptionAspect
	{
		readonly static Func<object, IAutoValidationController> Get = AutoValidation.Controller.Get;

		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = Get( args.Instance );
			if ( controller != null )
			{
				var parameter = new AutoValidationParameter( args, args.Arguments.GetArgument( 0 ) );
				args.ReturnValue = Execute( controller, parameter ) ?? args.ReturnValue;
			}
			else
			{
				base.OnInvoke( args );
			}
		}

		protected abstract object Execute( IAutoValidationController controller, AutoValidationParameter parameter );
	}

	[PSerializable]
	public class ExecutionAspect : AutoValidationCommandBase
	{
		public static ExecutionAspect Instance { get; } = new ExecutionAspect();

		protected override object Execute( IAutoValidationController controller, AutoValidationParameter parameter ) => controller.Execute( parameter );
	}

	[PSerializable]
	public class ValidatorAspect : AutoValidationCommandBase
	{
		public static ValidatorAspect Instance { get; } = new ValidatorAspect();

		protected override object Execute( IAutoValidationController controller, AutoValidationParameter parameter )
		{
			var result = parameter.Proceed<bool>();
			controller.MarkValid( parameter.Parameter, result );
			return null;
		}
	}

	public static class AutoValidation
	{
		// public static ImmutableArray<IProfile> DefaultProfiles { get; } = new IProfile[] { GenericFactoryProfile.Instance, FactoryProfile.Instance, GenericCommandProfile.Instance, CommandProfile.Instance }.ToImmutableArray();

		public static ICache<IAutoValidationController> Controller { get; } = new Cache<IAutoValidationController>();
		public static ICache<IParameterValidationAdapter> Adapter { get; } = new Cache<IParameterValidationAdapter>();

		public sealed class Factory : AutoValidationAttributeBase
		{
			public static AutoValidationTypeDescriptor Descriptor { get; } = new AutoValidationTypeDescriptor( typeof(IFactoryWithParameter), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( FactoryAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor ) );

			public Factory() : base( Profile ) {}
		}

		public sealed class GenericFactory : AutoValidationAttributeBase
		{
			public static AutoValidationTypeDescriptor Descriptor { get; } = new AutoValidationTypeDescriptor( typeof(IFactory<,>), nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( GenericFactoryAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor, Factory.Descriptor ) );
			public GenericFactory() : base( Profile ) {}
		}

		public sealed class Command : AutoValidationAttributeBase
		{
			public static AutoValidationTypeDescriptor Descriptor { get; } = new AutoValidationTypeDescriptor( typeof(ICommand), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( CommandAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor ) );

			public Command() : base( Profile ) {}
		}

		public sealed class GenericCommand : AutoValidationAttributeBase
		{
			public static AutoValidationTypeDescriptor Descriptor { get; } = new AutoValidationTypeDescriptor( typeof(ICommand<>), nameof(ICommand.CanExecute), nameof(ICommand.Execute) );
			readonly static AutoValidationProfile Profile = new AutoValidationProfile( GenericCommandAdapterFactory.Instance.ToDelegate(), ImmutableArray.Create( Descriptor, Command.Descriptor ) );
			public GenericCommand() : base( Profile ) {}
		}
	}

	[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
	[MulticastAttributeUsage( TargetMemberAttributes = MulticastAttributes.NonAbstract | MulticastAttributes.Instance )]
	public abstract class AutoValidationAttributeBase : InstanceLevelAspect, IAspectProvider
	{
		readonly AutoValidationProfile profile;

		protected AutoValidationAttributeBase( AutoValidationProfile profile )
		{
			this.profile = profile;
		}

		public override bool CompileTimeValidate( Type type )
		{
			var types = profile.Descriptors.Select( descriptor => descriptor.Type.Adapt() ).ToImmutableArray();
			if ( !types.IsAssignableFrom( type ) )
			{
				throw new InvalidOperationException( $"{type} does not implement any of the types defined in {GetType()}, which are: {string.Join( ",", types.Select( t => t.Type.FullName ) )}" );
			}
			return true;
		}

		public override void RuntimeInitializeInstance() => AutoValidation.Controller.Set( Instance, new AutoValidationController( profile.Factory( Instance ) ) );

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
						if ( !pair.MappedMethod.IsAbstract && ( pair.MappedMethod.IsFinal || pair.MappedMethod.IsVirtual ) )
						{
							var aspect = pair.InterfaceMethod.Name == descriptor.IsValid ? ValidatorAspect.Instance :
											pair.InterfaceMethod.Name == descriptor.Execute ? ExecutionAspect.Instance
											: default(IAspect);
							var method = pair.MappedMethod.AccountForGenericDefinition();
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
}