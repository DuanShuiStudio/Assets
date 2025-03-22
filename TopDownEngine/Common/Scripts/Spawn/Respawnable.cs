namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 玩家复活的界面
    /// </summary>
    public interface Respawnable
	{
		void OnPlayerRespawn(CheckPoint checkpoint, Character player);
	}
}