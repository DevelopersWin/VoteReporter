using System;
using DragonSpark.Aspects.Validation;
using DragonSpark.Sources.Parameterized;
using DragonSpark.Sources.Parameterized.Caching;
using DragonSpark.Specifications;

namespace DragonSpark.Application.Setup
{
	[ApplyAutoValidation]
	public class ActivatedServiceSource : ValidatedParameterizedSourceBase<Type, object>
	{
		readonly IServiceProvider provider;
		readonly IsActive active;

		public ActivatedServiceSource( IServiceProvider provider ) : this( provider, IsActive.Default.Get( provider ) ) {}

		ActivatedServiceSource( IServiceProvider provider, IsActive active ) : base( new DelegatedSpecification<object>( ServicesEnabled.Default.ToDelegate().Wrap() ).And( new DelegatedSpecification<Type>( active.Get ).Inverse() ) )
		{
			this.provider = provider;
			this.active = active;
		}

		public override object Get( Type parameter )
		{
			using ( active.Assignment( parameter, true ) )
			{
				var service = provider.GetService( parameter );
				return service;
			}
		}
	}
}