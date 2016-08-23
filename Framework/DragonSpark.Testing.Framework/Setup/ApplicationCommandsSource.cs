using DragonSpark.Commands;
using DragonSpark.ComponentModel;
using DragonSpark.Extensions;
using DragonSpark.Setup;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using System;
using System.Collections;
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
		public static Configure Default { get; } = new Configure();
		Configure() {}

		public override IServiceProvider Get( IServiceProvider parameter ) => 
			new CompositeServiceProvider( new SourceServiceProvider( FixtureContext.Default, MethodContext.Default ), new FixtureServiceProvider( FixtureContext.Default.Get() ), parameter );
	}

	public class ApplicationCommandSource : DragonSpark.Setup.ApplicationCommandSource
	{
		readonly static Func<MethodBase, ImmutableArray<ICommand<AutoData>>> Factory = MetadataCustomizationFactory<ICommand<AutoData>>.Default.Get;

		public static ApplicationCommandSource Default { get; } = new ApplicationCommandSource();
		ApplicationCommandSource() : base( Composition.ServiceProviderConfigurations.Default ) {}

		protected override IEnumerable<ICommand> Yield() => 
			base.Yield()
				.Append( MetadataCommand.Default )
				.Concat( Factory( MethodContext.Default.Get() ).AsEnumerable() );
	}

	sealed class MethodTypes : FactoryCache<ImmutableArray<Type>>, ITypeSource
	{
		readonly static Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> Locator = HostedValueLocator<Func<MethodBase, ImmutableArray<Type>>>.Default.Get;

		public static MethodTypes Default { get; } = new MethodTypes();
		MethodTypes() : this( MethodContext.Default.Get ) {}

		readonly Func<MethodBase> methodSource;
		readonly Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator;
		readonly Func<object, ImmutableArray<Type>> selector;

		public MethodTypes( Func<MethodBase> methodSource ) : this( methodSource, Locator ) {}

		public MethodTypes( Func<MethodBase> methodSource, Func<object, ImmutableArray<Func<MethodBase, ImmutableArray<Type>>>> locator )
		{
			this.methodSource = methodSource;
			this.locator = locator;
			selector = Get;
		}

		protected override ImmutableArray<Type> Create( object parameter ) => locator( parameter ).Introduce( methodSource() ).Concat().ToImmutableArray();

		public ImmutableArray<Type> Get() => this.ToImmutableArray();
		object ISource.Get() => Get();

		public IEnumerator<Type> GetEnumerator()
		{
			var method = methodSource();
			var result = new object[] { method, method.DeclaringType, method.DeclaringType.Assembly }.Select( selector ).Concat().GetEnumerator();
			return result;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}