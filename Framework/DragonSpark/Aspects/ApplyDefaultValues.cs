using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Extensibility;
using PostSharp.Patterns.Model;
using PostSharp.Patterns.Threading;
using PostSharp.Reflection;
using PostSharp.Serialization;
using System;

namespace DragonSpark.Aspects
{
	[MulticastAttributeUsage( MulticastTargets.Property, PersistMetaData = false )]
	[PSerializable, ProvideAspectRole( "Default Object Values" ), LinesOfCodeAvoided( 6 )]
	[AttributeUsage( AttributeTargets.Assembly )]
	[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading )]
	public sealed class ApplyDefaultValues : LocationInterceptionAspect, IInstanceScopedAspect
	{
		void Initialize() => Processor = Processor ?? new ValueProcessor( base.OnGetValue );

		ValueProcessor Processor { get; set; }

		public override bool CompileTimeValidate( LocationInfo locationInfo ) => DefaultValuePropertySpecification.Instance.IsSatisfiedBy( locationInfo.PropertyInfo );

		public override void RuntimeInitialize( LocationInfo locationInfo ) => Initialize();

		public override void OnGetValue( LocationInterceptionArgs args ) => Processor.With( processor => processor.Run( args ) );

		public override void OnSetValue( LocationInterceptionArgs args )
		{
			Processor.With( processor => processor.Apply() );
			base.OnSetValue( args );
		}

		object IInstanceScopedAspect.CreateInstance( AdviceArgs adviceArgs ) => MemberwiseClone();

		void IInstanceScopedAspect.RuntimeInitializeInstance() => Initialize();

		[Synchronized]
		class ValueProcessor : CommandBase<LocationInterceptionArgs>
		{
			[Reference]
			readonly ConditionMonitor monitor = new ConditionMonitor();

			readonly Action<LocationInterceptionArgs> @continue;

			public ValueProcessor( Action<LocationInterceptionArgs> @continue )
			{
				this.@continue = @continue;
			}

			protected override void OnExecute( LocationInterceptionArgs parameter )
			{
				var apply = monitor.Apply();
				if ( apply )
				{
					var context = new DefaultValueParameter( parameter.Instance ?? parameter.Location.DeclaringType, parameter.Location.PropertyInfo );
					var value = DefaultPropertyValueFactory.Instance.Create( context );
					parameter.SetNewValue( parameter.Value = value );
				}
				else
				{
					@continue( parameter );
				}
			}

			public void Apply() => monitor.Apply();
		}
	}
}