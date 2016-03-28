using DragonSpark.Activation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Setup;
using PostSharp.Aspects;
using PostSharp.Patterns.Model;
using System;
using Xunit.Abstractions;

namespace DragonSpark.Testing.Framework
{
	public class OutputValue : AssociatedValue<Type, string[]>
	{
		public OutputValue( Type instance ) : base( instance ) {}
	}

	public class InitializeOutputCommand : Command<Type>
	{
		readonly ITestOutputHelper helper;

		public InitializeOutputCommand( ITestOutputHelper helper )
		{
			this.helper = helper;
		}

		protected override void OnExecute( Type parameter )
		{
			var item = new OutputValue( parameter ).Item;
			item.With( lines => lines.Each( helper.WriteLine ) );
		}
	}

	[Serializable, LinesOfCodeAvoided( 8 )]
	public class AssignExecutionContextAspect : MethodInterceptionAspect
	{
		public sealed override void OnInvoke( MethodInterceptionArgs args )
		{
			using ( new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( args.Method ) ) )
			{
				args.Proceed();
				/*using ( Services.Get<IApplication>() )
				{
					
				}*/
			}
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<int?>
	{
		readonly IWritableValue<IServiceProvider> serviceProvider;

		public AssignExecutionContextCommand() : this( new CurrentServiceProvider(), CurrentExecution.Instance ) {}

		public AssignExecutionContextCommand( IWritableValue<IServiceProvider> serviceProvider, IWritableValue<int?> value ) : base( value )
		{
			this.serviceProvider = serviceProvider;
		}

		protected override void OnExecute( int? parameter )
		{
			if ( serviceProvider.Item == null )
			{
				serviceProvider.Assign( new ServiceProvider() );
			}

			base.OnExecute( parameter );
		}
	}

	[Disposable]
	public abstract class Tests
	{
		protected Tests( ITestOutputHelper output ) : this( output, new InitializeOutputCommand( output ).Run ) {}

		protected Tests( ITestOutputHelper output, Action<Type> initialize )
		{
			Output = output;
			initialize( GetType() );
		}

		[Reference]
		protected ITestOutputHelper Output { get; }

		protected virtual void Dispose( bool disposing ) {}
	}
}