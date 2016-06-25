using DragonSpark.Runtime;
using DragonSpark.Runtime.Specifications;
using Ploeh.AutoFixture.Xunit2;
using System.Windows.Input;
using DragonSpark.Aspects;
using Xunit;

namespace DragonSpark.Testing.Aspects
{
	public class ValidationAspectTests
	{
		[Fact]
		public void Validation()
		{}

		/*[AspectConfiguration( SerializerType = typeof(MsilAspectSerializer) )]
		[ProvideAspectRole( StandardRoles.Validation ), LinesOfCodeAvoided( 4 ), AttributeUsage( AttributeTargets.Class )]
		[AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Threading ), AspectRoleDependency( AspectDependencyAction.Order, AspectDependencyPosition.After, StandardRoles.Caching )]
		public abstract class AutoValidationAttributeBase : InstanceLevelAspect, IAspectProvider
		{
			
		}

		public abstract class FactoryBase<TParameter, TResult> : IFactory<TParameter, TResult>
		{
			readonly Coerce<TParameter> coercer;

			protected FactoryBase() : this( Parameter<TParameter>.Coercer ) {}

			protected FactoryBase( Coerce<TParameter> coercer )
			{
				this.coercer = coercer;
			}

			bool IFactoryWithParameter.CanCreate( object parameter ) => true;

			object IFactoryWithParameter.Create( object parameter )
			{
				var coerced = coercer( parameter );
				var result = coerced.IsAssigned() ? Create( coerced ) : default(TResult);
				return result;
			}

			public virtual bool CanCreate( TParameter parameter ) => true;

			public abstract TResult Create( TParameter parameter );
		}

		

		[AutoValidation.Factory]
		class TestFactory2 : FactoryBase<object, object>
		{
			public override object Create( object parameter ) => null;
		}*/

		[Theory, AutoData]
		void CanExecuteAsExpected( ValidatedCommand sut )
		{
			Assert.True( sut.CanExecute( null ) );
			Assert.False( sut.CanExecute( null ) );
		}

		[Theory, AutoData]
		void ExecuteAsExpected( ValidatedCommand sut )
		{
			Assert.True( sut.CanExecute( null ) );
			Assert.False( sut.Executed );

			sut.Execute( null );

			Assert.True( sut.Executed );

			sut.Reset();

			Assert.False( sut.Executed );

			sut.Execute( null );

			Assert.False( sut.Executed );
		}

		[Theory, AutoData]
		void ExecuteAsBaseCommandAsExpected( ValidatedCommand command )
		{
			var sut = (ICommand)command;

			Assert.True( command.CanExecute( null ) );
			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.True( command.Executed );

			command.Reset();

			Assert.False( command.Executed );

			sut.Execute( null );

			Assert.False( command.Executed );
		}

		[AutoValidation.GenericCommand]
		class ValidatedCommand : CommandBase<object>
		{
			public ValidatedCommand() : base( new OnlyOnceSpecification() ) {}

			public bool Executed { get; private set; }

			public void Reset() => Executed = false;

			public override void Execute( object parameter ) => Executed = true;
		}
	}
}
