﻿using System;
using System.Buffers;
using System.Collections.Generic;

namespace SharpNeat.Graphs
{
    public sealed class ConnectionSorterV1
    {
        #region Public Static Methods

        // Notes.
        // Array.Sort() can sort a secondary array, but here we need to sort a third array; the below code
        // does this but performs a lot of unnecessary memory allocation and copying. As such this logic was
        // replaced (see current implementation of SharpNeat.Graphs.ConnectionSorter) with a customised sort
        // routine that is faster and more efficient w.r.t memory allocations and copying.

        public static void Sort<S>(in ConnectionIdArrays connIdArrays, S[] weightArr) where S : struct
        {
            // Init array of indexes.
            Span<int> srcIds = connIdArrays.GetSourceIdSpan();
            Span<int> tgtIds = connIdArrays.GetTargetIdSpan();

            int len = srcIds.Length;
            int[] idxArr = ArrayPool<int>.Shared.Rent(len);
            for(int i=0; i < len; i++) {
                idxArr[i] = i;
            }

            // Sort the array of indexes based on the connections that each index points to.
            var comparer = new ConnectionComparer(in connIdArrays);
            Array.Sort(idxArr, comparer);
            
            int[] idArr = ArrayPool<int>.Shared.Rent(len << 1);
            Span<int> srcIdArr2 = idArr.AsSpan(0, len);
            Span<int> tgtIdArr2 = idArr.AsSpan(len, len);
            S[] weightArr2 = ArrayPool<S>.Shared.Rent(len);

            for(int i=0; i < len; i++)
            {
                int j = idxArr[i];
                srcIdArr2[i] = srcIds[j];
                tgtIdArr2[i] = tgtIds[j];
                weightArr2[i] = weightArr[j];
            }

            srcIdArr2.CopyTo(srcIds);
            tgtIdArr2.CopyTo(tgtIds);
            Array.Copy(weightArr2, weightArr, len);

            // Return the arrays rented from the array pool.
            ArrayPool<int>.Shared.Return(idxArr);
            ArrayPool<int>.Shared.Return(idArr);
            ArrayPool<S>.Shared.Return(weightArr2);
        }

        #endregion

        #region Private Static Methods

        private sealed class ConnectionComparer : IComparer<int>
        {
            readonly ConnectionIdArrays _connIdArrays;

            public ConnectionComparer(in ConnectionIdArrays connIdArrays)
            {
                _connIdArrays = connIdArrays;
            }

            public int Compare(int x, int y)
            {
                // Compare source IDs.
                int xval = _connIdArrays.GetSourceId(x);
                int yval = _connIdArrays.GetSourceId(y);

                if(xval < yval) {
                    return -1;
                }
                else if(xval > yval) {
                    return 1;
                }

                // Source IDs are equal; compare target IDs.
                xval = _connIdArrays.GetTargetId(x);
                yval = _connIdArrays.GetTargetId(y);

                if(xval < yval) {
                    return -1;
                }
                else if(xval > yval) {
                    return 1;
                }

                return 0;
            }
        }

        #endregion
    }
}
