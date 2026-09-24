using System;
using System.Collections.Generic;
using System.Linq;
using BaggingOrder;
using UnityEngine;

public static class MechanicsChecks
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static int nodes;
    static int rotationBudget=-1;
    static bool surveying;
    static bool exhausted;
    static int nodeLimit=250000;
    static readonly HashSet<string> arrangements=new HashSet<string>();
    static HashSet<string> seen;
    static Dictionary<string,List<(int orientation,bool turn)>> rotations=new Dictionary<string,List<(int,bool)>>();
    static List<(int orientation,bool turn)> Options(string key)
    {
        if(rotations.TryGetValue(key,out var result))return result;
        result=new List<(int,bool)>{(0,false)};
        for(int i=0;i<result.Count;i++)foreach(bool tip in new[]{false,true}){
            int next=PackingRules.Rotate(key,result[i].Item1,tip);bool turn=result[i].Item2||(!tip&&next!=result[i].Item1);
            if(!result.Contains((next,turn)))result.Add((next,turn));
        }
        rotations[key]=result;return result;
    }
    static bool Solve(Checkout level,List<List<PackedItem>> bags,List<string> remaining)
    {
        if(++nodes>nodeLimit){exhausted=true;return false;}
        if(surveying&&arrangements.Count>=24)return false;
        if(remaining.Count==0){
            bool solved=PackingRules.GoalMet(level,bags);
            if(surveying&&solved)arrangements.Add(string.Join("|",bags.Select(b=>string.Join(";",b.Select(i=>i.Key+":"+string.Join(",",PackingRules.Cells(i).OrderBy(c=>c.x).ThenBy(c=>c.y).ThenBy(c=>c.z))).OrderBy(v=>v))).OrderBy(v=>v)));
            return solved&&!surveying;
        }
        string state=string.Join(",",remaining)+"/"+string.Join("|",bags.Select(b=>string.Join(";",b.Select(i=>i.Key+":"+string.Join(",",PackingRules.Cells(i).OrderBy(c=>c.x).ThenBy(c=>c.y).ThenBy(c=>c.z))).OrderBy(v=>v))+":"+b.Any(i=>i.UsedTurn)+b.Any(i=>i.UsedTip)));
        if(!seen.Add(state))return false;
        var moves=new List<(int index,int bag,Placement p)>();
        var offered=new HashSet<string>();
        for(int index=0;index<Math.Min(3,remaining.Count);index++){
            string key=remaining[index];if(!offered.Add(key))continue;
            for(int bag=0;bag<Math.Min(level.TargetBags,bags.Count+1);bag++){
                if(bag==bags.Count&&bags.Any(b=>b.Count==0))continue;
                var packed=bag<bags.Count?bags[bag]:new List<PackedItem>();
                foreach(var option in Options(key))for(int x=0;x<level.Width;x++)for(int z=0;z<level.Depth;z++){
                    if(rotationBudget>=0&&bags.Sum(b=>b.Count(i=>i.Orientation!=0))+(option.orientation==0?0:1)>rotationBudget)continue;
                    var p=PackingRules.Preview(packed,key,bags.Sum(b=>b.Count),x,z,option.orientation,level.Height,level.Width,level.Depth);
                    if(!p.Valid||p.Crushed.Count>0)continue;
                    p.Item.UsedTurn=option.turn&&option.orientation!=0;p.Item.UsedTip=!PackingRules.IsUpright(option.orientation);
                    moves.Add((index,bag,p));
                }
            }
        }
        foreach(var move in moves.OrderBy(m=>m.bag).ThenBy(m=>level.RotationProof==2&&m.p.Item.Key=="flowers"&&m.p.Item.Y==1?-1:m.p.Item.Y).ThenBy(m=>m.p.Item.Size.y)){
            if(exhausted||(surveying&&arrangements.Count>=24))break;
            var next=new List<string>(remaining);next.RemoveAt(move.index);
            bool added=move.bag==bags.Count;if(added)bags.Add(new List<PackedItem>());
            bags[move.bag].Add(move.p.Item);
            if(Solve(level,bags,next))return true;
            bags[move.bag].RemoveAt(bags[move.bag].Count-1);if(added)bags.RemoveAt(bags.Count-1);
        }
        return false;
    }
    public static void Run()
    {
        Check(PackingRules.Orientations.Count==24,"Rotation group");
        Check(PackingRules.Rotate("flowers",0,true)==0,"Upright restriction");
        Check(PackingRules.Rotate("can",0,false)==0,"Symmetric rotation");
        Check(PackingRules.Catalog["cornflakes"].Size==new Vector3Int(2,3,1),"Family cereal box dimensions");
        Check(Enumerable.Range(0,PackingRules.Orientations.Count).Any(o=>new PackedItem{Key="cornflakes",Orientation=o}.Size==new Vector3Int(3,1,2)),"Family cereal can lay flat across the bag");
        int tipped=PackingRules.Rotate("milk",0,true);
        Check(new PackedItem{Key="milk",Orientation=tipped}.Size.y==1,"Milk tips onto its side");
        var banana=PackingRules.Preview(new List<PackedItem>(),"bananas",0,0,0,0,4);
        Check(banana.Valid&&banana.Item.Size==new Vector3Int(2,1,1)&&banana.Item.LocalCells.Length==2,"Bananas use a readable two-cell footprint");
        var notch=PackingRules.Preview(new List<PackedItem>{banana.Item},"can",1,1,1,0,4);
        Check(notch.Valid&&notch.Item.Y==0,"Bananas leave the rear row free");
        var a=new PackedItem{Key="yogurt",Id=0};var b=new PackedItem{Key="yogurt",Id=1,X=1,Z=1};
        Check(!PackingRules.ColdConnected(new List<List<PackedItem>>{new List<PackedItem>{a,b}}),"Diagonal cold contact rejected");
        b.Z=0;Check(PackingRules.ColdConnected(new List<List<PackedItem>>{new List<PackedItem>{a,b}}),"Face cold contact accepted");
        Check(!PackingRules.ColdConnected(new List<List<PackedItem>>{new List<PackedItem>{a},new List<PackedItem>{b}}),"Separate cold bags rejected");
        var tutorial=PackingRules.Levels.First(l=>l.TargetBags==2);
        Check(tutorial.Items.Count(k=>k=="flowers")>6,"Two-bag lesson must exceed upright floor capacity");
        var crushed=PackingRules.Preview(new List<PackedItem>{banana.Item},"can",1,0,0,0,4);
        Check(crushed.Crushed.Contains(banana.Item.Id),"Heavy groceries bruise bananas");
        var supported=new List<PackedItem>{new PackedItem{Key="can",Id=0},new PackedItem{Key="bread",Id=1,Y=1}};
        Check(!PackingRules.CanLift(supported,supported[0])&&PackingRules.CanLift(supported,supported[1]),"Only uncovered groceries can be rearranged");
        var difficulty=new List<string>{"# Puzzle difficulty audit","Rotation counts refer to products with changed orientation, not button presses. Arrangement counts are capped at 24 and 5,000 search states; they are observed lower bounds unless exhaustive. They exclude symmetric bag order and item identities. No rearrangement is required for the verified solution; players can rearrange to recover mistakes."};
        var solutions=new List<string>{"# Verified three-star solutions","Generated by the editor rule checks. Coordinates are zero-based (X, Y, Z); orientation indexes are internal cube rotations."};
        foreach(var level in PackingRules.Levels){nodes=0;exhausted=false;nodeLimit=250000;rotationBudget=-1;surveying=false;seen=new HashSet<string>();var packed=new List<List<PackedItem>>{new List<PackedItem>()};
            Check(level.Width>0&&level.Depth>0&&level.Height>0,"Positive bag dimensions");Check(Solve(level,packed,level.Items.ToList()),"No three-star solution: "+level.Name+" after "+nodes+" states");
            Check(PackingRules.Stars(level,packed)==3,"Solution score");
            solutions.Add("\n## "+level.Name+"\n\n"+string.Join("\n",packed.SelectMany((bag,index)=>bag.Select(i=>"- Bag "+(index+1)+": "+i.Key+" at ("+i.X+", "+i.Y+", "+i.Z+"); orientation "+i.Orientation))));
            Debug.Log("LEVEL_SOLVED "+level.Name+" states="+nodes);
            int minRotations=0,maxRotations=packed.Sum(b=>b.Count(i=>i.Orientation!=0));
            string proof="No geometric rotation requirement";
            if(level.RotationProof==1){
                Check(level.Items.All(k=>PackingRules.Catalog[k].Size==new Vector3Int(2,1,1)),"Wide-product proof prerequisites");
                int unrotatedSlots=(3/2)*2*level.Height*level.TargetBags;
                minRotations=level.Items.Length-unrotatedSlots;Check(minRotations>0,"Must exceed unturned width capacity");
                proof="At most "+unrotatedSlots+" unturned wide products fit. At least "+minRotations+" must change orientation, even if damage is allowed.";
            }
            if(level.RotationProof==2){
                Check(level.TargetBags==1&&level.Items.Count(k=>k=="flowers")==2&&level.Items.Count(k=>k=="cereal")==9&&level.Items.Length==11,"Upright proof prerequisites");
                Check(PackingRules.Catalog["flowers"].Upright&&PackingRules.Catalog["flowers"].Size.y==3&&PackingRules.Catalog["cereal"].Size==new Vector3Int(1,2,1),"Upright proof geometry");
                minRotations=1;proof="Without tipping, flowers must stand on the floor. Four remaining columns hold only eight upright cereal boxes, but the order has nine. Tipping is necessary regardless of damage or rearranging.";
            }
            if(level.RotationProof>0){
                for(int budget=minRotations;budget<maxRotations;budget++){
                    nodes=0;exhausted=false;rotationBudget=budget;seen=new HashSet<string>();
                    bool found=Solve(level,new List<List<PackedItem>>{new List<PackedItem>()},level.Items.ToList());
                    if(found){maxRotations=budget;break;}if(exhausted)break;minRotations=budget+1;
                }
            }
            int supportEdges=packed.Sum(b=>b.Sum(i=>b.Count(j=>j.Id!=i.Id&&PackingRules.Cells(i).Any(c=>PackingRules.Cells(j).Contains(c+Vector3Int.down)))));
            int spare=level.TargetBags*level.Width*level.Depth*level.Height-level.Items.Sum(k=>PackingRules.Catalog[k].Shape.Length);
            nodes=0;exhausted=false;nodeLimit=5000;rotationBudget=-1;surveying=true;arrangements.Clear();seen=new HashSet<string>();
            Solve(level,new List<List<PackedItem>>{new List<PackedItem>()},level.Items.ToList());
            string count=(exhausted||arrangements.Count>=24?"At least ":"Exactly ")+Math.Max(1,arrangements.Count);
            difficulty.Add("\n## "+level.Name+"\n\n- Spare cells: "+spare+"\n- Required rotated products: "+(minRotations==maxRotations?minRotations.ToString():minRotations+"–"+maxRotations)+"\n- Successful arrangements found: "+count+"\n- Support dependencies in verified solution: "+supportEdges+"\n- Conveyor choices: first three; full shopping list available\n- Proof: "+proof);

        }
        System.IO.File.WriteAllLines("VERIFIED-SOLUTIONS.md",solutions);
        System.IO.File.WriteAllLines("DIFFICULTY-AUDIT.md",difficulty);
        Debug.Log("BAGGING_MECHANICS_PASS");
    }
}
