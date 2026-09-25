using System;
using UnityEngine;

namespace BaggingOrder
{
    public partial class BaggingGame
    {
        const string ResumeLevelKey="bagging-resume-level-name";
        bool confirmProgressReset;
        int LoadProgressLevel()
        {
            string name=PlayerPrefs.GetString(ResumeLevelKey,"");
            int saved=Array.FindIndex(PackingRules.Levels,c=>c.Name==name);
            if(saved>=0)return saved;
            if(!PlayerPrefs.HasKey("checkout-curriculum-v2"))return 0;
            // Preserve the same checkout across the first-discoveries reorder.
            int old=Mathf.Clamp(PlayerPrefs.GetInt("checkout-curriculum-v2"),0,20);
            return old>=15?old-15:old+6;
        }
        void SaveProgressLevel(int index)
        {
            PlayerPrefs.SetString(ResumeLevelKey,PackingRules.Levels[index].Name);PlayerPrefs.Save();
        }
        void ResetProgress()
        {
            for(int i=0;i<PackingRules.Levels.Length;i++){PlayerPrefs.DeleteKey(BestKey(i));PlayerPrefs.DeleteKey(VisitKey(i));}
            PlayerPrefs.DeleteKey("checkout-curriculum-v2");PlayerPrefs.DeleteKey(ResumeLevelKey);
            PlayerPrefs.Save();confirmProgressReset=false;level=0;StartLevel();OpenWorldMap();
        }
    }
}
