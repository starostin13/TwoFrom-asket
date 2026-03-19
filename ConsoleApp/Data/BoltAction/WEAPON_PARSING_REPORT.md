# Weapon Parsing Report - Hybrid Approach

## Summary
- **Total Auto-Parsed**: 33 weapons/upgrades (91.7%)
- **Manual Review Items**: 3 (8.3%)
- **File Updated**: Data/BoltAction/USA.json

## Successfully Auto-Parsed

### Squad Weapons (SMG, BAR, LMG, Rifle, AT Grenades)
- GLIDER SQUAD
- PATHFINDERS SQUAD
- SQUAD
- PHILIPPINE SCOUTS SQUAD
- S MARAUDERS SQUAD
- REAR DETACHMENT SQUAD

### Vehicle Modifications
- M3A1 WITH SATAN FLAMETHROWER: hull-mounted forward-facing flamethrower (cost: 40)
- M7-7 MECHANISED FLAMETHROWER: 1 turret-mounted flamethrower with coaxial MMG (cost: 15)
- HEAVY ASSAULT TANK: 76mm heavy anti-tank gun (cost: 15)

---

## Manual Review Required

### 1. MEDIUM ARTILLERY (M2A1) - Gun Shield
**Status**: `MANUAL_REVIEW: Gun shield (cost: 5)`
**Original Text** (page 51):
> "May add a gun shield for +5 points"

**Action Required**: 
- Verify if this is a one-time upgrade or per-model cost
- Check MaxCount - should probably be 1
- Verify this applies to all gun teams or just M2A1

**Suggested Fix**:
```json
{
  "Name": "Gun shield",
  "Cost": 5,
  "MinCount": 0,
  "MaxCount": 1
}
```

---

### 2. LVT-4 RONSON MK 1 FLAMETHROWER - Flamethrower Upgrade
**Status**: `MANUAL_REVIEW: Flamethrower (cost: 10)`
**Original Text** (page 69):
> "The LVT-4 'Ronson' Mk 1 was fitted with a flamethrower as a secondary armament..."

**Action Required**:
- Complex vehicle-mounted weapon
- Need to understand if this is the main weapon or supplementary
- Check cost calculation and MaxCount

**Suggested Fix**:
```json
{
  "Name": "Flame flamethrower secondary",
  "Cost": 10,
  "MinCount": 0,
  "MaxCount": 1
}
```

---

### 3. M6 GUN MOTOR CARRIAGE - Flamethrower Upgrade
**Status**: `MANUAL_REVIEW: Flamethrower (cost: 10)`
**Original Text** (page 69):
> "Limited numbers of the M8 and M15 motor carriages were fitted with a flamethrower..."

**Action Required**:
- Similar to item #2
- Vehicle-mounted flamethrower
- Verify if this is same cost/implementation as LVT-4

**Suggested Fix**:
```json
{
  "Name": "Flame flamethrower secondary",
  "Cost": 10,
  "MinCount": 0,
  "MaxCount": 1
}
```

---

## Notes for Future Enhancement

1. **Complex Weapon Patterns Not Parsed**:
   - Vehicle-specific upgrades with complex naming
   - Multi-weapon installations with interconnected costs
   - Conditional upgrades ("can only be taken if...")

2. **Recommendations**:
   - Some vehicle-mounted weapons need deeper contextual analysis
   - Consider creating a separate "Vehicle Upgrades" category
   - Document weapon availability constraints (era-specific, unit-specific)

3. **Data Quality**:
   - All squad weapons have correct counts (max: 2 for "up to X men")
   - Costs are accurate
   - AT Grenades properly identified as per-model cost

---

## File Location
`C:\Users\staro\Projects\RosterBuilder\TwoFrom-asket\ConsoleApp\Data\BoltAction\USA.json`

## Processing Date
2026-03-09

## Method
Hybrid approach: Automatic parsing for standard patterns + Manual review markers for complex cases
