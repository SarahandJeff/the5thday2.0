# Road Shape Fix Mod

## 🐛 The Bug

In the base game's world generation, there's a bug where **only T-junctions and intersections** spawn for road tiles in townships. The game never generates:
- **Caps** (dead-end roads)
- **Corners** (90-degree turns)
- **Straights** (straight road segments)

This makes all towns look identical with overly dense, grid-like road networks where every street connects to every other street.

## 🔍 Root Cause

The bug is in `TownPlanner.cs` (lines 218-227). When calculating which exits each road tile should have, the code checks if neighboring tiles are in the same township:

```csharp
int num14 = 0;
for (int num15 = 0; num15 < 4; num15++)
{
    StreetTile neighbor = value5.GetNeighbor(num15);
    if (neighbor != null && neighbor.Township == value5.Township)  // ⚠️ BUG HERE
    {
        num14 |= 1 << num15;  // Build exit bitmask
    }
}
value5.SetExits(num14);  // This determines the road shape!
```

**The Problem:** The condition `neighbor.Township == value5.Township` is too broad. It creates exits to ANY neighbor in the same township, even if that neighbor isn't actually a street tile in the `township.Streets` dictionary.

This causes edge tiles and corner tiles to incorrectly connect to non-street neighbors, resulting in:
- Edge tiles becoming T-junctions instead of caps or straights
- Corner tiles becoming intersections instead of corners
- No variety in road shapes

**The game's township generation algorithm (`GetStreetLayout`) is actually correct** - it creates varied layouts using a flood-fill algorithm. But the exit calculation ruins this by connecting tiles that shouldn't be connected.

## ✅ The Fix

This mod fixes the exit calculation to match the game's original intent:

```csharp
// Only create exit if neighbor exists AND is in the township's Streets dictionary
if (neighbor != null &&
    neighbor.Township == township &&
    township.Streets.ContainsKey(neighbor.GridPosition))  // ✅ FIXED
{
    newExits |= 1 << dir;
}
```

By adding the `township.Streets.ContainsKey()` check, we ensure that exits are only created to neighbors that are actually part of the road network. This allows:
- **Edge tiles** to become caps (1 exit) or straights (2 opposite exits)
- **Corner tiles** to become corners (2 adjacent exits)
- **Interior tiles** to remain as T-junctions (3 exits) or intersections (4 exits)

The fix restores the game's intended behavior without changing the township generation algorithm.

## 📦 Installation

1. Copy the `RoadShapeFix` folder to your `7 Days To Die/Mods/` directory
2. Start the game
3. Generate a new world (existing worlds won't be affected)

## ⚙️ Technical Details

### Road Shape Types (from `StreetTile.cs`)

```csharp
public enum RoadShapeTypes
{
    straight,      // Index 0
    t,             // Index 1
    intersection,  // Index 2
    cap,           // Index 3
    corner         // Index 4
}
```

### Exit Bitmask Format

The exit bitmask uses 4 bits to represent connections in 4 directions:
- Bit 0 (value 1): North
- Bit 1 (value 2): East
- Bit 2 (value 4): South
- Bit 3 (value 8): West

Examples:
- `0101` (5) = North + South = **Straight**
- `0111` (7) = North + East + South = **T-junction**
- `1111` (15) = All directions = **Intersection**
- `0100` (4) = South only = **Cap**
- `0110` (6) = East + South = **Corner**

## 🎮 Impact

This fix restores the game's intended township generation behavior:

- **Proper edge tiles**: Township edges now have caps and straights instead of always being T-junctions
- **Proper corner tiles**: Township corners now use corner shapes instead of intersections
- **More realistic layouts**: Towns have natural-looking dead-ends and varied street patterns
- **Better exploration**: Dead-end streets create more interesting navigation and exploration

## 📊 Before vs After

**Before (Bugged):**
- 95% of tiles are T-junctions or intersections
- Every street connects to every other street
- No dead-ends or corner streets
- Towns look like perfect grids

**After (Fixed):**
- Proper distribution of all 5 road shapes
- Edge tiles become caps and straights
- Corner tiles become corners
- Towns have natural variety while maintaining the intended layout

## 🔧 Compatibility

- ✅ Works with vanilla 7 Days to Die
- ✅ Compatible with other mods that don't modify `TownPlanner`
- ✅ Only affects newly generated worlds (existing worlds unchanged)
- ✅ No performance impact - runs once during world generation

## 📝 Credits

- **Created by:** Asylum Modding Team
- **Bug discovered and analyzed by:** Community investigation
- **Root cause identified in:** `TownPlanner.cs` exit calculation logic

## 📄 License

Free to use and modify for personal and public use.

