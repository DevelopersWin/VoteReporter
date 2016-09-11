using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Specifications;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects.Invocation
{
	sealed class AutoValidationValidator : InvocationFactoryBase<object, bool>, ISpecification<object>
	{
		readonly IAutoValidationController controller;
		readonly Active active;

		public AutoValidationValidator( IAutoValidationController controller, Active active )
		{
			this.controller = controller;
			this.active = active;
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
		public bool IsSatisfiedBy( object parameter ) => !active.IsActive;
	}

	sealed class AutoValidationExecutor : InvocationFactoryBase
	{
		readonly IAutoValidationController controller;
		readonly Active active;

		public AutoValidationExecutor( IAutoValidationController controller, Active active )
		{
			this.controller = controller;
			this.active = active;
		}

		public override IInvocation Get( IInvocation parameter ) => new Context( controller, active, parameter );

		sealed class Context : IInvocation
		{
			readonly IAutoValidationController controller;
			readonly Active active;
			readonly IInvocation next;

			public Context( IAutoValidationController controller, Active active, IInvocation next )
			{
				this.controller = controller;
				this.active = active;
				this.next = next;
			}

			public object Invoke( object parameter )
			{
				active.IsActive = true;
				var result = controller.Execute( parameter, () => next.Invoke( parameter ) );
				active.IsActive = false;
				return result;
			}
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

		readonly Func<Type, IEnumerable<Pair>> source;
		readonly Func<object, IAutoValidationController> controllerSource;

		AutoValidationPolicy( Func<Type, IEnumerable<Pair>> source, Func<object, IAutoValidationController> controllerSource )
		{
			this.source = source;
			this.controllerSource = controllerSource;
		}

		struct Pair
		{
			public Pair( IExtensionPoint validation, IExtensionPoint execution )
			{
				Validation = validation;
				Execution = execution;
			}

			public IExtensionPoint Validation { get; }
			public IExtensionPoint Execution { get; }
		}

		sealed class PointSource : IParameterizedSource<Type, IEnumerable<Pair>>
		{
			public static IParameterizedSource<Type, IEnumerable<Pair>> DefaultNested { get; } = new PointSource().ToCache();
			PointSource() : this( AutoValidation.DefaultProfiles, ExtensionPoints.Default.Get ) {}

			readonly ImmutableArray<IAspectProfile> profiles;
			readonly Func<MethodBase, IExtensionPoint> pointSource;

			PointSource( ImmutableArray<IAspectProfile> profiles, Func<MethodBase, IExtensionPoint> pointSource )
			{
				this.profiles = profiles;
				this.pointSource = pointSource;
			}

			public IEnumerable<Pair> Get( Type parameter ) => Yield( parameter ).ToArray();

			IEnumerable<Pair> Yield( Type parameter )
			{
				foreach ( var profile in profiles.Introduce( parameter, tuple => tuple.Item1.Method.DeclaringType.Adapt().IsAssignableFrom( tuple.Item2 ) ) )
				{
					var validation = pointSource( profile.Validation.Find( parameter ) );
					var execution = pointSource( profile.Method.Find( parameter ) );
					yield return new Pair( validation, execution );
				}
			}
		}

		public override void Execute( object parameter )
		{
			var active = new Active();
			var controller = controllerSource( parameter );
			var validator = new AutoValidationValidator( controller, active );
			var execution = new AutoValidationExecutor( controller, active );

			foreach ( var pair in source( parameter.GetType() ).Fixed() )
			{
				pair.Validation.Get( parameter ).Add( validator );
				pair.Execution.Get( parameter ).Add( execution );
			}
		}
	}

	public class Active
	{
		public bool IsActive { get; set; }
	}
}
