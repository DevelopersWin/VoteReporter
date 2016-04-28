using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
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
			return targetElement.AsTo<ParameterInfo, IEnumerable<AspectInstance>>( info => new AspectInstance( info.Member, new ApplyFormatAspect( info.Position ) ).ToItem() );
		}
	}

	[PSerializable, ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 6 )]
	class ApplyFormatAspect : MethodInterceptionAspect
	{
		public ApplyFormatAspect( int index )
		{
			Index = index;
		}

		int Index { get; set; }

		/*public override void OnEntry( MethodExecutionArgs args )
		{
			
	
			base.OnEntry( args );
		}*/

		public override void OnInvoke( MethodInterceptionArgs args )
		{
			var formatter = Services.Get<FormatterFactory>();

			var current = args.Arguments[Index];
			var array = args.Method.GetParameters()[Index].Has<ParamArrayAttribute>();

			args.Arguments[Index] = array ? current.AsTo<object[], object[]>( objects => objects.Select( o => new FormatterFactory.Parameter( o ) ).Select( formatter.Create ).Fixed() ) 
				: formatter.Create( new FormatterFactory.Parameter( current ) );

			base.OnInvoke( args );	
		}

		/*Func<FormatterFactory.Parameter, object> Formatter
		{
			get { return formatter ?? ( formatter = /*PostSharpEnvironment.IsPostSharpRunning || !Services.IsAvailable ? null :#1# ; }
		}	Func<FormatterFactory.Parameter, object> formatter;*/
	}
}
