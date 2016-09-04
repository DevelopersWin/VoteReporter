using DragonSpark.Aspects.Invocation;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using JetBrains.Annotations;
using Ploeh.AutoFixture.Xunit2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace DragonSpark.Testing.Aspects.Invocation
{
	public class ApplyPoliciesAttributeTests
	{
		[Theory, AutoData]
		void Verify( Subject sut, string message )
		{
			var method = new Action<string>( sut.HelloWorld ).GetReference();
			Repositories<string>.Default.Get( method ).Add( new ModifyMessagePolicy() );

			sut.HelloWorld( message );

			var actual = sut.Messages.Only();
			Assert.StartsWith( ModifyMessagePolicy.Prefix, actual );
			Assert.Contains( message, actual );
		}

		[UsedImplicitly]
		class Subject
		{
			[ApplyPolicies]
			public void HelloWorld( string message ) => Messages.Add( message );

			public ICollection<string> Messages { get; } = new Collection<string>();
		}

		class ModifyMessagePolicy : AlterationBase<IDecorator<string>>
		{
			public const string Prefix = "[ModifyMessagePolicy] Hello World: ";

			public override IDecorator<string> Get( IDecorator<string> parameter ) => new Context( parameter );

			sealed class Context : CommandDecoratorBase<string>
			{
				readonly IDecorator<string> inner;

				public Context( IDecorator<string> inner )
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