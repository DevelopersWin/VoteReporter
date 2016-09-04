using DragonSpark.Aspects.Invocation;
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
	public class ApplyDecorationsAttributeTests
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

		[UsedImplicitly, ApplyPolicy( typeof(AutoValidationPolicy) )]
		class Command : ICommand
		{
			public event EventHandler CanExecuteChanged = delegate {};

			public int CanExecuteCalled { get; private set; }

			[ApplyDecorations]
			public bool CanExecute( object parameter )
			{
				CanExecuteCalled++;
				return parameter is int && (int)parameter == 6776;
			}

			[ApplyDecorations]
			public void Execute( object parameter ) => Parameters.Add( parameter );

			public ICollection<object> Parameters { get; } = new Collection<object>();
		}

		[UsedImplicitly]
		class Subject
		{
			[ApplyDecorations]
			public void HelloWorld( string message ) => Messages.Add( message );

			public ICollection<string> Messages { get; } = new Collection<string>();
		}

		sealed class ModifyMessagePolicy : DecoratorFactoryBase<string>
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

				protected override void Execute( string parameter )
				{
					var modified = $"{Prefix}{parameter}!";
					inner.Execute( modified );
				}
			}
		}
	}
}