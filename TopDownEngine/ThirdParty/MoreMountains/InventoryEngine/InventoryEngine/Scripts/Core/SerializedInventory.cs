using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
    /// <summary>
    /// 用于从文件中存储/加载库存的序列化类
    /// </summary>
    public class SerializedInventory 
	{
		public int NumberOfRows;
		public int NumberOfColumns;
		public string InventoryName = "Inventory";
		public MoreMountains.InventoryEngine.Inventory.InventoryTypes InventoryType ;
		public bool DrawContentInInspector=false;
		public string[] ContentType;
		public int[] ContentQuantity;		
	}
}