using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using KingsSafe.Probe;
public sealed class ProbeBuild : IPostBuildPlayerScriptDLLs, IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 100; } }
    public void OnPostBuildPlayerScriptDLLs(BuildReport report)
    {
        var own = CompilationPipeline.GetAssemblies(AssembliesType.Player)
            .Where(a => a.sourceFiles.Any(p => p.StartsWith("Assets/", StringComparison.Ordinal) && !p.StartsWith("Assets/KingsSafeProbe/", StringComparison.Ordinal)))
            .Select(a=>a.name+".dll").ToArray();
        var files = report.GetFiles().Select(f => f.path).Where(p => own.Contains(Path.GetFileName(p))).Distinct().ToArray();
        if (files.Length == 0) throw new BuildFailedException("PROBE: no staged own assemblies found");
        foreach (var path in files) Transform(path);
    }
    private static void Transform(string path)
    {
        // Unity's documented hook supplies staging copies, never patch Library/ScriptAssemblies.
        string full = Path.GetFullPath(path);
        string source = Path.GetFullPath("Library/ScriptAssemblies") + Path.DirectorySeparatorChar;
        if (full.StartsWith(source, StringComparison.Ordinal)) throw new BuildFailedException("PROBE: refusing source assembly");
        var resolver = new DefaultAssemblyResolver();
        resolver.AddSearchDirectory(Path.GetDirectoryName(full));
        resolver.AddSearchDirectory(Path.GetFullPath("Library/ScriptAssemblies"));
        using (var module = ModuleDefinition.ReadModule(full, new ReaderParameters { AssemblyResolver=resolver, InMemory=true }))
        {
            if (module.Types.Any(t=>t.Name=="__KingsSafeProbeApplied")) throw new BuildFailedException("PROBE: already transformed staging input");
            Debug.Log("PROBE_STAGING path="+full);
            foreach(var t in module.Types) Debug.Log("PROBE_TYPE name="+t.FullName+" base="+t.BaseType+" serializable="+t.IsSerializable+" fields="+String.Join(",",t.Fields.Select(f=>f.Name+":"+f.FieldType.FullName+":"+f.FieldType.MetadataType)));
            var methods = module.Types.SelectMany(t=>t.Methods).Where(m=>m.HasBody).ToArray();
            // C# emits ldflda for Int32.ToString(). Normalize this known readonly call
            // to a local copy; arbitrary ref/out or escaping addresses remain unsupported.
            foreach(var method in methods)
                foreach(var instruction in method.Body.Instructions.ToArray())
                {
                    var field=instruction.Operand as FieldReference;
                    var next=instruction.Next;
                    var call=next==null?null:next.Operand as MethodReference;
                    if(instruction.OpCode==OpCodes.Ldflda && field!=null && field.FieldType.MetadataType==MetadataType.Int32 &&
                       next!=null && next.OpCode==OpCodes.Call && call!=null && call.DeclaringType.FullName=="System.Int32" && call.Name=="ToString" && call.Parameters.Count==0)
                    {
                        var local=new VariableDefinition(module.TypeSystem.Int32);
                        method.Body.Variables.Add(local); method.Body.InitLocals=true;
                        instruction.OpCode=OpCodes.Ldfld;
                        var il=method.Body.GetILProcessor();
                        var store=il.Create(OpCodes.Stloc,local); il.InsertAfter(instruction,store);
                        il.InsertAfter(store,il.Create(OpCodes.Ldloca,local));
                    }
                }
            var fields = module.Types.Where(t=>t.IsClass && !t.IsSerializable && t.BaseType != null && t.BaseType.FullName=="System.Object")
                .SelectMany(t=>t.Fields).Where(f=>f.FieldType.MetadataType==MetadataType.Int32 && !f.IsStatic && !f.IsInitOnly && !f.IsLiteral)
                .Where(f=>!methods.Any(m=>m.Body.Instructions.Any(i=>(i.OpCode==OpCodes.Ldflda || i.OpCode==OpCodes.Ldsflda) && SameField(i.Operand,f)))).ToArray();
            int rewritten=0, redirects=0, renamed=0;
            foreach (var field in fields)
            {
                var owner = field.DeclaringType;
                var tag = new FieldDefinition("__ks_tag_"+field.Name, FieldAttributes.Private, module.TypeSystem.Int32);
                var init = new FieldDefinition("__ks_init_"+field.Name, FieldAttributes.Private, module.TypeSystem.Boolean);
                owner.Fields.Add(tag); owner.Fields.Add(init);
                var get = new MethodDefinition("__ks_get_"+field.Name, MethodAttributes.Assembly|MethodAttributes.HideBySig, module.TypeSystem.Int32);
                var set = new MethodDefinition("__ks_set_"+field.Name, MethodAttributes.Assembly|MethodAttributes.HideBySig, module.TypeSystem.Void);
                set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, module.TypeSystem.Int32));
                owner.Methods.Add(get); owner.Methods.Add(set);
                var il=get.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,field);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,tag);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,init);
                il.Emit(OpCodes.Call,module.ImportReference(typeof(ProbeRuntime).GetMethod("Decode"))); il.Emit(OpCodes.Ret);
                il=set.Body.GetILProcessor();
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call,module.ImportReference(typeof(ProbeRuntime).GetMethod("Encode"))); il.Emit(OpCodes.Stfld,field);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldfld,field); il.Emit(OpCodes.Not); il.Emit(OpCodes.Stfld,tag);
                il.Emit(OpCodes.Ldarg_0); il.Emit(OpCodes.Ldc_I4_1); il.Emit(OpCodes.Stfld,init); il.Emit(OpCodes.Ret);
                foreach (var method in methods)
                    foreach (var instruction in method.Body.Instructions)
                        if (SameField(instruction.Operand,field) && (instruction.OpCode==OpCodes.Ldfld || instruction.OpCode==OpCodes.Stfld))
                        { bool read=instruction.OpCode==OpCodes.Ldfld; instruction.OpCode=OpCodes.Call; instruction.Operand=read?get:set; rewritten++; }
            }
            foreach(var method in methods)
                foreach(var instruction in method.Body.Instructions)
                {
                    var target=instruction.Operand as MethodReference;
                    if(target!=null && target.DeclaringType.FullName=="UnityEngine.AssetBundle" && target.Name=="LoadFromFile" && target.Parameters.Count==1)
                    { instruction.Operand=module.ImportReference(typeof(ProbeRuntime).GetMethod("LoadBundle")); redirects++; }
                }
            // Name-only mechanism, not a full logic-obfuscation engine. Keep Unity callbacks untouched.
            foreach(var type in module.Types.Where(t=>t.BaseType!=null && t.BaseType.FullName=="System.Object"))
                foreach(var method in type.Methods.Where(m=>m.IsPrivate && !m.IsConstructor && !m.IsSpecialName && !m.Name.StartsWith("__ks_")))
                { method.Name="_ks_method_"+(renamed++); }
            module.Types.Add(new TypeDefinition("", "__KingsSafeProbeApplied",TypeAttributes.NotPublic|TypeAttributes.Class,module.TypeSystem.Object));
            module.Write(full);
            Debug.Log("PROBE_TRANSFORM assembly="+Path.GetFileName(path)+" fields="+fields.Length+" accesses="+rewritten+" resourceCalls="+redirects+" privateMethods="+renamed);
        }
        resolver.Dispose();
    }
    private static bool SameField(object value,FieldDefinition field)
    {
        var reference=value as FieldReference;
        return reference!=null && reference.FullName==field.FullName;
    }
    public void OnPostprocessBuild(BuildReport report)
    {
        string folder=Path.Combine(report.summary.outputPath,"Contents/Resources/Data/StreamingAssets");
        int count=0;
        foreach(var path in Directory.GetFiles(folder,"*.bundle"))
        {
            var plain=File.ReadAllBytes(path);
            var wrapped=ProbeRuntime.Wrap(plain);
            if(!plain.SequenceEqual(ProbeRuntime.Unwrap(wrapped))) throw new BuildFailedException("PROBE: resource roundtrip failed");
            File.WriteAllBytes(path,wrapped); count++;
        }
        if(count==0) throw new BuildFailedException("PROBE: no bundles protected");
        Debug.Log("PROBE_RESOURCE_PASS bundles="+count);
        // Local Mac research fixture only: output edits invalidate Unity's ad-hoc seal.
        // Re-seal automatically inside this plugin, never change the original build script.
        // Real release signing/notarization remains a separate unverified integration.
        var sign=new System.Diagnostics.Process();
        sign.StartInfo=new System.Diagnostics.ProcessStartInfo("/usr/bin/codesign", "--force --deep --sign - \""+report.summary.outputPath.Replace("\"","\\\"")+"\"") {
            UseShellExecute=false, RedirectStandardError=true, RedirectStandardOutput=true
        };
        sign.Start();
        string error=sign.StandardError.ReadToEnd();
        sign.WaitForExit();
        if(sign.ExitCode!=0) throw new BuildFailedException("PROBE: local ad-hoc signing failed: "+error);
        sign.Dispose();
        Debug.Log("PROBE_ADHOC_SIGN_PASS");
    }
}
