using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;

namespace lingvo.sentsplitting
{
    /// <summary>
    /// 
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    internal sealed class DirectAccessList< T >: IList< T >, ICollection< T >, IEnumerable< T >
    {
        private const int MAX_CAPACITY_THRESHOLD = 0x7FFFFFFF /*int.MaxValue*/ - 0x400 * 0x400 /*1MB*/; /* => 2146435071 == 0x7fefffff*/
        private const int DEFAULT_CAPACITY = 4;
        private static readonly T[] EMPTY_ARRAY = new T[ 0 ];

        internal T[] _Items;
        internal int _Size;

		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the default initial capacity.</summary>
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public DirectAccessList()
		{
			_Items = DirectAccessList< T >.EMPTY_ARRAY;
		}
		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that is empty and has the specified initial capacity.</summary>
		/// <param name="capacity">The number of elements that the new list can initially store.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="capacity" /> is less than 0. </exception>
		[TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public DirectAccessList( int capacity )
		{
			if ( capacity < 0 )
			{
                throw (new ArgumentException("capacity"));
			}
			if ( capacity == 0 )
			{
                _Items = DirectAccessList< T >.EMPTY_ARRAY;
			}
            else
            {
			    _Items = new T[ capacity ];
            }
		}
		/// <summary>Initializes a new instance of the <see cref="T:System.Collections.Generic.List`1" /> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.</summary>
		/// <param name="collection">The collection whose elements are copied to the new list.</param>
		/// <exception cref="T:System.ArgumentNullException">
		///   <paramref name="collection" /> is null.</exception>
        public DirectAccessList( IEnumerable< T > collection )
		{
			if ( collection == null )
			{
                throw (new ArgumentNullException("collection"));
			}
			var collection2 = collection as ICollection< T >;
			if ( collection2 == null )
			{
				_Size = 0;
                _Items = DirectAccessList< T >.EMPTY_ARRAY;
				using ( IEnumerator< T > enumerator = collection.GetEnumerator() )
				{
					while (enumerator.MoveNext())
					{
						Add(enumerator.Current);
					}
				}
				return;
			}
			int count = collection2.Count;
			if (count == 0)
			{
                _Items = DirectAccessList< T >.EMPTY_ARRAY;
				return;
			}
			_Items = new T[ count ];
			collection2.CopyTo( _Items, 0 );
			_Size = count;
		}

        /// <summary>Gets or sets the total number of elements the internal data structure can hold without resizing.</summary>
        /// <returns>The number of elements that the <see cref="T:System.Collections.Generic.List`1" /> can contain before resizing is required.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <see cref="P:System.Collections.Generic.List`1.Capacity" /> is set to a value that is less than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        /// <exception cref="T:System.OutOfMemoryException">There is not enough memory available on the system.</exception>
        public int Capacity
        {
            [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
            get
            {
                return (_Items.Length);
            }
            set
            {
                /*if ( value < _size )
                {
                    //ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.value, ExceptionResource.ArgumentOutOfRange_SmallCapacity );
                }*/
                if ( value != _Items.Length )
                {
                    if ( 0 < value )
                    {
                        var array = new T[ value ];
                        if ( 0 < _Size )
                        {
                            Array.Copy( _Items, 0, array, 0, _Size );
                        }
                        _Items = array;
                    }
                    else
                    {
                        _Items = DirectAccessList< T >.EMPTY_ARRAY;
                    }
                }
            }
        }
        public void Insert2Head( T item )
        {
            if ( _Size == _Items.Length )
            {
                EnsureCapacity( _Size + 1 );
            }
            if ( 0 < _Size )
            {
                Array.Copy( _Items, 0, _Items, 1, _Size );
            }
            _Items[ 0 ] = item;
            _Size++;
        }

        private void EnsureCapacity( int min )
        {
            if ( _Items.Length < min )
            {
                int n = (_Items.Length == 0) ? 4 : (_Items.Length * 2);
                if ( n > MAX_CAPACITY_THRESHOLD )
                {
                    n = MAX_CAPACITY_THRESHOLD;
                }
                if ( n < min )
                {
                    n = min;
                }
                Capacity = n;
            }
        }

        #region [.IList< T >.]
        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <returns>The element at the specified index.</returns>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        public T this[ int index ]
        {
            [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
            get
            {
                /*if ( index >= _size )
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }*/
                return (_Items[ index ]);
            }
            [TargetedPatchingOptOut( "Performance critical to inline across NGen image boundaries" )]
            set
            {
                /*if ( index >= _size )
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }*/
                _Items[ index ] = value;
            }
        }

        /// <summary>Inserts an element into the <see cref="T:System.Collections.Generic.List`1" /> at the specified index.</summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert. The value can be null for reference types.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index" /> is less than 0.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.Generic.List`1.Count" />.</exception>
        public void Insert( int index, T item )
        {
            /*if ( index > _Size )
            {
                ThrowHelper.ThrowArgumentOutOfRangeException( ExceptionArgument.index, ExceptionResource.ArgumentOutOfRange_ListInsert );
            }*/
            if ( _Size == _Items.Length )
            {
                EnsureCapacity( _Size + 1 );
            }
            if ( index < _Size )
            {
                Array.Copy( _Items, index, _Items, index + 1, _Size - index );
            }
            _Items[ index ] = item;
            _Size++;
        }


        public int IndexOf( T item )
        {
            throw new NotImplementedException();
        }
        public void RemoveAt( int index )
        {
            throw new NotImplementedException();
        }
        #endregion

        #region [.ICollection< T >.]
        /// <summary>Adds an object to the end of the <see cref="T:System.Collections.Generic.List`1" />.</summary>
        /// <param name="item">The object to be added to the end of the <see cref="T:System.Collections.Generic.List`1" />. The value can be null for reference types.</param>
        public void Add( T item )
        {
            if ( _Size == _Items.Length )
            {
                EnsureCapacity( _Size + 1 );
            }
            _Items[ _Size++ ] = item;
        }

        /// <summary>Removes all elements from the <see cref="T:System.Collections.Generic.List`1" />.</summary>
        public void Clear()
        {
            if ( _Size > 0 )
            {
                Array.Clear( _Items, 0, _Size );
                _Size = 0;
            }
        }

        /// <summary>Gets the number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.</summary>
        /// <returns>The number of elements actually contained in the <see cref="T:System.Collections.Generic.List`1" />.</returns>
        public int Count
        {
            [TargetedPatchingOptOut( "Performance critical to inline this type of method across NGen image boundaries" )]
            get
            {
                return (_Size);
            }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove( T item )
        {
            throw new NotImplementedException();
        }

        public bool Contains( T item )
        {
            throw new NotImplementedException();
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }
        #endregion

        #region [.IEnumerable< T >.]
        public IEnumerator< T > GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region [.IEnumerable.]
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
