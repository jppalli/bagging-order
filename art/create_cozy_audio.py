"""Original synthesized score and foley. No samples or third-party compositions."""
from pathlib import Path
import wave
import sys
import numpy as np

OUT = Path(__file__).resolve().parents[1] / 'UnityGame/Assets/Resources/Audio'
OUT.mkdir(parents=True, exist_ok=True)
SR = 32000
rng = np.random.default_rng(42)

def tone(midi, seconds=2.4, soft=False):
    t = np.arange(int(seconds*SR))/SR
    f = 440*2**((midi-69)/12)
    signal = sum(a*np.sin(2*np.pi*f*h*t)*np.exp(-t*(1.2+h*.5))
                 for h,a in [(1,1),(2,.23),(3,.065),(4,.018)])
    return signal*(1-np.exp(-t/(.028 if soft else .008)))

def rustle(seconds):
    t = np.arange(int(seconds*SR))/SR
    noise = np.convolve(rng.normal(0,1,len(t)),np.ones(25)/25,'same')
    return noise*np.sin(np.pi*t/seconds)**2*np.exp(-t*5)

def save(name, data, peak=.6):
    if '--effects-only' in sys.argv and name not in ('pickup','turn','ui','place'):
        return
    data = data/max(1e-6,np.max(np.abs(data)))*peak
    with wave.open(str(OUT/(name+'.wav')),'wb') as w:
        w.setnchannels(1 if data.ndim==1 else data.shape[1]); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((data*32767).astype('<i2').tobytes())

beat=60/76
music=np.zeros((round(64*beat*SR),2))
def put(buffer, sound, at, volume, pan=0):
    indices=(np.arange(len(sound))+round(at*SR))%len(buffer)
    if buffer.ndim==1: np.add.at(buffer,indices,sound*volume)
    else:
        np.add.at(buffer[:,0],indices,sound*volume*np.sqrt((1-pan)/2))
        np.add.at(buffer[:,1],indices,sound*volume*np.sqrt((1+pan)/2))

chords=[[48,55,59,64],[45,52,55,60],[41,48,52,57],[43,50,57,60]]
melodies=[[76,74,71,67],[72,71,69,64],[69,72,76,74],[74,72,69,67]]
for bar in range(16):
    chord=chords[(bar//2)%4]
    for j,note in enumerate(chord):
        put(music,tone(note,3.8,True),bar*4*beat+j*.045,.12,(j-1.5)*.23)
    for j in range(4):
        put(music,tone(chord[j]+12,1.8), (bar*4+j+.5)*beat,.055,(-1)**j*.4)
    for j in range(2):
        note=melodies[(bar//2)%4][(bar%2)*2+j]
        put(music,tone(note,2.8,True),(bar*4+j*1.75+.25)*beat,.10,.12)
# Circular room reflections preserve the loop's tails at the seam.
dry=music.copy()
for delay,gain in [(.13,.15),(.29,.10),(.47,.07)]:
    music+=np.roll(dry,round(delay*SR),axis=0)[:,::-1]*gain
save('cozy-market-loop',music,.52)
def cushioned_foley(duration, seed, body, brush):
    """Unpitched cloth/paper contact, with a rounded attack and no ringing note."""
    local_rng=np.random.default_rng(seed)
    t=np.arange(round(duration*SR))/SR
    noise=local_rng.normal(0,1,len(t))
    # Two soft noise bands: a low cushion and a faint paper surface.
    low=np.convolve(noise,np.hanning(181)/np.hanning(181).sum(),'same')
    surface=np.convolve(noise,np.hanning(31)/np.hanning(31).sum(),'same')
    attack=1-np.exp(-t/.012)
    tail=np.clip((duration-t)/.045,0,1)**2
    return (body*low*np.exp(-t/ .047)+brush*surface*np.exp(-t/.08))*attack*tail

for name,duration,seed,body,brush,peak in [
    ('ui',.105,201,1,.12,.10),
    ('place',.23,202,1.5,.18,.19),
    ('pickup',.19,203,.4,.40,.12),
    ('turn',.12,204,.65,.17,.10),
]:
    save(name,cushioned_foley(duration,seed,body,brush),peak)
save('paper',rustle(.55),.27)
success=np.zeros(int(3.1*SR))
for at,note in [(.12,60),(.48,64),(.86,67),(1.28,72),(1.8,76)]:
    sound=tone(note,min(1.25,3.1-at),True)
    put(success,sound,at,.26)
success*=np.minimum(1,np.arange(len(success))[::-1]/(SR*.25))
save('success',success,.48)
print('Created interaction effects' if '--effects-only' in sys.argv else 'Created music and effects', 'in', OUT)
