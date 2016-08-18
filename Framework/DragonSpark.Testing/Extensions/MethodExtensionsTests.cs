using DragonSpark.Diagnostics;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using DragonSpark.Sources.Parameterized;
using System;
using System.Linq;
using System.Windows.Input;
using DragonSpark.Diagnostics.Logging;
using Xunit;

namespace DragonSpark.Testing.Extensions
{
	public class MethodExtensionsTests
	{
		[Fact]
		public void BasicFactory()
		{
			var method = new Func<object, object>( new Factory().To<IParameterizedSource>().Get ).Method;
			var found = method.AccountForGenericDefinition();
			Assert.NotNull( found );
			Assert.NotSame( method, found );
			Assert.True( found.DeclaringType.IsGenericTypeDefinition );
		}

		[Fact]
		public void GenericFactory()
		{
			var method = new Func<object, bool>( new Factory().IsSatisfiedBy ).Method;
			var found = method.AccountForGenericDefinition();
			Assert.NotNull( found );
			Assert.NotSame( method, found );
			Assert.True( found.DeclaringType.IsGenericTypeDefinition );
		}

		[Fact]
		public void BasicCommand()
		{
			var method = new Action<object>( new Command().To<ICommand>().Execute ).Method;
			var found = method.AccountForGenericDefinition();
			Assert.NotNull( found );
			Assert.NotSame( method, found );
			Assert.True( found.DeclaringType.IsGenericTypeDefinition );
		}

		[Fact]
		public void GenericCommand()
		{
			var method = new Action<object>( new Command().Execute ).Method;
			var found = method.AccountForGenericDefinition();
			Assert.NotNull( found );
			Assert.Same( method, found );
			Assert.False( found.DeclaringType.IsGenericTypeDefinition );
		}

		[Fact]
		public void BreakingBuildTest()
		{
			var method = new Action<Action<string>>( new PurgeLoggerMessageHistoryCommand( new LoggerHistorySink() ).Execute ).Method;
			var found = method.AccountForGenericDefinition();
			Assert.NotNull( found );
			Assert.NotSame( method, found );
			Assert.True( found.DeclaringType.IsGenericTypeDefinition );
			var memberInfo = found.GetParameterTypes().Single();
			Assert.True( memberInfo.IsConstructedGenericType );
			Assert.True( memberInfo.ContainsGenericParameters );
			Assert.True( memberInfo.IsGenericType );
			Assert.False( memberInfo.IsGenericTypeDefinition );
			Assert.Equal( memberInfo.GetGenericTypeDefinition() , typeof(Action<>) );
		}

		class Factory : ValidatedParameterizedSourceBase<object, object>
		{
			public Factory() : base( Specifications.Assigned ) {}

			public override object Get( object parameter ) => null;
		}

		class Command : CommandBase<object>
		{
			public override void Execute( object parameter ) {}
		}
	}
}