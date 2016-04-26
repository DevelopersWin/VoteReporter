using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime.Values;
using DragonSpark.TypeSystem;
using PostSharp.Aspects;
using PostSharp.Aspects.Dependencies;
using PostSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSpark.Aspects
{
	[AttributeUsage( AttributeTargets.Parameter ), LinesOfCodeAvoided( 6 )]
	public class FormattedAttribute : Attribute, IAspectProvider
	{
		public IEnumerable<AspectInstance> ProvideAspects( object targetElement )
		{
			return targetElement.AsTo<ParameterInfo, IEnumerable<AspectInstance>>( info =>
																				  {
																					  return new AspectInstance( info.Member, new ApplyFormatAspect( info.Position, info.IsDefined( typeof(ParamArrayAttribute) ) ) ).ToItem();
																				  } );
		}
	}

	[PSerializable, LinesOfCodeAvoided( 3 ), ProvideAspectRole( StandardRoles.Validation ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.Before, StandardRoles.Caching )]
	public class RecursionGuardAttribute : OnMethodBoundaryAspect
	{
		public RecursionGuardAttribute( int maxCallCount = 4 )
		{
			MaxCallCount = maxCallCount;
		}

		int MaxCallCount { get; set; }

		class Count : ThreadAmbientStore<int>
		{
			public Count( MethodBase method ) : base( method.GetHashCode().ToString() ) {}

			int Update( bool up = true )
			{
				var amount = up ? 1 : -1;
				var result = Value + amount;
				Assign( result );
				return result;
			}

			public int Increment() => Update();

			public int Decrement() => Update( false );
		}

		/*public override void OnInvoke( MethodInterceptionArgs args )
		{
			throw new InvalidOperationException( "WTF" );
			base.OnInvoke( args );
		}*/

		public override void OnEntry( MethodExecutionArgs args )
		{
			if ( new Count( args.Method ).Increment() == MaxCallCount )
			{
				throw new InvalidOperationException( $"Recursion detected in method {new MethodFormatter(args.Method).ToString( null, null )}" );
			}

			base.OnEntry( args );
		}

		public override void OnExit( MethodExecutionArgs args )
		{
			base.OnExit( args );
			new Count( args.Method ).Decrement();
		}
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Validation )]
	class ApplyFormatAspect : OnMethodBoundaryAspect
	{
		public ApplyFormatAspect( int index, bool isArray )
		{
			Index = index;
			IsArray = isArray;
		}

		int Index { get; set; }
		bool IsArray { get; set; }

		// int count = 0;

		public override void OnEntry( MethodExecutionArgs args )
		{
			/*if ( count++ >= 3 )
			{
				throw new InvalidOperationException( "WTF" );
			}*/

			var formatter = Services.Get<Type[]>().With( types => new Func<FormatterFactory.Parameter, object>( new FormatterFactory( new FromKnownFactory<IFormattable>( new KnownTypeFactory( types ) ) ).Create ) );
			formatter.With( f =>
							{
								var current = args.Arguments[Index];

								var formatted = args.Arguments[Index] = IsArray ? current.AsTo<object[], object[]>( objects => objects.Select( o => new FormatterFactory.Parameter( o ) ).Select( f ).Fixed() ) 
									: f( new FormatterFactory.Parameter( current ) );

								args.Arguments.SetArgument( Index, formatted );
								// var temp = 9;
							} );
	
			base.OnEntry( args );
		}

		/*public override void OnInvoke( MethodInterceptionArgs args )
		{
			
		}*/

		/*Func<FormatterFactory.Parameter, object> Formatter
		{
			get { return formatter ?? ( formatter = /*PostSharpEnvironment.IsPostSharpRunning || !Services.IsAvailable ? null :#1# ; }
		}	Func<FormatterFactory.Parameter, object> formatter;*/
	}
}
