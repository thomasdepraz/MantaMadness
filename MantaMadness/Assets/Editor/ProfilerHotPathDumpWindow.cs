using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;

public class ProfilerHotPathDumpWindow : EditorWindow
{
    const int SCHEMA_VERSION = 1;
    const int MIN_DEPTH = 1;
    const int MAX_DEPTH = 32;
    const int DEFAULT_MAX_DEPTH = 8;
    const int SUMMARY_LIMIT = 20;
    const int CONFIRM_FRAME_COUNT = 90;
    const int MAX_NODE_COUNT = 250000;
    const int MAX_THREAD_SCAN = 256;
    const float DEFAULT_MIN_TOTAL_TIME_MS = 0.1f;
    const string MAIN_THREAD_NAME = "Main Thread";
    const string ANALYSIS_HINT =
        "Start with summary.threads hottestBySelfTime, then primaryHotPath. " +
        "Times are milliseconds. gcAllocBytes is memory allocated by that sample only, excluding its children. " +
        "average*Ms divides by summary.frameCount (every dumped frame). presentInFrameCount is how many frames contained the sample. " +
        "Hierarchy children are ordered by totalTimeMs descending. Samples below minTotalTimeMs are omitted; their total time is summed on the parent as omittedChildrenTotalTimeMs and those omitted samples still contribute to the summary. " +
        "The summary does not include descendants cut by maxDepth. truncatedByDepth means the sample still has children that were not written. " +
        "hotPath is the single chain from the thread root that always follows the child with the highest totalTimeMs, ignoring minTotalTimeMs, until maxDepth. " +
        "profilerWindow describes the Profiler window at dump time and may differ from the settings used to record a loaded capture.";

    [SerializeField] int _startFrame;
    [SerializeField] int _endFrame;
    [SerializeField] int _maxDepth = DEFAULT_MAX_DEPTH;
    [SerializeField] float _minTotalTimeMs = DEFAULT_MIN_TOTAL_TIME_MS;
    [SerializeField] bool _mainThreadOnly = true;
    [SerializeField] bool _hideEditorOnlySamples = true;
    [SerializeField] bool _mergeSamplesWithSameName;
    [SerializeField] bool _rangeInitialized;

    [MenuItem("Tools/Profiler Hot Path Dump")]
    public static void Open()
    {
        GetWindow<ProfilerHotPathDumpWindow>("Profiler Hot Path");
    }

