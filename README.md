# VitaQ
A high-performance, general-purpose object pool for .NET with cleanup policies and metrics support.
## Table of Contents
- [✨ Features](#-features)
- [📦 Installation](#-installation)
- [🚀 Quick Start](#-quick-start)
- [⚙️ API Levels: Safe & Unsafe](#️-api-levels-safe--unsafe)
- [🔗 Dependency Injection](#-dependency-injection)
- [🔧 Configuration](#-configuration)
- [📊 Metrics](#-metrics)
- [⚠️ Common Pitfalls](#️-common-pitfalls)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

## ✨ Features
    🚀 High Performance: Minimal overhead via ConcurrentQueue<T> and Interlocked operations.
    🧹 Automatic Cleanup: Objects are reset (Clear(), Length = 0) before returning to the pool.
    📊 Built-in Metrics: Track Hits, Misses, and ActiveCount to monitor pool efficiency.
    ⚙️ Flexible Design: Use specialized pools (StringBuilderPool, ListPool<T>) or build your own with ObjectPool<T>.
    🔥 Warmup Support: PreWarm(count) to pre-allocate objects and avoid cold-start misses.
    🔒 Safe by Default: Get() + using pattern guarantees return, even on exceptions.
    🧵 Thread-Safe: All operations are concurrency-safe out of the box.

## 📦 Installation
    Install the package via the .NET CLI:
    ```bash
    dotnet add package VitaQ.ObjectPool
    Or via .NET CLI:
    dotnet add package VitaQ.ObjectPool

## 🚀 Quick Start

    StringBuilderPool
        using VitaQ;
        var pool = new StringBuilderPool();
        // ✅ Safe API: automatic return via `using`
        using var sb = pool.Get();
        sb.Value.Append("Hello, ");
        sb.Value.AppendLine("World!");
        Console.WriteLine(sb.Value.ToString());
        // ← sb is automatically returned to the pool here

    ListPool<T>
        using VitaQ;
        var pool = new ListPool<int>();
        using var list = pool.Get();
        list.Value.Add(1);
        list.Value.Add(2);
        list.Value.Add(3);
        // Use the list...
        // ← automatically returned on dispose

## ⚙️ API Levels: Safe & Unsafe

Safe API (Recommended)
    Use Get() + using for guaranteed, exception-safe return:
    
    using var sb = pool.Get();
    sb.Value.Append("data");
    // ← auto-return on scope exit
    Benefits:
    No risk of forgetting to return the object
    Works with try/finally under the hood
    Detected by static analyzers (CA2000)
Unsafe API (Advanced)
    Manual Borrow()/Return() — use only if you've measured allocations and confirmed the need:
    
    var sb = pool.Borrow();
    try 
    {
        sb.Append("data");
    }
    finally 
    {
        pool.Return(sb); //  MUST call, or memory leak!
    }
    Warning: Forgetting Return() causes pool exhaustion and memory leaks. Always prefer the Safe API.
## 🔗 Dependency Injection
VitaQ integrates seamlessly with Microsoft.Extensions.DependencyInjection.


Add to Program.cs (or Startup.cs):

    builder.Services.AddStringBuilderPool();
    builder.Services.AddListPool<int>(); // specify your <T> type

Inject the ISafePool<T> interface, never the concrete class:

        public class MyService
        {
            private readonly ISafePool<StringBuilder> _pool;
            public MyService(ISafePool<StringBuilder> pool) => _pool = pool;

            public void Process()
            {
                using var sb = _pool.Get();
                sb.Value.Append("Data");
                // ← auto-returned to pool on dispose
            }
        }
Pools are registered as Singleton (single instance per application lifetime).
Always depend on ISafePool<T>, not the implementation.
Use Get() + using → guarantees safe return even on exceptions.

## 🔧 Configuration
    MaxCapacity / MaxCount--5_000_000 / 100_000--Max size for an object to be accepted back into the pool
    DefaultCapacity--256--Initial capacity for newly created objects
    MaxPoolSize--100--Maximum items stored in the pool (excess are discarded)

## 📊 Metrics
    Hits--Objects taken from the pool--High = good reuse, pool is effective
    Misses--New objects created (pool was empty)--High = increase PreWarm() or MaxPoolSize
    ActiveCount--Objects currently borrowed (not returned)--Should be ~0 at idle; sustained >0 may indicate leaks

## ⚠️ Common Pitfalls
    ❌ Don't save .Value outside using
   
    StringBuilder leaked;
    using (var sb = pool.Get())
    {
        leaked = sb.Value; // ⚠️ Object returns to pool after this block!
    }
    leaked.Append("data"); // ❌ Race condition / data corruption!

    ❌ Don't return external objects
    
    var mySb = new StringBuilder();
    pool.Return(mySb); // ⚠️ Breaks pool contract — avoid

    ❌Don't use after return
    
    using var sb = pool.Get();
    // ... use sb.Value ...
    // ← auto-return happens here

    sb.Value.Append("more"); // ObjectDisposedException or race condition

## 🤝 Contributing
Contributions are welcome! To get started:
    Fork the repository
    Create a feature branch (git checkout -b feature/amazing-feature)
    Add tests for new functionality
    Follow the existing code style and XML documentation conventions
    Submit a pull request with a clear description
    Development tips:
    Run dotnet test before pushing
    Add benchmarks to BenchmarkDotNet for performance changes
    Update this README if you change public APIs

## 📄 License
    Distributed under the MIT License. See LICENSE for more information.
    MIT License 

    Copyright (c) 2026 Georgie

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
