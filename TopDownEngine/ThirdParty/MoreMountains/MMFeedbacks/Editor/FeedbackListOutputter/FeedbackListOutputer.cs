﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个类用于自动安装MMFeedbacks中使用的可选依赖项
    /// </summary>
    public static class FeedbackListOutputer 
	{
        /// <summary>
        /// 输出所有MMFeedbacks到控制台（只有一个目标用户，就是我 hello！）
        /// </summary>
        [MenuItem("Tools/More Mountains/MMFeedbacks/Output MMF_Feedbacks list", false, 705)]
		public static void OutputIFeedbacksList()
		{
            // 检索可用的反馈
            List<System.Type> types = (from domainAssembly in System.AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where assemblyType.IsSubclassOf(typeof(MMF_Feedback))
				select assemblyType).ToList();
            
			List<string> typeNames = new List<string>();


			string previousType = "";
			for (int i = 0; i < types.Count; i++)
			{
				MMFeedbacksEditor.FeedbackTypePair newType = new MMFeedbacksEditor.FeedbackTypePair();
				newType.FeedbackType = types[i];
				newType.FeedbackName = FeedbackPathAttribute.GetFeedbackDefaultPath(types[i]);
				if (newType.FeedbackName == "MMF_FeedbackBase")
				{
					continue;
				}

				string newEntry = FeedbackPathAttribute.GetFeedbackDefaultPath(newType.FeedbackType);
				typeNames.Add(newEntry);
			}
            
			typeNames.Sort();
			StringBuilder builder = new StringBuilder();
			int counter = 1;
			foreach (string typeName in typeNames)
			{
				if (typeName == null)
				{
					continue;
				}
				string[] splitArray =  typeName.Split(char.Parse("/"));
                
				if ((previousType != splitArray[0]) && (counter > 1))
				{
					builder.Append("\n");
				}
                
				builder.Append("- [ ] ");
				builder.Append(counter.ToString("000"));
				builder.Append(" - ");
				builder.Append(typeName);
				builder.Append("\n");

				previousType = splitArray[0];
				counter++;
			}
			MMDebug.DebugLogInfo(builder.ToString());
		}
	}    
}