import './style.css';
import * as THREE from 'three';
import {RoundedBoxGeometry} from 'three/addons/geometries/RoundedBoxGeometry.js';
import {ITEMS,LEVELS,sizeOf,loads,preview} from './game.js';
import {grocery,loadBlenderAssets} from './models.js';
const $=id=>document.getElementById(id);
let level=0,queue=[],bags=[[]],bag=0,history=[],selected=null,row=0,column=0,rotated=false,current=null,sound=false,audioContext;
let saved={level:0};try{saved=JSON.parse(localStorage.getItem('bagging-order-progress'))||saved;}catch{}level=Math.max(0,Math.min(LEVELS.length-1,Number(saved.level)||0));
const scene=new THREE.Scene();scene.background=new THREE.Color(0xedf0e5);
const renderer=new THREE.WebGLRenderer({antialias:true,alpha:false});renderer.setPixelRatio(Math.min(devicePixelRatio,2));renderer.toneMapping=THREE.ACESFilmicToneMapping;renderer.toneMappingExposure=.85;renderer.shadowMap.enabled=true;renderer.shadowMap.type=THREE.PCFSoftShadowMap;renderer.outputColorSpace=THREE.SRGBColorSpace;renderer.setClearColor(0xedf0e5);$('scene').appendChild(renderer.domElement);
const camera=new THREE.OrthographicCamera(-5,5,4,-4,.1,100);camera.position.set(8,9,12);camera.lookAt(0,2.1,0);
scene.add(new THREE.HemisphereLight(0xfffbed,0x9ba78d,3));const sun=new THREE.DirectionalLight(0xfff5de,3.1);sun.position.set(-3,10,6);sun.castShadow=true;sun.shadow.mapSize.set(1024,1024);sun.shadow.camera.left=-6;sun.shadow.camera.right=6;sun.shadow.camera.top=8;sun.shadow.camera.bottom=-6;sun.shadow.normalBias=.03;scene.add(sun);
const matte=c=>new THREE.MeshStandardMaterial({color:c,roughness:.95});
function block(w,h,d,c,x,y,z){const mesh=new THREE.Mesh(new RoundedBoxGeometry(w,h,d,2,.04),matte(c));mesh.position.set(x,y,z);mesh.receiveShadow=true;scene.add(mesh);return mesh;}
const floor=new THREE.Mesh(new THREE.PlaneGeometry(200,200),matte(0xedf0e5));floor.rotation.x=-Math.PI/2;floor.position.y=-.22;floor.receiveShadow=true;scene.add(floor);
block(3.28,.15,2.26,0xbda27a,0,-.08,0);const backWall=block(3.26,5.04,.07,0xcab18a,0,2.43,-1.1);const sideWall=block(.07,5,2.2,0xd3bd97,-1.61,2.43,0);block(.07,.5,2.2,0xc4aa81,1.61,.17,0);block(3.26,.42,.07,0xd4bd95,0,.15,1.1);
// Handles and crease lines keep the bag readable with its front cut away.
const handles=new THREE.Group();scene.add(handles);
for(const x of [-.65,.65]){const curve=new THREE.CatmullRomCurve3([new THREE.Vector3(x-.24,4.95,-1.09),new THREE.Vector3(x-.2,5.43,-1.09),new THREE.Vector3(x+.2,5.43,-1.09),new THREE.Vector3(x+.24,4.95,-1.09)]);handles.add(new THREE.Mesh(new THREE.TubeGeometry(curve,20,.035,6,false),matte(0xaa8b61)));}
for(let x=-1.5;x<=1.5;x++)block(.012,.008,2,0x9e906f,x,.007,0);for(let z=-1;z<=1;z++)block(3,.008,.012,0x9e906f,0,.008,z);
const creases=[];for(let y=1;y<5;y++)creases.push(block(3,.012,.006,0xb39a75,0,y,-1.057));
const groceries=new THREE.Group(),ghost=new THREE.Group();scene.add(groceries,ghost);
const rowLight=new THREE.Mesh(new THREE.PlaneGeometry(3,.98),new THREE.MeshBasicMaterial({color:0x90a77a,transparent:true,opacity:.3,depthWrite:false}));rowLight.rotation.x=-Math.PI/2;rowLight.position.set(0,.014,.5);scene.add(rowLight);
function positionModel(model,item){const[w,d]=sizeOf(item);model.position.set(item.x+w/2-1.5,item.y,.999-item.z-d/2);model.rotation.y=item.rotated?Math.PI/2:0;}
function clear(group){while(group.children.length){const child=group.children[0];child.traverse(o=>{if(o.isMesh||o.isLine){o.geometry.dispose();const ms=Array.isArray(o.material)?o.material:[o.material];for(const m of ms){m.map?.dispose();m.dispose();}}});group.remove(child);}}
function rebuild(){clear(groceries);const weights=loads(bags[bag]);for(const item of bags[bag]){const model=grocery(item.type);positionModel(model,item);const crushed=item.crushed||weights.get(item.id)>ITEMS[item.type].tolerance;if(crushed)model.scale.y=.72;const fade=selected&&row===1&&item.z===0&&sizeOf(item)[1]===1;model.traverse(o=>{if(o.isMesh){const materials=Array.isArray(o.material)?o.material:[o.material];for(const material of materials){if(fade){material.transparent=true;material.opacity=.24;material.depthWrite=false;}if(crushed&&material.color)material.color.lerp(new THREE.Color(0xc97059),.45);}}});groceries.add(model);}rowLight.position.z=row===0?.5:-.5;}
function updatePreview(){clear(ghost);current=null;document.querySelectorAll('#columns button').forEach((b,i)=>b.classList.toggle('active',i===column));$('confirm').disabled=true;if(!selected){$('status').textContent='Drag a grocery into the bag.';return;}current=preview(bags[bag],selected.type,column,row,rotated,selected.id,LEVELS[level].height);if(!current.valid){$('status').textContent=current.reason;return;}
 const item=current.item,[w,d,h]=sizeOf(item);const color=current.crushed.length?0xc2644a:current.warning.length?0xc4a044:0x759554;const mesh=new THREE.Mesh(new RoundedBoxGeometry(w-.06,h-.06,d-.06,2,.06),new THREE.MeshBasicMaterial({color,transparent:true,opacity:.21,depthWrite:false}));mesh.position.set(item.x+w/2-1.5,item.y+h/2,.999-item.z-d/2);ghost.add(mesh);const edges=new THREE.LineSegments(new THREE.EdgesGeometry(mesh.geometry),new THREE.LineBasicMaterial({color}));edges.position.copy(mesh.position);ghost.add(edges);$('confirm').disabled=false;$('status').textContent=current.crushed.length?`Will crush: ${[...new Set(current.crushed.map(i=>ITEMS[i.type].name))].join(', ')}. Undo is available.`:current.warning.length?'Fits here. Some groceries will be near their weight limit.':'A good fit. Everything underneath stays safe.';}
