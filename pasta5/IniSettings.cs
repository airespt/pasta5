using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace pasta5 {
    public class IniSettings {
        public string from;
        public string to;

        int version = 1;
        string fname;

        public IniSettings(string filename = "iniSettings") {
            fname = filename;
        }

        public bool Load() {
            try {
                using( var iniStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null) ) {
                    using( var fs = iniStore.OpenFile(fname, FileMode.OpenOrCreate) ) {
                        using( var reader = new StreamReader(fs) ) {
                            var ver = int.Parse(reader.ReadLine());
                            if( ver != version )
                                return false;

                            from = reader.ReadLine();
                            to = reader.ReadLine();
                        }
                    }
                }
            }
            catch( Exception ex ) {
                Console.Error.WriteLine(ex.ToString());
                return false;
            }
            return true;
        }

        public void Save() {
            try {
                using( var iniStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null) ) {
                    using( var fs = iniStore.OpenFile(fname, FileMode.Create) ) {
                        using( var writer = new StreamWriter(fs) ) {
                            writer.WriteLine(version);
                            writer.WriteLine(from);
                            writer.WriteLine(to);
                        }
                    }
                }
            }
            catch( Exception ex ) {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
}
