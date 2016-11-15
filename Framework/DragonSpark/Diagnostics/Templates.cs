using DragonSpark.Activation;
using DragonSpark.Sources.Coercion;
using DragonSpark.Sources.Parameterized.Caching;
using JetBrains.Annotations;
using Serilog;
using System;
using System.Windows.Input;

namespace DragonSpark.Diagnostics
{
	public static class Extensions
	{
		public static T With<T>( this object @this ) where T : class, ICommand => Templates<T>.Default.Get( @this );
	}

	public sealed class Templates<T> : CommandCache<object, T> where T : class, ICommand
	{
		public static Templates<T> Default { get; } = new Templates<T>();
		Templates() : this( Logger.Default.Get, ParameterConstructor<ILogger, T>.Default ) {}

		[UsedImplicitly]
		public Templates( Func<object, ILogger> loggerSource, Func<ILogger, T> commandSource ) : base( loggerSource.To( commandSource ).Get ) {}
	}
}