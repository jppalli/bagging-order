# Teaching progression and generator foundation

## Product vocabulary

Dimensions are width x height x depth in packing cells. Weight and carried-load capacity are separate. Rotation preserves weight and strength; damage is reversible with Undo.

| Product | Size | Weight | Carries | Purpose |
|---|---|---:|---:|---|
| Soup can | 1 x 1 x 1 | 3 | Unlimited | Strong foundation |
| Rice box | 1 x 1 x 1 | 2 | 8 | Strong but finite support |
| Cereal | 1 x 2 x 1 | 1 | 5 | Tall, light orientation lesson |
| Family cornflakes | 2 x 3 x 1 | 1 | 3 | Large carton: three distinct dimensions reward turning and tipping |
| Milk | 1 x 2 x 1 | 3 | 7 | Tip a carton; cold in grouping lessons |
| Eggs | 2 x 1 x 1 | 1 | 1 | Wide fragile top layer |
| Bread | 2 x 1 x 1 | 1 | 2 | Wide, light support choice |
| Apples | 1 x 1 x 1 | 1 | 3 | Small gap filler |
| Bananas | 2 x 1 x 1 | 1 | 1 | Natural curved bunch, easy to bruise |
| Egg tray | 2 x 1 x 2 | 2 | 1 | Broad underside needs full support |
| Flowers | 1 x 3 x 1 | 1 | 0 | Upright and uncovered |
| Yogurt | 1 x 1 x 1 | 1 | 2 | Small cold-group connector |
| Frozen peas | 2 x 1 x 1 | 2 | 4 | Wide cold-group connector |

Bananas are four curved fingers attached to a shared crown, not an L-shaped assembly. Their collision footprint is a simple two-cell rectangle. Rounded produce still uses discrete packing cells; mesh-to-mesh physics is outside this foundation.

## Eight playable lessons

1. **A sturdy start:** cans, rice and cereal establish drag/drop, stacking and two rows with spare room.
2. **Delicate on top:** introduce bananas, eggs and bread. A can on bananas causes damage; sturdy-first placement is safe.
3. **A quarter turn:** reward turning a wide product; no tipping requirement.
4. **Lay it down:** reward tipping a milk carton; keep the order short.
5. **This way up:** introduce upright flowers without a second new objective.
6. **Room for another:** seven bouquets require two bags because a bag has only six floor cells and bouquets cannot stack. Familiar cans and rice fill the order. Teach New bag, the open-bag indicator and returning to Bag 1. Goal allows at most two used bags; empty bags do not affect the score.
7. **Keep it cool:** teach face-connected cold grouping in one bag. Milk is already familiar; yogurt and peas reinforce the same new rule.
8. **The weekly shop:** combine protection and cold grouping, and introduce the broad egg tray as an extension of the existing full-support rule.

All bags remain 3 x 2 x 4. Tools are available throughout; lesson objectives guide their introduction. The first two lessons are deliberately forgiving. A return to Bag 1 is encouraged in the hint, not a mandatory scored action.

## Verified feasibility

Editor checks solve every order without damage, within its bag target and with its teaching objective met. The search only selects from the first three conveyor groceries, uses legal Turn/Tip sequences and supports multiple bags. Solutions are written to VERIFIED-SOLUTIONS.md, including bag numbers. A failed or exhausted search blocks the build; it does not certify a level as impossible.

## Next: procedural generation

This pass supplies the curriculum and multi-bag feasibility solver, not a random level generator.

1. Choose a taught mechanic, allowed product pool, bag target and desired spare capacity.
2. Construct a legal solved arrangement using the real support and load rules.
3. Produce a candidate conveyor order; verify the first-three selection constraint with the solver.
4. Reject unverified candidates. Save the seed, recipe, solution, required rotations, bag count, load margins and solver cost.
5. Score difficulty using branching choices, forced orientation, fragile dependencies and spare space. Search cost alone is not a human difficulty measure.
6. Exclude mechanics not yet introduced, and avoid relying on visually misleading shapes.
7. Playtest and calibrate against these eight authored lessons before releasing generated orders.

## Puzzle pass

Three challenges follow the eight teaching lessons:

9. **The last loaf:** nine two-cell loaves in one bag. Only eight fit with unchanged orientation, so some must rotate.
10. **A careful squeeze:** ten wide groceries with stronger peas below and fragile eggs above. At least two products must change orientation; load capacity adds placement dependencies.
11. **Make room below:** nine cereal boxes and two upright bouquets use all 24 cells. Without tipping, the bouquets occupy two floor columns and leave room for only eight cereal boxes. A tipped carton can make a low platform beneath the flowers.

These geometric impossibility proofs remain valid even with rearrangement or damage. They do not rely on a rotation bonus. Editor search also verifies damage-free completion using the actual conveyor choices and writes DIFFICULTY-AUDIT.md. Rotation minimums may be reported as a proven lower bound through a verified upper bound when the search budget prevents finding the exact minimum. A bounded arrangement survey is not an exhaustive count unless it completes.

### Interaction rules

- TargetBags is now a hard bag limit, shown in the bag selector.
- A packed grocery can be picked up only if no other grocery rests on it. Move top items first.
- A move within the open bag is transactional: cancelling restores the original placement, committing keeps the conveyor unchanged, and Undo restores the full earlier state. Cross-bag transfer while holding a grocery is not enabled.
- Picking up a product shows a compact cream property tag beside the Turn/Tip dock: weight, support strength, cold and upright traits where applicable. A blocked pickup shows the same information briefly.
- Crush warnings describe the proposed placement, separately from inherent product strength.
- Shopping list remains available before committing to a packing plan; no timer is added.

12. **Family size:** three 2 x 3 x 1 cornflakes boxes plus milk, rice and soup occupy 22 of 24 cells. This checkout explores large-box orientation. A three-star solution must include tipping and no damage; it is verified against the conveyor order.

## Experiments 13-15 and direct access

Every level is unlocked on the map for out-of-order playtesting. The preview panel gives the actual width, depth and height before entering. New experiments are marked NEW. Best stars are stored per level name; a Play action also records tried status.

13. Breakfast to go: two 6-cell cartons in a shallow 12-cell bag. Intended construction is two horizontal 3 x 2 layers.
14. Narrow aisles: two milk cartons laid as 2 x 1 bases, one per depth row, each supporting a 2-wide x 3-high cornflakes carton. The 2 x 2 x 4 bag has no room to leave the milk upright.
15. Ready to seal: six rice boxes form a bottom layer; six upright small cereal boxes produce a level top at height 3. The standard bag retains one unused level above the marked seal line. Full columns under the line are required; unsupported or higher packing cannot complete it.

These are authored constructions, not newly executed solver results. The player requested that tests be left to them. Bag dimensions now travel with packed items and placement previews so visuals, target bounds and occupancy use the same geometry. The optional editor solver accepts level-specific dimensions, but was not run for this pass.

## Authored discovery chapter (16–21)

See AHA-LEVELS.md for six exact-fill puzzles, intended discoveries, load-aware construction plans, conveyor choices, and playtest questions. These new constructions are not covered by earlier solver-verification claims; no solver or gameplay tests were run for this pass. Use Map → New puzzles to jump to the chapter.
