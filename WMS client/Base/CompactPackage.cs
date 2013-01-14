using System;
using System.Collections.Generic;
using System.Text;

namespace WMS_client
{
    class CompactPackage
    {
        #region Private fields

        private const string PACKAGE_HEADER = "$T@RT";
        private const string PACKAGE_FOOTER = "#END>";
        private const string PACKAGE_SEPARATOR = "\t";

        private string PackageID;
        public string ClientName;
        public string QueryName;
        public string Parameters;

        private bool isClientParent;

        #endregion

        #region Constructors
        /*
         *  Format of the package: Header isClientParent PackageID ClientName | QueryName | Parameters Footer
         * 
         *  1.  Header:          $T@RT                               5 symbols
         *  2.  isClientParent:  "T" or "F"                          1 symbol
         *  3.  Separator        \t                                  1 symbol
         *  4.  PackageID:       some string like this "F84KU84U"    8 symbols
         *  5.  Separator        \t                                  1 symbol
         *  6.  ClientName:       unlimit string (1251 code page)     --
         *  7.  Separator        \t                                  1 symbol
         *  8.  QueryName:       unlimit string (1251 code page)     --
         *  7.  Separator        \t                                  1 symbol
         *  9.  Parameters:      unlimit string (1251 code page)     --
         * 10.  Footer:          #END>                               5 symbols
         * 
         *     Simple example: $T@RTFTU68Y8U6Denis\tGetBarCode\t2000000563421#END>
         *    
         */
        public CompactPackage():this("SERVER", false) {}
        // This is the major constructor (for creating package, not for recognizing)
        public CompactPackage(string ClientName, bool isClientParent)
            : base()
        {
            this.ClientName = ClientName;
            this.isClientParent = isClientParent;
            
            // Generation ID of this package
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();

            for (int i = 0; i < 8; i++)
            {
                int randValue = rand.Next(36);
                // Next string creates a symbol from this array - {"0","1",...,"9","A","B",..."Z"}
                byte[] byteMessage = new byte[] { (byte)(randValue + 55) };
                sb.Append((randValue < 10) ? randValue.ToString() : System.Text.Encoding.GetEncoding(1251).GetString(byteMessage,0,byteMessage.Length));
            }
            PackageID = sb.ToString();
        }                
        #endregion

        #region Public methods

        public static bool isCompletelyPackage(string data)
        {
            int headerIndex = data.IndexOf(PACKAGE_HEADER);
            if (headerIndex == -1) return false;
            int footerIndex = (data.Substring(headerIndex+PACKAGE_HEADER.Length)).IndexOf(PACKAGE_FOOTER);
            return footerIndex!=-1;
        }

        public Byte[] GetPackage()
        {
            string packageResult;

            packageResult = PACKAGE_HEADER +
                ((isClientParent) ? "T" : "F") + PackageID + ClientName + PACKAGE_SEPARATOR +
                QueryName + PACKAGE_SEPARATOR + Parameters + PACKAGE_FOOTER;

            return Encoding.GetEncoding(1251).GetBytes(packageResult);
        }

        public bool SetPackage(string parameters, out string tail)
        {
            if (!isCompletelyPackage(parameters))
            {
                tail = parameters;
                return false;
            }

            int nextPackageIndex = parameters.IndexOf(PACKAGE_FOOTER) + PACKAGE_FOOTER.Length;
            tail = parameters.Substring(nextPackageIndex);

            parameters = parameters.Substring(parameters.IndexOf(PACKAGE_HEADER) + PACKAGE_HEADER.Length, parameters.IndexOf(PACKAGE_FOOTER) - PACKAGE_FOOTER.Length);

            isClientParent = parameters[0] == 'T';
            if (!isClientParent && parameters[0] != 'F') return false;

            PackageID = parameters.Substring(1, 8);

            // Pointer to name field
            int IndexStart = 9;

            int IndexEnd = parameters.IndexOf('\t', IndexStart);
            if (IndexEnd == -1) { return false; }
            ClientName = parameters.Substring(IndexStart, IndexEnd - IndexStart);

            IndexStart = IndexEnd+1;

            IndexEnd = parameters.IndexOf('\t', IndexStart);
            if (IndexStart == -1) { return false; }
            QueryName = parameters.Substring(IndexStart, IndexEnd - IndexStart);
            Parameters = parameters.Substring(IndexEnd + 1);

            return true;
        }

        public void DefineQueryAndParams(string query, string parameters)
        {
            QueryName = query;
            Parameters = parameters;
        }

        #endregion
    }
}
