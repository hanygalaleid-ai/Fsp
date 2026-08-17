#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Fsp.EditorTools
{
    [InitializeOnLoad]
    public static class FspCompileAudit
    {
        private const string ReportPath = "Logs/FspCompileAudit.txt";
        private static readonly List<string> CurrentMessages = new();

        static FspCompileAudit()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        [MenuItem("Fsp/Project/Recompile and Audit")]
        public static void RecompileAndAudit()
        {
            CurrentMessages.Clear();
            CompilationPipeline.RequestScriptCompilation();
            Debug.Log("Fsp compile audit requested. Results will be written to " + ReportPath);
        }

        private static void OnCompilationStarted(object context)
        {
            CurrentMessages.Clear();
            CurrentMessages.Add("Fsp Unity compile audit");
            CurrentMessages.Add("UTC: " + DateTime.UtcNow.ToString("O"));
            CurrentMessages.Add(string.Empty);
        }

        private static void OnAssemblyFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (messages == null || messages.Length == 0) return;
            string assemblyName = Path.GetFileName(assemblyPath);
            foreach (CompilerMessage message in messages)
            {
                if (message.type != CompilerMessageType.Error && message.type != CompilerMessageType.Warning) continue;
                CurrentMessages.Add($"[{message.type}] {assemblyName} | {message.file}:{message.line}:{message.column} | {message.message}");
            }
        }

        private static void OnCompilationFinished(object context)
        {
            int errors = CurrentMessages.Count(x => x.StartsWith("[Error]", StringComparison.Ordinal));
            int warnings = CurrentMessages.Count(x => x.StartsWith("[Warning]", StringComparison.Ordinal));
            CurrentMessages.Add(string.Empty);
            CurrentMessages.Add($"Summary: {errors} error(s), {warnings} warning(s)");

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Logs");
            File.WriteAllLines(ReportPath, CurrentMessages);

            if (errors == 0)
                Debug.Log($"Fsp compile audit completed: 0 errors, {warnings} warning(s). Report: {ReportPath}");
            else
                Debug.LogError($"Fsp compile audit found {errors} error(s), {warnings} warning(s). Report: {ReportPath}");
        }
    }
}
#endif
