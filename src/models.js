import * as THREE from 'three';
import {RoundedBoxGeometry} from 'three/addons/geometries/RoundedBoxGeometry.js';
import {GLTFLoader} from 'three/addons/loaders/GLTFLoader.js';
import {ITEMS} from './game.js';
const cache=new Map();
export async function loadBlenderAssets(){try{const response=await fetch('/assets/models/manifest.json');if(!response.ok)return;const manifest=await response.json();const loader=new GLTFLoader();await Promise.all(Object.entries(manifest).map(async([type,url])=>{try{const gltf=await loader.loadAsync(url);cache.set(type,gltf.scene);}catch(error){console.warn('Using built-in grocery:',type,error.message);}}));}catch{/* Optional art pack; built-in assets remain playable. */}}
function mat(color){return new THREE.MeshStandardMaterial({color,roughness:.8});}
function box(w,h,d,color,r=.06){return new THREE.Mesh(new RoundedBoxGeometry(w,h,d,2,r),mat(color));}
function label(text,color='#344e3e'){
 const c=document.createElement('canvas');c.width=256;c.height=256;const ctx=c.getContext('2d');ctx.fillStyle='#fff9e7';ctx.fillRect(0,0,256,256);ctx.strokeStyle=color;ctx.lineWidth=3;ctx.strokeRect(16,16,224,224);ctx.fillStyle=color;ctx.textAlign='center';ctx.font='18px sans-serif';ctx.fillText('THE GOOD GROCER',128,55);ctx.font='bold 43px Georgia';ctx.fillText(text,128,138);ctx.font='17px sans-serif';ctx.fillText('FRESH & SIMPLE',128,197);const tex=new THREE.CanvasTexture(c);tex.colorSpace=THREE.SRGBColorSpace;return new THREE.Mesh(new THREE.PlaneGeometry(.6,.6),new THREE.MeshStandardMaterial({map:tex,roughness:1}));
}
export function grocery(type){
 if(cache.has(type)){const copy=cache.get(type).clone(true);copy.traverse(o=>{if(o.isMesh){o.geometry=o.geometry.clone();const clone=m=>{const result=m.clone();if(result.map)result.map=result.map.clone();return result;};o.material=Array.isArray(o.material)?o.material.map(clone):clone(o.material);o.castShadow=true;o.receiveShadow=true;}});return copy;}
 const item=ITEMS[type],[w,d,h]=item.size,g=new THREE.Group();
 if(type==='can'){
  const body=new THREE.Mesh(new THREE.CylinderGeometry(.39,.39,.83,24),mat(item.color));body.position.y=.48;g.add(body);
  for(const y of [.07,.9]){const rim=new THREE.Mesh(new THREE.CylinderGeometry(.415,.415,.045,24),mat(0xd2d2ba));rim.position.y=y;g.add(rim);}
  const top=new THREE.Mesh(new THREE.TorusGeometry(.09,.017,6,16),mat(0x8b958c));top.rotation.x=Math.PI/2;top.position.set(.05,.93,0);g.add(top);
 }else if(type==='eggs'||type==='tray'){
  const base=box(w-.12,.28,d-.12,item.color);base.position.y=.23;g.add(base);
  for(let x=0;x<3;x++)for(let z=0;z<(d===2?3:2);z++){const egg=new THREE.Mesh(new THREE.SphereGeometry(.21,12,12),mat(0xf6e5c5));egg.scale.y=1.35;egg.position.set((x-1)*.53,.54,(z-((d===2?3:2)-1)/2)*.43);g.add(egg);}
 }else if(type==='bread'){
  const loaf=box(w-.16,.72,.82,item.color,.22);loaf.position.y=.42;g.add(loaf);
  for(let x=0;x<4;x++){const slash=box(.06,.025,.51,0xe7bc80,.02);slash.position.set((x-1.5)*.35,.79,0);slash.rotation.y=-.25;g.add(slash);}
 }else if(type==='apple'){
  const base=box(.88,.25,.88,0xb79968);base.position.y=.18;g.add(base);
  for(const[x,z]of[[-.22,-.2],[.22,-.2],[-.22,.23],[.22,.23]]){const a=new THREE.Mesh(new THREE.SphereGeometry(.24,12,12),mat(x<0?0xbb6048:0xa4b663));a.position.set(x,.5,z);g.add(a);const stem=box(.035,.13,.035,0x695340,.01);stem.position.set(x,.77,z);g.add(stem);}
 }else{
  const body=box(.86,h-.16,.86,item.color);body.position.y=h/2;g.add(body);
  if(type==='milk'){const band=box(.89,.45,.89,0xf4eee0);band.position.y=1.54;g.add(band);const cap=new THREE.Mesh(new THREE.CylinderGeometry(.095,.095,.055,16),mat(0x587b8e));cap.position.set(.17,1.96,.13);g.add(cap);}
 }
 if(['can','milk','cereal'].includes(type)){const l=label({can:'SOUP',milk:'MILK',cereal:'OATS'}[type]);l.position.set(0,type==='can'?.48:.86,type==='can'?.397:.438);g.add(l);}
 g.traverse(o=>{if(o.isMesh){o.castShadow=true;o.receiveShadow=true;}});return g;
}
