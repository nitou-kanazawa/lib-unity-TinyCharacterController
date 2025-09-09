using System;

namespace Nitou.IO {

    public interface IUnityProjectPath {

        /// <summary>
        /// 
        /// </summary>
        public string RootFolder { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ProjectPath {get;}

        public bool IsAssetsPath => RootFolder == "Assets";
        public bool IsPackagesPath => RootFolder == "Packages";
    }
}
