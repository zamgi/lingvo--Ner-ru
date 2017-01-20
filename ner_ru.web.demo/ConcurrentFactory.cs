using System;
using System.Collections.Concurrent;
using System.Threading;

using lingvo.tokenizing;

namespace lingvo.ner
{
    /// <summary>
    /// 
    /// </summary>
	internal class ConcurrentFactory
	{
		private readonly Semaphore                       _Semaphore;
        private readonly ConcurrentStack< NerProcessor > _Stack;

        public ConcurrentFactory( NerProcessorConfig config, int instanceCount )
		{
            if ( instanceCount <= 0 ) throw (new ArgumentException("instanceCount"));
            if ( config == null     ) throw (new ArgumentNullException("config"));

            _Semaphore = new Semaphore( instanceCount, instanceCount );
            _Stack = new ConcurrentStack< NerProcessor >();
            for ( int i = 0; i < instanceCount; i++ )
			{
                _Stack.Push( new NerProcessor( config ) );
			}			
		}

        public word_t[] Run( string text, bool splitBySmiles )
		{
			_Semaphore.WaitOne();
			var worker = default(NerProcessor);
			try
			{
                worker = Pop( _Stack );
                if ( worker == null )
                {
                    for ( var i = 0; ; i++ )
                    {
                        worker = Pop( _Stack );
                        if ( worker != null )
                            break;

                        Thread.Sleep( 25 );

                        if ( 10000 <= i )
                            throw (new InvalidOperationException( this.GetType().Name + ": no (fusking) worker item in queue" ));
                    }
                }

                var result = worker.Run( text, splitBySmiles ).ToArray();
                return (result);
			}
			finally
			{
				if ( worker != null )
				{
					_Stack.Push( worker );
				}
				_Semaphore.Release();
			}

            throw (new InvalidOperationException( this.GetType().Name + ": nothing to return (fusking)" ));
		}

        private static T Pop< T >( ConcurrentStack< T > stack )
        {
            var t = default(T);
            if ( stack.TryPop( out t ) )
                return (t);
            return (default(T));
        }
	}
}