function renderQueue(){const holder=$('queue');holder.replaceChildren();for(const item of queue.slice(0,3)){const def=ITEMS[item.type];const b=document.createElement('button');b.className=`grocery ${selected?.id===item.id?'selected':''}`;b.innerHTML=`<span class="grocery-icon">${def.icon}</span><span><strong>${def.name}</strong><small>${def.size.join(' × ')} · Weight ${def.weight}</small></span><span class="arrow">↗</span>`;b.onpointerdown=event=>beginDrag(event,item,b);b.onclick=()=>{if(suppressClick)return;selected=item;rotated=false;renderQueue();updatePreview();showTargets();};holder.appendChild(b);}if(selected){const def=ITEMS[selected.type];$('item-info').textContent=`${def.name} · ${def.tolerance===Infinity?'Uncrushable':`Supports ${def.tolerance} weight units`}. Drag to a glowing spot. R rotates.`;}else $('item-info').textContent=queue.length?'Drag to pack · Tap an item, then a spot also works.':'All groceries packed.';
 const total=LEVELS[level].items.length,packed=total-queue.length;$('progress').textContent=`${packed} / ${total}`;$('progress-bar').style.width=`${packed/total*100}%`;$('bag-label').textContent=`PAPER BAG ${String(bag+1).padStart(2,'0')} / ${bags.length}`;$('undo').disabled=!history.length;$('next-bag').disabled=!queue.length;$('previous-bag').disabled=bag===0;}
