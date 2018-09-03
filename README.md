# Fone Dynamics Memory Cache Challenge with O(1) time complexity
## O(1) lookup time ~ O(1) insertion time ~ O(1) deletion time

LRUCache thread-safe implementation in C#, with eviction policy.

### Documentation
XML documentation can be found in:
```
MemoryCache\MemoryCache\bin\Debug
```
### Unit Testing
Unit testing is available for that project using:
* [NUnit](https://github.com/nunit/nunit/releases/tag/v3.10.1)
* [NUnitAdapter](https://github.com/nunit/nunit3-vs-adapter/releases/tag/3.10)

### Implementation
Below is a short snippet for a simple usage case:

```c#
var cache = new Cache<int, string>(capacity: 100);
string valueInCache;

cache.AddOrUpdate(0, "Item 1");
cache.AddOrUpdate(1, "Item 2");
cache.AddOrUpdate(2, "Item 3");
cache.AddOrUpdate(3, "Item 4");
cache.AddOrUpdate(4, "Item 5");

cache.TryGetValue(0, out valueInCache)
console.writeLine(valueInCache);

cache.TryGetValue(3, out valueInCache)
console.writeLine(valueInCache);

Console.WriteLine(cache.Count);
Console.WriteLine(cache.IsFull);
Console.WriteLine(cache.Capacity);
```
