import test from 'node:test';
import assert from 'node:assert/strict';
import {preview,loads,sizeOf} from '../src/game.js';
test('rows are independent until an item spans both',()=>{const front={type:'bread',id:0,x:0,z:0,y:0};const p=preview([front],'can',0,1,false,1);assert.equal(p.valid,true);assert.equal(p.item.y,0);assert.equal(p.weights.get(0),0);});
test('weight propagates through uncrushable cans',()=>{const stack=[{type:'apple',id:0,x:0,z:0,y:0},{type:'can',id:1,x:0,z:0,y:1}];const p=preview(stack,'can',0,0,false,2);assert.equal(p.weights.get(0),6);assert.equal(p.crushed[0].id,0);});
test('wide groceries require full support',()=>{const p=preview([{type:'can',id:0,x:0,z:0,y:0}],'bread',0,0);assert.equal(p.valid,false);assert.match(p.reason,/level base/);});
test('load splits over front and back supports',()=>{const items=[{type:'can',id:0,x:0,z:0,y:0},{type:'can',id:1,x:1,z:0,y:0},{type:'can',id:2,x:0,z:1,y:0},{type:'can',id:3,x:1,z:1,y:0}];const p=preview(items,'tray',0,0,false,4);assert.equal(p.valid,true);for(let id=0;id<4;id++)assert.equal(p.weights.get(id),.5);});
test('rotation swaps width and depth; boundaries and height hold',()=>{assert.deepEqual(sizeOf({type:'bread',rotated:true}),[1,2,1]);assert.equal(preview([],'bread',0,1,true).valid,false);assert.equal(preview([],'bread',2,0).valid,false);assert.equal(preview([{type:'milk',id:0,x:0,z:0,y:0},{type:'milk',id:1,x:0,z:0,y:2}],'milk',0,0).valid,false);});
