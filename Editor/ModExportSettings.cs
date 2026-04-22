using REPOLib.Objects.Sdk;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace REPOLibSdk.Editor
{
    public class ModExportSettings : ScriptableObject
    {
        [SerializeField] 
        private Mod _mod;

        [SerializeField]
        private List<Object> _extraFiles;

        [SerializeField]
        private List<Object> _extraBundleFiles;

        public Mod Mod
        {
            get => _mod; 
            set => _mod = value;
        }
        
        public IReadOnlyList<Object> ExtraFiles => _extraFiles;

        public IReadOnlyList<Object> ExtraBundleFiles => _extraBundleFiles;
    }
}
