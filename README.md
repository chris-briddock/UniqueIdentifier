| Operation | GUSID | Guid | Winner | Performance Difference |
|-----------|-------|------|--------|------------------------|
| **Parsing** | 53.54 ns | 28.16 ns | Guid | Guid is 1.9x faster |
| **Comparison** | 105.85 ns | 1,230.28 ns | GUSID | **GUSID is 11.6x faster** |
| **Serialization** | 188.51 ns | 643.80 ns | GUSID | **GUSID is 3.4x faster** |
| **Equality** | 48.96 ns | 603.43 ns | GUSID | **GUSID is 12.3x faster** |
| **Hashing** | 67.35 ns | 607.86 ns | GUSID | **GUSID is 9.0x faster** |
| **Generation** | 48.15 ns | 592.60 ns | GUSID | **GUSID is 12.3x faster** |
