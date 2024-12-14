using System;
using System.Linq;
using UnityEngine;

namespace TapSDK.Core.Internal {
    public static class PlatformTypeUtils {
        /// <summary>
        /// 创建平台接口实现类对象
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="startWith"></param>
        /// <returns></returns>
        public static object CreatePlatformImplementationObject(Type interfaceType, string startWith) {

            // 获取所有符合条件的程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().FullName.StartsWith(startWith));

            // 获取符合条件的类型
            var platformSupportTypes = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(clazz => interfaceType.IsAssignableFrom(clazz) && clazz.IsClass);

            // 返回对应平台的实现
            // Unity Editor处于Android/iOS时使用Standalone进行Mock
            #if UNITY_EDITOR || UNITY_STANDALONE
            var platformSupportType = platformSupportTypes.SingleOrDefault(clazz => clazz.Name.Contains(Platform.STANDALONE));
            #elif UNITY_ANDROID || UNITY_IOS
            var platformSupportType = platformSupportTypes.SingleOrDefault(clazz => clazz.Name.Contains(Platform.MOBILE));
            #endif
            
            if (platformSupportType != null) {
                try
                {
                    return Activator.CreateInstance(platformSupportType);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to create instance of {platformSupportType.FullName}: {ex}");
                }
            } else {
                Debug.LogError($"No type found that implements {interfaceType} in assemblies starting with {startWith}");
            }

            return null;
        }
    }
}