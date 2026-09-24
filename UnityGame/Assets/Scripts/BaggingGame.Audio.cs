using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        int ResultStars=>PackingRules.Stars(Level,bags);
        float WinRevealDelay=>ResultStars==3?3f:ResultStars==2?2.1f:1.5f;
        string ResultTitle=>ResultStars==3?"Perfectly packed!":ResultStars==2?"Checkout complete":"Room to improve";
        AudioSource musicOut,successOut;
        AudioClip pickupSound,turnSound,uiSound,paperSound,successSound;
        void InitializeAudio()
        {
            muted=PlayerPrefs.GetInt("bagging-muted",0)==1;
            audioOut=gameObject.AddComponent<AudioSource>();audioOut.playOnAwake=false;audioOut.volume=.58f;
            musicOut=gameObject.AddComponent<AudioSource>();musicOut.playOnAwake=false;musicOut.loop=true;musicOut.volume=0;
            successOut=gameObject.AddComponent<AudioSource>();successOut.playOnAwake=false;successOut.volume=.65f;
            placeSound=Resources.Load<AudioClip>("Audio/place");pickupSound=Resources.Load<AudioClip>("Audio/pickup");
            turnSound=Resources.Load<AudioClip>("Audio/turn");uiSound=Resources.Load<AudioClip>("Audio/ui");
            paperSound=Resources.Load<AudioClip>("Audio/paper");successSound=Resources.Load<AudioClip>("Audio/success");
            musicOut.clip=Resources.Load<AudioClip>("Audio/cozy-market-loop");
            if(Application.isPlaying&&musicOut.clip!=null)musicOut.Play();
            ApplyMute();
        }
        void PlaySoft(AudioClip clip){if(Application.isPlaying&&!muted&&audioOut!=null&&clip!=null)audioOut.PlayOneShot(clip);}
        void ToggleSound(){muted=!muted;PlayerPrefs.SetInt("bagging-muted",muted?1:0);PlayerPrefs.Save();ApplyMute();}
        void ApplyMute(){if(musicOut!=null)musicOut.mute=muted;if(audioOut!=null)audioOut.mute=muted;if(successOut!=null)successOut.mute=muted;}
        void UpdateCozyAudio()
        {
            if(musicOut==null)return;
            bool celebrating=complete&&Time.unscaledTime-completedAt<WinRevealDelay+.5f;
            musicOut.volume=Mathf.MoveTowards(musicOut.volume,celebrating?.07f:.23f,Time.unscaledDeltaTime*.12f);
            if(complete&&ResultStars==2&&successOut!=null&&successOut.isPlaying){
                float age=Time.unscaledTime-completedAt;
                successOut.volume=.38f*(1-Mathf.Clamp01((age-1.05f)/.3f));
                if(age>=1.35f)successOut.Stop();
            }
        }
        void BeginWinCelebration()
        {
            if(successOut==null||successSound==null||!Application.isPlaying)return;
            successOut.Stop();successOut.volume=ResultStars==3?.65f:.38f;
            if(ResultStars>=2)successOut.PlayOneShot(successSound);
        }
        void DrawWinCelebration(float w,float h)
        {
            float age=Time.unscaledTime-completedAt;
            float fade=Mathf.Clamp01((age-.2f)/.5f)*Mathf.Clamp01((WinRevealDelay-age)/.4f);
            Vector2 origin=new Vector2(w*.5f,h*.5f);
            int particles=ResultStars==3?32:ResultStars==2?8:0;
            for(int i=0;i<particles;i++){
                // Four matching particles share age, size and opacity, keeping the burst centered in both axes.
                int group=i/4,quadrant=i%4;
                float t=Mathf.Max(0,age-.28f-(group%4)*.085f);
                float angle=(group+.5f)/(particles/4f)*Mathf.PI*.5f;
                float expansion=1-Mathf.Exp(-t*2.2f);
                float x=Mathf.Cos(angle)*(166+expansion*70),y=Mathf.Sin(angle)*(42+expansion*95);
                Vector2 p=origin+new Vector2(quadrant%2==0?-x:x,quadrant<2?-y:y);
                float size=9+group%4*3;Color tint=group%3==0?Paper:group%3==1?Sun:Sage;tint.a=fade*Mathf.Clamp01(t*6);
                PrintTexture(new Rect(p.x-size*.5f,p.y-size*.5f,size,size),group%3==0?leafStamp:starStamp,tint,(quadrant%2==0?-1:1)*(angle*40+t*45));
            }
            var label=new Rect(origin.x-158,origin.y-28.5f,316,57);
            Color face=Paper;face.a=fade;PaintedTile(label,face);
            Color ink=Ink;ink.a=fade;
            Text(label,ResultTitle,27,ink,true,TextAnchor.MiddleCenter);
        }
    }
}
