"""Blender-only sculptural details and exportable material grain for the groceries."""
import bpy, math, random
from mathutils import Vector

def tube(name, points, radii, mat, sides=10):
    verts=[];faces=[]
    for j,point in enumerate(points):
        p=Vector(point);t=Vector(points[min(j+1,len(points)-1)])-Vector(points[max(0,j-1)])
        t.normalize();a=t.cross(Vector((0,0,1)))
        if a.length<.01:a=t.cross(Vector((0,1,0)))
        a.normalize();b=t.cross(a).normalized()
        for i in range(sides):
            angle=i*math.tau/sides;v=p+radii[j]*(a*math.cos(angle)+b*math.sin(angle));verts.append(v)
    for j in range(len(points)-1):
        for i in range(sides):faces.append((j*sides+i,j*sides+(i+1)%sides,(j+1)*sides+(i+1)%sides,(j+1)*sides+i))
    faces.extend([tuple(reversed(range(sides))),tuple((len(points)-1)*sides+i for i in range(sides))])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(verts,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
    for f in mesh.polygons:f.use_smooth=True
    return o

def lathe(name, profile, mat, at=(0,0,0), segments=40):
    vertices=[];faces=[]
    for radius,z in profile:
        for i in range(segments):
            a=i*math.tau/segments;vertices.append((at[0]+radius*math.cos(a),at[1]+radius*math.sin(a),at[2]+z))
    for j in range(len(profile)-1):
        for i in range(segments):faces.append((j*segments+i,j*segments+(i+1)%segments,(j+1)*segments+(i+1)%segments,(j+1)*segments+i))
    faces.extend([tuple(reversed(range(segments))),tuple((len(profile)-1)*segments+i for i in range(segments))])
    mesh=bpy.data.meshes.new(name);mesh.from_pydata(vertices,[],faces);mesh.update();o=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
    for f in mesh.polygons:f.use_smooth=True
    return o

def refine(kind,h):
    cube,sphere,cylinder,text,material=[h[k] for k in ['cube','sphere','cylinder','text','material']]
    cream,green,red,silver,gold,blue=[h[k] for k in ['cream','green','red','silver','gold','blue']]
    dark=material('Printed forest',(.055,.15,.085));brown=material('Warm kraft',(.63,.43,.23))
    if kind in ['milk','can','eggs','tray','bread','bananas','flowers','yogurt']:
        bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
    if kind=='milk':
        cube('Cream paperboard',(.86,.82,1.36),(0,0,.71),cream,.045)
        cube('Blue printed sleeve',(.868,.828,.73),(0,0,.65),blue,.028)
        # Real folded gable, instead of a rectangular block on top.
        vertices=[(-.43,-.41,1.38),(.43,-.41,1.38),(.43,.41,1.38),(-.43,.41,1.38),(-.43,0,1.83),(.43,0,1.83)]
        mesh=bpy.data.meshes.new('Gable');mesh.from_pydata(vertices,[],[(0,1,5,4),(3,4,5,2),(0,4,3),(1,2,5)]);mesh.update()
        o=bpy.data.objects.new('Folded gable roof',mesh);bpy.context.collection.objects.link(o);o.data.materials.append(cream)
        cube('Crimped seam',(.87,.075,.07),(0,0,1.85),blue,.013)
        cap=cylinder('Screw cap',(.20,-.22,1.65),.10,.09,blue);cap.rotation_euler.x=.68
        text('MILK',(0,-.421,.76),.235,cream);text('FARM FRESH',(0,-.423,.51),.067,cream)
        text('WHOLE',(0,-.419,1.17),.085,dark)
        cube('Bottom fold',(.77,.025,.035),(0,-.414,.12),brown,.008)
    elif kind=='can':
        lathe('Tin body',[(.35,.045),(.40,.09),(.40,.85),(.37,.90)],silver)
        lathe('Printed paper wrap',[(.405,.19),(.405,.76)],cream)
        for z in [.085,.125,.81,.86]:lathe('Rolled tin bead',[(.392,z-.01),(.413,z),(.392,z+.014)],silver)
        cylinder('Recessed lid',(0,0,.904),.368,.025,silver)
        for r in [.28,.32]:
            bpy.ops.mesh.primitive_torus_add(major_radius=r,minor_radius=.008,major_segments=40,minor_segments=6,location=(0,0,.922));bpy.context.object.data.materials.append(silver)
        bpy.ops.mesh.primitive_torus_add(major_radius=.082,minor_radius=.018,major_segments=24,minor_segments=6,location=(.02,0,.937));bpy.context.object.scale.y=1.4;bpy.context.object.data.materials.append(silver)
        text('SOUP',(0,-.412,.60),.18,dark);text('TOMATO',(0,-.414,.43),.077,red)
        sphere('Tomato illustration',(0,-.407,.28),(.11,.015,.065),red)
        for x in [-.035,.035]:sphere('Printed leaf',(x,-.423,.335),(.045,.009,.02),green)
    elif kind in ['eggs','tray']:
        depth=1 if kind=='eggs' else 2;carton=material('Molded paper pulp',(.69,.62,.44))
        cube('Molded carton base',(1.88,depth-.12,.34),(0,0,.20),carton,.14)
        for x in [-.61,0,.61]:
            for y in ([-.23,.23] if depth==1 else [-.57,0,.57]):
                lathe('Egg pocket',[(.19,.12),(.25,.23),(.24,.40)],carton,(x,y,0),24)
                egg=sphere('Warm shell egg',(x,y,.51),(.205,.205,.29),cream)
                # Taper egg crown while keeping the familiar rounded base.
                for v in egg.data.vertices:
                    f=1-.12*max(0,v.co.z);v.co.x*=f;v.co.y*=f
        cube('Pulp carton rim',(1.89,depth-.10,.075),(0,0,.37),carton,.028)
        cube('Paper label band',(.52,depth-.08,.035),(0,0,.80),cream,.012)
        text('FARM EGGS',(0,-depth*.5+.028,.235),.115,dark)
        for x in [-.61,.61]:cube('Carton latch',(.15,.065,.13),(x,-depth*.5+.015,.30),carton,.035)
    elif kind=='bread':
        crust=material('Toasted sourdough crust',(.60,.29,.075));flour=material('Golden scored crumb',(.93,.72,.40))
        loaf=sphere('Hand-shaped loaf',(0,0,.43),(.94,.43,.42),crust)
        for v in loaf.data.vertices:
            v.co.z=max(v.co.z,-.78);v.co*=1+.018*math.sin(v.co.x*17+v.co.y*9)
        for x in [-.57,-.19,.19,.57]:
            points=[(x+.13*t,-.28+.56*t,.73+.08*math.sin(t*math.pi)) for t in [i/12 for i in range(13)]]
            tube('Opened crust score',points,[.025+.022*math.sin(i/12*math.pi) for i in range(13)],flour,8)
        rng=random.Random(8)
        for i in range(26):
            x=rng.uniform(-.72,.72);y=rng.uniform(-.24,.24);z=.43+.40*math.sqrt(max(0,1-(x/.94)**2-(y/.43)**2))
            sphere('Flour fleck',(x,y,z),(.018,.014,.004),flour)
    elif kind=='bananas':
        peel=material('Sun-ripened peel',(.98,.73,.075));tip=material('Dried banana ends',(.25,.14,.035))
        stem=material('Fresh green crown',(.40,.47,.12))
        # A single fan of four curved fingers, joined at one crown. No grid-cell trays.
        for finger in range(4):
            spread=(finger-1.5)*.19
            points=[];radii=[]
            for i in range(33):
                t=i/32
                points.append((-.82+1.62*t,spread*math.sin(t*math.pi*.8)+.18*math.sin(t*math.pi),.26+.40*math.sin(t*math.pi)+finger*.045))
                radii.append(.025+.095*math.sin(t*math.pi)**.55)
            tube('Curved banana finger',points,radii,peel,16)
            sphere('Blossom tip',points[-1],(.043,.038,.035),tip)
            tube('Joined green neck',[(-.94,0,.36),(-.87,spread*.15,.31+finger*.035),points[0]],[.055,.04,.035],stem,12)
        cube('Shared crown',(.17,.18,.16),(-.96,0,.40),stem,.05)
        cube('Crown cut',(.025,.15,.12),(-1.055,0,.42),tip,.02)
    elif kind=='cornflakes':
        sunny=material('Cornflakes sunshine',(.96,.62,.10));orange=material('Cornflakes orange',(.87,.29,.055))
        cube('Family cereal carton',(1.88,.86,2.86),(0,0,1.46),sunny,.065)
        cube('Cream front label',(1.64,.026,2.32),(0,-.445,1.50),cream,.09)
        cube('Red title panel',(1.60,.035,.77),(0,-.465,2.12),red,.055)
        text('CORN',(0,-.489,2.29),.32,cream);text('FLAKES',(0,-.489,1.95),.27,cream)
        text('FAMILY SIZE',(0,-.468,2.66),.115,green)
        # Raised illustration of a milk bowl and golden flakes, readable at game scale.
        sphere('Printed breakfast bowl',(0,-.48,.80),(.63,.04,.28),blue)
        sphere('Milk in bowl',(0,-.526,.98),(.60,.025,.10),cream)
        rng=random.Random(12)
        for i in range(19):
            x=rng.uniform(-.48,.48);z=rng.uniform(.98,1.28)
            flake=sphere('Golden cornflake',(x,-.56,z),(.075,.018,.046),sunny if i%3 else orange);flake.rotation_euler.y=rng.uniform(-.7,.7)
        text('GOLDEN & CRUNCHY',(0,-.48,.43),.10,green)
        cube('Folded top flap',(1.76,.78,.045),(0,0,2.905),cream,.018)
        cube('Carton side seam',(.025,.025,2.62),(.90,-.435,1.45),orange,.008)
    elif kind=='rice':
        cube('Sturdy rice box',(.9,.86,.9),(0,0,.46),cream,.055)
        cube('Forest paper label',(.91,.022,.59),(0,-.436,.46),green,.016)
        text('RICE',(0,-.456,.55),.21,cream);text('PANTRY',(0,-.458,.34),.08,gold)
        for x in [-.27,-.14,0,.14,.27]:sphere('Printed rice grain',(x,-.463,.20),(.035,.009,.018),cream)
        cube('Folded box lid',(.84,.8,.045),(0,0,.93),gold,.02)
    elif kind=='flowers':
        lathe('Wrapped kraft bouquet',[(.16,.03),(.22,.4),(.35,1.22),(.30,1.36)],brown)
        for i,(x,y,z) in enumerate([(-.22,.08,2.36),(.22,.12,2.55),(0,-.20,2.69),(.05,.24,2.25)]):
            tube('Green stem',[(0,0,.5),(x*.7,y*.7,1.6),(x,y,z)],[.028,.028,.022],green,8)
            for j in range(7):
                a=j*math.tau/7;sphere('Flower petal',(x+.14*math.cos(a),y+.14*math.sin(a),z),(.115,.105,.075),gold if i%2==0 else cream)
            sphere('Flower center',(x,y,z+.045),(.085,.085,.07),red if i%2 else brown)
            leaf=sphere('Bouquet leaf',(x-.07,y,1.65),(.16,.045,.29),green);leaf.rotation_euler.y=-.6
        cube('Bouquet ribbon',(.47,.045,.13),(0,-.25,.66),red,.04)
    elif kind=='yogurt':
        lathe('Tapered yogurt pot',[(.28,.04),(.29,.12),(.39,.72),(.40,.77)],cream)
        lathe('Blue dairy sleeve',[(.318,.27),(.382,.65)],blue)
        lathe('Rolled rim',[(.39,.74),(.425,.77),(.425,.80),(.39,.82)],cream)
        cylinder('Foil top',(0,0,.825),.405,.025,silver)
        cube('Foil pull tab',(.14,.16,.018),(.27,-.26,.83),silver,.04)
        text('YOGURT',(0,-.368,.49),.105,cream)
        sphere('Berry illustration',(0,-.334,.31),(.07,.01,.045),red)
    elif kind=='cereal':
        text('HONEY',(0,-.463,1.61),.09,dark)
        bowl=lathe('Breakfast bowl',[(.12,.02),(.23,.12),(.25,.17)],cream)
        bowl.location=(0,-.37,.38);bowl.scale=(1,.38,1)
        for x in [-.15,-.05,.05,.15]:sphere('Breakfast oats',(x,-.43,.57),(.04,.025,.027),brown)
        cube('Folded box seam',(.72,.025,.025),(0,-.434,1.90),cream,.006)
    elif kind=='peas':
        for x in [-.85,.85]:cube('Crimped pouch seam',(.075,.85,.72),(x,0,.46),gold,.02)
        for x in [-.35,-.16,.03,.22,.41]:sphere('Pea illustration',(x,-.473,.23),(.072,.02,.067),green)
        text('GARDEN FRESH',(0,-.468,.73),.08,green)
    elif kind=='apple':
        for o in list(bpy.context.scene.objects):
            if o.name.startswith('Apple') and o.type=='MESH':
                for v in o.data.vertices:
                    v.co.z-=.16*math.exp(-(v.co.x*v.co.x+v.co.y*v.co.y)*16)*max(v.co.z,0)
                leaf=sphere('Fresh apple leaf',(o.location.x+.07,o.location.y,.76),(.095,.035,.025),green);leaf.rotation_euler.z=.5
        for x in [-.42,.42]:
            for y in [-.36,.36]:sphere('Crate nail',(x,y,.35),(.018,.018,.018),silver)

def export_grain(kind, directory):
    """Use real image textures: Blender procedural nodes alone do not survive FBX."""
    directory.mkdir(parents=True,exist_ok=True)
    used={m for o in bpy.context.scene.objects if o.type=='MESH' for m in o.data.materials if m}
    for n,m in enumerate(sorted(used,key=lambda m:m.name)):
        if any(s in m.name.lower() for s in ['tin','silver','foil','printed forest']):continue
        color=m.diffuse_color;size=64;image=bpy.data.images.new(kind+'_'+str(n),width=size,height=size)
        rng=random.Random(37+n);pixels=[]
        for y in range(size):
            for x in range(size):
                noise=.95+rng.random()*.08+.015*math.sin(x*.7+y*.3)
                pixels.extend([min(1,color[0]*noise),min(1,color[1]*noise),min(1,color[2]*noise),1])
        image.pixels=pixels;image.filepath_raw=str(directory/(kind+'_'+str(n)+'.png'));image.file_format='PNG';image.save()
        node=m.node_tree.nodes.new('ShaderNodeTexImage');node.image=image
        # FBX multiplies diffuse tint by the texture: keep the tint neutral to avoid squaring colors in Unity.
        m.diffuse_color=(1,1,1,1);bsdf=m.node_tree.nodes.get('Principled BSDF');bsdf.inputs['Base Color'].default_value=(1,1,1,1)
        m.node_tree.links.new(node.outputs['Color'],bsdf.inputs['Base Color'])
    for o in bpy.context.scene.objects:
        if o.type=='MESH' and not o.data.uv_layers:
            uv=o.data.uv_layers.new(name='Grain UV')
            for loop in o.data.loops:
                v=o.data.vertices[loop.vertex_index].co;uv.data[loop.index].uv=(v.x*2+v.y,v.z*2+v.y)
