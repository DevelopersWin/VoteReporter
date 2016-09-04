using DragonSpark.Aspects.Invocation;
using DragonSpark.Aspects.Validation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using JetBrains.Annotations;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Aspects.Invocation
{
	public class ApplyPoliciesAttributeTests
	{
		[Theory, AutoData]
		void Verify( Subject sut, string message )
		{
			var reference = new Action<string>( sut.HelloWorld ).GetReference();
			Repositories<string>.Default.Get( reference ).Add( new ModifyMessagePolicy() );

			sut.HelloWorld( message );

			var actual = sut.Messages.Only();
			Assert.StartsWith( ModifyMessagePolicy.Prefix, actual );
			Assert.Contains( message, actual );
		}

		[Theory, AutoData]
		void VerifyAutoValidation( Command sut )
		{
			var applied = sut.Apply<Command>( AutoValidationInstaller.Default );
			Assert.False( applied.CanExecute( 123 ) );
			Assert.True( applied.CanExecute( 6776 ) );
			Assert.Equal( 2, applied.CanExecuteCalled );

			Assert.Empty( applied.Parameters );
			applied.Execute( 123 );
			Assert.Empty( applied.Parameters );
			Assert.Equal( 3, applied.CanExecuteCalled );

			Assert.True( applied.CanExecute( 6776 ) );
			Assert.Equal( 4, applied.CanExecuteCalled );

			Assert.Empty( applied.Parameters );
			applied.Execute( 6776 );
			Assert.Single( applied.Parameters, 6776 );
			Assert.Equal( 4, applied.CanExecuteCalled );
		}

		class Command : ICommand
		{
			public event EventHandler CanExecuteChanged = delegate {};

			public int CanExecuteCalled { get; private set; }

			[Decorated]
			public bool CanExecute( object parameter )
			{
				CanExecuteCalled++;
				return parameter is int && (int)parameter == 6776;
			}

			[Decorated]
			public void Execute( object parameter ) => Parameters.Add( parameter );

			public ICollection<object> Parameters { get; } = new Collection<object>();
		}

		sealed class AutoValidationValidator : DecoratorBase<object, bool>
		{
			readonly IAutoValidationController controller;

			public AutoValidationValidator( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override IDecorator<object, bool> Get( IDecorator<object, bool> parameter ) => new Context( controller, parameter );

			sealed class Context : IDecorator<object, bool>
			{
				readonly IAutoValidationController controller;
				readonly IDecorator<object, bool> next;
				public Context( IAutoValidationController controller, IDecorator<object, bool> next )
				{
					this.controller = controller;
					this.next = next;
				}

				public bool Execute( object parameter )
				{
					var result = controller.IsSatisfiedBy( parameter ) || controller.Marked( parameter, next.Execute( parameter ) );
					return result;
				}
			}
		}

		sealed class AutoValidationExecutor : DecoratorBase<object>
		{
			readonly IAutoValidationController controller;

			public AutoValidationExecutor( IAutoValidationController controller )
			{
				this.controller = controller;
			}

			public override IDecorator<object, object> Get( IDecorator<object, object> parameter ) => new Context( controller, parameter );

			sealed class Context : IDecorator<object>
			{
				readonly IAutoValidationController controller;
				readonly IDecorator<object, object> next;
				public Context( IAutoValidationController controller, IDecorator<object, object> next )
				{
					this.controller = controller;
					this.next = next;
				}

				public object Execute( object parameter )
				{
					var result = controller.Execute( parameter, () => next.Execute( parameter ) );
					return result;
				}
			}
		}

		sealed class AutoValidationInstaller : IDecorator<ICommand>
		{
			public static AutoValidationInstaller Default { get; } = new AutoValidationInstaller();
			AutoValidationInstaller() {}

			public object Execute( ICommand parameter )
			{
				var controller = new AutoValidationController( new CommandAdapter( parameter ) );

				var specification = parameter.GetDelegate( nameof(ICommand.CanExecute) );
				Repositories<object, bool>.Default.Get( specification ).Add( new AutoValidationValidator( controller ) );

				var execute = parameter.GetDelegate( nameof(ICommand.Execute) );
				Repositories<object>.Default.Get( execute ).Add( new AutoValidationExecutor( controller ) );
				return null;
			}
		}

		[UsedImplicitly]
		class Subject
		{
			[Decorated]
			public void HelloWorld( string message ) => Messages.Add( message );

			public ICollection<string> Messages { get; } = new Collection<string>();
		}

		sealed class ModifyMessagePolicy : DecoratorBase<string>
		{
			public const string Prefix = "[ModifyMessagePolicy] Hello World: ";

			public override IDecorator<string, object> Get( IDecorator<string, object> parameter ) => new Context( parameter );

			sealed class Context : CommandDecoratorBase<string>
			{
				readonly IDecorator<string, object> inner;

				public Context( IDecorator<string, object> inner )
				{
					this.inner = inner;
				}

				public override void Execute( string parameter )
				{
					var modified = $"{Prefix}{parameter}!";
					inner.Execute( modified );
				}
			}
		}
	}
}