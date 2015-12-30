using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure;
using SharepointCommon.Linq;

namespace SharepointCommon.Test.Linq
{
    public class CamlableVisitorTests
    {
        [Test]
        public void Items_Query_Mocked_Test()
        {

         //   var visitor = new CamlableVisitor();
          //  var camlModel = visitor.VisitQuery(queryModel);
          
          
           // QueryParser.CreateDefault().GetParsedQuery(


            var q = new CamlableQuery<Item>();

            var query = q.Where(i => i.Title == "asd" && i.Id != 3).ToList();



            


        }
    }
}
