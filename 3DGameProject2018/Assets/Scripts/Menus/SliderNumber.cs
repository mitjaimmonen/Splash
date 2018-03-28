using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/********************************************
 * SliderNumber class
 *  Changes text element based on slider value
 */
public class SliderNumber : MonoBehaviour {

    /******************/
    /*Member Variables*/
    public Text text;



    #region Public Functions

    /// <summary>
    /// Call on slider change to change a label for the number
    /// </summary>
    /// <param name="slider"> Slider to take value from</param>
    public void ChangeText(Slider slider)
    {
        if(slider.value == 0)
        {
            text.text = "Off";
        } else
        {
            text.text = slider.value.ToString();
        }
    }

    #endregion

}
