

namespace Nitou.TCC.Foundation
{
    public readonly struct ProfilerScope : System.IDisposable{

        public ProfilerScope(string name) {
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        public void Dispose() {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}
