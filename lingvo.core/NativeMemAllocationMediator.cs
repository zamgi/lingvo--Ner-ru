using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace lingvo.core
{
    /// <summary>
    /// 
    /// </summary>
    unsafe public sealed class NativeMemAllocationMediator : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        private struct native_mem_block_t
        {
            private IntPtr _BaseAddr;
            private int    _Size;
            private int    _Index;
            [M(O.AggressiveInlining)] public native_mem_block_t( IntPtr baseAddr, int size )
            {
                _BaseAddr = baseAddr;
                _Index    = 0;
                _Size     = size;
            }
            [M(O.AggressiveInlining)] public int FreeSize() => _Size - _Index;
            [M(O.AggressiveInlining)] public IntPtr Alloc( int size )
            {
                Debug.Assert( size <= FreeSize() );

                var strtPtr = _BaseAddr + _Index;
                _Index += size;
                return (strtPtr);
            }
            [M(O.AggressiveInlining)] public IntPtr GetBaseAddr() => _BaseAddr;

            public override string ToString() => $"{FreeSize()}";
        }

        private int _NativeBlockAllocSize;
        private native_mem_block_t _ActiveBlock;
        private List< native_mem_block_t > _FullCompleteBlocks;
        public NativeMemAllocationMediator( int nativeBlockAllocSize )
        {
            _NativeBlockAllocSize = nativeBlockAllocSize;
            _FullCompleteBlocks = new List< native_mem_block_t >();

            _ActiveBlock = AllocBlock( _NativeBlockAllocSize );
        }
        ~NativeMemAllocationMediator() => DisposeNativeResources();
        public void Dispose()
        {
            DisposeNativeResources();
            GC.SuppressFinalize( this );
        }
        private void DisposeNativeResources()
        {
            if ( _FullCompleteBlocks != null )
            {
                foreach ( var t in _FullCompleteBlocks )
                {
                    Marshal.FreeHGlobal( t.GetBaseAddr() );
                }
                _FullCompleteBlocks = null;
            }
            if ( _ActiveBlock.GetBaseAddr() != IntPtr.Zero )
            {
                Marshal.FreeHGlobal( _ActiveBlock.GetBaseAddr() );
                _ActiveBlock = default;
            }
        }

        [M(O.AggressiveInlining)] private static native_mem_block_t AllocBlock( int size ) => new native_mem_block_t( Marshal.AllocHGlobal( size ), size );

        [M(O.AggressiveInlining)] public IntPtr Alloc( int size )
        {
            Debug.Assert( size <= _NativeBlockAllocSize );

            if ( _ActiveBlock.FreeSize() < size )
            {
                _FullCompleteBlocks.Add( _ActiveBlock );
                _ActiveBlock = AllocBlock( _NativeBlockAllocSize );
            }
            var ptr = _ActiveBlock.Alloc( size );
            return (ptr);
        }
        [M(O.AggressiveInlining)] public IntPtr AllocAndCopy( char* source, int sourceLength )
        {
            var copyLenInBytes   = sourceLength * sizeof(char);
            var allocSizeInBytes = copyLenInBytes + sizeof(char);

            Debug.Assert( allocSizeInBytes <= _NativeBlockAllocSize );

            if ( _ActiveBlock.FreeSize() < allocSizeInBytes )
            {
                _FullCompleteBlocks.Add( _ActiveBlock );
                _ActiveBlock = AllocBlock( _NativeBlockAllocSize );
            }
            var dest = _ActiveBlock.Alloc( allocSizeInBytes );

            Buffer.MemoryCopy( source, (void*) dest, copyLenInBytes, copyLenInBytes );
            ((char*) dest)[ sourceLength ] = '\0';
            return (dest);
        }
        [M(O.AggressiveInlining)] public IntPtr AllocAndCopy( string s )
        {
            fixed ( char* s_ptr = s )
            {
                return (AllocAndCopy( s_ptr, s.Length ));
            }
        }

        public override string ToString() => $"{_FullCompleteBlocks.Count}, {_ActiveBlock.FreeSize()}";
    }
}
