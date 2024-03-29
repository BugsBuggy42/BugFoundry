﻿namespace BugFoundry.BugFoundryEditor.Management
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "BugFoundryColors", menuName = "ScriptableObjects/BugFoundryColorsUnite", order = 1)]
    public class BugFoundryColors: ScriptableObject
    {
        public Color keyword;
        public Color keywordControl;
        public Color className;
        public Color identifier;
        public Color stringLiteral;
        public Color methodName;
        public Color interfaceName;
        public Color namespaceName;
        public Color parameterName;
        public Color staticSymbol;
        public Color propertyName;
        public Color structName;
        public Color defaultColor;
        public Color enumMemberName;
        public Color enumName;
        public Color delegateName;
        public Color fieldName;
        public Color comment;
        public Color number;
        public Color operatorOverloaded;
        public Color backgroundColor;
        public Color scrollbarBody;
        public Color scrollbarHandle;
    }
}