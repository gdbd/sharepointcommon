using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharepointCommon.Linq
{
    internal class CamlModel
    {
        private List<WhereModel> _wheres;

        public CamlModel()
        {
            _wheres = new List<WhereModel>();
        }

        public void AddWhere(CompareType compare, string fieldRef, object value)
        {
            _wheres.Add(new WhereModel { Comapare = compare, FieldRef = fieldRef, Value = value, });
        }
    }

    internal class WhereModel
    {
        public CompareType Comapare;
        public string FieldRef;
        public object Value;
    }

    internal enum CompareType
    {
        Eq,
        Gt,
        Lt
    }
}
