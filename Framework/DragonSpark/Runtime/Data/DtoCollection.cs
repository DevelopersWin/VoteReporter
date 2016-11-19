using DragonSpark.Extensions;
using DragonSpark.Sources;
using DragonSpark.Sources.Parameterized;
using DragonSpark.TypeSystem;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DragonSpark.Runtime.Data
{
	[CollectionDataContract]
	public sealed class DtoCollection<T> : DtoCollectionBase<ISource<T>, T> where T : class 
	{
		readonly Func<ISource<T>, IParameterizedSource<T, T>> create;

		public DtoCollection() : this( Items<ISource<T>>.Default ) {}
		public DtoCollection( params ISource<T>[] sources ) : this( ObjectMapper<T>.Default.Get, sources.AsEnumerable() ) {}

		[UsedImplicitly]
		public DtoCollection( Func<ISource<T>, IParameterizedSource<T, T>> create, IEnumerable<ISource<T>> list ) : base( list.ToList() )
		{
			this.create = create;
		}

		protected override IEnumerable<T> Yield()
		{
			foreach ( var item in Items )
			{
				yield return create( item ).Get( item.Get() );
			}
		}
	}
}