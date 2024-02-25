using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatInput : MonoBehaviour
{
    public TMP_InputField inputField;

    public void ParseFloat() 
    {
        string floatString = "";

        for(int i = 0; i < inputField.text.Length; i++)
        {
            char curChar = inputField.text[i];

            if((curChar < '0' || curChar > '9') && (curChar != '.' && curChar != '-')) continue;
            if(curChar == '.' && floatString.Contains(".")) continue;
            if(curChar == '-' && i != 0) continue;
            
            floatString = floatString + curChar;
        }

        inputField.text = floatString;
    }
}
