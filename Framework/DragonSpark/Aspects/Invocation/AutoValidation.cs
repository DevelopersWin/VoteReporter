using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator : InvocationFactoryBase<object, bool>
	{
		readonly IAutoValidationController controller;
		public AutoValidationValidator( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		protected override IInvocation<object, bool> Create( IInvocation<object, bool> parameter ) => new Context( controller, parameter );

		sealed class Context : InvocationBase<object, bool>
		{
			readonly IAutoValidationController controller;
			readonly IInvocation<object, bool> next;

			public Context( IAutoValidationController controller, IInvocation<object, bool> next )
			{
				this.controller = controller;
				this.next = next;
			}

			public override bool Invoke( object parameter ) => 
				controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Invoke( parameter ) );
		}
	}

	sealed class AutoValidationExecutor : InvocationFactoryBase
	{
		readonly IAutoValidationController controller;
		/*public static AutoValidationExecutor Default { get; } = new AutoValidationExecutor();
		AutoValidationExecutor() {}*/

		public AutoValidationExecutor( IAutoValidationController controller )
		{
			this.controller = controller;
		}

		public override IInvocation Get( IInvocation parameter ) => new Context( controller, parameter );

		sealed class Context : IInvocation
		{
			readonly IAutoValidationController controller;
			readonly IInvocation next;

			public Context( IAutoValidationController controller, IInvocation next )
			{
				this.controller = controller;
				this.next = next;
			}

			public object Invoke( object parameter ) => controller.Execute( parameter, () => next.Invoke( parameter ) );
		}
	}

	[MulticastAttributeUsage( PersistMetaData =  true )]
	public class ApplyAutoValidationAttribute : ApplyPoliciesAttribute
	{
		public ApplyAutoValidationAttribute() : base( typeof(AutoValidationPolicy) ) {}
	}

	public sealed class AutoValidationPolicy : PolicyBase
	{
		public static AutoValidationPolicy Default { get; } = new AutoValidationPolicy();
		AutoValidationPolicy() : this( PointSource.DefaultNested.Get, Validation.Defaults.ControllerSource ) {}

		readonly Func<Type, IEnumerable<IExtensionPoint>> source;
		readonly Func<object, IAutoValidationController> controllerSource;

		AutoValidationPolicy( Func<Type, IEnumerable<IExtensionPoint>> source, Func<object, IAutoValidationController> controllerSource )
		{
			this.source = source;
			this.controllerSource = controllerSource;
		}

		protected override IEnumerable<PolicyMapping> Get( object parameter )
		{
			var controller = controllerSource( parameter );
			var links = new AutoValidationValidator( controller ).Append<IInvocationLink>( new AutoValidationExecutor( controller ) ).Repeat();

			var result = source( parameter.GetType() ).Zip( links, ( point, link ) => new PolicyMapping( point, link ) );
			return result;
		}

		sealed class PointSource : IParameterizedSource<Type, IEnumerable<IExtensionPoint>>
		{
			public static IParameterizedSource<Type, IEnumerable<IExtensionPoint>> DefaultNested { get; } = new PointSource().ToCache();
			PointSource() : this( AutoValidation.DefaultProfiles, ExtensionPoints.Default.Get ) {}

			readonly ImmutableArray<IAspectProfile> profiles;
			readonly Func<MethodBase, IExtensionPoint> pointSource;

			PointSource( ImmutableArray<IAspectProfile> profiles, Func<MethodBase, IExtensionPoint> pointSource )
			{
				this.profiles = profiles;
				this.pointSource = pointSource;
			}

			public IEnumerable<IExtensionPoint> Get( Type parameter )
			{
				foreach ( var profile in profiles.Introduce( parameter, tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ) )
				{
					yield return pointSource( profile.Validation.Find( parameter ) );
					yield return pointSource( profile.Method.Find( parameter ) );
				}
			}
		}
	}
}