function refresh(){renderQueue();rebuild();updatePreview();showTargets();}
function snapshot(){return structuredClone({queue,bags,bag,selected,row,column,rotated});}
function beep(){if(!sound)return;audioContext??=new AudioContext();audioContext.resume();const osc=audioContext.createOscillator(),gain=audioContext.createGain();osc.connect(gain);gain.connect(audioContext.destination);osc.frequency.setValueAtTime(480,audioContext.currentTime);osc.frequency.exponentialRampToValueAtTime(720,audioContext.currentTime+.09);gain.gain.setValueAtTime(.04,audioContext.currentTime);gain.gain.exponentialRampToValueAtTime(.001,audioContext.currentTime+.15);osc.start();osc.stop(audioContext.currentTime+.16);}
function pack(){if(!current?.valid||!selected)return;history.push(snapshot());bags[bag].push({...current.item});for(const i of bags[bag])if(current.crushed.some(c=>c.id===i.id))i.crushed=true;queue=queue.filter(i=>i.id!==selected.id);selected=null;beep();refresh();if(!queue.length)finish();}
function finish(){const damage=bags.flat().filter(i=>i.crushed).length;const stars=damage>1?1:damage||bags.length>LEVELS[level].bags?2:3;$('stars').textContent='★'.repeat(stars)+'☆'.repeat(3-stars);$('result-copy').textContent=`${bags.length} ${bags.length===1?'bag':'bags'} packed · ${damage} crushed. ${stars===3?'Thoughtful packing. Nicely done.':'Another arrangement could earn you three stars.'}`;$('next-level').textContent=level===LEVELS.length-1?'Play from the beginning ↗':'Next checkout ↗';$('result').hidden=false;try{localStorage.setItem('bagging-order-progress',JSON.stringify({level:Math.min(level+1,LEVELS.length-1)}));}catch{}}
function start(){const height=LEVELS[level].height;for(const wall of [backWall,sideWall]){wall.scale.y=height/5;wall.position.y=height/2-.07;}handles.position.y=height-5;creases.forEach(line=>line.visible=line.position.y<height);camera.lookAt(0,height/2-.4,0);queue=LEVELS[level].items.map((type,id)=>({type,id}));bags=[[]];bag=0;history=[];selected=null;row=0;column=0;rotated=false;$('result').hidden=true;$('level-number').textContent=String(level+1).padStart(2,'0');$('level-title').textContent=LEVELS[level].name;$('level-description').textContent=LEVELS[level].description;setRow(0);refresh();}
function setRow(value){row=value;$('front').classList.toggle('active',!row);$('back').classList.toggle('active',!!row);$('front').setAttribute('aria-pressed',String(!row));$('back').setAttribute('aria-pressed',String(!!row));rebuild();updatePreview();}
$('front').onclick=()=>setRow(0);$('back').onclick=()=>setRow(1);$('rotate').onclick=()=>{rotated=!rotated;updatePreview();showTargets();};document.querySelectorAll('#columns button').forEach(b=>b.onclick=()=>{column=Number(b.dataset.column);updatePreview();});$('confirm').onclick=pack;
$('undo').onclick=()=>{if(!history.length)return;({queue,bags,bag,selected,row,column,rotated}=history.pop());$('result').hidden=true;setRow(row);refresh();};$('restart').onclick=start;$('replay').onclick=start;$('next-level').onclick=()=>{level=(level+1)%LEVELS.length;start();};$('next-bag').onclick=()=>{history.push(snapshot());if(bags[bag].length&&bag===bags.length-1){bags.push([]);bag++;}else bag=(bag+1)%bags.length;refresh();};$('previous-bag').onclick=()=>{if(bag>0){history.push(snapshot());bag--;refresh();}};$('sound').onclick=()=>{sound=!sound;$('sound').textContent=sound?'Sound on':'Sound off';beep();};
let dragging=null,suppressClick=false,targets=[];
const targetLayer=$('drop-targets'),dragToken=$('drag-token');
function showTargets(){
 targetLayer.replaceChildren();targets=[];if(!selected)return;
 const rect=$('scene').getBoundingClientRect();
 for(let z=0;z<2;z++)for(let x=0;x<3;x++){
  const p=preview(bags[bag],selected.type,x,z,rotated,selected.id,LEVELS[level].height);
  if(!p.valid)continue;
  const [w,d,h]=sizeOf(p.item),point=new THREE.Vector3(x+w/2-1.5,p.item.y+h/2,.999-z-d/2).project(camera);
  const cx=(point.x+1)*rect.width/2,cy=(1-point.y)*rect.height/2;
  const button=document.createElement('button');button.className='drop-spot';button.setAttribute('aria-label',`${z?'Back':'Front'} ${['left','middle','right'][x]} packing spot`);
  button.style.left=`${cx}px`;button.style.top=`${cy}px`;button.dataset.danger=String(p.crushed.length>0);
  button.onclick=()=>{column=x;setRow(z);pack();showTargets();};targetLayer.appendChild(button);targets.push({x,z,cx,cy,button});
 }
}
function beginDrag(event,item,element){
 if(event.button!==0)return;event.preventDefault();if(selected?.id!==item.id)rotated=false;selected=item;
 dragging={id:event.pointerId,startX:event.clientX,startY:event.clientY,moved:false,element,touch:event.pointerType==='touch',target:null};
 element.setPointerCapture(event.pointerId);document.body.classList.add('dragging');
 document.querySelectorAll('.grocery').forEach(b=>b.classList.toggle('selected',b===element));
 updatePreview();clear(ghost);showTargets();dragToken.textContent=ITEMS[item.type].icon;
 $('status').textContent='Drag onto a glowing spot. Release to pack.';
}
function moveDrag(event){
 if(!dragging||event.pointerId!==dragging.id)return;
 if(Math.hypot(event.clientX-dragging.startX,event.clientY-dragging.startY)>5)dragging.moved=true;
 if(!dragging.moved)return;
 const rect=$('scene').getBoundingClientRect(),px=event.clientX-rect.left,py=event.clientY-rect.top-(dragging.touch?42:0);
 dragToken.hidden=false;dragToken.style.left=`${event.clientX}px`;dragToken.style.top=`${event.clientY-(dragging.touch?42:0)}px`;
 let best=null,distance=Infinity;
 if(px>=0&&px<=rect.width&&py>=0&&py<=rect.height)for(const target of targets){const d=Math.hypot(px-target.cx,py-target.cy);if(d<distance){best=target;distance=d;}}
 if(distance>Math.min(90,rect.width*.22))best=null;
 for(const target of targets)target.button.classList.toggle('hovered',target===best);
 if(best){if(dragging.target!==best){column=best.x;row=best.z;rebuild();updatePreview();}dragToken.classList.add('snapped');}
 else{clear(ghost);current=null;$('status').textContent='Move over a glowing spot to place the grocery.';dragToken.classList.remove('snapped');}
 dragging.target=best;
}
function endDrag(event,cancelled=false){
 if(!dragging||event.pointerId!==dragging.id)return;
 const drag=dragging;dragging=null;drag.element.releasePointerCapture?.(event.pointerId);
 dragToken.hidden=true;document.body.classList.remove('dragging');suppressClick=true;setTimeout(()=>suppressClick=false,0);
 if(!cancelled&&drag.moved&&drag.target)pack();
 else{renderQueue();updatePreview();if(drag.moved||cancelled){clear(ghost);$('status').textContent='Not placed. Try another spot.';}}
 showTargets();
}
window.addEventListener('pointermove',moveDrag);window.addEventListener('pointerup',event=>endDrag(event));window.addEventListener('pointercancel',event=>endDrag(event,true));
window.addEventListener('blur',()=>{if(dragging)endDrag({pointerId:dragging.id},true);});
window.addEventListener('keydown',e=>{if(!$('result').hidden)return;if(e.key==='Escape'&&dragging){endDrag({pointerId:dragging.id},true);return;}if(e.key==='Enter'&&e.target instanceof HTMLButtonElement)return;if(e.key==='Enter'){e.preventDefault();pack();}if(e.key.toLowerCase()==='r')$('rotate').click();if(e.key==='ArrowLeft'||e.key==='ArrowRight'){e.preventDefault();column=THREE.MathUtils.clamp(column+(e.key==='ArrowLeft'?-1:1),0,2);updatePreview();}if(e.key==='ArrowUp'||e.key==='ArrowDown'){e.preventDefault();setRow(e.key==='ArrowUp'?1:0);}});
function resize(){const w=$('scene').clientWidth,h=$('scene').clientHeight;renderer.setSize(w,h);const aspect=w/h,span=7.4;camera.left=-span*aspect/2;camera.right=span*aspect/2;camera.top=span/2;camera.bottom=-span/2;camera.updateProjectionMatrix();showTargets();}
new ResizeObserver(resize).observe($('scene'));document.addEventListener('visibilitychange',()=>{if(document.hidden)audioContext?.suspend();else if(sound)audioContext?.resume();});renderer.setAnimationLoop(()=>renderer.render(scene,camera));start();loadBlenderAssets().then(rebuild);
