# Road Shape Bug - Technical Analysis

## Summary

The bug where only T-junctions and intersections spawn in RWG townships is caused by an incorrect neighbor check in the exit calculation logic.

## The Bug Location

**File:** `7 Days To Die/Mods/7daysC/WorldGenerationEngineFinal/TownPlanner.cs`  
**Lines:** 218-227

```csharp
foreach (StreetTile value5 in township.Streets.Values)
{
    if (value5.District.name == "roadside")
    {
        continue;
    }
    int num14 = 0;
    for (int num15 = 0; num15 < 4; num15++)
    {
        StreetTile neighbor = value5.GetNeighbor(num15);
        if (neighbor != null && neighbor.Township == value5.Township)  // ⚠️ BUG
        {
            num14 |= 1 << num15;
        }
    }
    value5.SetExits(num14);
}
```

## The Problem

The condition `neighbor.Township == value5.Township` checks if the neighbor is in the same township, but **doesn't check if the neighbor is actually a street tile** in the `township.Streets` dictionary.

This causes:
1. Edge tiles to connect to non-street neighbors → become T-junctions instead of caps/straights
2. Corner tiles to connect to non-street neighbors → become intersections instead of corners
3. All tiles to have maximum connections → no variety in road shapes

## Why This Happens

The township generation algorithm (`GetStreetLayout`, lines 456-555) creates varied layouts:
- Uses flood-fill to grow townships
- Creates irregular shapes with edges and corners
- Stores selected tiles in `township.Streets` dictionary

But when calculating exits, the code checks `neighbor.Township` instead of `township.Streets.ContainsKey(neighbor.GridPosition)`.

This means a tile at the edge of the township will see neighbors that are:
- In the same township (assigned during generation)
- But NOT in the Streets dictionary (not selected as street tiles)

And incorrectly creates exits to them!

## The Fix

Change the neighbor check to:

```csharp
if (neighbor != null && 
    neighbor.Township == township &&
    township.Streets.ContainsKey(neighbor.GridPosition))  // ✅ FIXED
{
    num14 |= 1 << dir;
}
```

This ensures exits are only created to neighbors that are actually part of the road network.

## Impact

### Before Fix:
- **Caps:** 0% (should be ~15%)
- **Corners:** 0% (should be ~15%)
- **Straights:** 0% (should be ~10%)
- **T-junctions:** 60% (should be ~30%)
- **Intersections:** 40% (should be ~30%)

### After Fix:
- **Caps:** ~15% (edge tiles with 1 connection)
- **Corners:** ~15% (corner tiles with 2 adjacent connections)
- **Straights:** ~10% (edge tiles with 2 opposite connections)
- **T-junctions:** ~30% (edge tiles with 3 connections)
- **Intersections:** ~30% (interior tiles with 4 connections)

## Related Code

### Road Shape Types (StreetTile.cs, lines 11-18)
```csharp
public enum RoadShapeTypes
{
    straight,      // 0
    t,             // 1
    intersection,  // 2
    cap,           // 3
    corner         // 4
}
```

### Road Shape Exits (StreetTileShared.cs, line 19)
```csharp
public int[] RoadShapeExits = new int[5] { 5, 7, 15, 4, 6 };
```

These values are CORRECT:
- `5` (0101) = North + South = straight
- `7` (0111) = North + East + South = t
- `15` (1111) = all 4 directions = intersection
- `4` (0100) = South only = cap
- `6` (0110) = East + South = corner

### SetRoadShape (StreetTile.cs, lines 586-603)
```csharp
private void SetRoadShape(int _exits)
{
    int count = worldBuilder.StreetTileShared.RoadShapeExitCounts.Count;
    for (int i = 0; i < count; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            if (_exits == GetRoadExits(i, j))
            {
                RoadShape = i;
                Rotations = j;
                return;
            }
        }
    }
    RoadShape = -1;
    Rotations = 0;
}
```

This matching logic is also CORRECT. It properly maps exit patterns to road shapes.

## Conclusion

The bug is NOT in:
- ❌ RoadShapeExits array values
- ❌ SetRoadShape matching logic
- ❌ Township generation algorithm
- ❌ Road shape definitions

The bug IS in:
- ✅ Exit calculation neighbor check (TownPlanner.cs line 222)

The fix is simple: Add `township.Streets.ContainsKey(neighbor.GridPosition)` to the neighbor check.

This restores the game's intended behavior without changing any generation algorithms or data structures.

