using DragonSpark.Aspects.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;

namespace DragonSpark.Testing.Aspects.Extensions
{
	public class ExtensibleCommandBaseTests
	{
		[Fact]
		public void Verify()
		{
			var sut = new Command();
			sut.Execute( 123 );
			Assert.Empty( sut.Parameters );

			const int valid = 6776;
			sut.Execute( valid );
			Assert.Equal( valid, Assert.Single( sut.Parameters ) );
		}

		[ApplyAutoValidation]
		class Command : ExtensibleCommandBase<int>
		{
			public override bool IsSatisfiedBy( int parameter ) => parameter == 6776;

			public override void Execute( int parameter ) => Parameters.Add( parameter );

			public ICollection<int> Parameters { get; } = new Collection<int>();
		}
	}
}