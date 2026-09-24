"""Run with Blender --background --python art/create_groceries.py.
Creates editable Blender sources and Unity FBX assets; no external assets needed.
"""
import bpy, math, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'UnityGame/Assets/Resources/Groceries'
SOURCE = ROOT / 'art/blender'
OUT.mkdir(parents=True, exist_ok=True)
SOURCE.mkdir(parents=True, exist_ok=True)
sys.path.insert(0,str(Path(__file__).resolve().parent))
from grocery_detail import refine, export_grain

def material(name, color, metal=0):
    m=bpy.data.materials.new(name); m.diffuse_color=(*color,1); m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*color,1)
    p.inputs['Roughness'].default_value=.65; p.inputs['Metallic'].default_value=metal
    return m

def cube(name, size, at, mat, bevel=.05):
    bpy.ops.mesh.primitive_cube_add(size=1, location=at); o=bpy.context.object; o.name=name
    o.dimensions=size; bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    if bevel:
        mod=o.modifiers.new('Soft edges','BEVEL'); mod.width=bevel; mod.segments=3
        bpy.context.view_layer.objects.active=o; bpy.ops.object.modifier_apply(modifier=mod.name)
        o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    o.data.materials.append(mat); return o

def sphere(name, at, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=20,ring_count=12,location=at)
    o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(mat)
    for p in o.data.polygons:p.use_smooth=True
    return o

def cylinder(name, at, radius, depth, mat):
    bpy.ops.mesh.primitive_cylinder_add(vertices=32,radius=radius,depth=depth,location=at)
    o=bpy.context.object;o.name=name;o.data.materials.append(mat)
    mod=o.modifiers.new('Rim softness','BEVEL');mod.width=.018;mod.segments=2
    bpy.ops.object.modifier_apply(modifier=mod.name)
    o.modifiers.new('Weighted normals','WEIGHTED_NORMAL');return o

def text(body, at, size, mat):
    bpy.ops.object.text_add(location=at,rotation=(math.pi/2,0,0));o=bpy.context.object;o.name=body
    o.data.body=body;o.data.align_x='CENTER';o.data.align_y='CENTER';o.data.size=size;o.data.extrude=.001
    o.data.materials.append(mat);bpy.ops.object.convert(target='MESH')

kinds=sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else ['can','milk','cereal','eggs','bread','apple','tray','flowers','bananas','yogurt','peas','rice','cornflakes']
for kind in kinds:
    bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    cream=material('Cream',(0.95,.90,.73));green=material('Forest',(.13,.29,.21))
    red=material('Tomato',(.72,.19,.10));silver=material('Tin',(.68,.72,.68),.65)
    gold=material('Golden oat',(.79,.52,.15));blue=material('Dairy blue',(.25,.50,.65))
    if kind=='can':
        cylinder('Tomato soup', (0,0,.47),.40,.82,red)
        for z in [.055,.895]:cylinder('Rolled rim',(0,0,z),.42,.055,silver)
        cube('Paper label',(.56,.03,.46),(0,-.39,.48),cream,.03)
        text('SOUP',(0,-.414,.50),.14,green)
        text('TOMATO',(0,-.414,.35),.055,green)
        bpy.ops.mesh.primitive_torus_add(major_radius=.09,minor_radius=.019,location=(.07,0,.93));bpy.context.object.data.materials.append(silver)
    elif kind=='milk':
        cube('Milk carton',(.87,.86,1.53),(0,0,.80),cream)
        cube('Blue sleeve',(.88,.87,.70),(0,0,.63),blue,.035)
        roof=cube('Folded top',(.87,.86,.29),(0,0,1.68),blue,.07)
        cube('Top seam',(.78,.10,.11),(0,0,1.88),cream,.015)
        text('MILK',(0,-.447,.73),.19,cream);text('FARM FRESH',(0,-.447,.48),.063,cream)
    elif kind=='cereal':
        cube('Oat box',(.88,.86,1.90),(0,0,.97),gold)
        cube('Label',(.67,.02,1.13),(0,-.438,1.03),cream,.05)
        text('OATS',(0,-.455,1.23),.22,green);text('WHOLE GRAIN',(0,-.455,.95),.065,green)
        for x in [-.15,0,.15]:sphere('Oat grain',(x,-.46,.66),(.06,.025,.12),gold)
    elif kind in ['eggs','tray']:
        depth=2 if kind=='tray' else 1
        carton=material('Recycled carton',(.56,.65,.40))
        cube('Egg carton',(1.88,depth-.12,.28),(0,0,.18),carton,.10)
        for x in [-.57,0,.57]:
            for y in ([-.55,0,.55] if depth==2 else [-.23,.23]):
                sphere('Free range egg',(x,y,.48),(.22,.21,.31),cream)
        cube('Paper band',(.28,depth-.10,.045),(0,0,.82),gold,.02)
    elif kind=='bread':
        crust=material('Bread crust',(.66,.34,.12));score=material('Scored crust',(.91,.65,.34))
        cube('Sourdough loaf',(1.85,.84,.75),(0,0,.44),crust,.27)
        for x in [-.6,-.2,.2,.6]:
            slash=cube('Baked score',(.09,.56,.025),(x,0,.815),score,.025);slash.rotation_euler.z=-.3
    elif kind=='flowers':
        cylinder('Flower sleeve',(0,0,.65),.30,1.3,cream)
        for x,y,z in [(-.23,0,2.35),(.23,.12,2.55),(0,-.22,2.65)]:
            cylinder('Stem',(x,y,1.65),.035,1.6,green)
            sphere('Flower',(x,y,z),(.22,.22,.22),gold if x<0 else red)
    elif kind=='yogurt':
        cylinder('Yogurt cup',(0,0,.43),.40,.78,blue)
        cylinder('Foil lid',(0,0,.85),.44,.055,cream)
        text('YOGURT',(0,-.41,.47),.12,cream)
    elif kind=='peas':
        cube('Frozen pea pack',(1.88,.88,.88),(0,0,.46),green,.14)
        cube('Label',(1.35,.025,.53),(0,-.44,.48),cream,.03)
        text('PEAS',(0,-.465,.49),.25,green)
    elif kind=='bananas':
        # After Unity's facing rotation, the empty corner is +X,+Z.
        for x,y in [(-.5,-.5),(.5,-.5),(-.5,.5)]:
            cube('Bundle cell',(0.90,.90,.12),(x,y,.08),green,.04)
            for dx in [-.23,0,.23]:
                banana=sphere('Banana',(x+dx,y,.38),(.105,.39,.23),gold)
                banana.rotation_euler.x=.18
    else:
        wood=material('Crate wood',(.63,.44,.24));leaf=material('Apple leaf',(.29,.45,.13))
        cube('Crate floor',(.89,.89,.14),(0,0,.10),wood)
        for y in [-.42,.42]:cube('Crate slat',(.94,.075,.23),(0,y,.27),wood,.02)
        for x in [-.42,.42]:cube('Crate slat',(.075,.89,.23),(x,0,.27),wood,.02)
        for x in [-.22,.22]:
            for y in [-.22,.22]:
                sphere('Apple',(x,y,.48),(.24,.23,.24),red if x<0 else leaf)
                cube('Stem',(.035,.035,.12),(x,y,.76),wood,.01)
    refine(kind,globals())
    export_grain(kind,OUT/'Textures')
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE/(kind+'.blend')))
    bpy.ops.export_scene.fbx(filepath=str(OUT/(kind+'.fbx')),use_selection=True,object_types={'MESH'},
        apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_anim=False,add_leaf_bones=False)
print('Generated '+str(len(kinds))+' Blender grocery models and Unity FBX exports.')
