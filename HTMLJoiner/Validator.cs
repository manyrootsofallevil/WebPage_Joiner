using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTMLJoiner
{
    class Validator : IDataErrorInfo
    {
        public string Id { get; set; }

        #region IDataErrorInfo Members

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public string this[string columnName]
        {
            get
            {
                string result = null;

                if (columnName == "Id")
                {
                    if (string.IsNullOrEmpty(Id))
                    {
                        result = "Id must be populated";
                    }

                }

                return result;
            }
        }

        #endregion
    }
}
