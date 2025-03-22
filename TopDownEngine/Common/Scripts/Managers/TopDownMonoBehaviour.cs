using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// TopDownMonoBehaviour 类是所有 TopDownEngine 类的基础类
    /// TopDownMonoBehaviour 类本身不执行任何操作，但它确保如果您希望您的类继承自其他内容而不是普通的 MonoBehaviour，那么您有一个单一的更改点
    /// 一种常见的使用情况是用于改编脚本
    /// </summary>
    public class TopDownMonoBehaviour : MMMonoBehaviour { }	
}