    void OnEnable()
    {
        minSize = new Vector2(380f, 360f);
        EditorApplication.update += OnEditorUpdate;
        ApplyAvailableRange(false);
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    void OnEditorUpdate()
    {
        if (ProfilerDriver.enabled)
            Repaint();
    }

    void OnGUI()
    {
        int availableStart = ProfilerDriver.firstFrameIndex;
        int availableEnd = ProfilerDriver.lastFrameIndex;
        bool hasRecord = availableStart >= 0 && availableEnd >= availableStart;

        EditorGUILayout.LabelField("Profiler Hot Path", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Writes the Profiler window's current record to JSON for an agent to analyze. Pick the frame range and how deep the call hierarchy goes.",
            MessageType.Info);

        if (hasRecord == false)
        {
            EditorGUILayout.HelpBox(
                "No Profiler data is loaded. Record with the Profiler window, or open a saved capture, then click Refresh.",
                MessageType.Warning);
        }
        else
        {
            int availableCount = availableEnd - availableStart + 1;
            EditorGUILayout.LabelField("Current Record", availableStart + " – " + availableEnd + " (" + availableCount + " frames)");
            EditorGUILayout.LabelField("Recording", ProfilerDriver.enabled ? "Yes" : "No");

            if (_rangeInitialized == false)
            {
                _startFrame = availableStart;
                _endFrame = availableEnd;
                _rangeInitialized = true;
            }
        }

        if (GUILayout.Button("Refresh Full Range"))
            ApplyAvailableRange(true);

        EditorGUILayout.Space();
        _startFrame = EditorGUILayout.IntField("Start Frame", _startFrame);
        _endFrame = EditorGUILayout.IntField("End Frame", _endFrame);
        _maxDepth = EditorGUILayout.IntSlider(
            new GUIContent("Max Depth", "Root is depth 0. Raise this for Deep Profile captures."),
            _maxDepth,
            MIN_DEPTH,
            MAX_DEPTH);
        _minTotalTimeMs = EditorGUILayout.FloatField(
            new GUIContent("Min Total Time (ms)", "Hierarchy samples under this total time are omitted. 0 keeps every sample up to Max Depth. The hot path ignores this cutoff."),
            _minTotalTimeMs);
        if (_minTotalTimeMs < 0f)
            _minTotalTimeMs = 0f;

        _mainThreadOnly = EditorGUILayout.Toggle(
            new GUIContent("Main Thread Only", "Off includes every thread in the current record."),
            _mainThreadOnly);
        _hideEditorOnlySamples = EditorGUILayout.Toggle(
            new GUIContent("Hide Editor-Only Samples", "Turn off when the capture was recorded in the Editor."),
            _hideEditorOnlySamples);
        _mergeSamplesWithSameName = EditorGUILayout.Toggle(
            new GUIContent("Merge Same Names", "On matches the Profiler Hierarchy view. Off keeps the raw call tree."),
            _mergeSamplesWithSameName);

        int requestedStart = Mathf.Min(_startFrame, _endFrame);
        int requestedEnd = Mathf.Max(_startFrame, _endFrame);
        int selectedCount = 0;
        if (hasRecord)
        {
            int dumpStart = Mathf.Max(requestedStart, availableStart);
            int dumpEnd = Mathf.Min(requestedEnd, availableEnd);
            if (dumpEnd >= dumpStart)
                selectedCount = dumpEnd - dumpStart + 1;
        }

        EditorGUILayout.LabelField("Frames To Dump", selectedCount.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(hasRecord == false || selectedCount == 0))
        {
            if (GUILayout.Button("Dump JSON"))
                Dump();
        }
    }

    void ApplyAvailableRange(bool forceFullRange)
    {
        int availableStart = ProfilerDriver.firstFrameIndex;
        int availableEnd = ProfilerDriver.lastFrameIndex;
        bool hasRecord = availableStart >= 0 && availableEnd >= availableStart;
        if (hasRecord == false)
            return;

        bool outside = _endFrame < availableStart || _startFrame > availableEnd;
        if (forceFullRange || _rangeInitialized == false || outside)
        {
            _startFrame = availableStart;
            _endFrame = availableEnd;
            _rangeInitialized = true;
        }
    }

    void Dump()
    {
        int availableStart = ProfilerDriver.firstFrameIndex;
        int availableEnd = ProfilerDriver.lastFrameIndex;
        bool hasRecord = availableStart >= 0 && availableEnd >= availableStart;
        if (hasRecord == false)
        {
            EditorUtility.DisplayDialog("Profiler Hot Path", "The Profiler has no current record.", "OK");
            return;
        }

        int requestedStart = Mathf.Min(_startFrame, _endFrame);
        int requestedEnd = Mathf.Max(_startFrame, _endFrame);
        _startFrame = requestedStart;
        _endFrame = requestedEnd;

        int dumpStart = Mathf.Max(requestedStart, availableStart);
        int dumpEnd = Mathf.Min(requestedEnd, availableEnd);
        if (dumpStart > dumpEnd)
        {
            EditorUtility.DisplayDialog("Profiler Hot Path", "The selected frames are outside the current Profiler record.", "OK");
            return;
        }

        int frameCount = dumpEnd - dumpStart + 1;
        if (frameCount > CONFIRM_FRAME_COUNT)
        {
            bool continueDump = EditorUtility.DisplayDialog(
                "Profiler Hot Path",
                "Dump " + frameCount + " frames at depth " + _maxDepth + "? The JSON can be large for an agent to read.",
                "Dump",
                "Cancel");
            if (continueDump == false)
                return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string path = EditorUtility.SaveFilePanel("Dump Profiler Hot Path", projectRoot, "profiler-hot-path", "json");
        if (string.IsNullOrEmpty(path))
            return;

        HierarchyFrameDataView.ViewModes viewMode = HierarchyFrameDataView.ViewModes.Default;
        if (_hideEditorOnlySamples)
            viewMode |= HierarchyFrameDataView.ViewModes.HideEditorOnlySamples;
        if (_mergeSamplesWithSameName)
            viewMode |= HierarchyFrameDataView.ViewModes.MergeSamplesWithTheSameName;

        try
        {
            DumpCapture capture = DumpCapture.Read(
                dumpStart,
                dumpEnd,
                availableStart,
                availableEnd,
                _maxDepth,
                _minTotalTimeMs,
                _mainThreadOnly,
                viewMode);

            if (capture.Frames.Count == 0)
            {
                EditorUtility.DisplayDialog("Profiler Hot Path", "No frames were read from the current Profiler record.", "OK");
                return;
            }

            string json = HotPathJson.Write(
                capture,
                _mainThreadOnly,
                _hideEditorOnlySamples,
                _mergeSamplesWithSameName);
            File.WriteAllText(path, json, new UTF8Encoding(false));
            ImportIfInsideAssets(path);

            EditorUtility.DisplayDialog(
                "Profiler Hot Path",
                "Wrote " + capture.Frames.Count + " frames (" + capture.NodeCount + " hierarchy samples) to\n" + path,
                "OK");
        }
        catch (DumpLimitException exception)
        {
            EditorUtility.DisplayDialog("Profiler Hot Path", exception.Message, "OK");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Profiler Hot Path", exception.Message, "OK");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static void ImportIfInsideAssets(string path)
    {
        string assetsFullPath = Path.GetFullPath(Application.dataPath);
        string fullPath = Path.GetFullPath(path);
        string assetsPrefix = assetsFullPath + Path.DirectorySeparatorChar;
        bool insideAssets = string.Equals(fullPath, assetsFullPath, StringComparison.OrdinalIgnoreCase)
            || fullPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase);
        if (insideAssets)
            AssetDatabase.Refresh();
    }

    sealed class DumpLimitException : Exception
    {
        public DumpLimitException(string message) : base(message)
        {
        }
    }

    sealed class SampleStats
    {
        public string Name;
        public double TotalTimeMsSum;
        public double SelfTimeMsSum;
        public double MaxTotalTimeMs;
        public double MaxSelfTimeMs;
        public int TotalCalls;
        public long TotalGcAllocBytes;
        public int PresentInFrameCount;
        public int OccurrenceCount;
    }

    sealed class SampleNode
    {
        public string Name;
        public int MarkerId;
        public string Target;
        public string Path;
        public int Depth;
        public double TotalTimeMs;
        public double SelfTimeMs;
        public double TotalPercentOfFrame;
        public double SelfPercentOfFrame;
        public int Calls;
        public long GcAllocBytes;
        public int ChildCount;
        public int OmittedChildCount;
        public double OmittedChildrenTotalTimeMs;
        public bool TruncatedByDepth;
        public bool WriteChildren;
        public List<SampleNode> Children;
    }

    sealed class CapturedThread
    {
        public int ThreadIndex;
        public string Name;
        public string GroupName;
        public List<SampleNode> HotPath;
        public SampleNode Hierarchy;
    }

    sealed class CapturedFrame
    {
        public int Index;
        public double FrameTimeMs;
        public double GpuTimeMs;
        public List<CapturedThread> Threads = new List<CapturedThread>();
    }

    sealed class DumpCapture
    {
        public int RequestedStart;
        public int RequestedEnd;
        public int AvailableStart;
        public int AvailableEnd;
        public int MaxDepth;
        public float MinTotalTimeMs;
        public List<CapturedFrame> Frames = new List<CapturedFrame>();
        public int NodeCount;
        public Dictionary<string, Dictionary<string, SampleStats>> StatsByThread =
            new Dictionary<string, Dictionary<string, SampleStats>>(StringComparer.Ordinal);

        public static DumpCapture Read(
            int dumpStart,
            int dumpEnd,
            int availableStart,
            int availableEnd,
            int maxDepth,
            float minTotalTimeMs,
            bool mainThreadOnly,
            HierarchyFrameDataView.ViewModes viewMode)
        {
            var capture = new DumpCapture();
            capture.RequestedStart = dumpStart;
            capture.RequestedEnd = dumpEnd;
            capture.AvailableStart = availableStart;
            capture.AvailableEnd = availableEnd;
            capture.MaxDepth = maxDepth;
            capture.MinTotalTimeMs = minTotalTimeMs;

            var reader = new FrameReader(capture, maxDepth, minTotalTimeMs, mainThreadOnly, viewMode);
            int frameCount = dumpEnd - dumpStart + 1;
            for (int frameIndex = dumpStart; frameIndex <= dumpEnd; frameIndex++)
            {
                float progress = frameCount <= 1 ? 1f : (float)(frameIndex - dumpStart) / frameCount;
                bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                    "Profiler Hot Path",
                    "Reading frame " + frameIndex,
                    progress);
                if (cancelled)
                    throw new DumpLimitException("Dump cancelled.");

                capture.Frames.Add(reader.ReadFrame(frameIndex));
            }

            return capture;
        }
    }

    sealed class FrameReader
    {
        readonly DumpCapture _capture;
        readonly int _maxDepth;
        readonly float _minTotalTimeMs;
        readonly bool _mainThreadOnly;
        readonly HierarchyFrameDataView.ViewModes _viewMode;
        readonly List<int> _childIds = new List<int>(64);

        public FrameReader(
            DumpCapture capture,
            int maxDepth,
            float minTotalTimeMs,
            bool mainThreadOnly,
            HierarchyFrameDataView.ViewModes viewMode)
        {
            _capture = capture;
            _maxDepth = maxDepth;
            _minTotalTimeMs = minTotalTimeMs;
            _mainThreadOnly = mainThreadOnly;
            _viewMode = viewMode;
        }

        public CapturedFrame ReadFrame(int frameIndex)
        {
            var frame = new CapturedFrame();
            frame.Index = frameIndex;
            bool frameTimeSet = false;

            for (int threadIndex = 0; threadIndex < MAX_THREAD_SCAN; threadIndex++)
            {
                using (HierarchyFrameDataView view = ProfilerDriver.GetHierarchyFrameDataView(
                    frameIndex,
                    threadIndex,
                    _viewMode,
                    HierarchyFrameDataView.columnTotalTime,
                    false))
                {
                    if (view == null || view.valid == false)
                        break;

                    if (frameTimeSet == false)
                    {
                        frame.FrameTimeMs = view.frameTimeMs;
                        frame.GpuTimeMs = view.frameGpuTimeMs;
                        frameTimeSet = true;
                    }

                    string threadName = string.IsNullOrEmpty(view.threadName) ? "(unnamed thread)" : view.threadName;
                    bool isMainThread = string.Equals(threadName, MAIN_THREAD_NAME, StringComparison.Ordinal);
                    if (_mainThreadOnly && isMainThread == false)
                        continue;

                    int rootId = view.GetRootItemID();
                    if (rootId == HierarchyFrameDataView.invalidSampleId)
                        continue;

                    var seen = new HashSet<string>(StringComparer.Ordinal);
                    Dictionary<string, SampleStats> threadStats = GetThreadStats(threadName);

                    var thread = new CapturedThread();
                    thread.ThreadIndex = threadIndex;
                    thread.Name = threadName;
                    thread.GroupName = view.threadGroupName;
                    thread.HotPath = ReadHotPath(view, rootId, frame.FrameTimeMs);
                    thread.Hierarchy = ReadNode(view, rootId, 0, frame.FrameTimeMs, true, true, threadStats, seen);
                    frame.Threads.Add(thread);
                }
            }

            return frame;
        }

        Dictionary<string, SampleStats> GetThreadStats(string threadName)
        {
            Dictionary<string, SampleStats> threadStats;
            if (_capture.StatsByThread.TryGetValue(threadName, out threadStats))
                return threadStats;

            threadStats = new Dictionary<string, SampleStats>(StringComparer.Ordinal);
            _capture.StatsByThread.Add(threadName, threadStats);
            return threadStats;
        }

        List<SampleNode> ReadHotPath(HierarchyFrameDataView view, int rootId, double frameTimeMs)
        {
            var path = new List<SampleNode>();
            int itemId = rootId;
            int depth = 0;
            while (itemId != HierarchyFrameDataView.invalidSampleId && depth <= _maxDepth)
            {
                SampleNode node = ReadNode(view, itemId, depth, frameTimeMs, false, false, null, null);
                node.Path = view.GetItemPath(itemId);
                path.Add(node);

                if (depth >= _maxDepth)
                    break;

                view.GetItemChildren(itemId, _childIds);
                if (_childIds.Count == 0)
                    break;

                itemId = FindHottestChild(view);
                depth++;
            }

            return path;
        }

        int FindHottestChild(HierarchyFrameDataView view)
        {
            int hottestId = _childIds[0];
            double hottestTotal = view.GetItemColumnDataAsSingle(hottestId, HierarchyFrameDataView.columnTotalTime);
            for (int i = 1; i < _childIds.Count; i++)
            {
                int childId = _childIds[i];
                double totalTimeMs = view.GetItemColumnDataAsSingle(childId, HierarchyFrameDataView.columnTotalTime);
                if (totalTimeMs > hottestTotal)
                {
                    hottestTotal = totalTimeMs;
                    hottestId = childId;
                }
            }

            return hottestId;
        }

        SampleNode ReadNode(
            HierarchyFrameDataView view,
            int itemId,
            int depth,
            double frameTimeMs,
            bool expandChildren,
            bool recordStats,
            Dictionary<string, SampleStats> threadStats,
            HashSet<string> seen)
        {
            if (recordStats)
            {
                _capture.NodeCount++;
                if (_capture.NodeCount > MAX_NODE_COUNT)
                {
                    throw new DumpLimitException(
                        "Dump stopped after " + MAX_NODE_COUNT + " samples. Narrow the frame range, lower Max Depth, or raise Min Total Time.");
                }
            }

            string name;
            int markerId;
            string target;
            double totalTimeMs;
            double selfTimeMs;
            int calls;
            long gcAllocBytes;
            ReadScalars(view, itemId, out name, out markerId, out target, out totalTimeMs, out selfTimeMs, out calls, out gcAllocBytes);
            if (recordStats)
                AddStats(threadStats, seen, name, totalTimeMs, selfTimeMs, calls, gcAllocBytes);

            var sample = new SampleNode();
            sample.Name = name;
            sample.MarkerId = markerId;
            sample.Target = target;
            sample.Depth = depth;
            sample.TotalTimeMs = totalTimeMs;
            sample.SelfTimeMs = selfTimeMs;
            sample.Calls = calls;
            sample.GcAllocBytes = gcAllocBytes;
            if (frameTimeMs > 0d)
            {
                sample.TotalPercentOfFrame = totalTimeMs / frameTimeMs * 100d;
                sample.SelfPercentOfFrame = selfTimeMs / frameTimeMs * 100d;
            }

            bool hasChildren = view.HasItemChildren(itemId);
            if (hasChildren == false)
            {
                sample.WriteChildren = expandChildren;
                if (expandChildren)
                    sample.Children = new List<SampleNode>();
                return sample;
            }

            view.GetItemChildren(itemId, _childIds);
            sample.ChildCount = _childIds.Count;
            bool canExpand = expandChildren && depth < _maxDepth;
            if (canExpand == false)
            {
                if (depth >= _maxDepth)
                    sample.TruncatedByDepth = true;
                sample.WriteChildren = expandChildren;
                if (expandChildren)
                    sample.Children = new List<SampleNode>();
                return sample;
            }

            int childCount = _childIds.Count;
            var childIds = new int[childCount];
            var childTotals = new double[childCount];
            for (int i = 0; i < childCount; i++)
            {
                childIds[i] = _childIds[i];
                childTotals[i] = view.GetItemColumnDataAsSingle(childIds[i], HierarchyFrameDataView.columnTotalTime);
            }

            if (childCount > 1)
            {
                Array.Sort(childTotals, childIds);
                Array.Reverse(childIds);
                Array.Reverse(childTotals);
            }

            sample.WriteChildren = true;
            sample.Children = new List<SampleNode>(childCount);
            for (int i = 0; i < childCount; i++)
            {
                int childId = childIds[i];
                double childTotalTimeMs = childTotals[i];
                if (childTotalTimeMs < _minTotalTimeMs)
                {
                    string childName;
                    int childMarkerId;
                    string childTarget;
                    double childTotal;
                    double childSelf;
                    int childCalls;
                    long childGc;
                    ReadScalars(view, childId, out childName, out childMarkerId, out childTarget, out childTotal, out childSelf, out childCalls, out childGc);
                    AddStats(threadStats, seen, childName, childTotal, childSelf, childCalls, childGc);
                    sample.OmittedChildCount++;
                    sample.OmittedChildrenTotalTimeMs += childTotal;
                    continue;
                }

                sample.Children.Add(ReadNode(view, childId, depth + 1, frameTimeMs, true, true, threadStats, seen));
            }

            return sample;
        }

        static void ReadScalars(
            HierarchyFrameDataView view,
            int itemId,
            out string name,
            out int markerId,
            out string target,
            out double totalTimeMs,
            out double selfTimeMs,
            out int calls,
            out long gcAllocBytes)
        {
            string itemName = view.GetItemName(itemId);
            name = string.IsNullOrEmpty(itemName) ? "(unnamed)" : itemName;
            markerId = view.GetItemMarkerID(itemId);
            target = view.GetItemColumnData(itemId, HierarchyFrameDataView.columnObjectName);
            if (target == null)
                target = string.Empty;

            totalTimeMs = view.GetItemColumnDataAsSingle(itemId, HierarchyFrameDataView.columnTotalTime);
            selfTimeMs = view.GetItemColumnDataAsSingle(itemId, HierarchyFrameDataView.columnSelfTime);
            calls = Mathf.RoundToInt(view.GetItemColumnDataAsSingle(itemId, HierarchyFrameDataView.columnCalls));

            double gcValue = view.GetItemColumnDataAsDouble(itemId, HierarchyFrameDataView.columnGcMemory);
            if (gcValue < 0d)
                gcValue = 0d;
            gcAllocBytes = (long)Math.Round(gcValue);
        }

        static void AddStats(
            Dictionary<string, SampleStats> threadStats,
            HashSet<string> seen,
            string name,
            double totalTimeMs,
            double selfTimeMs,
            int calls,
            long gcAllocBytes)
        {
            SampleStats stats;
            if (threadStats.TryGetValue(name, out stats) == false)
            {
                stats = new SampleStats();
                stats.Name = name;
                threadStats.Add(name, stats);
            }

            stats.TotalTimeMsSum += totalTimeMs;
            stats.SelfTimeMsSum += selfTimeMs;
            if (totalTimeMs > stats.MaxTotalTimeMs)
                stats.MaxTotalTimeMs = totalTimeMs;
            if (selfTimeMs > stats.MaxSelfTimeMs)
                stats.MaxSelfTimeMs = selfTimeMs;
            stats.TotalCalls += calls;
            stats.TotalGcAllocBytes += gcAllocBytes;
            stats.OccurrenceCount++;
            if (seen.Add(name))
                stats.PresentInFrameCount++;
        }
    }

    static class HotPathJson
    {
        public static string Write(DumpCapture capture, bool mainThreadOnly, bool hideEditorOnlySamples, bool mergeSamplesWithSameName)
        {
            var builder = new StringBuilder(1024 * 256);
            var writer = new JsonWriter(builder);

            int frameCount = capture.Frames.Count;
            double frameTimeSum = 0d;
            double gpuTimeSum = 0d;
            double minFrameTimeMs = 0d;
            double maxFrameTimeMs = 0d;
            int slowestFrameIndex = 0;
            int fastestFrameIndex = 0;
            CapturedFrame slowestFrame = null;
            if (frameCount > 0)
            {
                minFrameTimeMs = capture.Frames[0].FrameTimeMs;
                maxFrameTimeMs = capture.Frames[0].FrameTimeMs;
                slowestFrameIndex = capture.Frames[0].Index;
                fastestFrameIndex = capture.Frames[0].Index;
                slowestFrame = capture.Frames[0];
            }

            for (int i = 0; i < frameCount; i++)
            {
                CapturedFrame frame = capture.Frames[i];
                frameTimeSum += frame.FrameTimeMs;
                gpuTimeSum += frame.GpuTimeMs;
                if (frame.FrameTimeMs > maxFrameTimeMs)
                {
                    maxFrameTimeMs = frame.FrameTimeMs;
                    slowestFrameIndex = frame.Index;
                    slowestFrame = frame;
                }

                if (frame.FrameTimeMs < minFrameTimeMs)
                {
                    minFrameTimeMs = frame.FrameTimeMs;
                    fastestFrameIndex = frame.Index;
                }
            }

            writer.BeginObject();
            writer.Property("schemaVersion", SCHEMA_VERSION);
            writer.Property("purpose", "Profiler hot-path dump for performance analysis");
            writer.Property("analysisHint", ANALYSIS_HINT);
            writer.Property("unityVersion", Application.unityVersion);
            writer.Property("capturedAtUtc", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));

            writer.BeginObject("profilerWindow");
            writer.Property("recording", ProfilerDriver.enabled);
            writer.Property("deepProfiling", ProfilerDriver.deepProfiling);
            writer.Property("profileEditor", ProfilerDriver.profileEditor);
            writer.Property("note", "Profiler window settings at dump time. A loaded capture may have been recorded with different settings.");
            writer.EndObject();

            writer.BeginObject("units");
            writer.Property("time", "milliseconds");
            writer.Property("gcAlloc", "bytes allocated by the sample itself, excluding children");
            writer.Property("calls", "call count represented by the sample");
            writer.Property("percentOfFrame", "percent of that frame's frameTimeMs");
            writer.Property("averages", "average*Ms divides by summary.frameCount");
            writer.EndObject();

            writer.BeginObject("options");
            writer.Property("startFrame", capture.RequestedStart);
            writer.Property("endFrame", capture.RequestedEnd);
            writer.Property("availableStart", capture.AvailableStart);
            writer.Property("availableEnd", capture.AvailableEnd);
            writer.Property("maxDepth", capture.MaxDepth);
            writer.Property("minTotalTimeMs", capture.MinTotalTimeMs);
            writer.Property("mainThreadOnly", mainThreadOnly);
            writer.Property("hideEditorOnlySamples", hideEditorOnlySamples);
            writer.Property("mergeSamplesWithSameName", mergeSamplesWithSameName);
            writer.EndObject();

            writer.BeginObject("summary");
            writer.Property("frameCount", frameCount);
            writer.Property("averageFrameTimeMs", frameCount > 0 ? frameTimeSum / frameCount : 0d);
            writer.Property("minFrameTimeMs", minFrameTimeMs);
            writer.Property("maxFrameTimeMs", maxFrameTimeMs);
            writer.Property("slowestFrameIndex", slowestFrameIndex);
            writer.Property("fastestFrameIndex", fastestFrameIndex);
            writer.Property("averageGpuTimeMs", frameCount > 0 ? gpuTimeSum / frameCount : 0d);
            WriteThreadSummaries(writer, capture.StatsByThread, frameCount);
            writer.EndObject();

            WritePrimaryHotPath(writer, slowestFrame);
            WriteFrames(writer, capture.Frames);
            writer.EndObject();
            builder.Append('\n');
            return builder.ToString();
        }

        static void WriteThreadSummaries(JsonWriter writer, Dictionary<string, Dictionary<string, SampleStats>> statsByThread, int frameCount)
        {
            writer.BeginArray("threads");
            foreach (KeyValuePair<string, Dictionary<string, SampleStats>> pair in statsByThread)
            {
                var stats = new List<SampleStats>(pair.Value.Count);
                foreach (KeyValuePair<string, SampleStats> samplePair in pair.Value)
                    stats.Add(samplePair.Value);

                writer.BeginObject();
                writer.Property("name", pair.Key);
                WriteRankedSamples(writer, "hottestBySelfTime", stats, frameCount, CompareBySelfTime);
                WriteRankedSamples(writer, "hottestByTotalTime", stats, frameCount, CompareByTotalTime);
                WriteRankedSamples(writer, "hottestByGcAlloc", stats, frameCount, CompareByGcAlloc);
                writer.EndObject();
            }

            writer.EndArray();
        }

        static void WriteRankedSamples(
            JsonWriter writer,
            string propertyName,
            List<SampleStats> stats,
            int frameCount,
            Comparison<SampleStats> comparison)
        {
            stats.Sort(comparison);
            int count = stats.Count;
            if (count > SUMMARY_LIMIT)
                count = SUMMARY_LIMIT;

            writer.BeginArray(propertyName);
            double divisor = frameCount > 0 ? frameCount : 1d;
            for (int i = 0; i < count; i++)
            {
                SampleStats sample = stats[i];
                writer.BeginObject();
                writer.Property("name", sample.Name);
                writer.Property("selfTimeMsSum", sample.SelfTimeMsSum);
                writer.Property("averageSelfTimeMs", sample.SelfTimeMsSum / divisor);
                writer.Property("maxSelfTimeMs", sample.MaxSelfTimeMs);
                writer.Property("totalTimeMsSum", sample.TotalTimeMsSum);
                writer.Property("averageTotalTimeMs", sample.TotalTimeMsSum / divisor);
                writer.Property("maxTotalTimeMs", sample.MaxTotalTimeMs);
                writer.Property("totalCalls", sample.TotalCalls);
                writer.Property("totalGcAllocBytes", sample.TotalGcAllocBytes);
                writer.Property("presentInFrameCount", sample.PresentInFrameCount);
                writer.Property("occurrenceCount", sample.OccurrenceCount);
                writer.EndObject();
            }

            writer.EndArray();
        }

        static int CompareBySelfTime(SampleStats a, SampleStats b)
        {
            int bySelf = b.SelfTimeMsSum.CompareTo(a.SelfTimeMsSum);
            if (bySelf != 0)
                return bySelf;
            return string.CompareOrdinal(a.Name, b.Name);
        }

        static int CompareByTotalTime(SampleStats a, SampleStats b)
        {
            int byTotal = b.TotalTimeMsSum.CompareTo(a.TotalTimeMsSum);
            if (byTotal != 0)
                return byTotal;
            return string.CompareOrdinal(a.Name, b.Name);
        }

        static int CompareByGcAlloc(SampleStats a, SampleStats b)
        {
            int byGc = b.TotalGcAllocBytes.CompareTo(a.TotalGcAllocBytes);
            if (byGc != 0)
                return byGc;
            return string.CompareOrdinal(a.Name, b.Name);
        }

        static void WritePrimaryHotPath(JsonWriter writer, CapturedFrame frame)
        {
            writer.BeginObject("primaryHotPath");
            if (frame == null)
            {
                writer.Property("found", false);
                writer.EndObject();
                return;
            }

            CapturedThread thread = null;
            for (int i = 0; i < frame.Threads.Count; i++)
            {
                if (string.Equals(frame.Threads[i].Name, MAIN_THREAD_NAME, StringComparison.Ordinal))
                {
                    thread = frame.Threads[i];
                    break;
                }
            }

            if (thread == null && frame.Threads.Count > 0)
                thread = frame.Threads[0];

            writer.Property("found", thread != null);
            writer.Property("frameIndex", frame.Index);
            writer.Property("frameTimeMs", frame.FrameTimeMs);
            writer.Property("gpuTimeMs", frame.GpuTimeMs);
            writer.Property("thread", thread == null ? string.Empty : thread.Name);
            writer.BeginArray("samples");
            if (thread != null)
            {
                for (int i = 0; i < thread.HotPath.Count; i++)
                    WriteSample(writer, thread.HotPath[i]);
            }

            writer.EndArray();
            writer.EndObject();
        }

        static void WriteFrames(JsonWriter writer, List<CapturedFrame> frames)
        {
            writer.BeginArray("frames");
            for (int i = 0; i < frames.Count; i++)
            {
                CapturedFrame frame = frames[i];
                writer.BeginObject();
                writer.Property("index", frame.Index);
                writer.Property("frameTimeMs", frame.FrameTimeMs);
                writer.Property("gpuTimeMs", frame.GpuTimeMs);
                writer.BeginArray("threads");
                for (int threadIndex = 0; threadIndex < frame.Threads.Count; threadIndex++)
                {
                    CapturedThread thread = frame.Threads[threadIndex];
                    writer.BeginObject();
                    writer.Property("name", thread.Name);
                    writer.Property("group", thread.GroupName);
                    writer.Property("threadIndex", thread.ThreadIndex);
                    writer.BeginArray("hotPath");
                    for (int sampleIndex = 0; sampleIndex < thread.HotPath.Count; sampleIndex++)
                        WriteSample(writer, thread.HotPath[sampleIndex]);
                    writer.EndArray();
                    WriteSampleProperty(writer, "hierarchy", thread.Hierarchy);
                    writer.EndObject();
                }

                writer.EndArray();
                writer.EndObject();
            }

            writer.EndArray();
        }

        static void WriteSample(JsonWriter writer, SampleNode sample)
        {
            writer.BeginObject();
            WriteSampleBody(writer, sample);
            writer.EndObject();
        }

        static void WriteSampleProperty(JsonWriter writer, string propertyName, SampleNode sample)
        {
            writer.BeginObject(propertyName);
            WriteSampleBody(writer, sample);
            writer.EndObject();
        }

        static void WriteSampleBody(JsonWriter writer, SampleNode sample)
        {
            writer.Property("name", sample.Name);
            writer.Property("markerId", sample.MarkerId);
            writer.Property("target", sample.Target);
            if (sample.Path != null)
                writer.Property("path", sample.Path);
            writer.Property("depth", sample.Depth);
            writer.Property("totalTimeMs", sample.TotalTimeMs);
            writer.Property("selfTimeMs", sample.SelfTimeMs);
            writer.Property("totalPercentOfFrame", sample.TotalPercentOfFrame);
            writer.Property("selfPercentOfFrame", sample.SelfPercentOfFrame);
            writer.Property("calls", sample.Calls);
            writer.Property("gcAllocBytes", sample.GcAllocBytes);
            writer.Property("childCount", sample.ChildCount);
            writer.Property("omittedChildCount", sample.OmittedChildCount);
            writer.Property("omittedChildrenTotalTimeMs", sample.OmittedChildrenTotalTimeMs);
            writer.Property("truncatedByDepth", sample.TruncatedByDepth);
            if (sample.WriteChildren)
            {
                writer.BeginArray("children");
                if (sample.Children != null)
                {
                    for (int i = 0; i < sample.Children.Count; i++)
                        WriteSample(writer, sample.Children[i]);
                }

                writer.EndArray();
            }
        }
    }

    sealed class JsonWriter
    {
        readonly StringBuilder _builder;
        readonly List<bool> _first = new List<bool>(16);
        int _indent;

        public JsonWriter(StringBuilder builder)
        {
            _builder = builder;
        }

        public void BeginObject()
        {
            WriteValuePrefix();
            if (_first.Count > 0)
                WriteIndent();
            _builder.Append('{');
            _indent++;
            _first.Add(true);
        }

        public void BeginObject(string propertyName)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": {");
            _indent++;
            _first.Add(true);
        }

