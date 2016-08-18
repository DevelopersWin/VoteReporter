using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace DragonSpark.Testing.Framework.Setup
{
	public sealed class Configure : TransformerBase<IServiceProvider>
	{
		[Export( typeof(ITransformer<IServiceProvider>) )]
		public static Configure Instance { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new SourceInstanceServiceProvider( FixtureContext.Instance, MethodContext.Instance ), new FixtureServiceProvider( FixtureContext.Instance.Get() ), parameter );
	}

	public class ApplicationCommandSource : DragonSpark.Setup.ApplicationCommandSource
	{
		readonly static Func<MethodBase, ImmutableArray<ICommand<AutoData>>> Factory = MetadataCustomizationFactory<ICommand<AutoData>>.Instance.Get;

		public static ApplicationCommandSource Instance { get; } = new ApplicationCommandSource();
		ApplicationCommandSource() : base( Composition.ServiceProviderConfigurations.Instance ) {}

		protected override IEnumerable<ICommand> Yield() => 
			base.Yield().Append( MetadataCommand.Instance ).Concat( Factory( MethodContext.Instance.Get() ).CastArray<ICommand>().AsEnumerable() );
	}

	sealed class MethodTypes : FactoryCache<ImmutableArray<Type>>, ITypeSource
	{
		readonly static Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> Creator = HostedValueLocator<Func<MethodBase, ImmutableArray<Type>>>.Instance.Get;

		public static MethodTypes Instance { get; } = new MethodTypes();
		MethodTypes() : this( MethodContext.Instance.Get ) {}

		readonly Func<MethodBase> methodSource;
		readonly Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator;
		readonly Func<object, ImmutableArray<Type>> selector;

		public MethodTypes( Func<MethodBase> methodSource ) : this( methodSource, Creator ) {}

		public MethodTypes( Func<MethodBase> methodSource, Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator )
		{
			this.methodSource = methodSource;
			this.locator = locator;
			selector = Get;
		}

		protected override ImmutableArray<Type> Create( object parameter ) => locator( parameter ).Introduce( methodSource() ).Concat().Distinct().ToImmutableArray();

		public ImmutableArray<Type> Get()
		{
			var method = methodSource();
			var result = new TypeSource( new object[] { method, method.DeclaringType, method.DeclaringType.Assembly }.Select( selector ).Concat() ).Get();
			return result;
		}

		object ISource.Get() => Get();
	}
}