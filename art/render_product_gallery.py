"""Render the actual editable Blender groceries as a review sheet."""
import bpy, math
from mathutils import Vector
from pathlib import Path
root=Path(__file__).resolve().parents[1]
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
names=['can','milk','cereal','eggs','bread','apple','tray','flowers','bananas','yogurt','peas','rice']
ink=bpy.data.materials.new('Gallery ink');ink.diffuse_color=(.04,.14,.085,1);ink.use_nodes=True;ink.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=ink.diffuse_color
for idx,name in enumerate(names):
    with bpy.data.libraries.load(str(root/'art/blender'/(name+'.blend')),link=False) as (src,dst):dst.objects=[n for n in src.objects]
    objects=[o for o in dst.objects if o and o.type=='MESH']
    for o in objects:bpy.context.collection.objects.link(o)
    corners=[o.matrix_world@Vector(c) for o in objects for c in o.bound_box]
    low=Vector(tuple(min(c[i] for c in corners) for i in range(3)));high=Vector(tuple(max(c[i] for c in corners) for i in range(3)))
    scale=1.8/max(high-low);center=Vector(((low.x+high.x)/2,(low.y+high.y)/2,low.z))
    at=Vector(((idx%4-1.5)*3.1,(2-idx//4)*3.4,0))
    for o in objects:o.location=(o.location-center)*scale+at;o.scale*=scale
    bpy.ops.object.text_add(location=(at.x,at.y-1.23,.025));label=bpy.context.object;label.data.body=name.upper();label.data.align_x='CENTER';label.data.size=.22;label.data.materials.append(ink)
bpy.ops.mesh.primitive_plane_add(size=200);floor=bpy.context.object
mat=bpy.data.materials.new('Warm gallery paper');mat.diffuse_color=(.83,.77,.61,1);mat.use_nodes=True;mat.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value=mat.diffuse_color;floor.data.materials.append(mat)
world=bpy.context.scene.world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.7,.77,.65,1);world.node_tree.nodes['Background'].inputs[1].default_value=.55
for p,energy,size in [((-4,-5,12),1900,8),((7,5,10),1200,7)]:
    bpy.ops.object.light_add(type='AREA',location=p);lamp=bpy.context.object;lamp.data.energy=energy;lamp.data.shape='DISK';lamp.data.size=size;lamp.rotation_euler=(Vector((0,3,0))-lamp.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(3,-12,18));cam=bpy.context.object;cam.rotation_euler=(Vector((0,3.2,0))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=14.7;bpy.context.scene.camera=cam
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.render.image_settings.file_format='PNG';scene.render.filepath=str(root/'art/product-gallery.png')
bpy.ops.wm.save_as_mainfile(filepath=str(root/'art/blender/product-gallery.blend'))
bpy.ops.render.render(write_still=True)
