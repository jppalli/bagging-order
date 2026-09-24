using System;
using System.IO;
using UnityEditor;

// Unity 6000.0.39f1's decompression-fallback loader requests a nonexistent
// workerUrl in single-threaded builds. Keep the request conditional.
public static class WebLoaderFix
{
    [MenuItem("Bagging Order/Repair existing web loader")]
    public static void Apply()
    {
        const string marker="/* optional-worker-fix */";
        const string request="T(\"workerUrl\").then(function(e){var t=URL.createObjectURL(new Blob([e],{type:\"application/javascript\"}));b.compressedWorkerUrl=b.workerUrl,b.workerUrl=t,b.deinitializers.push(function(){URL.revokeObjectURL(t)})})";
        foreach(string path in Directory.GetFiles("Builds/WebGL/Build","*.loader.js")){
            string source=File.ReadAllText(path);
            if(source.Contains(marker))continue;
            if(!source.Contains(request))throw new InvalidOperationException("Web loader changed: review the optional-worker workaround before publishing.");
            source=source.Replace(request,"(b.workerUrl?"+request+":Promise.resolve())"+marker);
            File.WriteAllText(path,source);
        }
    }
}
