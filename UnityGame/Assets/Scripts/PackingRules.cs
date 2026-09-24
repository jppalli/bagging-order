using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BaggingOrder
{
    public class Grocery
    {
        public string Key, Name;
        public Vector3Int Size;
        public Vector3Int[] Shape;
        public float Weight, Capacity;
        public bool Upright, Cold;
        public Grocery(string key,string name,int w,int h,int d,float weight,float capacity,bool upright=false,bool cold=false,Vector3Int[] shape=null)
        {
            Key=key;Name=name;Size=new Vector3Int(w,h,d);Weight=weight;Capacity=capacity;Upright=upright;Cold=cold;
            var cells=new List<Vector3Int>();for(int x=0;x<w;x++)for(int y=0;y<h;y++)for(int z=0;z<d;z++)cells.Add(new Vector3Int(x,y,z));Shape=shape??cells.ToArray();
        }
    }
    [Serializable] public class PackedItem
    {
        public int Id,X,Y,Z,Orientation;
        public int BagWidth=3,BagDepth=2;
        public string Key,CrushedBy;
        public bool Crushed,UsedTurn,UsedTip;
        public PackedItem Copy()=>(PackedItem)MemberwiseClone();
        public Vector3Int[] LocalCells=>PackingRules.OrientedCells(Key,Orientation);
        public Vector3Int Size { get {var c=LocalCells;return new Vector3Int(c.Max(v=>v.x)+1,c.Max(v=>v.y)+1,c.Max(v=>v.z)+1);} }
        public Vector3 Position=>new Vector3(X+Size.x*.5f-BagWidth*.5f,Y,Z+Size.z*.5f-BagDepth*.5f);
        public Quaternion Rotation=>PackingRules.Orientations[Orientation];
        public Vector3 VisualPosition=>Position+Vector3.up*Size.y*.5f-Rotation*(Vector3.up*PackingRules.Catalog[Key].Size.y*.5f);
    }
    public class Placement
    {
        public bool Valid;
        public PackedItem Item;
        public string Reason;
        public Dictionary<int,float> Loads=new Dictionary<int,float>();
        public HashSet<int> Crushed=new HashSet<int>();
    }
    public class Checkout
    {
        public string Name,Hint;
        public int Width=3,Depth=2,Height=4,TargetBags=1,SealHeight;
        public bool PerfectFit,Experimental;
        public string Dimensions=>Width+" wide / "+Depth+" deep / "+Height+" high";
        public bool TurnGoal,TipGoal,BackGoal,ColdGoal;
        public int RotationProof; // 1: wide-product bound; 2: upright-carton bound
        public string[] Items;
        public Checkout(string name,string hint,string[] items,bool turn=false,bool tip=false,bool back=false,bool cold=false)
        {Name=name;Hint=hint;Items=items;TurnGoal=turn;TipGoal=tip;BackGoal=back;ColdGoal=cold;}
        public string Goal=>(TargetBags==1?"One bag":"Up to "+TargetBags+" bags")+(BackGoal?" + use both rows":"")+(TurnGoal?" + turn a grocery":"")+(TipGoal?" + tip a grocery":"")+(ColdGoal?" + one connected cold group":"")+(PerfectFit?" + fill every space":"")+(SealHeight>0?" + level top at "+SealHeight:"");
    }
    public static class PackingRules
    {
        public static readonly Dictionary<string,Grocery> Catalog=new Dictionary<string,Grocery>{
            {"rice",new Grocery("rice","Pantry rice",1,1,1,2,8)},
            {"can",new Grocery("can","Tomato soup",1,1,1,3,float.PositiveInfinity)},
            {"milk",new Grocery("milk","Farm milk",1,2,1,3,7,cold:true)},
            {"cornflakes",new Grocery("cornflakes","Family cornflakes",2,3,1,1,3)},
            {"cereal",new Grocery("cereal","Whole grain oats",1,2,1,1,5)},
            {"eggs",new Grocery("eggs","Free-range eggs",2,1,1,1,1)},
            {"bread",new Grocery("bread","Sourdough",2,1,1,1,2)},
            {"apple",new Grocery("apple","Apple crate",1,1,1,1,3)},
            {"tray",new Grocery("tray","Family egg tray",2,1,2,2,1)},
            {"flowers",new Grocery("flowers","Fresh flowers",1,3,1,1,0,upright:true)},
            {"bananas",new Grocery("bananas","Banana bunch",2,1,1,1,1)},
            {"yogurt",new Grocery("yogurt","Yogurt",1,1,1,1,2,cold:true)},
            {"peas",new Grocery("peas","Frozen peas",2,1,1,2,4,cold:true)}
        };
        public static readonly Checkout[] Levels={
            new Checkout("A sturdy start","Start with sturdy groceries. Try both rows of the bag.",new[]{"can","can","rice","cereal","can","rice"}),
            new Checkout("Delicate on top","Bananas bruise like eggs. Put sturdy groceries underneath them.",new[]{"bananas","eggs","can","can","bread","apple"}),
            new Checkout("A quarter turn","Select bread or eggs, then use Turn to change their footprint.",new[]{"can","rice","eggs","bread","cereal","apple"},turn:true),
            new Checkout("Lay it down","Select milk, then use Tip to lay it on its side.",new[]{"milk","cereal","can","rice","bread"},tip:true),
            new Checkout("This way up","Flowers stay upright. Leave headroom and keep them uncovered.",new[]{"flowers","can","rice","cereal","eggs","bread"}),
            new Checkout("Room for another","Seven upright bouquets need two bags. Use New bag, then try returning to Bag 1.",new[]{"flowers","flowers","flowers","flowers","flowers","flowers","flowers","can","rice"}){TargetBags=2},
            new Checkout("Keep it cool","Keep groceries with snowflake badges touching in one cold group.",new[]{"milk","yogurt","peas","can","rice","bread"},cold:true),
            new Checkout("The weekly shop","Build a supported base for the egg tray. Keep cold groceries together.",new[]{"can","rice","milk","peas","yogurt","bananas","tray","eggs"},cold:true),
            new Checkout("The last loaf","One bag, nine loaves. Turn some lengthways to use the narrow spaces.",Enumerable.Repeat("bread",9).ToArray()){RotationProof=1},
            new Checkout("A careful squeeze","Build with peas, protect the eggs, and rotate to fill the remaining spaces.",new[]{"peas","peas","bread","peas","bread","bread","eggs","bread","eggs","eggs"}){RotationProof=1},
            new Checkout("Make room below","Flowers need three levels of headroom. What can lie flat beneath them?",new[]{"cereal","cereal","flowers","cereal","cereal","flowers","cereal","cereal","cereal","cereal","cereal"}){RotationProof=2},
            new Checkout("Family size","Three big cereal boxes, one bag. Turn and tip to find room for breakfast.",new[]{"cornflakes","cornflakes","cornflakes","milk","rice","can"},tip:true),
            new Checkout("Breakfast to go","A shallow bag: lay both family boxes flat and fill every space.",new[]{"cornflakes","cornflakes"}){Height=2,PerfectFit=true,Experimental=true},
            new Checkout("Narrow aisles","Tip the milk to make a low base. The tall cereal boxes need their headroom.",new[]{"milk","cornflakes","milk","cornflakes"},tip:true){Width=2,Experimental=true},
            new Checkout("Ready to seal","Build a level top at the gold mark. Leave nothing unsupported.",new[]{"rice","rice","rice","cereal","cereal","cereal","rice","rice","rice","cereal","cereal","cereal"}){SealHeight=3,Experimental=true},
            // Authored discovery sequence. Construction notes live in AHA-LEVELS.md.
            new Checkout("The narrow strip","Three loaves, one shallow bag. Every space counts.",new[]{"bread","bread","bread"}){Height=1,PerfectFit=true,Experimental=true},
            new Checkout("The breakfast lid","A family box and a shallow bag. Choose what goes in first.",new[]{"cornflakes","milk","peas","yogurt","rice"}){Height=2,PerfectFit=true,Experimental=true},
            new Checkout("Soft landing","The yogurt is delicate. Fill the bag without crushing it.",new[]{"yogurt","can","can","yogurt","can","can","yogurt","yogurt"}){Width=2,Height=2,PerfectFit=true,Experimental=true},
            new Checkout("Save the headroom","Flowers stay upright and need clear space above them.",new[]{"flowers","cereal","cereal","flowers","flowers","cereal","rice"}){Width=2,PerfectFit=true,Experimental=true},
            new Checkout("A chilled corner","Milk, peas and yogurt must touch in one group. Corners do not connect.",new[]{"milk","cereal","rice","peas","rice","yogurt","rice","rice","rice"},cold:true){Height=2,PerfectFit=true,Experimental=true},
            new Checkout("Two different jobs","Fill both bags. Keep chilled groceries together and flowers uncovered.",new[]{"flowers","rice","cornflakes","milk","rice","flowers","peas","rice","flowers","yogurt","rice"},cold:true){Width=2,Height=3,TargetBags=2,PerfectFit=true,Experimental=true}

        };
        public static readonly List<Quaternion> Orientations=BuildOrientations();
        static readonly Dictionary<string,Vector3Int[]> shapeCache=new Dictionary<string,Vector3Int[]>();
        static List<Quaternion> BuildOrientations()
        {
            var list=new List<Quaternion>{Quaternion.identity};
            for(int i=0;i<list.Count;i++)foreach(var axis in new[]{Vector3.up,Vector3.forward}){
                var q=Quaternion.AngleAxis(90,axis)*list[i];if(!list.Any(v=>Mathf.Abs(Quaternion.Dot(q,v))>.999f))list.Add(q);
            }
            return list;
        }
        public static Vector3Int[] OrientedCells(string key,int orientation)
        {
            string cacheKey=key+":"+orientation;if(shapeCache.TryGetValue(cacheKey,out var cached))return cached;
            var cells=Catalog[key].Shape.Select(v=>Vector3Int.RoundToInt(Orientations[orientation]*(Vector3)v)).ToArray();
            var min=new Vector3Int(cells.Min(v=>v.x),cells.Min(v=>v.y),cells.Min(v=>v.z));
            var result=cells.Select(v=>v-min).OrderBy(v=>v.x).ThenBy(v=>v.y).ThenBy(v=>v.z).ToArray();shapeCache[cacheKey]=result;return result;
        }
        public static bool IsUpright(int orientation)=>Vector3.Dot(Orientations[orientation]*Vector3.up,Vector3.up)>.99f;
        public static int Rotate(string key,int orientation,bool tip)
        {
            if(tip&&Catalog[key].Upright)return orientation;
            var original=OrientedCells(key,orientation);
            foreach(var axis in tip?new[]{Vector3.forward,Vector3.right}:new[]{Vector3.up})for(int turns=1;turns<=3;turns++){
                var q=Quaternion.AngleAxis(90*turns,axis)*Orientations[orientation];int next=Orientations.FindIndex(v=>Mathf.Abs(Quaternion.Dot(q,v))>.999f);
                if(Catalog[key].Upright&&!IsUpright(next))continue;
                if(!original.SequenceEqual(OrientedCells(key,next)))return next;
            }
            return orientation;
        }
        public static IEnumerable<Vector3Int> Cells(PackedItem i)=>i.LocalCells.Select(c=>c+new Vector3Int(i.X,i.Y,i.Z));
        public static bool CanLift(List<PackedItem> items,PackedItem item)
        {
            var own=new HashSet<Vector3Int>(Cells(item));
            return !items.Where(i=>i.Id!=item.Id).SelectMany(Cells).Any(c=>own.Contains(c+Vector3Int.down));
        }
        public static string WeightLabel(Grocery item)=>item.Weight>=3?"Heavy":item.Weight>=2?"Medium":"Light";
        public static string StrengthLabel(Grocery item)=>item.Capacity==0?"Keep clear on top":item.Capacity<=1?"Fragile - light items only":item.Capacity<=3?"Crushable - light loads":"Sturdy support";
        public static bool TryLoads(List<PackedItem> items,out Dictionary<int,float> result)
        {
            result=items.ToDictionary(i=>i.Id,i=>0f);
            var grid=new Dictionary<Vector3Int,PackedItem>();foreach(var i in items)foreach(var cell in Cells(i))grid[cell]=i;
            var edges=new Dictionary<int,List<int>>();var count=items.ToDictionary(i=>i.Id,i=>0);
            foreach(var item in items){
                var own=new HashSet<Vector3Int>(Cells(item));var below=new List<int>();
                foreach(var cell in own){var under=cell+Vector3Int.down;if(own.Contains(under))continue;
                    if(under.y<0)below.Add(-1);else if(grid.TryGetValue(under,out var support)){below.Add(support.Id);count[support.Id]++;}
                }
                edges[item.Id]=below;
            }
            var ready=new Queue<int>(count.Where(p=>p.Value==0).Select(p=>p.Key));int processed=0;
            while(ready.Count>0){int id=ready.Dequeue();processed++;var supports=edges[id];float total=result[id]+Catalog[items.First(i=>i.Id==id).Key].Weight;
                foreach(int support in supports)if(support>=0){result[support]+=total/supports.Count;if(--count[support]==0)ready.Enqueue(support);}
            }
            return processed==items.Count;
        }
        public static Placement Preview(List<PackedItem> items,string key,int id,int x,int z,bool turned,int height)=>Preview(items,key,id,x,z,turned?Rotate(key,0,false):0,height);
        public static Placement Preview(List<PackedItem> items,string key,int id,int x,int z,int orientation,int height,int width=3,int depth=2)
        {
            var p=new Placement{Item=new PackedItem{Key=key,Id=id,X=x,Z=z,Orientation=orientation,BagWidth=width,BagDepth=depth}};var size=p.Item.Size;
            if(Catalog[key].Upright&&!IsUpright(orientation)){p.Reason="Keep this grocery upright";return p;}
            if(x<0||z<0||x+size.x>width||z+size.z>depth){p.Reason="Outside the bag";return p;}
            var occupied=new HashSet<Vector3Int>(items.SelectMany(Cells));int y=height;
            while(y>0&&!p.Item.LocalCells.Any(c=>occupied.Contains(c+new Vector3Int(x,y-1,z))))y--;
            p.Item.Y=y;if(y+size.y>height){p.Reason="Too tall for this spot";return p;}
            var own=new HashSet<Vector3Int>(Cells(p.Item));
            foreach(var c in own){var under=c+Vector3Int.down;if(under.y>=0&&!own.Contains(under)&&!occupied.Contains(under)){p.Reason="Support the whole underside";return p;}}
            var all=new List<PackedItem>(items){p.Item};
            if(!TryLoads(all,out p.Loads)){p.Reason="These supports form an unstable loop";return p;}
            foreach(var i in all)if(p.Loads[i.Id]>Catalog[i.Key].Capacity)p.Crushed.Add(i.Id);
            p.Valid=true;p.Reason=p.Crushed.Count>0?"Will crush a grocery. Undo is available.":"Release to pack";return p;
        }
        static readonly Vector3Int[] Directions={Vector3Int.up,Vector3Int.down,Vector3Int.left,Vector3Int.right,new Vector3Int(0,0,1),new Vector3Int(0,0,-1)};
        public static bool ColdConnected(List<List<PackedItem>> bags)
        {
            var cold=bags.SelectMany(b=>b).Where(i=>Catalog[i.Key].Cold).ToList();if(cold.Count==0)return false;
            var owner=bags.First(b=>b.Contains(cold[0]));if(cold.Any(i=>!owner.Contains(i)))return false;
            var cells=new HashSet<Vector3Int>(cold.SelectMany(Cells));var reached=new HashSet<Vector3Int>();var queue=new Queue<Vector3Int>();queue.Enqueue(cells.First());
            while(queue.Count>0){var c=queue.Dequeue();if(!reached.Add(c))continue;foreach(var d in Directions)if(cells.Contains(c+d)&&!reached.Contains(c+d))queue.Enqueue(c+d);}
            return reached.Count==cells.Count;
        }
        public static bool GoalMet(Checkout level,List<List<PackedItem>> bags)
        {
            var items=bags.SelectMany(b=>b).ToList();return bags.Count(b=>b.Count>0)<=level.TargetBags
                &&(!level.TurnGoal||items.Any(i=>i.UsedTurn))&&(!level.TipGoal||items.Any(i=>i.UsedTip))
                &&ShapeGoalMet(level,bags)
                &&(!level.BackGoal||(items.Any(i=>i.Z==0)&&items.Any(i=>i.Z+i.Size.z>1)))&&(!level.ColdGoal||ColdConnected(bags));
        }
        public static bool ShapeGoalMet(Checkout level,List<List<PackedItem>> bags)
        {
            if(!level.PerfectFit&&level.SealHeight==0)return true;
            var used=bags.Where(b=>b.Count>0).ToList();if(used.Count==0)return false;
            foreach(var bag in used){
                var cells=new HashSet<Vector3Int>(bag.SelectMany(Cells));
                int target=level.PerfectFit?level.Height:level.SealHeight;
                if(cells.Any(c=>c.x<0||c.x>=level.Width||c.z<0||c.z>=level.Depth||c.y<0||c.y>=target))return false;
                for(int x=0;x<level.Width;x++)for(int z=0;z<level.Depth;z++)for(int y=0;y<target;y++)if(!cells.Contains(new Vector3Int(x,y,z)))return false;
            }
            return true;
        }
        public static int Stars(Checkout level,List<List<PackedItem>> bags)=>1+(bags.SelectMany(b=>b).Any(i=>i.Crushed)?0:1)+(GoalMet(level,bags)?1:0);
    }
}
