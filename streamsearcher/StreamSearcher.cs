// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
// 
// Copyright (c) 2016, Dr. Masroor Ehsan
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// * Redistributions of source code must retain the above copyright
//   notice, this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright
//   notice, this list of conditions and the following disclaimer in the
//   documentation and/or other materials provided with the distribution.
// * Neither the name of the <organization> nor the
//   names of its contributors may be used to endorse or promote products
//   derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 
// $Id$
// 
// Last modified: 2016.01.03 1:39 PM
// 
// * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

using System;
using System.IO;

namespace streamsearcher
{
    public sealed class StreamSearcher
    {
        // An upper bound on pattern length for searching. Results are undefined for longer patterns.
        public const int MAX_PATTERN_LENGTH = 1024;
        private readonly int[] _borders;
        private readonly byte[] _pattern;

        public StreamSearcher(byte[] pattern)
        {
            _pattern = new byte[pattern.Length];
            Array.Copy(pattern, _pattern, pattern.Length);
            _borders = new int[_pattern.Length + 1];
            preProcess();
        }

        /**
         * Builds up a table of longest "borders" for each prefix of the pattern to find. This table is stored internally
         * and aids in implementation of the Knuth-Moore-Pratt string search.
         * 
         * For more information, see: http://www.inf.fh-flensburg.de/lang/algorithmen/pattern/kmpen.htm.
         */

        private void preProcess()
        {
            var i = 0;
            var j = -1;
            _borders[i] = j;

            while (i < _pattern.Length)
            {
                while (j >= 0 && _pattern[i] != _pattern[j])
                {
                    j = _borders[j];
                }
                _borders[++i] = ++j;
            }
        }

        /**
         * Searches for the next occurrence of the pattern in the stream, starting from the current stream position. Note
         * that the position of the stream is changed. If a match is found, the stream points to the end of the match -- i.e. the
         * byte AFTER the pattern. Else, the stream is entirely consumed. The latter is because InputStream semantics make it difficult to have
         * another reasonable default, i.e. leave the stream unchanged.
         *
         * @return bytes consumed if found, -1 otherwise.
         * @throws IOException
         */

        public long Search(Stream stream)
        {
            long bytesRead = 0;

            int b;
            var j = 0;

            while ((b = stream.ReadByte()) != -1)
            {
                bytesRead++;

                while (j >= 0 && (byte) b != _pattern[j])
                {
                    j = _borders[j];
                }
                // Move to the next character in the pattern.
                ++j;

                // If we've matched up to the full pattern length, we found it.  Return,
                // which will automatically save our position in the InputStream at the point immediately
                // following the pattern match.
                if (j == _pattern.Length)
                {
                    return bytesRead;
                }
            }

            // No dice, Note that the stream is now completely consumed.
            return -1;
        }
    }
}