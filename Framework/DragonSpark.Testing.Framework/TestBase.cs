using DragonSpark.Activation;
using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Values;
using DragonSpark.Setup;
using DragonSpark.Testing.Framework.Diagnostics;
using DragonSpark.Testing.Framework.Setup;
using PostSharp.Aspects;
using PostSharp.Patterns.Contracts;
using PostSharp.Patterns.Model;
using System;
using System.Diagnostics;
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
			using ( var command = new AssignExecutionContextCommand().ExecuteWith( MethodContext.Get( args.Method ) ) )
			{
				var output = args.Instance.AsTo<IValue<ITestOutputHelper>, Action<string>>( value => value.Item.WriteLine ) ?? ( s => { Debug.WriteLine( s ); } );
				using ( new TracerFactory( output, command.Provider.Get<ILoggerHistory>(), args.Method.Name ).Create() )
				{
					args.Proceed();
				}
			}
		}
	}

	public class AssignExecutionContextCommand : AssignValueCommand<string>
	{
		readonly IWritableValue<IServiceProvider> serviceProvider;

		public AssignExecutionContextCommand() : this( CurrentServiceProvider.Instance, CurrentExecution.Instance ) {}

		public AssignExecutionContextCommand( IWritableValue<IServiceProvider> serviceProvider, IWritableValue<string> value ) : base( value )
		{
			this.serviceProvider = serviceProvider;
		}

		public IServiceProvider Provider => serviceProvider.Item;

		protected override void OnExecute( [NotEmpty]string parameter )
		{
			base.OnExecute( parameter );

			if ( serviceProvider.Item == null )
			{
				serviceProvider.Assign( new ServiceProvider() );
			}
		}
	}

	[Disposable]
	public abstract class TestBase : FixedValue<ITestOutputHelper>
	{
		protected TestBase( ITestOutputHelper output )
		{
			Assign( output );
		}

		/*protected TestBase( ITestOutputHelper output, Action<Type> initialize )
		{
			Output = output;
			initialize( GetType() );
		}*/

		protected ITestOutputHelper Output => Item;

		protected virtual void Dispose( bool disposing ) {}
	}
}