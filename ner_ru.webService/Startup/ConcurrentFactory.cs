using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using lingvo.tokenizing;

namespace lingvo.ner.webService
{
    /// <summary>
    /// 
    /// </summary>
	public sealed class ConcurrentFactory : IDisposable
	{
		private readonly SemaphoreSlim                   _Semaphore;
        private readonly ConcurrentStack< NerProcessor > _Stack;

        internal ConcurrentFactory( NerEnvironment env, Config opts )
		{
			var instanceCount = opts.CONCURRENT_FACTORY_INSTANCE_COUNT;
            if ( instanceCount <= 0 ) throw (new ArgumentException( nameof(instanceCount) ));
			Config = opts ?? throw (new ArgumentNullException( nameof(opts) ));

            _Semaphore = new SemaphoreSlim( instanceCount, instanceCount );
            _Stack     = new ConcurrentStack< NerProcessor >();
            for ( int i = 0; i < instanceCount; i++ )
			{
                _Stack.Push( env.CreateNerProcessor() );
			}			
		}
        public void Dispose()
        {
            foreach ( var worker in _Stack )
            {
				worker.Dispose();
			}
			_Stack.Clear();
        }

        internal Config Config { get; }

        public async Task< List< word_t[] > > Run_Details( string text, bool splitBySmiles )
        {
			await _Semaphore.WaitAsync().ConfigureAwait( false );
			var worker = default(NerProcessor);
			var result = default(List< word_t[] >);
			try
			{
                worker = Pop( _Stack );
                result = worker.Run_Details( text, splitBySmiles );
			}
			finally
			{
                if ( worker != null )
				{
                    _Stack.Push( worker );
				}
				_Semaphore.Release();
			}
			return (result);
		}
		public async Task< List< word_t > > Run( string text, bool splitBySmiles )
        {
			await _Semaphore.WaitAsync().ConfigureAwait( false );
			var worker = default(NerProcessor);
			var result = default(List< word_t >);
			try
			{
                worker = Pop( _Stack );
                result = worker.Run( text, splitBySmiles );
			}
			finally
			{
                if ( worker != null )
				{
                    _Stack.Push( worker );
				}
				_Semaphore.Release();
			}
			return (result);
		}

        private static T Pop< T >( ConcurrentStack< T > stack ) => stack.TryPop( out var t ) ? t : default;
	}
}
