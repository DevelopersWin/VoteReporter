﻿using System;

namespace DragonSpark.Runtime
{
	public class DelegatedDisposable : Disposable
	{
		readonly Action action;
		public DelegatedDisposable( Action action )
		{
			this.action = action;
		}

		protected override void OnDispose( bool disposing )
		{
			if ( disposing )
			{
				action();
			}
		}
	}
}