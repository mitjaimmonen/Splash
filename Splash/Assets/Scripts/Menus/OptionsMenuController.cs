using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*************************************
 * OptionsMenuController class
 *  Holds functions to change options,
 *  save the options, and call state handler 
 *  to rebind icontroller and delete this object
 */
public class OptionsMenuController : MonoBehaviour, IController {

    /******************/
    /*Member Variables*/
    public IController lastController;



    /*****************/
    /*Implementations*/
    public void InputHandle(string[] input)
    {
        throw new NotImplementedException();
    }
    

    
    /******************************
    *            To Do
    *  Functions to change settings
    *  save settings change statehandler
    *  options setting exit functions 
    *  to set state icontroller to last 
    *  controller
    *  
    ******************************/
}
