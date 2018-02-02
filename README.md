# C# port of TLSH (Trend Locality Sensitive Hash)

TLSH is a fuzzy matching library designed by [Trend Micro](http://www.trendmicro.com) (Hosted in [GitHub](https://github.com/trendmicro/tlsh))

Given a byte stream with a minimum length of 256 characters (and a minimum amount of randomness), TLSH generates a hash value which can be used for similarity comparisons. Similar objects will have similar hash values which allows for the detection of similar objects by comparing their hash values. Note that the byte stream should have a sufficient amount of complexity. For example, a byte stream of identical bytes will not generate a hash value. The force option can be used to create hashes from streams as short as 50 characters.

The computed hash is 35 bytes long (output as 70 hexidecimal charactes). The first 3 bytes are used to capture the information about the file as a whole (length, ...), while the last 32 bytes are used to capture information about incremental parts of the file.

You can also pass in a larger bucket size of 256 which produces a 134 character hash. The longer version is more accurate.


## Installation

From the package manager console:

	PM> Install-Package TlshSharp

or by simply searching for `TlshSharp` in the package manager UI.

## How it's used

With TLSH mainly you can calculate a hash using supported Strings and compute the difference between two resultant hashes.

### How-To calculate a Hash

To compute a Hash using TLSH, you should do the following:

```csharp
// Quote extracted from 'The UNIX-HATERS Handbook'
string data = "The best documentation is the UNIX source. After all, this is what the "
				+ "system uses for documentation when it decides what to do next! The "
				+ "manuals paraphrase the source code, often having been written at "
				+ "different times and by different people than who wrote the code. "
				+ "Think of them as guidelines. Sometimes they are more like wishes... "
				+ "Nonetheless, it is all too common to turn to the source and find "
				+ "options and behaviors that are not documented in the manual. Sometimes "
				+ "you find options described in the manual that are unimplemented "
				+ "and ignored by the source.";

var tlshBuilder = new TlshBuilder();

// Load in data.
tlshBuilder.LoadFromString(data);

// Or load it in async
await tlshBuilder.LoadFromStringAsync(data);

// Builds TlshHash instance which can generate encoded output.
var tlshHash = tlshBuilder.GetHash(false);

// Get encoded output in hexidecimal string format.
var encodedHash = tlshHash.GetEncoded();

Assert.AreEqual("6FF02BEF718027B0160B4391212923ED7F1A463D563B1549B86CF62973B197AD2731F8", encodedHash);
```

### Requirements

The input data must contain:

* At least 256 characters (or 50 characters if using TlshBuilder.GetHash(true)).
* A certain amount of randomness.

to generate a hash value. In other case an **InvalidOperationException** will be thrown.

### How-To compute difference between two hashes

1. You should to create two hashes using the static TlshHash.FromTlshStr(string tlshStr) method with hashes as inputs:

```csharp
var tlshHash1 = TlshHash.FromTlshStr("301124198C869A5A4F0F9380A9AE92F2B9278F42089EA34272885F0FB2D34E6911444C");
var tlshHash2 = TlshHash.FromTlshStr("09F05A198CC69A5A4F0F9380A9EE93F2B927CF42089EA74276DC5F0BB2D34E68114448");
```

2. You can compute the difference using one TlshHash against the other one

```csharp
var diff = tlshHash1.TotalDiff(tlshHash2, true);
Assert.AreEqual(121, diff);
```

#### How to measure the difference?

* A difference of 0 means the objects are almost identical.
* A difference of 200 or higher means the objects are **very** different.

#### Ignoring the input data length

The difference should be calculated using the file length component or removing it (giving _false_ as second parameter). If an input with a repeating pattern is compared to an input with only a single instance of the pattern, then the difference will be increased if the length is included. Giving a false value to the second parameter, the input data length will be removed from consideration.

## Requirements

The library has been built with .Net standard 1.1

## Design choices

I have adopted the original Trend Locality Sensitive Hashing design choices to build this C# port.

## TODO

* Cleanup code.
* More compact way to generate encoded hash.

## License

Read [LICENSE.txt](LICENSE.txt) attached to the project⏎
