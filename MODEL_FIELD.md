# Model Field Feature

## Overview

The `Model` field is an optional property on the `Unit` class that allows multiple units to share the same model limit. This is useful for scenarios where different unit configurations represent the same model type.

## How It Works

### Unit Definition

Units can optionally specify a `Model` field in the JSON data:

```json
{
  "Name": "Crisis Suit Team Alpha",
  "Model": "Crisis",
  "MinModels": 3,
  "MaxModels": 6,
  ...
}
```

### Limit Lookup

When checking limits:
1. If a unit has a `Model` field, that value is used to look up the limit in `ModelLimits.json`
2. If a unit does NOT have a `Model` field, the unit's `Name` is used for the lookup
3. If no matching entry exists in `ModelLimits.json`, the unit has no limit

### Example

Consider these units:
- "Crisis Suit Team Alpha" with `Model: "Crisis"`
- "Crisis Suit Team Beta" with `Model: "Crisis"`
- "Strike Squad" with no Model field

And this ModelLimits.json:
```json
[
  {
    "ModelName": "Crisis",
    "MaxQuantity": 12,
    "MinQuantity": 3
  }
]
```

Results:
- Both Crisis Suit teams **share** the 12 model limit
- If Team Alpha has 6 models, Team Beta can have at most 6 models
- Strike Squad has no limit (since "Strike Squad" is not in ModelLimits.json)

## ModelLimits.json Format

```json
[
  {
    "ModelName": "Crisis",          // The lookup key (can be a Model value or Unit Name)
    "MaxQuantity": 12,               // Maximum total models (null = unlimited)
    "MinQuantity": 3                 // Minimum models required (null = 0)
  }
]
```

## Implementation Details

- Limits are checked in `IsUnitLimitExceed()` which counts total models across all unit configurations with the same lookup key
- The limit is checked both when adding units and when adding leaded units
- `UnitConfiguration` validates that model count is within the unit's MinModels/MaxModels range
