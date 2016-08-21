using DragonSpark.Runtime;
using DragonSpark.Sources.Parameterized;
using System;
using System.Linq;
using System.Windows.Input;
using Xunit;

namespace DragonSpark.Testing.Aspects.Validation
{
	public class AutoValidationRegistryTests
	{
		[Fact]
		public void VerifyRegistry()
		{
			
		}

		class AutoValidationRegistry : RepositoryBase<RegistryListing>
		{
			public static AutoValidationRegistry Default { get; } = new AutoValidationRegistry();
			AutoValidationRegistry() : base( new []
			{
				new RegistryListing( typeof(IParameterizedSource<,>), nameof(IParameterizedSource.Get) ),
				new RegistryListing( typeof(IParameterizedSource), nameof(IParameterizedSource.Get) ),

				new RegistryListing( typeof(ICommand<>), nameof(ICommand.Execute) ),
				new RegistryListing( typeof(ICommand), nameof(ICommand.Execute) ),
			}.AsEnumerable() ) {}
		}

		public struct RegistryListing
		{
			public RegistryListing( Type declaringType, string methodName )
			{
				DeclaringType = declaringType;
				MethodName = methodName;
			}

			public Type DeclaringType { get; }
			public string MethodName { get; }
		}
	}
}