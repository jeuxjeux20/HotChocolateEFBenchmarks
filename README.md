# HotChocolate EF Benchmarks

This is a dumb simple attempt at benchmarking 
different options for using a `DbContext` within HotChocolate resolvers,
using PostgreSQL.

## Getting started
You will need the .NET 6 SDK to run this benchmark, and you also need to
configure your [user secrets](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows)
for connecting to a database.

To configure your connection string, run this command inside the 
project directory:
```shell
dotnet user-secrets set ConnectionString "Host=...;Username=...;Password=...;Database=..."
```

Once that's done, you can directly run the benchmark: the database creation
and  migrations will be handled automatically.

## Last results

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.25140
AMD Ryzen 7 1700, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.300-preview.22204.3
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT


|            Method | DbContextKind | limit |      Mean |     Error |    StdDev |    Median |     Gen 0 | Allocated |
|------------------ |-------------- |------ |----------:|----------:|----------:|----------:|----------:|----------:|
|    ExtensiveQuery |      Resolver |     1 |  2.850 ms | 0.0801 ms | 0.2286 ms |  2.809 ms |         - |     71 KB |
|    ExtensiveQuery |        Pooled |     1 |  3.156 ms | 0.1113 ms | 0.3140 ms |  3.137 ms |         - |    115 KB |
| IntermediateQuery |      Resolver |     1 |  2.250 ms | 0.0735 ms | 0.2133 ms |  2.259 ms |         - |     51 KB |
| IntermediateQuery |        Pooled |     1 |  2.692 ms | 0.0761 ms | 0.2196 ms |  2.657 ms |         - |     96 KB |
|        SmallQuery |      Resolver |     1 |  1.227 ms | 0.0395 ms | 0.1141 ms |  1.191 ms |         - |     23 KB |
|        SmallQuery |        Pooled |     1 |  1.221 ms | 0.0327 ms | 0.0933 ms |  1.210 ms |         - |     23 KB |
|                   |               |       |           |           |           |           |           |           |
|    ExtensiveQuery |      Resolver |    10 |  3.891 ms | 0.1155 ms | 0.3334 ms |  3.872 ms |         - |    146 KB |
|    ExtensiveQuery |        Pooled |    10 |  4.317 ms | 0.0987 ms | 0.2833 ms |  4.256 ms |         - |    190 KB |
| IntermediateQuery |      Resolver |    10 |  2.848 ms | 0.0805 ms | 0.2296 ms |  2.830 ms |         - |     80 KB |
| IntermediateQuery |        Pooled |    10 |  3.277 ms | 0.0852 ms | 0.2429 ms |  3.252 ms |         - |    125 KB |
|        SmallQuery |      Resolver |    10 |  1.350 ms | 0.0393 ms | 0.1127 ms |  1.329 ms |         - |     32 KB |
|        SmallQuery |        Pooled |    10 |  1.286 ms | 0.0358 ms | 0.1022 ms |  1.285 ms |         - |     32 KB |
|                   |               |       |           |           |           |           |           |           |
|    ExtensiveQuery |      Resolver |    50 |  6.287 ms | 0.2459 ms | 0.7096 ms |  6.191 ms |         - |    500 KB |
|    ExtensiveQuery |        Pooled |    50 |  6.881 ms | 0.2644 ms | 0.7713 ms |  6.731 ms |         - |    535 KB |
| IntermediateQuery |      Resolver |    50 |  3.425 ms | 0.0931 ms | 0.2641 ms |  3.394 ms |         - |    223 KB |
| IntermediateQuery |        Pooled |    50 |  4.424 ms | 0.1641 ms | 0.4657 ms |  4.344 ms |         - |    262 KB |
|        SmallQuery |      Resolver |    50 |  1.368 ms | 0.0513 ms | 0.1480 ms |  1.348 ms |         - |     73 KB |
|        SmallQuery |        Pooled |    50 |  1.591 ms | 0.0515 ms | 0.1495 ms |  1.563 ms |         - |     71 KB |
|                   |               |       |           |           |           |           |           |           |
|    ExtensiveQuery |      Resolver |   500 | 26.353 ms | 1.4590 ms | 4.2560 ms | 25.273 ms | 1000.0000 |  4,438 KB |
|    ExtensiveQuery |        Pooled |   500 | 28.407 ms | 1.5261 ms | 4.4757 ms | 28.353 ms |         - |  4,487 KB |
| IntermediateQuery |      Resolver |   500 | 10.599 ms | 0.4353 ms | 1.2835 ms | 10.171 ms |         - |  1,814 KB |
| IntermediateQuery |        Pooled |   500 | 14.019 ms | 0.9494 ms | 2.7845 ms | 13.080 ms |         - |  1,861 KB |
|        SmallQuery |      Resolver |   500 |  3.049 ms | 0.1837 ms | 0.5415 ms |  2.781 ms |         - |    520 KB |
|        SmallQuery |        Pooled |   500 |  2.761 ms | 0.1671 ms | 0.4874 ms |  2.512 ms |         - |    520 KB |
|                   |               |       |           |           |           |           |           |           |
|    ApplyInflation |      Resolver |     ? |  1.172 ms | 0.0356 ms | 0.1038 ms |  1.160 ms |         - |     19 KB |
|    ApplyInflation |        Pooled |     ? |  1.315 ms | 0.0417 ms | 0.1210 ms |  1.288 ms |         - |     18 KB |
```