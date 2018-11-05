using UnityEngine;
using System.Collections;
using System;

namespace TMPro
{
    /// <summary>
    /// Example of a Custom Character Input Validator to only allow phone number in the (800) 555-1212 format.
    /// </summary>
    [Serializable]
    //[CreateAssetMenu(fileName = "InputValidator - Phone Numbers.asset", menuName = "TextMeshPro/Input Validators/Phone Numbers")]
    public class TMP_PhoneNumberValidator : TMP_InputValidator
    {
        // Custom text input validation function
        public override char Validate(ref string text, ref int pos, char ch)
        {
            Debug.Log("Trying to validate...");
            
            // Return unless the character is a valid digit
            if (ch < '0' && ch > '9') return (char)0;

            int length = text.Length;

            // Enforce Phone Number format for every character input.
            for (int i = 0; i < length + 1; i++)
            {
                switch (i)
                {
                    case 0:
                        if (i == length)
                            text = "(" + ch;
                        pos = 2;
                        break;
                    case 1:
                        if (i == length)
                            text += ch;
                        pos = 2;
                        break;
                    case 2:
                        if (i == length)
                            text += ch;
                        pos = 3;
                        break;
                    case 3:
                        if (i == length)
                            text += ch + ") ";
                        pos = 6;
                        break;
                    case 4:
                        if (i == length)
                            text += ") " + ch;
                        pos = 7;
                        break;
                    case 5:
                        if (i == length)
                            text += " " + ch;
                        pos = 7;
                        break;
                    case 6:
                        if (i == length)
                            text += ch;
                        pos = 7;
                        break;
                    case 7:
                        if (i == length)
                            text += ch;
                        pos = 8;
                        break;
                    case 8:
                        if (i == length)
                            text += ch + "-";
                        pos = 10;
                        break;
                    case 9:
                        if (i == length)
                            text += "-" + ch;
                        pos = 11;
                        break;
                    case 10:
                        if (i == length)
                            text += ch;
                        pos = 11;
                        break;
                    case 11:
                        if (i == length)
                            text += ch;
                        pos = 12;
                        break;
                    case 12:
                        if (i == length)
                            text += ch;
                        pos = 13;
                        break;
                    case 13:
                        if (i == length)
                            text += ch;
                        pos = 14;
                        break;
                }
            }

            return ch;
        }
    }
}
