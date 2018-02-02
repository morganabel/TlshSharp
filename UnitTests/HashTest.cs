/*
 * Ported from: https://github.com/trendmicro/tlsh
 */

/*
 * TLSH is provided for use under two licenses: Apache OR BSD.
 * Users may opt to use either license depending on the license
 * restictions of the systems with which they plan to integrate
 * the TLSH code.
 */

/* ==============
 * Apache License
 * ==============
 * Copyright 2017 Trend Micro Incorporated
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

/* ===========
 * BSD License
 * ===========
 * Copyright (c) 2017, Trend Micro Incorporated
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 3. Neither the name of the copyright holder nor the names of its contributors
 *    may be used to endorse or promote products derived from this software without
 *    specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
 * OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Palit.TLSHSharp;

namespace UnitTests
{
    [TestClass]
    public class HashTest
    {
        private const string data = "The best documentation is the UNIX source. After all, this is what the "
                + "system uses for documentation when it decides what to do next! The "
                + "manuals paraphrase the source code, often having been written at "
                + "different times and by different people than who wrote the code. "
                + "Think of them as guidelines. Sometimes they are more like wishes... "
                + "Nonetheless, it is all too common to turn to the source and find "
                + "options and behaviors that are not documented in the manual. Sometimes "
                + "you find options described in the manual that are unimplemented "
                + "and ignored by the source.";

        private const string dataSlightlyChanged = "The best documentation is the Windows source. After all, this is what the "
                + "system uses for documentation when it decides what to do next! The "
                + "manuals paraphrase the source code, often having been written at "
                + "different times and by different people than who wrote the code. "
                + "Think of them as guidelines. Sometimes they are more like wishes... "
                + "Nonetheless, it is all too common to turn to the sources and find "
                + "options and behaviors that are not documented in the manuals. Sometimes "
                + "you find options described in the manual that are unimplemented "
                + "and ignored by the sources.";

        [TestMethod]
        public void SyncHashEncodingTest()
        {
            var tlshBuilder = new TlshBuilder();
            tlshBuilder.LoadFromString(data);

            var tlshHash = tlshBuilder.GetHash(false);
            var encodedHash = tlshHash.GetEncoded();

            Assert.AreEqual("6FF02BEF718027B0160B4391212923ED7F1A463D563B1549B86CF62973B197AD2731F8", encodedHash);
        }

        [TestMethod]
        public async Task HashEncodingTest()
        {
            var tlsh = new TlshBuilder(BucketSize.Buckets128, ChecksumSize.Checksum1Byte);
            await tlsh.LoadFromStringAsync(data);

            var hash = tlsh.GetHash(false);
            Assert.AreEqual("6FF02BEF718027B0160B4391212923ED7F1A463D563B1549B86CF62973B197AD2731F8", hash.GetEncoded());
        }

        [TestMethod]
        public async Task ForceHashEncodingTest()
        {
            var shortData = "a quick brown fox jumps over the lazy dog and now this is long enough.";

            var tlsh = new TlshBuilder(BucketSize.Buckets128, ChecksumSize.Checksum1Byte);
            await tlsh.LoadFromStringAsync(shortData);

            var hash = tlsh.GetHash(true);
            var encodedHash = hash.GetEncoded();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task EncodeToStringAndBackTest()
        {
            var tlsh = new TlshBuilder(BucketSize.Buckets128, ChecksumSize.Checksum1Byte);
            await tlsh.LoadFromStringAsync(data);

            var hash = tlsh.GetHash(false);
            var encoded = hash.GetEncoded();

            var hashFromEncoding = TlshHash.FromTlshStr(encoded);

            var match = hash.TotalDiff(hashFromEncoding, true);
            Assert.AreEqual(0, match);
        }

        [TestMethod]
        public async Task CompareHashTest()
        {
            var tlsh = new TlshBuilder(BucketSize.Buckets256, ChecksumSize.Checksum3Bytes);

            await tlsh.LoadFromStringAsync(data);
            var hash1 = tlsh.GetHash(false);

            tlsh.Reset();

            await tlsh.LoadFromStringAsync(dataSlightlyChanged);
            var hash2 = tlsh.GetHash(false);

            var match = hash1.TotalDiff(hash2, true);

            Assert.IsTrue(match < 30);
        }

        [TestMethod]
        public async Task DecodeAndCompareTest()
        {
            var tlshHash1 = TlshHash.FromTlshStr("301124198C869A5A4F0F9380A9AE92F2B9278F42089EA34272885F0FB2D34E6911444C");
            var tlshHash2 = TlshHash.FromTlshStr("09F05A198CC69A5A4F0F9380A9EE93F2B927CF42089EA74276DC5F0BB2D34E68114448");

            var diff = tlshHash1.TotalDiff(tlshHash2, true);
            Assert.AreEqual(121, diff);
        }

        [TestMethod]
        public async Task CompareAccuracyTest()
        {
            var highAccuracyTlsh = new TlshBuilder(BucketSize.Buckets256, ChecksumSize.Checksum3Bytes);
            var lowAccuracyTlsh = new TlshBuilder(BucketSize.Buckets128, ChecksumSize.Checksum1Byte);

            await highAccuracyTlsh.LoadFromStringAsync(data);
            var highAccuracyHash1 = highAccuracyTlsh.GetHash(false);
            await lowAccuracyTlsh.LoadFromStringAsync(data);
            var lowAccuracyHash1 = lowAccuracyTlsh.GetHash(false);

            highAccuracyTlsh.Reset();
            lowAccuracyTlsh.Reset();

            await highAccuracyTlsh.LoadFromStringAsync(dataSlightlyChanged);
            var highAccuracyHash2 = highAccuracyTlsh.GetHash(false);
            await lowAccuracyTlsh.LoadFromStringAsync(dataSlightlyChanged);
            var lowAccuracyHash2 = lowAccuracyTlsh.GetHash(false);

            var highAccuracyMatch = highAccuracyHash1.TotalDiff(highAccuracyHash2, true);
            var lowAccuracyMatch = lowAccuracyHash1.TotalDiff(lowAccuracyHash2, true);

            Assert.IsTrue(highAccuracyMatch < 30);
            Assert.IsTrue(lowAccuracyMatch < 30);

            Assert.IsTrue(lowAccuracyMatch < highAccuracyMatch);
        }

        [TestMethod]
        public async Task CompareNotSimilarTest()
        {
            var dataVeryChanged = "The worst docs are the Windows source. After all, this is not what the "
                + "system uses for docs when it decides what to do next! The "
                + "manuals paraphrase the source code, often having been written at "
                + "many times and by the same people than who wrote the code. "
                + "Think of them as nothing like guidelines. Always are more like wishes... "
                + "Therefore, it is all too common to ignore sources and find "
                + "options and weird behaviors that are not put in the docs. Sometimes "
                + "you find options not described in the docs that are incomplete "
                + "and weird by the source.";

            var tlsh = new TlshBuilder(BucketSize.Buckets256, ChecksumSize.Checksum3Bytes);

            await tlsh.LoadFromStringAsync(data);
            var hash1 = tlsh.GetHash(false);

            tlsh.Reset();

            await tlsh.LoadFromStringAsync(dataVeryChanged);
            var hash2 = tlsh.GetHash(false);

            var match = hash1.TotalDiff(hash2, true);

            Assert.IsTrue(match > 200);
        }
    }
}
