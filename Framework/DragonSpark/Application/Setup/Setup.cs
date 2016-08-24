﻿using DragonSpark.Aspects.Validation;
using DragonSpark.Commands;
using DragonSpark.Extensions;
using DragonSpark.Runtime;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;
using DragonSpark.TypeSystem;
using System.Windows.Input;

namespace DragonSpark.Application.Setup
{
	[ApplyAutoValidation]
	public class Setup : CompositeCommand, ISetup
	{
		public static ISetup Current() => AmbientStack.GetCurrentItem<ISetup>();

		public Setup() : this( Items<ICommand>.Default ) {}

		public Setup( params ICommand[] commands ) : this( new OncePerScopeSpecification<object>(), commands ) {}
		public Setup( ISpecification<object> specification, params ICommand[] commands ) : base( specification, commands ) {}

		public DeclarativeCollection<object> Items { get; } = new DeclarativeCollection<object>();

		public Priority Priority { get; set; } = Priority.Normal;

		public override void Execute( object parameter = null )
		{
			using ( new AmbientStackCommand<ISetup>().Run( this ) )
			{
				base.Execute( parameter );
			}
		}
	}
}
