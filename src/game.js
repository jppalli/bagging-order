export const ITEMS = {
 can: {name:'Soup can',icon:'🥫',size:[1,1,1],weight:3,tolerance:Infinity,color:0xd47b52},
 milk: {name:'Milk carton',icon:'🥛',size:[1,1,2],weight:3,tolerance:7,color:0x7796ab},
 cereal: {name:'Oat cereal',icon:'🌾',size:[1,1,2],weight:1,tolerance:5,color:0xd8ae55},
 eggs: {name:'Free-range eggs',icon:'🥚',size:[2,1,1],weight:1,tolerance:1,color:0xc5c09a},
 bread: {name:'Sourdough loaf',icon:'🍞',size:[2,1,1],weight:1,tolerance:2,color:0xc58d4c},
 apple: {name:'Apple crate',icon:'🍎',size:[1,1,1],weight:1,tolerance:3,color:0xaab77c},
 tray: {name:'Family egg tray',icon:'🥚',size:[2,2,1],weight:2,tolerance:1,color:0xc5c09a}
};
export const LEVELS = [
 {height:3,name:'Room for two',description:'Try both rows. Pack the cans first, then find a home for the lighter groceries.',items:['can','can','milk','cereal','eggs','bread'],bags:1},
 {height:3,name:'Handle with care',description:'Build a sturdy base. Every item passes its weight to the groceries beneath it.',items:['milk','can','can','cereal','apple','milk','eggs','bread'],bags:1},
 {height:4,name:'Across the aisle',description:'The family egg tray spans both rows. Leave a supported place for it on top.',items:['milk','milk','can','can','cereal','apple','tray'],bags:1},
 {height:4,name:'The weekly shop',description:'A fuller checkout. Rotate wide groceries to run from front to back.',items:['can','milk','cereal','can','milk','apple','cereal','apple','eggs','bread','tray'],bags:2},
 {height:5,name:'One careful bag',description:'Plan your supports and keep the delicate groceries near the top.',items:['can','can','milk','milk','cereal','cereal','apple','apple','eggs','bread'],bags:1}
];
export function sizeOf(item){const [w,d,h]=ITEMS[item.type].size;return item.rotated?[d,w,h]:[w,d,h];}
export function cells(item){const [w,d,h]=sizeOf(item),out=[];for(let x=0;x<w;x++)for(let z=0;z<d;z++)for(let y=0;y<h;y++)out.push(`${item.x+x},${item.z+z},${item.y+y}`);return out;}
export function loads(items){const result=new Map(items.map(i=>[i.id,0]));const occupied=new Map();for(const i of items)for(const c of cells(i))occupied.set(c,i);for(const i of [...items].sort((a,b)=>b.y-a.y)){if(!i.y)continue;const[w,d]=sizeOf(i),supports=[];for(let x=0;x<w;x++)for(let z=0;z<d;z++){const s=occupied.get(`${i.x+x},${i.z+z},${i.y-1}`);if(s)supports.push(s);}const total=ITEMS[i.type].weight+result.get(i.id);for(const s of supports)result.set(s.id,result.get(s.id)+total/supports.length);}return result;}
export function preview(items,type,x,z,rotated=false,id='preview',height=5){
 const item={type,x,z,rotated,id,y:0},[w,d,h]=sizeOf(item);
 if(x<0||z<0||x+w>3||z+d>2)return {valid:false,reason:'This grocery extends beyond the bag. Rotate it or change column.'};
 const occupied=new Set(items.flatMap(cells));let y=0;
 for(const other of items){const[ow,od,oh]=sizeOf(other);if(x<other.x+ow&&x+w>other.x&&z<other.z+od&&z+d>other.z)y=Math.max(y,other.y+oh);}
 item.y=y;if(y+h>height)return {valid:false,reason:'Too tall for this space. Try another column or row.'};
 if(y>0)for(let a=0;a<w;a++)for(let b=0;b<d;b++)if(!occupied.has(`${x+a},${z+b},${y-1}`))return {valid:false,reason:'Needs a level base under its whole footprint.'};
 const all=[...items,item],weights=loads(all),crushed=all.filter(i=>weights.get(i.id)>ITEMS[i.type].tolerance),warning=all.filter(i=>Number.isFinite(ITEMS[i.type].tolerance)&&weights.get(i.id)>0&&weights.get(i.id)>=ITEMS[i.type].tolerance*.75);
 return {valid:true,item,weights,crushed,warning};
}
