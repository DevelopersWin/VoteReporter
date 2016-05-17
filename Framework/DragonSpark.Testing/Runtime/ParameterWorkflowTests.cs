using DragonSpark.Activation;
using DragonSpark.Aspects;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using PostSharp.Aspects;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Dependencies;
using PostSharp.Aspects.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DragonSpark.Testing.Runtime
{
	public class Controller
	{
		readonly IParameterAware workflow;
		readonly IAssignableParameterAware assignable;

		public Controller( object target, IAssignableParameterAware assignable ) : this( new ParameterWorkflow( target, assignable ), assignable ) {}

		Controller( IParameterAware workflow, IAssignableParameterAware assignable )
		{
			this.workflow = workflow;
			this.assignable = assignable;
		}

		public bool IsAllowed( Func<object, bool> assign, object parameter )
		{
			using ( new CanAssignment( assignable, assign ) )
			{
				return workflow.IsAllowed( parameter );
			}
		}

		public object Execute( Func<object, object> assign, object parameter )
		{
			using ( new Assignment( assignable, assign ) )
			{
				return workflow.Execute( parameter );
			}
		}

		class CanAssignment : Assignment<Func<object, bool>>
		{
			public CanAssignment( IAssignableParameterAware assignable, Func<object, bool> first ) : base( assignable.Assign, new Value<Func<object, bool>>( first ) ) {}
		}

		class Assignment : Assignment<Func<object, object>>
		{
			public Assignment( IAssignableParameterAware assignable, Func<object, object> first ) : base( assignable.Assign, new Value<Func<object, object>>( first ) ) {}
		}
	}

	[MethodInterceptionAspectConfiguration( SerializerType = typeof(MsilAspectSerializer) ), Serializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
	public class ValidateParameter : InstanceLevelAspect, IAspectProvider
	{
		readonly static IDictionary<Type, Profile> Profiles = new Dictionary<Type, Profile>
		{
			{ typeof(IFactoryWithParameter), new Profile( FactoryControllerFactory.Instance, nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) },
			{ typeof(IFactory<,>), new Profile( GenericFactoryControllerFactory.Instance, nameof(IFactoryWithParameter.CanCreate), nameof(IFactoryWithParameter.Create) ) }
		};

		public override void RuntimeInitializeInstance()
		{
			WithProfiles( Instance.GetType(), ( implementation, profile ) =>
						  {
								var controller = profile.Factory.Create( Instance );
								new AssociatedController( Instance, implementation ).Assign( controller );
						  } );
		}

		static void WithProfiles( Type type, Action<Type, Profile> action )
		{
			var adapter = type.Adapt();
			Profiles.Each( pair =>
							{
								var implementation = adapter.DetermineImplementation( pair.Key );
								if ( implementation != null )
								{
									action( implementation, pair.Value );
								}
							} );
		}

		class Profile
		{
			public Profile( IControllerFactory factory, string isAllowed, string execute )
			{
				Factory = factory;
				IsAllowed = isAllowed;
				Execute = execute;
			}

			public IControllerFactory Factory { get; }
			public string IsAllowed { get; }
			public string Execute { get; }
		}

		public interface IControllerFactory
		{
			Controller Create( object instance );
		}

		abstract class ControllerFactoryBase<T> : IControllerFactory
		{
			public Controller Create( object instance )
			{
				var aware = instance.AsTo<T, IParameterAware>( Create );
				var assignable = new AssignableParameterAware( aware );
				var result = new Controller( instance, assignable );
				return result;
			}

			protected abstract IParameterAware Create( T instance );
		}

		class GenericFactoryControllerFactory : IControllerFactory
		{
			public static GenericFactoryControllerFactory Instance { get; } = new GenericFactoryControllerFactory();

			public Controller Create( object instance )
			{
				var implementation = instance.GetType().Adapt().GetImplementations( typeof(IFactory<,>) ).First();
				var result = (Controller)GetType().InvokeGeneric( nameof(Create), implementation.GenericTypeArguments, instance );
				return result;
			}

			static Controller Create<TParameter, TResult>( object instance ) => FactoryControllerFactory<TParameter, TResult>.Instance.Create( instance );
		}

		class FactoryControllerFactory<TParameter, TResult> : ControllerFactoryBase<IFactory<TParameter, TResult>>
		{
			public static FactoryControllerFactory Instance { get; } = new FactoryControllerFactory();

			protected override IParameterAware Create( IFactory<TParameter, TResult> instance ) => new FactoryParameterAware<TParameter, TResult>( instance );
		}

		class FactoryControllerFactory : ControllerFactoryBase<IFactoryWithParameter>
		{
			public static FactoryControllerFactory Instance { get; } = new FactoryControllerFactory();

			protected override IParameterAware Create( IFactoryWithParameter instance ) => new FactoryWithParameterAware( instance );
		}

		/*[OnMethodInvokeAdvice, MethodPointcut( nameof(DetermineIsAllowed) )]
		public void IsAllowed( MethodInterceptionArgs args ) => 
			args.ReturnValue = controller.IsAllowed( o => args.GetReturnValue<bool>(), args.Arguments.Single() );*/

		/*[OnMethodInvokeAdvice, MulticastPointcut( MemberName = "Create" )]
		public void Create( MethodInterceptionArgs args ) => 
			args.ReturnValue = controller.Execute( o => args.GetReturnValue(), args.Arguments.Single() );*/
		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			var result = new List<AspectInstance>();
			targetElement.As<Type>( type => WithProfiles( type, ( implementation, profile ) =>
																{
																	var targets = type.GetInterfaceMap( implementation ).TargetMethods;
																	var items = targets.Where( info => info.Name == profile.IsAllowed || info.Name == profile.Execute ).Select( info => new AspectInstance( info, info.Name == profile.IsAllowed ? (IAspect)new IsAllowedAspect( implementation ) : new ExecuteAspect( implementation ) ) );
																	result.AddRange( items );
																} ) );
			// MessageSource.MessageSink.Write( new Message( MessageLocation.Unknown, SeverityType.ImportantInfo, "6776", $"YO: {targetElement.GetType().AssemblyQualifiedName}", null, null, null ));
			return result;
		}
	}

	[Serializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public class IsAllowedAspect : MethodInterceptionAspect
	{
		readonly Type type;
		public IsAllowedAspect( Type type )
		{
			this.type = type;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = new AssociatedController( args.Instance, type ).Value;
			args.ReturnValue = controller.IsAllowed( o => args.GetReturnValue<bool>(), args.Arguments.Single() );
		}
	}

	[Serializable]
	[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Method )]
	public class ExecuteAspect : MethodInterceptionAspect
	{
		readonly Type type;
		public ExecuteAspect( Type type )
		{
			this.type = type;
		}

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var controller = new AssociatedController( args.Instance, type ).Value;
			args.ReturnValue = controller.Execute( o => args.GetReturnValue(), args.Arguments.Single() );;
		}
	}

	class AssociatedController : AssociatedStore<Controller>
	{
		public AssociatedController( object instance, Type awareType ) : base( instance, KeyFactory.Instance.ToString( awareType, typeof(AssociatedController) ) ) {}
	}

	class AssignableParameterAware : IAssignableParameterAware
	{
		readonly IParameterAware inner;
			
		public AssignableParameterAware( IParameterAware inner )
		{
			this.inner = inner;
		}

		public void Assign( Func<object, bool> condition ) => Condition = condition;

		public void Assign( Func<object, object> factory ) => Factory = factory;

		Func<object, bool> Condition { get; set; }

		Func<object, object> Factory { get; set; }

		public bool IsAllowed( object parameter ) => Condition?.Invoke( parameter ) ?? inner.IsAllowed( parameter );

		public object Execute( object parameter ) => Factory?.Invoke( parameter ) ?? inner.Execute( parameter );
	}

	public interface IAssignableParameterAware : IParameterAware
	{
		void Assign( Func<object, bool> condition );

		void Assign( Func<object, object> factory );
	}

	public class ParameterWorkflowTests
	{
		[Fact]
		public void BasicCondition()
		{
			var sut = new Factory();
			var cannot = sut.To<IFactoryWithParameter>().CanCreate( 456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );

			var can = sut.To<IFactoryWithParameter>().CanCreate( 123 );
			Assert.True( can );
			Assert.Equal( 2, sut.CanCreateCalled );

			Assert.Equal( 0, sut.CreateCalled );

			var created = sut.Create( 123 );
			Assert.Equal( 2, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 6776, created );
		}

		[Fact]
		public void ExtendedCheck()
		{
			var sut = new ExtendedFactory();
			Assert.Equal( 0, sut.CanCreateCalled );
			Assert.Equal( 0, sut.CanCreateGenericCalled );
			var cannot = sut.CanCreate( (object)456 );
			Assert.False( cannot );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 1, sut.CanCreateGenericCalled );

			var can = sut.CanCreate( 6776 );
			Assert.True( can );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );

			Assert.Equal( 0, sut.CreateCalled );
			Assert.Equal( 0, sut.CreateGenericCalled );

			var created = sut.Create( (object)6776 );
			Assert.Equal( 1, sut.CanCreateCalled );
			Assert.Equal( 2, sut.CanCreateGenericCalled );
			Assert.Equal( 1, sut.CreateCalled );
			Assert.Equal( 1, sut.CreateGenericCalled );
			Assert.Equal( 6776 + 123f, created );
		}

		[ValidateParameter]
		public class Factory : IFactoryWithParameter
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return (int)parameter == 123;
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return 6776;
			}
		}

		[ValidateParameter]
		class ExtendedFactory : IFactory<int, float>
		{
			public int CanCreateCalled { get; private set; }

			public int CreateCalled { get; private set; }

			public int CanCreateGenericCalled { get; private set; }

			public int CreateGenericCalled { get; private set; }

			public bool CanCreate( object parameter )
			{
				CanCreateCalled++;
				return parameter is int && CanCreate( (int)parameter );
			}

			public object Create( object parameter )
			{
				CreateCalled++;
				return Create( (int)parameter );
			}

			public bool CanCreate( int parameter )
			{
				CanCreateGenericCalled++;
				return parameter == 6776;
			}

			public float Create( int parameter )
			{
				CreateGenericCalled++;
				return parameter + 123;
			}
		}
	}
}
