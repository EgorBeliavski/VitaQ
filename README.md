# VitaQ
A high-performance, general-purpose object pool for .NET with cleanup policies and metrics support.

## characteristics

- 🚀 **High Speed:** Minimal overhead thanks to the use of `ConcurrentQueue` and `Interlocked`.
- 🧹 **Automatic Cleanup:** Built-in mechanisms for resetting object state before returning it to the pool.
- 📊 **Metrics:** Tracking hits, misses, and active objects.
- ⚙️ **Flexibility:** General-purpose `ObjectPool<T>` class or ready-made specialized pools (`StringBuilderPool`, `ListPoolWay<T>`).
- 🔥 **Warmup:** `PreWarm` method for pre-creating objects.

## Installation

Install the package via the .NET CLI:

```bash
dotnet add package VitaQ.ObjectPool

Or via .NET CLI:
dotnet add package VitaQ.ObjectPool


** Quick Start
Using the Universal Pool

using VitaQ.Core;


// Create a policy for StringBuilder



// Take an object from the pool
var sb = pool.Borrow();
sb.Append("Hello, World!");

// Return it (the object is automatically cleared)
pool.Return(sb);



using VitaQ.Pools;

// StringBuilder
var sb = StringBuilderPool.Borrow();
sb.Append("Data");
StringBuilderPool.Return(sb);

// List<int>
var list = ListPoolWay<int>.Borrow();
list.Add(42);
ListPool<int>.Return(list);