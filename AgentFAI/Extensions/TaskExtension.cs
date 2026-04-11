using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace AgentFAI.Extensions;

public static class CoroutineTaskExtension
{
    extension(IEnumerator coroutine)
    {
        /// <summary>
        /// 将协程包装为 Task。
        /// </summary>
        public Task ToTask()
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            CoroutineRunner.Instance.Run(coroutine.Wrap(tcs));
            return tcs.Task;
        }

        /// <summary>
        /// 将协程包装为 Task&lt;T&gt;，通过 CoroutineResult&lt;T&gt; 回传结果。
        /// </summary>
        public Task<T> ToTask<T>(CoroutineResult<T> result)
        {
            var tcs = new TaskCompletionSource<T>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            CoroutineRunner.Instance.Run(coroutine.WrapWithResult(result, tcs));
            return tcs.Task;
        }

        // 私有包装器：逐帧驱动，捕获异常
        private IEnumerator Wrap(TaskCompletionSource<bool> tcs)
        {
            while (true)
            {
                object current;
                try
                {
                    if (!coroutine.MoveNext()) break;
                    current = coroutine.Current;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    yield break;
                }
                yield return current;
            }
            tcs.SetResult(true);
        }

        private IEnumerator WrapWithResult<T>(
            CoroutineResult<T> result,
            TaskCompletionSource<T> tcs)
        {
            while (true)
            {
                object current;
                try
                {
                    if (!coroutine.MoveNext()) break;
                    current = coroutine.Current;
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                    yield break;
                }
                yield return current;
            }

            if (result.HasValue)
                tcs.SetResult(result.Value);
            else
                tcs.SetException(new InvalidOperationException(
                    "Coroutine finished without setting a result."));
        }
    }

    extension(AsyncOperation op)
    {
        /// <summary>
        /// 将 AsyncOperation 包装为 Task。
        /// </summary>
        public Task ToTask()
        {
            var tcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            CoroutineRunner.Instance.Run(op.Wait(tcs));
            return tcs.Task;
        }

        private IEnumerator Wait(TaskCompletionSource<bool> tcs)
        {
            yield return op;
            tcs.SetResult(true);
        }
    }
    
}
public sealed class CoroutineResult<T>
{
    public T Value { get; private set; }
    public bool HasValue { get; private set; }

    public void Set(T value)
    {
        Value = value;
        HasValue = true;
    }
}
internal sealed class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            // 必须在主线程调用，通常在首次使用时由主线程触发
            var go = new GameObject("[CoroutineRunner]") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<CoroutineRunner>();
            return _instance;
        }
    }

    public void Run(IEnumerator coroutine) => StartCoroutine(coroutine);

    private void OnDestroy() => _instance = null;
}