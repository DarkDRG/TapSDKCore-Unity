using System;
using System.Linq;
using UnityEngine;

namespace TapSDK.Core.Internal.Utils {
    public static class BridgeUtils {
        public static bool IsSupportMobilePlatform => Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer;

        public static bool IsSupportStandalonePlatform => Application.platform == RuntimePlatform.OSXPlayer ||
            Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.LinuxPlayer;

        public static object CreateBridgeImplementation(Type interfaceType, string startWith) {
            // 跳过初始化直接使用 TapLoom会在子线程被TapSDK.Core.BridgeCallback.Invoke 初始化
            TapLoom.Initialize();
            var bridgeImplementationTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asssembly => asssembly.GetName().FullName.StartsWith(startWith))
                .SelectMany(assembly => assembly.GetTypes())
                .Where(clazz => interfaceType.IsAssignableFrom(clazz) && clazz.IsClass);
            
            // 返回对应平台的实现
            // Unity Editor处于Android/iOS时使用Standalone进行Mock
#if UNITY_EDITOR || UNITY_STANDALONE
            var bridgeImplementationType = bridgeImplementationTypes.SingleOrDefault(clazz => clazz.Name.Contains(Platform.STANDALONE));
#elif UNITY_ANDROID || UNITY_IOS
            var bridgeImplementationType = bridgeImplementationTypes.SingleOrDefault(clazz => clazz.Name.Contains(Platform.MOBILE));
#endif
            if (bridgeImplementationType == null){
                Debug.LogWarningFormat(
                    $"[TapTap] TapSDK Can't find bridge implementation for {interfaceType} on platform {Application.platform}.");
                return null;
            }
            return Activator.CreateInstance(bridgeImplementationType);
        }
    }
}