        public void EndObject()
        {
            bool empty = _first[_first.Count - 1];
            _first.RemoveAt(_first.Count - 1);
            _indent--;
            if (empty == false)
                WriteIndent();

            _builder.Append('}');
        }

        public void BeginArray(string propertyName)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": [");
            _indent++;
            _first.Add(true);
        }

        public void EndArray()
        {
            bool empty = _first[_first.Count - 1];
            _first.RemoveAt(_first.Count - 1);
            _indent--;
            if (empty == false)
                WriteIndent();

            _builder.Append(']');
        }

        public void Property(string propertyName, string value)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": ");
            AppendEscaped(value);
        }

        public void Property(string propertyName, int value)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": ");
            _builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        public void Property(string propertyName, long value)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": ");
            _builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        public void Property(string propertyName, double value)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": ");
            AppendNumber(value);
        }

        public void Property(string propertyName, float value)
        {
            Property(propertyName, (double)value);
        }

        public void Property(string propertyName, bool value)
        {
            WriteValuePrefix();
            WriteIndent();
            AppendEscaped(propertyName);
            _builder.Append(": ");
            _builder.Append(value ? "true" : "false");
        }

        void WriteValuePrefix()
        {
            if (_first.Count == 0)
                return;

            int last = _first.Count - 1;
            if (_first[last] == false)
                _builder.Append(',');
            else
                _first[last] = false;
        }

        void WriteIndent()
        {
            _builder.Append('\n');
            for (int i = 0; i < _indent; i++)
                _builder.Append("  ");
        }

        void AppendNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                _builder.Append('0');
                return;
            }

            _builder.Append(value.ToString("0.######", CultureInfo.InvariantCulture));
        }

        void AppendEscaped(string value)
        {
            _builder.Append('"');
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    char character = value[i];
                    switch (character)
                    {
                        case '"':
                            _builder.Append("\\\"");
                            break;
                        case '\\':
                            _builder.Append("\\\\");
                            break;
                        case '\n':
                            _builder.Append("\\n");
                            break;
                        case '\r':
                            _builder.Append("\\r");
                            break;
                        case '\t':
                            _builder.Append("\\t");
                            break;
                        default:
                            if (character < ' ')
                            {
                                _builder.Append("\\u");
                                _builder.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                _builder.Append(character);
                            }

                            break;
                    }
                }
            }

            _builder.Append('"');
        }
    }
}
