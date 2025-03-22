using UnityEngine;
using System.Collections;
using System.Text;
using MoreMountains.Tools;
using UnityEngine.UI;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个将进度条和文本显示结合在一起的类
    /// 并且可以用来显示武器的当前弹药量
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Ammo Display")]
	public class AmmoDisplay : MMProgressBar 
	{
		[MMInspectorGroup("Ammo Display", true, 12)]
		/// the ID of the AmmoDisplay 
		[Tooltip("弹药显示的ID")]
		public int AmmoDisplayID = 0;
		/// the Text object used to display the current ammo numbers
		[Tooltip("用于显示当前弹药数量的文本对象")]
		public Text TextDisplay;
		#if MM_TEXTMESHPRO
		/// the TMP object used to display the current ammo numbers
		[Tooltip("用于显示当前弹药数量的TMP（可能是“TextMeshPro”的缩写）对象")]
		public TMP_Text TextDisplayTextMeshPro;
		#endif

		protected int _totalAmmoLastTime, _maxAmmoLastTime, _ammoInMagazineLastTime, _magazineSizeLastTime;
		protected StringBuilder _stringBuilder;
		protected bool _isTextDisplayTextMeshProNotNull;

        /// <summary>
        /// 在初始化时，我们初始化我们的字符串构建器
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_stringBuilder = new StringBuilder();
			#if MM_TEXTMESHPRO
			_isTextDisplayTextMeshProNotNull = TextDisplayTextMeshPro != null;
			#endif
		}

        /// <summary>
        /// 用参数字符串更新文本显示
        /// </summary>
        /// <param name="newText">New text.</param>
        public virtual void UpdateTextDisplay(string newText)
		{
			if (TextDisplay != null)
			{
				TextDisplay.text = newText;
			}
			
			#if MM_TEXTMESHPRO
			if (_isTextDisplayTextMeshProNotNull)
			{
				TextDisplayTextMeshPro.text = newText;
			}
			#endif
		}

        /// <summary>
        /// 更新弹药显示的文本和进度条
        /// </summary>
        /// <param name="magazineBased">If set to <c>true</c> magazine based.</param>
        /// <param name="totalAmmo">Total ammo.</param>
        /// <param name="maxAmmo">Max ammo.</param>
        /// <param name="ammoInMagazine">Ammo in magazine.</param>
        /// <param name="magazineSize">Magazine size.</param>
        /// <param name="displayTotal">If set to <c>true</c> display total.</param>
        public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, bool displayTotal)
		{
            // 我们确保有实际的内容需要更新
            if ((_totalAmmoLastTime == totalAmmo)
			    && (_maxAmmoLastTime == maxAmmo)
			    && (_ammoInMagazineLastTime == ammoInMagazine)
			    && (_magazineSizeLastTime == magazineSize))
			{
				return;
			}

			_stringBuilder.Clear();
			
			if (magazineBased)
			{
				this.UpdateBar(ammoInMagazine,0,magazineSize);	
				if (displayTotal)
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					_stringBuilder.Append(" - ");
					_stringBuilder.Append((totalAmmo - ammoInMagazine).ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());					
				}
				else
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());
				}
			}
			else
			{
				_stringBuilder.Append(totalAmmo.ToString());
				_stringBuilder.Append("/");
				_stringBuilder.Append(maxAmmo.ToString());
				this.UpdateBar(totalAmmo,0,maxAmmo);	
				this.UpdateTextDisplay (_stringBuilder.ToString());
			}

			_totalAmmoLastTime = totalAmmo;
			_maxAmmoLastTime = maxAmmo;
			_ammoInMagazineLastTime = ammoInMagazine;
			_magazineSizeLastTime = magazineSize;
		}
	}
}