using DragonSpark.Aspects.Extensibility;
using DragonSpark.Aspects.Extensibility.Validation;
using DragonSpark.Extensions;
using JetBrains.Annotations;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Aspects.Extensibility
{
	public class ExtensionPointAttributeTests
	{
		[Theory, AutoData]
		void Verify( Subject sut, string message )
		{
			var reference = new Action<string>( sut.HelloWorld ).Method;
			var context = ExtensionPoints.Default.Get( reference ).Get( sut );
			context.Assign( new ModifyMessagePolicy( context.Get<string, object>() ) );

			sut.HelloWorld( message );

			var actual = sut.Messages.Only();
			Assert.StartsWith( ModifyMessagePolicy.Prefix, actual );
			Assert.Contains( message, actual );
		}

		[Theory, AutoData]
		void VerifyAutoValidation( Command sut )
		{
			var applied = sut; //.Apply<Command>( AutoValidationPolicy.Default );
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

		[UsedImplicitly, ApplyAutoValidation]
		class Command : ICommand
		{
			public event EventHandler CanExecuteChanged = delegate {};

			public int CanExecuteCalled { get; private set; }

			[ExtensionPoint]
			public bool CanExecute( object parameter )
			{
				CanExecuteCalled++;
				return parameter is int && (int)parameter == 6776;
			}

			[ExtensionPoint]
			public void Execute( object parameter ) => Parameters.Add( parameter );

			public ICollection<object> Parameters { get; } = new Collection<object>();
		}

		[UsedImplicitly, EnableExtensions]
		class Subject
		{
			[ExtensionPoint]
			public void HelloWorld( string message ) => Messages.Add( message );

			public ICollection<string> Messages { get; } = new Collection<string>();
		}

		sealed class ModifyMessagePolicy : CommandInvocationBase<string>
		{
			public const string Prefix = "[ModifyMessagePolicy] Hello World: ";
			readonly IInvocation<string, object> inner;

			public ModifyMessagePolicy( IInvocation<string, object> inner )
			{
				this.inner = inner;
			}

			public override void Execute( string parameter ) => inner.Invoke( $"{Prefix}{parameter}!" );
		}
	}
}