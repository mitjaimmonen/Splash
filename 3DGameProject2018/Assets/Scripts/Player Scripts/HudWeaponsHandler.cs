using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HudWeaponsHandler : MonoBehaviour {

	public List<Image> weaponIconImages = new List<Image>();

	public List<Image> backgroundImages = new List<Image>();
	public Sprite activeBackgroundSprite;
	public Sprite inactiveBackgroundSprite;

	public Sprite pistolSprite;
	public Sprite shotgunSprite;
	public Sprite autorifleSprite;
	public Sprite launcherSprite;

	public void SetWeaponUI(int maxWeapons, List<Weapon> carriedWeapons, int weaponIndex)
	{
		Debug.Log("weapon icon images count: " + weaponIconImages.Count);

		for (int i = 0; i < weaponIconImages.Count; i++)
		{
			if (i >= maxWeapons)
			{
				Debug.Log("setting weapon icon false");
				weaponIconImages[i].gameObject.SetActive(false);
				backgroundImages[i].gameObject.SetActive(false);
				continue;
			}
			else
			{
				weaponIconImages[i].gameObject.SetActive(false);

				backgroundImages[i].gameObject.SetActive(true);

				if (i < carriedWeapons.Count)
				{
					switch (carriedWeapons[i].weaponData.weaponType)
					{
						case WeaponType.pistol :
							weaponIconImages[i].gameObject.SetActive(true);
							weaponIconImages[i].sprite = pistolSprite;
						break;
						case WeaponType.shotgun :
							weaponIconImages[i].gameObject.SetActive(true);
							weaponIconImages[i].sprite = shotgunSprite;
						break;
						case WeaponType.autorifle :
							weaponIconImages[i].gameObject.SetActive(true);
							weaponIconImages[i].sprite = autorifleSprite;
						break;
						case WeaponType.launcher :
							weaponIconImages[i].gameObject.SetActive(true);
							weaponIconImages[i].sprite = launcherSprite;
						break;

						default :
							weaponIconImages[i].gameObject.SetActive(false);
							weaponIconImages[i].sprite = null;
						break;
					}
				}

				if (i == weaponIndex)
					backgroundImages[i].sprite = activeBackgroundSprite;

				else
					backgroundImages[i].sprite = inactiveBackgroundSprite;

				
				
			}
		}
	}
}
