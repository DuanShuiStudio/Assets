#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#endif
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个类用于自动安装MMFeedbacks中使用的可选依赖项
    /// </summary>
    public static class MMFDependencyInstaller 
	{
		#if UNITY_EDITOR
		static ListRequest _listRequest;
		static AddRequest _addRequest;
		static int _currentIndex;
        
		private static string[] _packages = new string[] 
		{"com.unity.cinemachine", 
			"com.unity.postprocessing", 
			"com.unity.textmeshpro",
			"com.unity.2d.animation"
		};

        /// <summary>
        /// 安装 _packages 中列出的所有依赖项
        /// </summary>
        [MenuItem("Tools/More Mountains/MMFeedbacks/Install All Dependencies", false, 703)]
		public static void InstallAllDependencies()
		{
			_currentIndex = 0;
			_listRequest = null;
			_addRequest = null;
            
			MMDebug.DebugLogInfo("[MMFDependencyInstaller] 开始安装");
			_listRequest = Client.List();    
            
			EditorApplication.update += ListProgress;
		}

        /// <summary>
        /// 装所有依赖项，当从正在运行的应用程序中调用时使用此方法
        /// </summary>
        public async static void InstallFromPlay()
		{
			EditorApplication.ExitPlaymode();
			while (Application.isPlaying)
			{
				await Task.Delay(500);
			}
			await Task.Delay(500);
            
			ClearConsole();
            
			await Task.Delay(500);
			InstallAllDependencies();
		}

        /// <summary>
        /// 清空控制台。为什么这不是一个内置的一行代码呢？谁知道呢
        /// </summary>
        public static void ClearConsole()
		{
			var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
			if (logEntries != null)
			{
				var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
				if (clearMethod != null)
				{
					clearMethod.Invoke(null, null);    
				}
			}
		}

        /// <summary>
        /// 接着安装下一行中的下一个软件包
        /// </summary>
        static void InstallNext()
		{
			if (_currentIndex < _packages.Length)
			{
				bool packageFound = false;
				foreach (var package in _listRequest.Result)
				{
					if (package.name == _packages[_currentIndex])
					{
						packageFound = true;
						MMDebug.DebugLogInfo("[MMFDependencyInstaller] "+package.name+ " 已经安装了");
						_currentIndex++;
						InstallNext();
						return;
					} 
				}

				if (!packageFound)
				{
					MMDebug.DebugLogInfo("[MMFDependencyInstaller] 安装中 "+_packages[_currentIndex]);
					_addRequest = Client.Add(_packages[_currentIndex]);
					EditorApplication.update += AddProgress;
				}
			}
			else
			{
				MMDebug.DebugLogInfo("[MMFDependencyInstaller] 安装完成");
				MMDebug.DebugLogInfo("[MMFDependencyInstaller] 建议现在关闭该场景，然后重新打开它再进行播放。");
			}
		}

        /// <summary>
        /// 处理该列表请求
        /// </summary>
        static void ListProgress()
		{
			if (_listRequest.IsCompleted)
			{
				EditorApplication.update -= ListProgress;
				if (_listRequest.Status == StatusCode.Success)
				{
					InstallNext();
				}
				else if (_listRequest.Status >= StatusCode.Failure)
				{
					MMDebug.DebugLogInfo(_listRequest.Error.message);
				}
			}
		}

        /// <summary>
        /// 处理添加请求
        /// </summary>
        static void AddProgress()
		{
			if (_addRequest.IsCompleted)
			{
				if (_addRequest.Status == StatusCode.Success)
				{
					MMDebug.DebugLogInfo("[MMFDependencyInstaller] "+_addRequest.Result.packageId+" 已安装");
					_currentIndex++;
					InstallNext();
				}
				else if (_addRequest.Status >= StatusCode.Failure)
				{
					MMDebug.DebugLogInfo(_addRequest.Error.message);
				}
				EditorApplication.update -= AddProgress;
			}
		}
		#endif
	}    
}